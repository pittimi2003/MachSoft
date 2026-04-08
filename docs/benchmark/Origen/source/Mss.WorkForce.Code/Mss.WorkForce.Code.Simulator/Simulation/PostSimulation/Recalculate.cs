using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;

namespace Mss.WorkForce.Code.Simulator.Simulation.PostSimulation
{
    public class Recalculate
    {
        #region Variables
        private Simulation simulation;
        #endregion

        #region Constructor
        public Recalculate(Simulation simulation)
        {
            this.simulation = simulation;
        }
        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Adds the closed processes to the planning
        /// </summary>
        public void AddClosedProcessesToPlanning()
        {
            AddOpenAndResumedOrders();
            AddClosedAndCancelledOrders();
        }

        public void FakeWorkOrders()
        {
            var blockedAfterHours = this.simulation.Data.InputOrder.Where(x => x.Status == OrderStatus.Waiting && x.IsBlocked.GetValueOrDefault() && x.BlockDate < this.simulation.StartSimulationDate);

            foreach (InputOrder inputOrder in blockedAfterHours)
            {
                WorkOrderPlanningReturn fakeWorkOrderPlanning = new WorkOrderPlanningReturn()
                {
                    Id = Guid.NewGuid(),
                    InputOrder = inputOrder,
                    InputOrderId = inputOrder.Id,
                    IsBlocked = inputOrder.IsBlocked.Value,
                    AssignedDock = inputOrder.AssignedDock,
                    AssignedDockId = inputOrder.AssignedDockId,
                    Status = inputOrder.Status,
                    Priority = inputOrder.Priority,
                    IsOutbound = inputOrder.IsOutbound,
                    IsEstimated = inputOrder.IsEstimated,
                    IsStarted = inputOrder.IsStarted,
                    IsStored = false,
                    EndDate = inputOrder.EndBlockDate.Value,
                    InitDate = inputOrder.BlockDate.Value,
                    AppointmentDate = inputOrder.BlockDate.Value,
                    Planning = this.simulation.PlanningReturn,
                    WorkTime = (inputOrder.EndBlockDate.Value - inputOrder.BlockDate.Value).TotalSeconds,
                    PlanningId = this.simulation.PlanningReturn.Id,
                    Progress = inputOrder.Progress.Value,
                    ItemPlanning = new List<ItemPlanningReturn>() { }
                };
                this.simulation.PlanningReturn.WorkOrderPlanning.Add(fakeWorkOrderPlanning);
            }
        }

        #endregion

        #region Private
        /// <summary>
        /// Adds the closed processes of the closed and cancelled orders to the planning
        /// </summary>
        private void AddClosedAndCancelledOrders()
        {
            var closedAndCancelledOrders = this.simulation.Data.InputOrder
                .Where(x => x.WarehouseId == this.simulation.Warehouse.Id)
                .Where(x => x.Status == OrderStatus.Closed || x.Status == OrderStatus.Cancelled || x.Status == OrderStatus.Paused)
                .Where(x => !string.IsNullOrEmpty(x.OrderCode))
                .Where(x => this.simulation.Data.InputOrderProcessClosing.Any(i => i.InputOrder == x.OrderCode));

            if (closedAndCancelledOrders.Any())
                CreateWorkOrderAndItemPlanning(closedAndCancelledOrders); // Creamos el workOrderPlanning correspondiente y a partir de ahi sus ItemPlanning
        }

