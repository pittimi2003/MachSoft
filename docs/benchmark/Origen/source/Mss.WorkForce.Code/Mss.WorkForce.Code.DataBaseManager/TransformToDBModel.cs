using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.DataBaseManager
{
    public class TransformToDBModel
    {
        public static DBModelSimulationResult GetTransformation(DataResponseSimulation Response)
        {
            DBModelSimulationResult responseSimulate = new DBModelSimulationResult();

            if(Response.Planning != null)
            {
                responseSimulate.Planning = Response.Planning;

                List<WorkOrderPlanning> workOrderList = new List<WorkOrderPlanning>();
                List<ItemPlanning> itemPlanningList = new List<ItemPlanning>();

                foreach (var workOrder in Response.Planning.WorkOrderPlanning)
                {
                    WorkOrderPlanning workOrderToInsert = new WorkOrderPlanning
                    {
                        Id = workOrder.Id,
                        IsOutbound = workOrder.IsOutbound,
                        AppointmentDate = workOrder.AppointmentDate,
                        InitDate = workOrder.InitDate,
                        EndDate = workOrder.EndDate,
                        WorkTime = workOrder.WorkTime,
                        IsEstimated = workOrder.IsEstimated,
                        IsStored = workOrder.IsStored,
                        IsBlocked = workOrder.IsBlocked,
                        IsStarted = workOrder.IsStarted,
                        Status = workOrder.Status,
                        Priority = workOrder.Priority,
                        Progress = workOrder.Progress,
                        PlanningId = workOrder.PlanningId,
                        Planning = Response.Planning,
                        InputOrderId = workOrder.InputOrderId,
                        InputOrder = workOrder.InputOrder,
                        AssignedDock = workOrder.AssignedDock,
                        AssignedDockId = workOrder.AssignedDockId,
                        IsOnTime = workOrder.IsOnTime,
                        IsInVehicleTime = workOrder.IsInVehicleTime,
                        SLATarget = workOrder.SLATarget,
                        OrderDelay = workOrder.OrderDelay,
                        SLAMet = workOrder.SLAMet,
                        AppointmentEndDate = workOrder.AppointmentEndDate
                    };

                    workOrderList.Add(workOrderToInsert);

                    if (workOrder.ItemPlanning.Count == 0 && workOrder.Status == InputOrderStatus.Waiting && workOrder.IsBlocked)
                    {
                        var processFake = Response.Planning.WorkOrderPlanning.First(x => x.ItemPlanning.Count > 0).ItemPlanning.First().Process;
;
                        ItemPlanning itemPlanningToInsert = new ItemPlanning
                        {
                            Id = Guid.NewGuid(),
                            Process = processFake,
                            ProcessId = processFake.Id,
                            IsOutbound = workOrder.IsOutbound,
                            LimitDate = new DateTime(),
                            InitDate = workOrder.InitDate,
                            EndDate = workOrder.EndDate,
                            WorkTime = workOrder.WorkTime,
                            IsStored = workOrder.IsStored,
                            IsBlocked = workOrder.IsBlocked,
                            IsStarted = workOrder.IsStarted,
                            WorkOrderPlanningId = workOrder.Id,
                            WorkOrderPlanning = workOrderToInsert,
                            WorkerId = null,
                            Worker = null,
                            EquipmentGroupId = null,
                            EquipmentGroup = null,
                            IsFaked = true,
                            Progress = 0,
                            ShiftId = null,
                            Shift = null,
                            StageId = null,
                            Stage = null
                        };
                        itemPlanningList.Add(itemPlanningToInsert);
                    };                

                    foreach (var itemPlanning in workOrder.ItemPlanning)
                    {
                        ItemPlanning itemPlanningToInsert = new ItemPlanning
                        {
                            Id = itemPlanning.Id,
                            Process = itemPlanning.Process,
                            ProcessId = itemPlanning.ProcessId,
                            IsOutbound = itemPlanning.IsOutbound,
                            LimitDate = itemPlanning.LimitDate,
                            InitDate = itemPlanning.InitDate,
                            EndDate = itemPlanning.EndDate,
                            WorkTime = itemPlanning.WorkTime,
                            IsStored = itemPlanning.IsStored,
                            IsBlocked = itemPlanning.IsBlocked,
                            IsStarted = itemPlanning.IsStarted,
                            WorkOrderPlanningId = itemPlanning.WorkOrderPlanningId,
                            WorkOrderPlanning = workOrderToInsert,
                            WorkerId = itemPlanning.WorkerId,
                            EquipmentGroupId = itemPlanning.EquipmentGroupId,
                            Progress = itemPlanning.Progress,
                            ShiftId = itemPlanning.ShiftId,
                            Shift = itemPlanning.Shift,
                            StageId = itemPlanning.StageId
                        };
                        itemPlanningList.Add(itemPlanningToInsert);
                    }
                }
                responseSimulate.WorkOrderPlanning = workOrderList;
                responseSimulate.ItemPlanning = itemPlanningList;

                List<WarehouseProcessPlanning> warehouseProcessPlanning = new List<WarehouseProcessPlanning>();

                foreach (var warehouseProcess in Response.Planning.WarehouseProcessPlanning)
                {
                    WarehouseProcessPlanning warehouseProcessToInsert = new WarehouseProcessPlanning
                    {
                        Id = warehouseProcess.Id,
                        Code = warehouseProcess.Code,
                        ProcessId = warehouseProcess.ProcessId,
                        Process = warehouseProcess.Process,
                        LimitDate = warehouseProcess.LimitDate,
                        InitDate = warehouseProcess.InitDate,
                        EndDate = warehouseProcess.EndDate,
                        WorkTime = warehouseProcess.WorkTime,
                        IsStored = warehouseProcess.IsStored,
                        IsBlocked = warehouseProcess.IsBlocked,
                        IsStarted = warehouseProcess.IsStarted,
                        PlanningId = warehouseProcess.PlanningId,
                        Planning = Response.Planning,
                        WorkerId = warehouseProcess.WorkerId,
                        EquipmentGroupId = warehouseProcess.EquipmentGroupId,
                    };
                    warehouseProcessPlanning.Add(warehouseProcessToInsert);
                }
                responseSimulate.WarehouseProcessPlanning = warehouseProcessPlanning;
                
                return responseSimulate;
            }
            else
            {
                return responseSimulate;
            }
        }
    }
}
