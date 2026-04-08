using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator
{
    public class PlanningData
    {
        #region Methods

        /// <summary>
        /// Creates the general planning instance for a warehouse
        /// </summary>
        /// <param name="simulation">Specific simulation data</param>
        /// <param name="warehouse">Warehouse for which we are going to save the simulation planning</param>
        /// <returns>Initial planning object to be completed within the simulation</returns>
        public static PlanningReturn InitialCreate(Simulation simulation, Core.Layout.Warehouse warehouse)
        {
            PlanningReturn planningReturn = new PlanningReturn()
            {
                Id = Guid.NewGuid(),
                Date = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc).Date,
                CreationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                IsStored = false,
                IsWorkforcePlanning = true,
                WarehouseId = warehouse.Id,
                Warehouse = new Models.Models.Warehouse()
                {
                    Id = warehouse.Id,
                    Code = simulation.Data.Warehouse.Code,
                    Name = warehouse.Name,
                    TimeZone_ = simulation.Data.Warehouse.TimeZone_,
                    Description = simulation.Data.Warehouse.Description,
                    Address = simulation.Data.Warehouse.Address,
                    AddressLine = simulation.Data.Warehouse.AddressLine,
                    ZIPCode = simulation.Data.Warehouse.ZIPCode,
                    City = simulation.Data.Warehouse.City,
                    State = simulation.Data.Warehouse.State,
                    Country = simulation.Data.Warehouse.Country,
                    AddressComment = simulation.Data.Warehouse.AddressComment,
                    ContactName = simulation.Data.Warehouse.ContactName,
                    Telephone = simulation.Data.Warehouse.Telephone,
                    Extension = simulation.Data.Warehouse.Extension,
                    Telephone2 = simulation.Data.Warehouse.Telephone2,
                    Fax = simulation.Data.Warehouse.Fax,
                    Email = simulation.Data.Warehouse.Email,
                    ContactComment = simulation.Data.Warehouse.ContactComment,
                    MeasureSystem = simulation.Data.Warehouse.MeasureSystem,
                    OrganizationId = simulation.Data.Warehouse.OrganizationId,
                    Organization = simulation.Data.Warehouse.Organization,
                },
                WorkOrderPlanning = new List<WorkOrderPlanningReturn>(),
                WarehouseProcessPlanning = new List<WarehouseProcessPlanningReturn>()
            };

            return planningReturn;
        }

        /// <summary>
        /// Assigns the dates and times to a work order from its specific processes.
        /// </summary>
        /// <param name="planningReturn">Planning filled in the simulation</param>
        /// <returns>Planning with the complete information of the work orders</returns>
        public static PlanningReturn CompleteDatesAndWorkTimeForWorkOrders(PlanningReturn planningReturn)
        {
            var workOrders = planningReturn.WorkOrderPlanning;

            foreach (var workOrder in workOrders)
            {
                if (workOrder.ItemPlanning.Any())
                {
                    workOrder.InitDate = workOrder.ItemPlanning.Min(x => x.InitDate);
                    workOrder.EndDate = workOrder.ItemPlanning.Max(x => x.EndDate);
                    workOrder.WorkTime = workOrder.ItemPlanning.Sum(x => x.WorkTime);
                }
            }

            return planningReturn;
        }

        /// <summary>
        /// Creates a WorkOrderPlanning for the simulation Planning
        /// </summary>
        /// <param name="simulator">Specific simulator data</param>
        /// <param name="process">Process that needs that WorkOrderPlanning to be saved at</param>
        /// <returns>Planning with the new work order</returns>
        public static WorkOrderPlanningReturn CreateWorkOrderPlanning(Simulator simulator, Grouping process)
        {
            var inputOrder = simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId);

            return new WorkOrderPlanningReturn
            {
                Id = Guid.NewGuid(),
                IsOutbound = inputOrder.IsOutbound,
                AppointmentDate = inputOrder.AppointmentDate,
                InitDate = new DateTime(),
                EndDate = new DateTime(),
                WorkTime = 0,
                IsEstimated = inputOrder.IsEstimated,
                IsStored = false,
                IsBlocked = inputOrder.IsBlocked.GetValueOrDefault(false),
                IsStarted = inputOrder.IsStarted,
                Status = inputOrder.Status,
                Priority = inputOrder.Priority,
                Progress = inputOrder.Progress == null ? 0 : inputOrder.Progress.Value,
                Planning = simulator.Simulation.PlanningReturn,
                PlanningId = simulator.Simulation.PlanningReturn.Id,
                InputOrderId = process.OrderId,
                InputOrder = inputOrder,
                ItemPlanning = new List<ItemPlanningReturn>(),
                IsOnTime = false,
                IsInVehicleTime = false,
            };
        }

        /// <summary>
        /// Creates an ItemPlanning of a WorkOrderPlanning for the simulation Planning
        /// </summary>
        /// <param name="simulator">Specific simulator data</param>
        /// <param name="process">Process that correspondes to the ItemPlanning</param>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        /// <returns>Planning with the new item planning</returns>
        public static ItemPlanningReturn CreateItemPlanning(Simulator simulator, Grouping process, bool whatIf)
        {
            var workOrderPlanning = simulator.Simulation.PlanningReturn.WorkOrderPlanning.FirstOrDefault(x => x.InputOrderId == process.OrderId);

            var itemPlanning = new ItemPlanningReturn()
            {
                Id = process.Id,
                ProcessId = process.Process.Id,
                Process = simulator.Simulation.Data.Process.FirstOrDefault(x => x.Id == process.Process.Id),
                IsOutbound = workOrderPlanning.IsOutbound,
                LimitDate = workOrderPlanning.AppointmentDate,
                InitDate = simulator.Simulation.Environment.Now,
                EndDate = new DateTime(),
                WorkTime = process.Duration,
                IsStored = workOrderPlanning.IsStored,
                IsBlocked = workOrderPlanning.IsBlocked,
                IsStarted = workOrderPlanning.IsStarted,
                WorkOrderPlanningId = workOrderPlanning.Id,
                WorkOrderPlanning = workOrderPlanning,
                WorkerId = process.AssignedWorker.Id,
                EquipmentGroupId = process.AssignedEquipment.Id,
                Progress = 0.0,
                WorkerName =  process.AssignedWorker.Name,
                RolName = simulator.Simulation.Data.Rol.FirstOrDefault(x => x.Id == process.AssignedWorker.RolId)?.Name,
            };

            if (simulator.Simulation.Data.Zone.FirstOrDefault(m => m.Id == process.SelectedStation.Id).Type == ZoneType.Dock)
            {
                var workOrder = simulator.Simulation.PlanningReturn.WorkOrderPlanning.FirstOrDefault(x => x.InputOrderId == process.OrderId);
                var dock = simulator.Simulation.Data.Dock.FirstOrDefault(m => m.ZoneId == process.SelectedStation.Id);

                workOrder.AssignedDockId = dock.Id;
                workOrder.AssignedDock = dock;
            }

            return itemPlanning;
        }

        /// <summary>
        /// Creates an WarehouseProcessPlanning for the simulation Planning
        /// </summary>
        /// <param name="simulator">Specific simulator data</param>
        /// <param name="process">Process that correspondes to the ItemPlanning</param>
        /// <returns>Warehouse process planning with the new warehouse item planning</returns>
        public static WarehouseProcessPlanningReturn CreateWarehouseProcessPlanning(Simulator simulator, Grouping process)
        {
            return new WarehouseProcessPlanningReturn()
            {
                Id = process.Id,
                Code = process.Process.Code,
                ProcessId = process.Process.Id,
                Process = simulator.Simulation.Data.Process.FirstOrDefault(x => x.Id == process.Process.Id),
                LimitDate = null,
                InitDate = simulator.Simulation.Environment.Now,
                EndDate = new DateTime(),
                WorkTime = process.Duration,
                IsStored = false,
                IsBlocked = false,
                IsStarted = false,
                PlanningId = simulator.Simulation.PlanningReturn.Id,
                Planning = simulator.Simulation.PlanningReturn,
                WorkerId = process.AssignedWorker.Id,
                EquipmentGroupId = process.AssignedEquipment.Id,
            };
        }

        #endregion
    }
}