        /// <summary>
        /// Adds the closed processes of the open and resumed orders to the planning
        /// </summary>
        private void AddOpenAndResumedOrders()
        {
            var openAndResumedOrders = this.simulation.Warehouse.Orders.Where(x => x.Status == OrderStatus.Released);

            if (openAndResumedOrders.Any())
            {
                foreach (var process in this.simulation.Data.InputOrderProcessClosing)
                {
                    // TODO: Aquí deberíamos de tener en cuenta si es proceso de almacén ya cerrado o no
                    var workOrder = this.simulation.PlanningReturn.WorkOrderPlanning.FirstOrDefault(x => x.InputOrder.OrderCode == process.InputOrder);

                    if (workOrder != null)
                    {
                        try
                        {
                            var areaId = this.simulation.Data.Zone.FirstOrDefault(x => x.Name == process.ZoneCode)?.AreaId;
                            var dataProcess = this.simulation.Data.Process.FirstOrDefault(x => x.Type == process.ProcessType && x.AreaId == areaId)
                                ?? this.simulation.Data.Process.FirstOrDefault(x => x.Type == process.ProcessType);

                            workOrder.ItemPlanning.Add(new ItemPlanningReturn()
                            {
                                Id = Guid.NewGuid(),
                                ProcessId = dataProcess!.Id,
                                Process = dataProcess,
                                IsOutbound = workOrder.IsOutbound,
                                LimitDate = workOrder.AppointmentDate,
                                InitDate = process.InitDate,
                                EndDate = process.EndDate,
                                WorkTime = (process.EndDate - process.InitDate).TotalSeconds, // Faltaría Duration o lo sacamos asi
                                IsStored = workOrder.IsStored,
                                IsBlocked = workOrder.IsBlocked,
                                IsStarted = workOrder.IsStarted,
                                WorkOrderPlanningId = workOrder.Id,
                                WorkOrderPlanning = workOrder,
                                WorkerId = this.simulation.Data.AvailableWorker.FirstOrDefault(x => x.Name == process.Worker)?.Worker.Id, // No veo claro que tenga sentido buscar por Code aqui
                                EquipmentGroupId = this.simulation.Data.EquipmentGroup.FirstOrDefault(x => x.Name == process.EquipmentGroup && x.AreaId == areaId)?.Id,
                                Progress = 100,
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to add closed processes to open and resumed work orders. {ex.Message}");
                            Console.WriteLine(ex.StackTrace);
                        }

                        workOrder.InitDate = workOrder.ItemPlanning.Min(x => x.InitDate);
                        workOrder.EndDate = workOrder.ItemPlanning.Max(x => x.EndDate);
                        workOrder.WorkTime = workOrder.ItemPlanning.Sum(x => x.WorkTime);

                        if (workOrder.AssignedDockId == null)
                        {
                            workOrder.AssignedDockId = this.simulation.Data.InputOrder.FirstOrDefault(x => x.Id == workOrder.InputOrderId).AssignedDockId;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates work order plannings and their item plannings if necessary
        /// </summary>
        /// <param name="orders">Orders to create the work order plannings</param>
        private void CreateWorkOrderAndItemPlanning(IEnumerable<InputOrder> orders)
        {
            foreach (var order in orders)
            {
                try
                {
                    var assignedDockId = this.simulation.Data.InputOrder.FirstOrDefault(x => x.OrderCode == order.OrderCode).AssignedDockId;

                    var workOrder = new WorkOrderPlanningReturn()
                    {
                        Id = Guid.NewGuid(),
                        IsOutbound = this.simulation.Data.InputOrder.FirstOrDefault(x => x.OrderCode == order.OrderCode).IsOutbound,
                        AppointmentDate = this.simulation.Data.InputOrder.FirstOrDefault(x => x.OrderCode == order.OrderCode).AppointmentDate,
                        InitDate = this.simulation.StartSimulationDate,
                        EndDate = this.simulation.StartSimulationDate.AddHours(1),
                        WorkTime = 60,
                        Status = order.Status,
                        Priority = this.simulation.Data.InputOrder.FirstOrDefault(x => x.OrderCode == order.OrderCode).Priority,
                        IsEstimated = this.simulation.Data.InputOrder.FirstOrDefault(x => x.OrderCode == order.OrderCode).IsEstimated,
                        IsStored = false,
                        IsBlocked = this.simulation.Data.InputOrder.FirstOrDefault(x => x.OrderCode == order.OrderCode).IsBlocked.GetValueOrDefault(false),
                        IsStarted = false,
                        Progress = order.Progress == null ? 0 : order.Progress.Value,
                        PlanningId = this.simulation.PlanningReturn.Id,
                        InputOrderId = order.Id,
                        InputOrder = order,
                        ItemPlanning = new List<ItemPlanningReturn>(),
                        AssignedDockId = assignedDockId
                    };

                    var processes = this.simulation.Data.InputOrderProcessClosing.Where(x => x.InputOrder == order.OrderCode);

                    foreach (var process in processes)
                    {
                        // Tenemos que sacar si hay algún break para ese worker entre las fechas de inicio y fin, 
                        // para restarle el tiempo de break al end-init para sacar worktime realista

                        var worker = this.simulation.Data.AvailableWorker.FirstOrDefault(x => x.Name == process.Worker);

                        List<CustomBreak> breaks = new();
                        if (worker != null) breaks = this.simulation.Resources[worker.WorkerId].Breaks;

                        var workTime = (process.EndDate - process.InitDate).TotalSeconds - GetBreakSeconds(process.InitDate, process.EndDate, breaks);

                        var areaId = this.simulation.Data.Zone.FirstOrDefault(x => x.Name == process.ZoneCode)?.AreaId;
                        var dataProcess = this.simulation.Data.Process.FirstOrDefault(x => x.Type == process.ProcessType && x.AreaId == areaId)
                            ?? this.simulation.Data.Process.FirstOrDefault(x => x.Type == process.ProcessType);

                        workOrder.ItemPlanning.Add(new ItemPlanningReturn()
                        {
                            Id = Guid.NewGuid(),
                            ProcessId = dataProcess!.Id,
                            Process = dataProcess,
                            IsOutbound = workOrder.IsOutbound,
                            LimitDate = workOrder.AppointmentDate,
                            InitDate = process.InitDate,
                            EndDate = process.EndDate,
                            WorkTime = workTime,
                            IsStored = workOrder.IsStored,
                            IsBlocked = workOrder.IsBlocked,
                            IsStarted = workOrder.IsStarted,
                            WorkOrderPlanningId = workOrder.Id,
                            WorkerId = worker?.WorkerId, // No veo claro que tenga sentido buscar por Code aqui
                            EquipmentGroupId = this.simulation.Data.EquipmentGroup.FirstOrDefault(x => x.Name == process.EquipmentGroup && x.AreaId == areaId)?.Id,
                            Progress = 100
                        });
                    }

                    if (workOrder.ItemPlanning.Any())
                    {
                        workOrder.InitDate = this.simulation.Data.InputOrderProcessClosing.Where(x => x.InputOrder == order.OrderCode).Min(x => x.InitDate);
                        workOrder.EndDate = this.simulation.Data.InputOrderProcessClosing.Where(x => x.InputOrder == order.OrderCode).Max(x => x.EndDate);
                        workOrder.WorkTime = workOrder.ItemPlanning.Sum(x => x.WorkTime);
                    }

                    this.simulation.PlanningReturn.WorkOrderPlanning.Add(workOrder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add closed processes to closed and cancelled work orders. {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        private double GetBreakSeconds(DateTime initDate, DateTime endDate, List<CustomBreak> breaks)
        {
            double totalOverlapSeconds = 0;

            foreach (var b in breaks)
            {
                DateTime breakStart = this.simulation.Environment.Now.Date.AddHours(b.InitBreak);
                DateTime breakEnd = this.simulation.Environment.Now.Date.AddHours(b.EndBreak);

                // El break cruza medianoche (inicio > fin), Lo partimos en dos rangos
                if (breakEnd < breakStart)
                {
                    DateTime endOfDay = breakStart.Date.AddDays(1);
                    totalOverlapSeconds += GetRangeOverlapSeconds(initDate, endDate, breakStart, endOfDay);
                    totalOverlapSeconds += GetRangeOverlapSeconds(initDate, endDate, breakEnd.Date, breakEnd);
                }
                else
                {
                    totalOverlapSeconds += GetRangeOverlapSeconds(initDate, endDate, breakStart, breakEnd);
                }
            }

            return totalOverlapSeconds;
        }

        private static double GetRangeOverlapSeconds(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            // Calcula el inicio y fin del solapamiento
            DateTime overlapStart = start1 > start2 ? start1 : start2;
            DateTime overlapEnd = end1 < end2 ? end1 : end2;

            // Si hay solapamiento, devuelve los segundos
            if (overlapEnd > overlapStart)
                return (overlapEnd - overlapStart).TotalSeconds;

            // Si no hay solapamiento
            return 0;
        }

        #endregion

        #endregion
    }
}
