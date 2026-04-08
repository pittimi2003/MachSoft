using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Labor.Core;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Yard;
using Mss.WorkForce.Code.Yard.Core;

namespace Mss.WorkForce.Code.Simulator.DatabaseUtilities
{
    public class DataBaseUtilitiesSimulator
    {
        public ApplicationDbContext context { get; set; }

        public DataBaseUtilitiesSimulator(ApplicationDbContext context) { this.context = context; }


        /// <summary>
        /// Calculates the labor data from the simulation ItemPlannings and inserts them into the database
        /// </summary>
        /// <param name="itemPlannings">List of item plannings returned by the simulation</param>
        /// <param name="data">Data already brought from the database</param>
        public void CalculateAndSaveLabor(IEnumerable<ItemPlanning> itemPlannings, DataSimulatorTablaRequest data)
        {
            var items = itemPlannings?.ToList();
            if (items == null || items.Count == 0)
            {
                Console.WriteLine("No item plannings for Labor calculations.");
                return;
            }

            var utcNow = DateTime.UtcNow;
            var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0, DateTimeKind.Utc);

            var prevQueryTracking = context.ChangeTracker.QueryTrackingBehavior;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            foreach (var ip in items)
                ip.EquipmentGroup = data.EquipmentGroup.FirstOrDefault(x => x.Id == ip.EquipmentGroupId);

            var schedules = data.Schedule;
            var breaks = data.Break;
            var processes = data.Process;
            var steps = data.Step;
            var shifts = data.Shift;

            // WORKERS
            var wfmLabor = LaborCalculations.WFMLabor(items, data.Schedule, breaks, steps, processes, now).ToList();
            var wfmLaborPerFlow = LaborCalculations.WFMLaborPerFlow(items, schedules, breaks, steps, processes, now, wfmLabor).ToList();
            var wfmLaborPerProcessType = LaborCalculations.WFMLaborPerProcessType(items, schedules, breaks, steps, processes, now, wfmLaborPerFlow).ToList();

            // EQUIPMENTS
            var wfmLaborEquipment = LaborCalculations.WFMLaborEquipment(items, shifts, steps, processes, now).ToList();
            var wfmLaborEquipmentPerFlow = LaborCalculations.WFMLaborEquipmentPerFlow(items, shifts, steps, processes, wfmLaborEquipment, now).ToList();
            var wfmLaborPerProcessEquipment = LaborCalculations.WFMLaborPerProcessEquipment(items, shifts, steps, processes, wfmLaborEquipmentPerFlow, now).ToList();

            // ITEM PLANNINGS
            var wfmLaborData = LaborCalculations.WFMLaborItemPlanning(items, schedules, wfmLaborPerProcessType, wfmLaborPerProcessEquipment).ToList();

            context.ChangeTracker.QueryTrackingBehavior = prevQueryTracking;

