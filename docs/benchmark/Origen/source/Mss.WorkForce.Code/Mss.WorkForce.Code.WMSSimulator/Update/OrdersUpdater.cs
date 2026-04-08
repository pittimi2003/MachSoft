using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.WMSSimulator.Helper;
using Mss.WorkForce.Code.WMSSimulator.Helper.Methods;

namespace Mss.WorkForce.Code.WMSSimulator.Update
{
    public static class OrdersUpdater
    {
        #region Methods

        #region Public

        /// <summary>
        /// Update the orders and their lines in the databases
        /// </summary>
        /// <param name="isOut">Boolean that indicates if the orders are outbound or inbound</param>
        /// <param name="delay">Miliseconds to refresh the update service</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="warehouseId">Id of the warehouse to update the orders</param>
        /// <param name="wfmData">WFM data</param>
        public static void DoUpdate(bool isOut, int delay, List<WMSModel.Parameter> parameters, Guid warehouseId, DataBaseResponse wfmData) 
        {
            /*
             * 1. Todas las InputOrders de WFM estarán en Waiting 0%
             * 2. Entramos a mirar las distintas WOP para actualizar:
             *  1. Para las órdenes en Waiting a 0%, si estamos entre un tiempo por parámetro y el InitDate de la WOP, la pasamos a Released 0%
             *  2. Para Released, miramos que IP de esa WOP tienen el EndDate <= UtcNow, lo añadimos a InputOrderProcessClosing, hacemos el cálculo para guardar el NumProcesses
             *  y marcamos el IsDrawn para saber si ya lo hemos pintado o no
             *  3. Miramos que si tenemos algún Picking insertado lo pasamos al 25%, algún shipping al 75 %, y algún Loading al 100%. Si fuera de entrada sería igual pero
             *  con Inbound, reception, putaway.
             *  4. Miramos si tenemos algún proceso por pintar, y en caso de que no tengamos, marcamos como Closed.
             */

            var utcNow = DateTime.UtcNow;

            var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

            var optionsBuilderWFM = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilderWFM.UseNpgsql(DataBaseActions.WFMConnectionString);

            using (ApplicationDbContext wfmContext = new ApplicationDbContext(optionsBuilderWFM.Options))
            {
                using (WMSDbContext? wmsData = new WMSDbContext(optionsBuilderWMS.Options))
                {
                    var todayPlanning = wmsData.Plannings.FirstOrDefault(x => x.WarehouseId == warehouseId && x.CreationDate.Date == utcNow.Date);
                    var workOrders = todayPlanning != null ? wmsData.WorkOrderPlannings.Where(x => x.PlanningId == todayPlanning.Id && x.IsOutbound == isOut).ToList()
                        : new List<WMSModel.WorkOrderPlanning>();

                    foreach (var wop in workOrders)
                    {
                        var inputOrder = wfmData.InputOrders.FirstOrDefault(x => x.Id == wop.InputOrderId);

                        // Waiting 0% a Released 0%
                        if (inputOrder.Status == OrderStatus.Waiting && !inputOrder.IsBlocked.GetValueOrDefault(false) && inputOrder.Progress == 0)
                        {
                            var minMarginTime = wop.InitDate.AddMinutes(-parameters.FirstOrDefault(x => x.Code == ParameterValue.AppointmentDelayTime).Value);

                            if (utcNow >= minMarginTime)
                            {
                                inputOrder.ReleaseDate = utcNow;
                                inputOrder.UpdateDate = utcNow;
                                inputOrder.Status = OrderStatus.Released;
                                wfmContext.InputOrders.Update(inputOrder);
                            }
                        }
                        
                        // Cambio de estado y porcentaje de las Released e insercción en InputOrderProcessClosing si fuera necesario (aquí va la resta de los steps)
                        else if (inputOrder.Status == OrderStatus.Released)
                        {
                            // Si no tiene ningún ItemPlanning es porque aún no ha llegado la hora de pintarlo, pero no puede ser porque no haya ninguno sin pintar,
                            // porque en el momento que se pinte el último lo cerramos y ya no entraría aquí pues su estado sería Closed

                            var allItemPlanning = wmsData.ItemPlannings.Where(x => x.WorkOrderPlanningId == wop.Id).ToList();
                            foreach (var ip in allItemPlanning.Where(x => !x.IsDrawn && x.EndDate <= utcNow))
                            {
                                // RESTA
                                var sumSteps =
                                    wfmData.Steps.Where(x => x.ProcessId == ip.ProcessId && x.InitProcess).Sum(x => x.TimeMin) +
                                    wfmData.Steps.Where(x => x.ProcessId == ip.ProcessId && x.EndProcess).Sum(x => x.TimeMin);
                                var process = wfmData.Processses.FirstOrDefault(x => x.Id == ip.ProcessId);
                                var pickingRoadTime = 0;
                                if (process.Type == ProcessType.Picking)
                                    pickingRoadTime = wfmData.Pickings.ToList().FirstOrDefault(x => x.ProcessId == ip.ProcessId).PickingRoadTime.GetValueOrDefault(0);
                                var sumProcess = process.PreprocessTime.GetValueOrDefault(0) + process.MinTime + process.PostprocessTime.GetValueOrDefault(0);
                                var numProcesses = sumProcess == 0 ? 0 : Convert.ToInt32(Math.Round((decimal)((wfmData.ItemPlannings.FirstOrDefault(x => x.Id == ip.Id).WorkTime - sumSteps - pickingRoadTime) / (sumProcess)), 0));

                                wfmContext.InputOrderProcessesClosing.Add(new Models.Models.InputOrderProcessClosing()
                                {
                                    Id = ip.Id,
                                    NotificationId = Guid.NewGuid(),
                                    ProcessType = wfmData.Processses.FirstOrDefault(x => x.Id == ip.ProcessId)?.Type,
                                    Worker = wfmData.Workers.FirstOrDefault(x => x.Id == ip.WorkerId)?.Name,
                                    EquipmentGroup = wfmData.EquipmentGroups.FirstOrDefault(x => x.Id == ip.EquipmentGroupId)?.Name,
                                    InitDate = ip.InitDate,
                                    EndDate = ip.EndDate,
                                    InputOrder = wfmData.InputOrders.FirstOrDefault(x => x.Id == wop.InputOrderId)?.OrderCode,
                                    NumProcesses = numProcesses,
                                    ZoneCode = wfmData.Zones.FirstOrDefault(x => x.Id == ip.ZoneId)?.Name
                                });

                                ip.IsDrawn = true;
                                wmsData.ItemPlannings.Update(ip);
                                wmsData.SaveChanges();
                            }

                            if (allItemPlanning.Any())
                            {
                                var progress = (double)allItemPlanning.Count(x => x.IsDrawn) / allItemPlanning.Count;
                                inputOrder.Progress = Math.Round(progress * 100, 2);
                            }
                            else
                            {
                                inputOrder.Progress = 0;
                            }

                            inputOrder.UpdateDate = utcNow;

                            // Si están todos ya dibujados la cierro y asocio el dock
                            if (wmsData.ItemPlannings.Where(x => x.WorkOrderPlanningId == wop.Id).All(x => x.IsDrawn))
                            {
                                inputOrder.Status = OrderStatus.Closed;
                                if (inputOrder.IsOutbound) inputOrder.AssignedDockId = wfmData.WorkOrderPlannings.FirstOrDefault(x => x.Id == wop.Id)?.AssignedDockId;
                                wfmContext.InputOrders.Update(inputOrder);
                            }
                            // Si hay procesos de Picking o Inbound asocio el dock
                            else if (
                                wmsData.ItemPlannings.ToList()
                                    .Any(x => (x.WorkOrderPlanningId == wop.Id && wfmData.Processses.First(p => p.Id == x.ProcessId).Type == ProcessType.Inbound) && x.IsDrawn))
                            {
                                inputOrder.AssignedDockId = wfmData.WorkOrderPlannings.FirstOrDefault(x => x.Id == wop.Id)?.AssignedDockId;
                                wfmContext.InputOrders.Update(inputOrder);
                            }
                            // Por defecto actualizo la orden con el nuevo valor del progreso
                            else wfmContext.InputOrders.Update(inputOrder);
                        }
                    }
                }

                wfmContext.SaveChanges();
            }

            optionsBuilderWFM = null;
            optionsBuilderWMS = null;

            GC.Collect();
        }

