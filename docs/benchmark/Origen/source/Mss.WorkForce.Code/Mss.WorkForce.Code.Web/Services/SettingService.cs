using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public class SettingService : ISettingService
    {

        #region Fields

        private readonly DataAccess _dataAccess;

        private readonly ISimulateService _simulateService;

        #endregion

        #region Constructors

        public SettingService(DataAccess dataAccess, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _simulateService = simulateService;
        }

        #endregion

        #region Properties

        [Inject]
        private ILogger<SettingService> _logger { get; set; }

        #endregion

        #region Methods

        public IEnumerable<SelectItemComboBox> GetAvailableAreaList(Guid warehouseId)
        {
            return _dataAccess.GetAreas().Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<SelectItemComboBox> GetBreakProfileList(Guid warehouseId)
        {
            return _dataAccess.GetBreakProfiles(warehouseId).Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<EquipmentDto> GetEquipments(Guid warehouseId)
        {
            List<EquipmentDto> equipments = new();

            foreach (var equipmentData in _dataAccess.GetEquipments(warehouseId))
                equipments.Add(MapperEquipment(equipmentData));

            return equipments;
        }

        public IEnumerable<SelectItemComboBox> GetEquipmentTypeList(Guid warehouseId)
        {
            return _dataAccess.GetEquipmentTypes(warehouseId).Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<SelectItemComboBox> GetLoadProfiles(Guid warehouseId)
        {
            return _dataAccess.GetLoadProfiles(warehouseId).Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<OrderScheduleDto> GetOrderScheduleInbound(Guid warehouseId)
        {
            List<OrderScheduleDto> orderSchedules = new();

            foreach (var order in _dataAccess.GetInboundLoad(warehouseId, Guid.Empty))
                orderSchedules.Add(MapperOrderSchedule(order));

            return orderSchedules;
        }

        public IEnumerable<OrderScheduleDto> GetOrderScheduleOutbound(Guid warehouseId)
        {

            List<OrderScheduleDto> orderSchedules = new();

            foreach (var order in _dataAccess.GetOutboundLoad(warehouseId, Guid.Empty))
                orderSchedules.Add(MapperOrderSchedule(order));

            return orderSchedules;
        }

        public IEnumerable<SelectItemComboBox> GetRolList(Guid warehouseId)
        {
            return _dataAccess.GetRoles(warehouseId).Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<SelectItemComboBox> GetShiftList(Guid warehouseId)
        {
            return _dataAccess.GetShifts(warehouseId).Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<SelectItemComboBox> GetVehicleProfiles(Guid warehouseId)
        {
            return _dataAccess.GetVehicleProfiles(warehouseId).Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<SelectItemComboBox> GetWorkerList(Guid warehouseId)
        {
            return _dataAccess.GetWorkers().Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        public IEnumerable<WorkerScheduleDto> GetWorkerSchedules(Guid warehouseId)
        {
            List<WorkerScheduleDto> workerSchedules = new();

            foreach (var schedule in _dataAccess.GetOperators(warehouseId))
                workerSchedules.Add(MapperWorkerShedule(schedule));

            return workerSchedules;
        }

        public void SaveChanges(Guid warehouseId, PlanningSettingDto model)
        {
            OperationDB operation = new OperationDB();
            try
            {
                foreach (var equipment in model.Equipments)
                {
                    if (equipment.DataOperationType == OperationType.Insert)
                        operation.AddNew(EntityNamesConst.EquipmentGroup, MapperEquipment(equipment));
                    else if (equipment.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.EquipmentGroup, MapperEquipment(equipment));
                    else if (equipment.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.EquipmentGroup, new EntityDto() { Id = equipment.Id });
                }

                foreach (var workerSchedule in model.WorkerSchedules)
                {
                    if (workerSchedule.DataOperationType == OperationType.Insert)
                        operation.AddNew(EntityNamesConst.Schedule, MapperWorkerShedule(workerSchedule));
                    else if (workerSchedule.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.Schedule, MapperWorkerShedule(workerSchedule));
                    else if (workerSchedule.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.Schedule, workerSchedule.Id);
                }

                foreach (var orderInbound in model.InputProfiles)
                {
                    if (orderInbound.DataOperationType == OperationType.Insert)
                        operation.AddNew(EntityNamesConst.OrderSchedule, MapperOrderSchedule(orderInbound, false));
                    else if (orderInbound.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.OrderSchedule, MapperOrderSchedule(orderInbound, false));
                    else if (orderInbound.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.OrderSchedule, new EntityDto() { Id = orderInbound.Id });
                }

                foreach (var orderOutbound in model.OutputProfiles)
                {
                    if (orderOutbound.DataOperationType == OperationType.Insert)
                        operation.AddNew(EntityNamesConst.OrderSchedule, MapperOrderSchedule(orderOutbound, true));
                    else if (orderOutbound.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.OrderSchedule, MapperOrderSchedule(orderOutbound, true));
                    else if (orderOutbound.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.OrderSchedule, new EntityDto() { Id = orderOutbound.Id });
                }

                _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SettingService::SaveChanges => Error to save changes");
            }
        }

        private static OrderSchedule MapperOrderSchedule(OrderScheduleDto orderDto, bool isOut)
        {
            OrderSchedule orderSchedule = new()
            {
                Id = orderDto.Id,
                InitHour = default,
                EndHour = default,
                VehicleId = Guid.Parse("1e5186f3-7c14-4ad6-93d5-4ba9b6db59fb"),
                Vehicle = null,
                NumberVehicles = 0,
                LoadId = Guid.Parse("3eb1d714-9f92-46d0-a337-6a83bba3c1fc"),
                Load = null,
                IsOut = isOut,
                WarehouseId = Guid.Parse("12764f20-063e-4df9-b762-4d0589521dee"),
                Warehouse = null
            };

            return orderSchedule;
        }

        private static OrderScheduleDto MapperOrderSchedule(OrderSchedule order)
        {
            OrderScheduleDto orderDto = new();

            orderDto.Id = order.Id;
            orderDto.InitHour = order.InitHour;
            orderDto.EndHour = order.EndHour;
            orderDto.NumberVehicles = order.NumberVehicles;

            if (order.Load != null)
            {
                orderDto.LoadId = order.Load.Id;
                orderDto.LoadName = order.Load.Name;
                orderDto.Name = order.Load.Name;
            }

            if (order.Vehicle != null)
            {
                orderDto.VehicleId = order.Vehicle.Id;
                orderDto.VehicleName = order.Vehicle.Name;
            }

            return orderDto;
        }

        private static WorkerScheduleDto MapperWorkerShedule(Schedule schedule)
        {
            WorkerScheduleDto workerScheduleDto = new();

            workerScheduleDto.Id = schedule.Id;
            workerScheduleDto.Name = schedule.Name;

            if (schedule.AvailableWorker != null && schedule.AvailableWorker.Worker != null)
            {
                workerScheduleDto.WorkerId = schedule.AvailableWorker.Worker.Id;
                workerScheduleDto.WorkerName = schedule.AvailableWorker.Worker.Name;

                if (schedule.AvailableWorker.Worker.Rol != null)
                {
                    workerScheduleDto.RolId = schedule.AvailableWorker.Worker.Rol.Id;
                    workerScheduleDto.RolName = schedule.AvailableWorker.Worker.Rol.Name;
                }
            }

            if (schedule.BreakProfile != null)
            {
                workerScheduleDto.BreakProfileId = schedule.BreakProfile.Id;
                workerScheduleDto.BreakProfileName = schedule.BreakProfile.Name;
            }

            if (schedule.Shift != null)
            {
                workerScheduleDto.ShiftId = schedule.Shift.Id;
                workerScheduleDto.ShiftName = schedule.Shift.Name;
            }

            return workerScheduleDto;
        }
        
        private static Schedule MapperWorkerShedule(WorkerScheduleDto scheduleDto)
        {
            Schedule workerScheduleDto = new()
            {
                Id = scheduleDto.Id,
                Name = scheduleDto.Name,
                AvailableWorkerId = scheduleDto.WorkerId,
                AvailableWorker = null,
                ShiftId = scheduleDto.ShiftId,
                Shift = null,
                BreakProfileId = scheduleDto.BreakProfileId,
                BreakProfile = null
            };

            return workerScheduleDto;
        }
        private EquipmentDto MapperEquipment(EquipmentGroup equipment)
        {
            EquipmentDto equipmentDto = new()
            {
                Id = equipment.Id,
                Name = equipment.Name,
                Equipments = equipment.Equipments,
                EquipmentTypeId = equipment.TypeEquipmentId,
                EquipmentTypeName = equipment.TypeEquipment?.Name ?? "",
                AreaId = equipment.AreaId,
                AreaName = equipment.Area?.Name ?? ""
            };

            return equipmentDto;
        }

        private EquipmentGroup MapperEquipment(EquipmentDto equipmentDto)
        {
            var equipmentGroup = new EquipmentGroup()
            {
                Id = equipmentDto.Id,
                Name = equipmentDto.Name,
                Equipments = equipmentDto.Equipments,
                AreaId = equipmentDto.AreaId,
                TypeEquipmentId = equipmentDto.EquipmentTypeId,
                Area = null,
                TypeEquipment = null,
            };

            return equipmentGroup;
        }

        public IEnumerable<SelectItemComboBox> GetEquipmentGroupList()
        {
            return _dataAccess.GetEquipmentGroups().Select(x => new SelectItemComboBox { Key = x.Id, Value = x.Name });
        }

        #endregion

    }
}

