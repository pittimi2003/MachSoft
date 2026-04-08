using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface ISettingService
    {

        #region Methods

        IEnumerable<SelectItemComboBox> GetAvailableAreaList(Guid warehouseId);

        IEnumerable<SelectItemComboBox> GetBreakProfileList(Guid warehouseId);

        IEnumerable<SelectItemComboBox> GetEquipmentGroupList();

        IEnumerable<EquipmentDto> GetEquipments(Guid warehouseId);

        IEnumerable<SelectItemComboBox> GetEquipmentTypeList(Guid warehouseId);
        IEnumerable<SelectItemComboBox> GetLoadProfiles(Guid warehouseId);

        IEnumerable<OrderScheduleDto> GetOrderScheduleInbound(Guid warehouseId);

        IEnumerable<OrderScheduleDto> GetOrderScheduleOutbound(Guid warehouseId);

        IEnumerable<SelectItemComboBox> GetRolList(Guid warehouseId);

        IEnumerable<SelectItemComboBox> GetShiftList(Guid warehouseId);
        
        IEnumerable<SelectItemComboBox> GetVehicleProfiles(Guid warehouseId);
        
        IEnumerable<SelectItemComboBox> GetWorkerList(Guid warehouseId);
        
        IEnumerable<WorkerScheduleDto> GetWorkerSchedules(Guid warehouseId);

        void SaveChanges(Guid warehouseId, PlanningSettingDto model);

        #endregion

    }
}