        #endregion

        #region Private

        /// <summary>
        /// Assignes a Dock to the InputOrder
        /// </summary>
        /// <param name="isOut">Boolean that indicates whether the order is outbound or inbound</param>
        /// <param name="wfmOrders">List of WFM orders to update</param>
        /// <param name="context">Contexto to save in the data base</param>
        private static void AssignDock(bool isOut, List<Models.Models.InputOrder> wfmOrders, ApplicationDbContext? context)
        {
            foreach (var o in wfmOrders.Where(x => x.AssignedDockId == null))
            {
                if (isOut && (o.Status == OrderStatus.Closed || o.Status == OrderStatus.Cancelled)
                    && context.InputOrderProcessesClosing.Where(x => x.InputOrder == o.OrderCode).Select(x => x.ProcessType).Contains(ProcessType.Loading))
                {
                    var dock = context.Docks.Include(x => x.Zone).Where(m => m.Zone.Area.Layout.Warehouse.Id == context.InputOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).WarehouseId).FirstOrDefault(x => x.AllowOutbound == true);

                    foreach (var i in wfmOrders.Where(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode))
                    {
                        context.InputOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).AssignedDockId = dock.Id;
                    }

                    if (context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).DockCode == null)
                    {
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).DockCode = dock.Zone.Name;
                    }