            var prevDetect = context.ChangeTracker.AutoDetectChangesEnabled;
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                context.WFMLaborWorker.AddRange(wfmLabor);
                context.WFMLaborWorkerPerFlow.AddRange(wfmLaborPerFlow);
                context.WFMLaborWorkerPerProcessType.AddRange(wfmLaborPerProcessType);
                context.WFMLaborEquipment.AddRange(wfmLaborEquipment);
                context.WFMLaborEquipmentPerFlow.AddRange(wfmLaborEquipmentPerFlow);
                context.WFMLaborEquipmentPerProcessType.AddRange(wfmLaborPerProcessEquipment);
                context.WFMLaborItemPlanning.AddRange(wfmLaborData);

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = prevDetect;
            }
        }

        /// <summary>
        /// Calculates the yard data from the simulation ItemPlannings and inserts them into the database
        /// </summary>
        /// <param name="itemPlannings">List of item plannings returned by the simulation</param>
        public void CalculateAndSaveYard(IEnumerable<ItemPlanning> itemPlannings)
        {
            if (!itemPlannings.Any())
            {
                Console.WriteLine("No item plannings for Yard calculations.");
                return;
            }

            var now = DateTime.UtcNow;

            var appointments = context.YardAppointmentsNotifications.Where(x => x.AppointmentDate.Date == now.Date).ToList();
            var outboundFlows = context.OutboundFlowGraphs.Where(x => x.WarehouseId == itemPlannings.FirstOrDefault().WorkOrderPlanning.Planning.WarehouseId).ToList();
            var inboundFlows = context.InboundFlowGraphs.Where(x => x.WarehouseId == itemPlannings.FirstOrDefault().WorkOrderPlanning.Planning.WarehouseId).ToList();
            var docks = context.Docks.Where(x => x.Zone.Area.Layout.WarehouseId == itemPlannings.FirstOrDefault().Process.Area.Layout.WarehouseId).Include(m => m.Zone).ToList();
            var stages = context.Stages.Where(x => x.Zone.Area.Layout.WarehouseId == itemPlannings.FirstOrDefault().Process.Area.Layout.WarehouseId).ToList();

            var yardPerDocks = YardDockCalculations.YardMetricsPerDocks(itemPlannings, appointments).ToList();
            context.YardMetricsPerDock.AddRange(yardPerDocks);

            var yardPerStages = YardStageCalculations.YardMetricsPerStages(itemPlannings, appointments).ToList();
            context.YardMetricsPerStage.AddRange(yardPerStages);

            var yardDockUsage = YardDockCalculations.YardDockUsagePerHour(itemPlannings, docks, now).ToList();
            context.YardDockUsagePerHour.AddRange(yardDockUsage);

            var yardStageUsage = YardStageCalculations.YardStageUsagePerHour(itemPlannings, stages, now).ToList();
            context.YardStageUsagePerHour.AddRange(yardStageUsage);

            var yardAppointmentsPerDock = YardDockCalculations.YardMetricsAppointmentsPerDock(appointments, itemPlannings, yardPerDocks, outboundFlows, inboundFlows).ToList();
            context.YardMetricsAppointmentsPerDock.AddRange(yardAppointmentsPerDock);

            var yardAppointmentsPerStage = YardStageCalculations.YardMetricsAppointmentsPerStage(appointments, itemPlannings, yardPerStages, outboundFlows, inboundFlows).ToList();
            context.YardMetricsAppointmentsPerStage.AddRange(yardAppointmentsPerStage);

            context.SaveChanges();
        }

        public DBModelSimulationResult SaveSimulationResult(DBModelSimulationResult responseSimulation)
        {
            try
            {

                if (responseSimulation.Planning != null)
                {
                    Planning Planning = new Planning
                    {
                        Id = responseSimulation.Planning.Id,
                        Date = responseSimulation.Planning.Date,
                        CreationDate = responseSimulation.Planning.CreationDate,
                        IsStored = responseSimulation.Planning.IsStored,
                        IsWorkforcePlanning = responseSimulation.Planning.IsWorkforcePlanning,
                        WarehouseId = responseSimulation.Planning.Warehouse.Id,
                        SLAWorkOrdersOnTimePercentage = responseSimulation.Planning.SLAWorkOrdersOnTimePercentage,
                        SLAShippedStock = responseSimulation.Planning.SLAShippedStock
                    };

                    context.Plannings.Add(Planning);

                    foreach (var workOrderPlanning in responseSimulation.WorkOrderPlanning)
                    {
                        WorkOrderPlanning workOrderPlanningReturn = new WorkOrderPlanning
                        {
                            Id = workOrderPlanning.Id,
                            IsOutbound = workOrderPlanning.IsOutbound,
                            AppointmentDate = workOrderPlanning.AppointmentDate,
                            InitDate = workOrderPlanning.InitDate,
                            EndDate = workOrderPlanning.EndDate,
                            WorkTime = workOrderPlanning.WorkTime,
                            IsEstimated = workOrderPlanning.IsEstimated,
                            IsStored = workOrderPlanning.IsStored,
                            IsBlocked = workOrderPlanning.IsBlocked,
                            IsStarted = workOrderPlanning.IsStarted,
                            Status = workOrderPlanning.Status,
                            Priority = workOrderPlanning.Priority,
                            Progress = workOrderPlanning.Progress,
                            PlanningId = Planning.Id,
                            //TODO: Change after IsEstimated
                            InputOrderId = workOrderPlanning.IsEstimated ? null : workOrderPlanning.InputOrderId,
                            AssignedDockId = workOrderPlanning.AssignedDockId,
                            IsOnTime = workOrderPlanning.IsOnTime,
                            IsInVehicleTime = workOrderPlanning.IsInVehicleTime,
                            SLATarget = workOrderPlanning.SLATarget,
                            OrderDelay = workOrderPlanning.OrderDelay,
                            SLAMet = workOrderPlanning.SLAMet,
                            AppointmentEndDate = workOrderPlanning.AppointmentEndDate
                        };
                        if (workOrderPlanning.IsBlocked)
                        {
                            context.InputOrders.FirstOrDefault(m => m.Id == workOrderPlanning.InputOrderId)!.EndBlockDate = workOrderPlanning.EndDate;
                        }

                        context.WorkOrdersPlanning.Add(workOrderPlanningReturn);
                    }

                    foreach (var warehouseProcessPlanning in responseSimulation.WarehouseProcessPlanning)
                    {
                        WarehouseProcessPlanning warehouseProcessPlanningReturn = new WarehouseProcessPlanning
                        {
                            Id = warehouseProcessPlanning.Id,
                            Code = warehouseProcessPlanning.Code,
                            ProcessId = warehouseProcessPlanning.ProcessId,
                            LimitDate = warehouseProcessPlanning.LimitDate,
                            InitDate = warehouseProcessPlanning.InitDate,
                            EndDate = warehouseProcessPlanning.EndDate,
                            WorkTime = warehouseProcessPlanning.WorkTime,
                            IsStored = warehouseProcessPlanning.IsStored,
                            IsBlocked = warehouseProcessPlanning.IsBlocked,
                            IsStarted = warehouseProcessPlanning.IsStarted,
                            PlanningId = Planning.Id,
                            WorkerId = warehouseProcessPlanning.WorkerId,
                            EquipmentGroupId = warehouseProcessPlanning.EquipmentGroupId
                        };

                        context.WarehouseProcessPlanning.Add(warehouseProcessPlanningReturn);
                    }

                    foreach (var ItemPlanning in responseSimulation.ItemPlanning)
                    {
                        ItemPlanning ItemPlanningReturn = new ItemPlanning
                        {
                            Id = ItemPlanning.Id,
                            ProcessId = ItemPlanning.ProcessId,
                            IsOutbound = ItemPlanning.IsOutbound,
                            LimitDate = ItemPlanning.LimitDate,
                            InitDate = ItemPlanning.InitDate,
                            EndDate = ItemPlanning.EndDate,
                            WorkTime = ItemPlanning.WorkTime,
                            IsStored = ItemPlanning.IsStored,
                            IsBlocked = ItemPlanning.IsBlocked,
                            IsStarted = ItemPlanning.IsStarted,
                            IsFaked = ItemPlanning.IsFaked,
                            WorkOrderPlanningId = ItemPlanning.WorkOrderPlanningId,
                            WorkerId = ItemPlanning.WorkerId,
                            EquipmentGroupId = ItemPlanning.EquipmentGroupId,
                            Progress = ItemPlanning.Progress,
                            ShiftId = ItemPlanning.ShiftId,
                            StageId = ItemPlanning.StageId
                        };

                        context.ItemsPlanning.Add(ItemPlanningReturn);
                    }
                    context.SaveChanges();
                }
                return responseSimulation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método para obtener la lista de ordenes teniendo en cuenta el calendario de ordenes y las ordenes notificadas.
        /// </summary>
        /// <param name="warehouseId">Guid del site (almacén) para el cual se obtienen las ordenes.</param>
        /// <returns>Enumeración de objetos InputOrder.</returns>
        public DataSimulatorTablaRequest GetCurrentInputOrders(DataSimulatorTablaRequest responseSimulation, Guid warehouseId)
        {
            try
            {
                var ordersScheduler = responseSimulation.OrderSchedule.Where(x => ((x.InitHour >= responseSimulation.Date.Value.TimeOfDay) || (x.InitHour <= responseSimulation.Date.Value.TimeOfDay && x.EndHour >= responseSimulation.Date.Value.TimeOfDay)))
                .ToList();
                var orderLoadRatio = responseSimulation.OrderLoadRatio.ToList();


                var parsedOrderData = Process(
                    responseSimulation.InputOrder.Where(x => x.AppointmentDate.Date == DateTime.UtcNow.Date).ToList(),
                    ordersScheduler, orderLoadRatio);

                var OrderLines = CreateLinesForOrders(parsedOrderData, context.OutboundFlowGraphs.Select(x => x.AverageLinesPerOrder).FirstOrDefault());

                responseSimulation.InputOrder = parsedOrderData.Concat(responseSimulation.InputOrder.Where(x => x.AppointmentDate.Date == DateTime.UtcNow.Date)).ToList();
                responseSimulation.InputOrderLine = OrderLines.Concat(responseSimulation.InputOrderLine.Where(x => x.InputOutboundOrder.AppointmentDate.Date == DateTime.UtcNow.Date)).ToList();

                return responseSimulation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<InputOrder> Process(List<InputOrder> orders, List<OrderSchedule> orderSchedulers, List<OrderLoadRatio> orderLoadRatio)
        {
            try
            {
                List<InputOrder> tmpOrders = new List<InputOrder>();
                tmpOrders.AddRange(GenerateOrders(orders, orderSchedulers, orderLoadRatio, true));
                tmpOrders.AddRange(GenerateOrders(orders, orderSchedulers, orderLoadRatio, false));

                return tmpOrders;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<InputOrder> GenerateOrders(List<InputOrder> orders, List<OrderSchedule> orderSchedulers, List<OrderLoadRatio> orderLoadRatio, bool isOut)
        {
            try
            {
                List<InputOrder> finalOrders = new List<InputOrder>();

                int iteration = 0;
                foreach (var orderScheduler in orderSchedulers.Where(x => x.IsOut == isOut))
                {
                    double numberVehicles = orderScheduler.NumberVehicles;
                    double loadPerVehicleRatio = orderLoadRatio.FirstOrDefault(x => x.VehicleId == orderScheduler.VehicleId && x.LoadId == orderScheduler.LoadId) == null ? 1 : orderLoadRatio.FirstOrDefault(x => x.VehicleId == orderScheduler.VehicleId && x.LoadId == orderScheduler.LoadId).LoadInVehicle;
                    double orderPerLoadRatio = orderLoadRatio.FirstOrDefault(x => x.VehicleId == orderScheduler.VehicleId && x.LoadId == orderScheduler.LoadId) == null ? 1 : orderLoadRatio.FirstOrDefault(x => x.VehicleId == orderScheduler.VehicleId && x.LoadId == orderScheduler.LoadId).OrderInLoad;

                    double estimatedOrders = loadPerVehicleRatio * orderPerLoadRatio;

                    for (int i = 0; i < numberVehicles; i++)
                    {
                        int orderCount = (int)Math.Round(estimatedOrders, MidpointRounding.AwayFromZero);
                        int inputOrders = orders.Count(x => x.AppointmentDate.TimeOfDay >= orderScheduler.InitHour && x.AppointmentDate.TimeOfDay <= orderScheduler.EndHour && x.IsOutbound == isOut);
                        int restOfOrders = orderCount - inputOrders;

                        string flow = isOut ? "Outbound" : "Inbound";

                        string vehicleCode = $"{flow}_" + "Vehicle" + $"_{iteration}_{i}";
                        DateTime appointmentDate = RandomDateTime(orderScheduler.InitHour, orderScheduler.EndHour);

                        for (int j = 0; j < restOfOrders; j++)
                        {
                            finalOrders.Add(new InputOrder()
                            {
                                Id = Guid.NewGuid(),
                                OrderCode = string.Empty,
                                Warehouse = orderSchedulers.FirstOrDefault().Warehouse,
                                WarehouseId = orderSchedulers.FirstOrDefault().WarehouseId,
                                IsOutbound = isOut,
                                IsEstimated = true,
                                Status = InputOrderStatus.Waiting,
                                RealArrivalTime = null,
                                AppointmentDate = appointmentDate,
                                VehicleCode = vehicleCode,
                                Progress = 0
                            });
                        }
                    }

                    iteration++;
                }

                return finalOrders;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<InputOrderLine> CreateLinesForOrders(List<InputOrder> orders, int avgLinesPerOrder)
        {
            try
            {
                avgLinesPerOrder = avgLinesPerOrder > 0 ? avgLinesPerOrder : 1;
                var lines = new List<InputOrderLine>();
                foreach (var order in orders)
                {
                    for (var i = 0; i < avgLinesPerOrder; i++)
                        lines.Add(new InputOrderLine()
                        {
                            InputOutboundOrder = order,
                            Id = new Guid(),
                            InputOutboundOrderId = order.Id,
                            IsClosed = false,
                            Product = null,
                            Quantity = 10,
                            UnitOfMeasure = null,
                        });
                }

                return lines;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static DateTime RandomDateTime(TimeSpan initHour, TimeSpan endHour)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = today.Add(initHour);
                var end = today.Add(endHour);

                var rangeTicks = end.Ticks - start.Ticks;
                var randomTicks = (long)(Random.Shared.NextDouble() * rangeTicks);

                return new DateTime(start.Ticks + randomTicks, DateTimeKind.Utc);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveAlertResponses(List<AlertResponse>? alertResponses)
        {
            if (alertResponses != null && alertResponses.Count != 0)
            {
                foreach (var response in alertResponses)
                {
                    try
                    {
                        var alertResponseReturn = new AlertResponse
                        {
                            Id = response.Id,
                            AlertId = response.AlertId,
                            PlanningId = response.PlanningId,
                            EntityId = response.EntityId,
                            TriggerDate = response.TriggerDate,
                            Severity = response.Severity,
                            Type = response.Type
                        };

                        context.AlertResponses.Add(alertResponseReturn);
                    }

                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                context.SaveChanges();
            }
        }

            

        public void RemoveDeprecatedAlertResponses(Guid warehouseId)
        {
            try
            {
                if (warehouseId != Guid.Empty)
                {
                    var deprecatedMessages = context.AlertResponses.Where(x => x.Planning.WarehouseId == warehouseId);

                    foreach (var m in deprecatedMessages)
                    {
                        try
                        {
                            context.AlertResponses.Remove(m);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Warn] Could not remove AlertResponse {m.Id}");
                            continue;
                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[Error] Exception founded while trying to remove AlertResponses for Warehouse {warehouseId} => {ex.ToString()}");
            }
        }

        private static bool ShiftTouchesInterval(
            Shift shift,
            DateTime intervalStart,
            DateTime intervalEnd)
        {
            for (var day = intervalStart.Date; day <= intervalEnd.Date; day = day.AddDays(1))
            {
                var shiftStart = day.AddHours(shift.InitHour);

                var shiftEnd = shift.EndHour > shift.InitHour
                    ? day.AddHours(shift.EndHour)
                    : day.AddDays(1).AddHours(shift.EndHour);

                if (!((intervalEnd < shiftStart) || (intervalStart > shiftEnd)))
                    return true;
            }

            return false;
        }


        public List<WorkerWhatIf> BuildWorkersDistribution(
                 IEnumerable<WorkOrderPlanningReturn> workOrderPlanning,
                 IEnumerable<Rol> roles,
                 IEnumerable<Shift> shifts,
                 IEnumerable<Worker> workersBase,
                 Guid warehouseId)
        {
            var workers = new Dictionary<Guid, WorkerWhatIf>();

            foreach (var order in workOrderPlanning)
            {
                foreach (var ip in order.ItemPlanning)
                {
                    if (!workers.TryGetValue(ip.WorkerId ?? Guid.Empty, out var w))
                    {
                        var rol = roles.First(r =>
                            r.Name == ip.RolName &&
                            r.WarehouseId == warehouseId);

                        var workerBase = workersBase.FirstOrDefault(x => x.Id == (ip.WorkerId ?? Guid.Empty));

                        w = new WorkerWhatIf
                        {
                            Name = ip.Shift?.Name ?? "NotFound",
                            Rol = ip.RolName ?? "NotFound",
                            RolId = rol.Id,
                            Init = ip.InitDate,
                            End = ip.EndDate,
                            WorkTime = ip.WorkTime,
                            LogMessages = new Dictionary<string, List<string>>()
                        };

                        workers.Add(workerBase.Id, w);
                    }
                    else
                    {
                        if (ip.WorkerId != null)
                        {
                            w.Init = w.Init < ip.InitDate ? w.Init : ip.InitDate;
                            w.End = w.End > ip.EndDate ? w.End : ip.EndDate;
                            w.WorkTime += ip.WorkTime;
                        }
                    }
                }
            }

            foreach (var w in workers.Values)
            {
                var totalSeconds = (w.End - w.Init)
                    .GetValueOrDefault(TimeSpan.Zero)
                    .TotalSeconds;

                w.Percentage = totalSeconds > 0
                    ? Math.Round(w.WorkTime / totalSeconds, 2)
                    : 0;
            }

            var result = new List<WorkerWhatIf>();

            foreach (var worker in workers.Values)
            {
                if (worker.Init == null || worker.End == null)
                    continue;

                var start = worker.Init.Value;
                var end = worker.End.Value;

                var touchedShifts = shifts
                    .Where(s => ShiftTouchesInterval(s, start, end))
                    .ToList();

                foreach (var shift in touchedShifts)
                {
                    result.Add(new WorkerWhatIf
                    {
                        Name = worker.Name,
                        Rol = worker.Rol,
                        RolId = worker.RolId,
                        Shift = shift.Name,
                        ShiftId = shift.Id,
                        Init = worker.Init,
                        End = worker.End,
                        WorkTime = worker.WorkTime,
                        Percentage = worker.Percentage,
                        LogMessages = worker.LogMessages != null
                            ? new Dictionary<string, List<string>>(worker.LogMessages)
                            : new Dictionary<string, List<string>>()
                    });
                }
            }

            return result;
        }


        public List<WorkerWhatIf> BuildWorkersWhatIf(
                 IEnumerable<WorkOrderPlanningReturn> workOrderPlanning,
                 IEnumerable<Rol> roles,
                 IEnumerable<Shift> shifts,
                 Guid warehouseId)
        {
            var workers = new Dictionary<string, WorkerWhatIf>();

            foreach (var order in workOrderPlanning)
            {
                foreach (var ip in order.ItemPlanning)
                {
                    var workerName = ip.WorkerName ?? "NotFound";

                    if (!workers.TryGetValue(workerName, out var w))
                    {
                        var rol = roles.First(r =>
                            r.Name == ip.RolName &&
                            r.WarehouseId == warehouseId);

                        w = new WorkerWhatIf
                        {
                            Name = workerName,
                            Rol = ip.RolName ?? "NotFound",
                            RolId = rol.Id,
                            Init = ip.InitDate,
                            End = ip.EndDate,
                            WorkTime = ip.WorkTime,
                            LogMessages = new Dictionary<string, List<string>>()
                        };

                        workers.Add(workerName, w);
                    }
                    else
                    {
                        w.Init = w.Init < ip.InitDate ? w.Init : ip.InitDate;
                        w.End = w.End > ip.EndDate ? w.End : ip.EndDate;
                        w.WorkTime += ip.WorkTime;
                    }
                }
            }

            foreach (var w in workers.Values)
            {
                var totalSeconds = (w.End - w.Init)
                    .GetValueOrDefault(TimeSpan.Zero)
                    .TotalSeconds;

                w.Percentage = totalSeconds > 0
                    ? Math.Round(w.WorkTime / totalSeconds, 2)
                    : 0;
            }

            var result = new List<WorkerWhatIf>();

            foreach (var worker in workers.Values)
            {
                if (worker.Init == null || worker.End == null)
                    continue;

                var start = worker.Init.Value;
                var end = worker.End.Value;

                var touchedShifts = shifts
                    .Where(s => ShiftTouchesInterval(s, start, end))
                    .ToList();

                foreach (var shift in touchedShifts)
                {
                    result.Add(new WorkerWhatIf
                    {
                        Name = worker.Name,
                        Rol = worker.Rol,
                        RolId = worker.RolId,
                        Shift = shift.Name,
                        ShiftId = shift.Id,
                        Init = worker.Init,
                        End = worker.End,
                        WorkTime = worker.WorkTime,
                        Percentage = worker.Percentage,
                        LogMessages = worker.LogMessages != null
                            ? new Dictionary<string, List<string>>(worker.LogMessages)
                            : new Dictionary<string, List<string>>()
                    });
                }
            }

            return result;
        }


    }
}