                    context.SaveChanges();
                }
                else if (!isOut && (o.Status == OrderStatus.Released || o.Status == OrderStatus.Cancelled)
                    && context.InputOrderProcessesClosing.Where(x => x.InputOrder == o.OrderCode).Select(x => x.ProcessType).Contains(ProcessType.Inbound))
                {
                    var dock = context.Docks.Include(x => x.Zone).Where(m => m.Zone.Area.Layout.Warehouse.Id == context.InputOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).WarehouseId).FirstOrDefault(x => x.AllowInbound == true);

                    foreach (var i in wfmOrders.Where(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode))
                    {
                        context.InputOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).AssignedDockId = dock.Id;
                    }

                    if (context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).DockCode == null)
                    {
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).DockCode = dock.Zone.Name;
                    }

                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Updates the WMS orders
        /// </summary>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="context">WMS database context</param>
        /// <param name="isOut">Boolean that specifies if the orders are inobund or outbound</param>
        /// <param name="wfmData">WFM data</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="releaseDate">Release date of the order when opens</param>
        private static void UpdateWMSOrders(List<WMSModel.InputOrder> wmsOrders, WMSDbContext? context, bool isOut, DataBaseResponse wfmData, List<WMSModel.Parameter> parameters,
            DateTime releaseDate)
        {
            if (wmsOrders.Any())
            {
                var order = isOut ? "outbound" : "inbound";
                List<string> distinctStatus = new List<string>();
                distinctStatus.AddRange(wmsOrders.Select(x => x.Status).Distinct());

                foreach (var s in distinctStatus)
                {
                    switch (s)
                    {
                        case OrderStatus.Waiting:
                            Console.WriteLine($"Updating WMS {order} orders with status '{s}'");
                            //wmsOrders = UpdateStatus.Prepared(wmsOrders, s, wfmData, parameters, releaseDate);
                            continue;

                        case OrderStatus.Released:
                            Console.WriteLine($"Updating WMS {order} orders with status '{s}'");
                            //wmsOrders = UpdateStatus.Open(wmsOrders, s, wfmData, parameters);
                            continue;

                        default:
                            continue;
                    }
                }
                

                foreach (var o in wmsOrders)
                {
                    o.UpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                    context.Update(o);
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the WFM orders and lines
        /// </summary>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="wfmOrders">Orders in the WFM database</param>
        /// <param name="wfmOrderLines">Order lines in the WFM database</param>
        /// <param name="context">WFM database context</param>
        /// <param name="isOut">Boolean that specifies if the orders are inobund or outbound</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="releaseDate">Release date of the order when opens</param>
        private static void UpdateWFMOrdersAndLines(List<Models.Models.InputOrder> wfmOrders, List<Models.Models.InputOrderLine> wfmOrderLines,
            List<WMSModel.InputOrder> wmsOrders, ApplicationDbContext? context, bool isOut, List<WMSModel.Parameter> parameters, DateTime releaseDate)
        {
            if (wfmOrders.Any() && wmsOrders.Any())
            {
                var order = isOut ? "outbound" : "inbound";
                List<string> distinctStatus = new List<string>();
                distinctStatus.AddRange(wfmOrders.Select(x => x.Status).Distinct());

                foreach (var s in distinctStatus)
                {
                    switch (s)
                    {
                        case OrderStatus.Waiting:
                            Console.WriteLine($"Updating WFM {order} orders and lines with status '{s}'");
                            //wfmOrders = UpdateStatus.Prepared(wfmOrders, s, parameters, wmsOrders, releaseDate);
                            //wfmOrderLines = UpdateStatus.Lines(wmsOrders, wfmOrders, wfmOrderLines);
                            continue;

                        case OrderStatus.Released:
                            Console.WriteLine($"Updating WFM {order} orders and lines with status '{s}'");
                            //wfmOrders = UpdateStatus.Open(wfmOrders, wmsOrders, s, parameters);
                            //wfmOrderLines = UpdateStatus.Lines(wmsOrders, wfmOrders, wfmOrderLines);
                            continue;

                        default:
                            Console.WriteLine($"Can not update orders with status '{s}'");
                            continue;

                    }

                }

                foreach (var o in wfmOrders)
                {
                    context.Update(o);

                    if (o.IsOutbound && o.Status == OrderStatus.Released && o.Progress == 75 &&
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).InitDate == null)
                    {
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).InitDate = o.AppointmentDate;
                    }
                    else if (o.IsOutbound && o.Status == OrderStatus.Closed &&
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).EndDate == null)
                    {
                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.InputCreationWindowTime).Value * 60;
                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.InboundDurationTime).Value;
                        var duration = delay > processTime ? processTime : delay;
                        var date = o.AppointmentDate.AddSeconds(duration);

                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).EndDate = date;
                    }
                    else if (!o.IsOutbound && o.Status == OrderStatus.Released && o.Progress == 0 &&
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).InitDate == null)
                    {
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).InitDate = o.AppointmentDate;
                    }
                    else if (!o.IsOutbound && o.Status == OrderStatus.Released && o.Progress == 25 &&
                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).EndDate == null)
                    {
                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.OutputCreationWindowTime).Value * 60;
                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.LoadingDurationTime).Value;
                        var duration = delay > processTime ? processTime : delay;
                        var date = o.AppointmentDate.AddSeconds(duration);

                        context.YardAppointmentsNotifications.FirstOrDefault(x => x.AppointmentDate == o.AppointmentDate && x.VehicleCode == o.VehicleCode).EndDate = date;
                    }

                    var lines = wfmOrderLines.Where(x => x.InputOutboundOrder.OrderCode == o.OrderCode);
                    foreach (var l in lines)
                    {
                        context.Update(l);
                    }
                }

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts the closed processes in the WFM database
        /// </summary>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="wfmOrders">Orders in the WFM database</param>
        /// <param name="wfmOrderLines">Order lines in the WFM database</param>
        /// <param name="context">WFM database context</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        private static void InsertClosedProcesses(List<Models.Models.InputOrder> wfmOrders, List<Models.Models.InputOrderLine> wfmOrderLines,
            List<WMSModel.InputOrder> wmsOrders, ApplicationDbContext? context, List<WMSModel.Parameter> parameters)
        {
            List<Models.Models.InputOrderProcessClosing> processesClosed = new List<Models.Models.InputOrderProcessClosing>();

            if (wfmOrders.Any() && wmsOrders.Any())
            {
                try
                {
                    Console.WriteLine("Creating and inserting closed processes");

                    foreach (var o in wfmOrders)
                    {
                        if (wmsOrders.Select(x => x.OrderCode).Contains(o.OrderCode))
                        {
                            var lines = wfmOrderLines.Where(x => x.InputOutboundOrder.OrderCode == o.OrderCode);

                            foreach (var l in lines)
                            {
                                if (o.IsOutbound)
                                {
                                    if (context.Processes.Any(x => x.Type == ProcessType.Loading && x.Area.Layout.WarehouseId == o.WarehouseId) && 
                                        o.Status == OrderStatus.Closed && !context.InputOrderProcessesClosing.Where(x => x.InputOrder == o.OrderCode).Any(x => x.ProcessType == ProcessType.Loading))
                                    {
                                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.OutputCreationWindowTime).Value * 60;
                                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.LoadingDurationTime).Value;
                                        var duration = delay > processTime ? processTime : delay;

                                        processesClosed.Add(new Models.Models.InputOrderProcessClosing()
                                        {
                                            Id = Guid.NewGuid(),
                                            NotificationId = Guid.NewGuid(),
                                            ProcessType = ProcessType.Loading,
                                            Worker = SelectResource.Worker(context, o, ProcessType.Loading, processTime),
                                            EquipmentGroup = SelectResource.Equipment(context, o),
                                            InitDate = o.AppointmentDate,
                                            EndDate = o.AppointmentDate.AddSeconds(duration),
                                            InputOrder = o.OrderCode
                                        });

                                        break;
                                    }

                                    if (context.Processes.Any(x => x.Type == ProcessType.Picking && x.Area.Layout.WarehouseId == o.WarehouseId) &&
                                        wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress >= 25 && wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress < 50)
                                    {
                                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.OutputCreationWindowTime).Value * 60;
                                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.PickingDurationTime).Value;
                                        var duration = delay > processTime ? processTime : delay;

                                        processesClosed.Add(new Models.Models.InputOrderProcessClosing()
                                        {
                                            Id = Guid.NewGuid(),
                                            NotificationId = Guid.NewGuid(),
                                            ProcessType = ProcessType.Picking,
                                            Worker = SelectResource.Worker(context, o, ProcessType.Picking, processTime),
                                            EquipmentGroup = SelectResource.Equipment(context, o),
                                            InitDate = DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(-duration), DateTimeKind.Utc),
                                            EndDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                                            InputOrder = o.OrderCode
                                        });
                                    }
                                    else if (context.Processes.Any(x => x.Type == ProcessType.Shipping && x.Area.Layout.WarehouseId == o.WarehouseId) && wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress >= 50 && wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress < 75)
                                    {
                                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.OutputCreationWindowTime).Value * 60;
                                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.ShippingDurationTime).Value;
                                        var duration = delay > processTime ? processTime : delay;

                                        processesClosed.Add(new Models.Models.InputOrderProcessClosing()
                                        {
                                            Id = Guid.NewGuid(),
                                            NotificationId = Guid.NewGuid(),
                                            ProcessType = ProcessType.Shipping,
                                            Worker = SelectResource.Worker(context, o, ProcessType.Shipping, processTime),
                                            EquipmentGroup = SelectResource.Equipment(context, o),
                                            InitDate = DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(-duration), DateTimeKind.Utc),
                                            EndDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                                            InputOrder = o.OrderCode
                                        });
                                    }
                                }
                                else
                                {
                                    if (context.Processes.Any(x => x.Type == ProcessType.Inbound && x.Area.Layout.WarehouseId == o.WarehouseId) && wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress >= 25 && wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress < 50)
                                    {
                                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.InputCreationWindowTime).Value * 60;
                                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.InboundDurationTime).Value;
                                        var duration = delay > processTime ? processTime : delay;

                                        processesClosed.Add(new Models.Models.InputOrderProcessClosing()
                                        {
                                            Id = Guid.NewGuid(),
                                            NotificationId = Guid.NewGuid(),
                                            ProcessType = ProcessType.Inbound,
                                            Worker = SelectResource.Worker(context, o, ProcessType.Inbound, processTime),
                                            EquipmentGroup = SelectResource.Equipment(context, o),
                                            InitDate = o.AppointmentDate,
                                            EndDate = o.AppointmentDate.AddSeconds(duration),
                                            InputOrder = o.OrderCode
                                        });

                                        break;
                                    }
                                    else if (context.Processes.Any(x => x.Type == ProcessType.Reception && x.Area.Layout.WarehouseId == o.WarehouseId) && wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress >= 50 && wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress < 75)
                                    {
                                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.InputCreationWindowTime).Value * 60;
                                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.ReceptionDurationTime).Value;
                                        var duration = delay > processTime ? processTime : delay;

                                        processesClosed.Add(new Models.Models.InputOrderProcessClosing()
                                        {
                                            Id = Guid.NewGuid(),
                                            NotificationId = Guid.NewGuid(),
                                            ProcessType = ProcessType.Reception,
                                            Worker = SelectResource.Worker(context, o, ProcessType.Reception, processTime),
                                            EquipmentGroup = SelectResource.Equipment(context, o),
                                            InitDate = DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(-duration), DateTimeKind.Utc),
                                            EndDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                                            InputOrder = o.OrderCode
                                        });
                                    }
                                    else if (context.Processes.Any(x => x.Type == ProcessType.Putaway && x.Area.Layout.WarehouseId == o.WarehouseId) && o.Status == OrderStatus.Closed && !context.InputOrderProcessesClosing.Where(x => x.InputOrder == o.OrderCode).Any(x => x.ProcessType == ProcessType.Putaway))
                                    {
                                        var delay = parameters.FirstOrDefault(x => x.Code == ParameterValue.InputCreationWindowTime).Value * 60;
                                        var processTime = parameters.FirstOrDefault(x => x.Code == ParameterValue.PutawayDurationTime).Value;
                                        var duration = delay > processTime ? processTime : delay;

                                        processesClosed.Add(new Models.Models.InputOrderProcessClosing()
                                        {
                                            Id = Guid.NewGuid(),
                                            NotificationId = Guid.NewGuid(),
                                            ProcessType = ProcessType.Putaway,
                                            Worker = SelectResource.Worker(context, o, ProcessType.Putaway, processTime),
                                            EquipmentGroup = SelectResource.Equipment(context, o),
                                            InitDate = DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(-duration), DateTimeKind.Utc),
                                            EndDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                                            InputOrder = o.OrderCode
                                        });
                                    }
                                }
                            }
                        }
                    }

                    context.InputOrderProcessesClosing.AddRange(processesClosed);
                    context.SaveChanges();

                    Console.WriteLine("Closed processes generated and saved successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while creating closed processes. {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No orders to insert closed processes");
            }
        }

        #endregion
        #endregion
    }
}
