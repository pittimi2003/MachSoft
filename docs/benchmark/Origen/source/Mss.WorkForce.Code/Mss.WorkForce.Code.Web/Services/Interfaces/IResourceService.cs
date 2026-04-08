using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IResourceService
    {

        #region Methods

        Task<bool> BreakProfileSaveChanges(IEnumerable<BreakProfilesDto> breakProfiles, Guid warehouseId);

        Task BreaksSaveChanges(IEnumerable<BreakDto> breaks, Guid warehouseId);

        Task EquipmentTypeSaveChanges(IEnumerable<TypeEquipmentDto> equipments, Guid warehouseId);
        

        IEnumerable<BreakProfilesDto> GetBreakProfiles(Guid warehouseId);

        IEnumerable<BreakDto> GetBreaks(Guid warehouseId);

        IEnumerable<ResourceRolProcessDto> GetRolProcesses(Guid warehouseId);

        IEnumerable<ShiftsDto> GetShifts(Guid warehouseId);

        IEnumerable<TeamsDto> GetTeams(Guid warehouseId);

        IEnumerable<TypeEquipmentDto> GetTypeEquipments(Guid warehouseId);
        IEnumerable<EquipmentGroupsDto> GetEquipmentGroups(Guid warehouseId);


        IEnumerable<ResourceWorkerScheduleDto> GetWorkers(Guid warehouseId);

        Task ShiftSaveChanges(IEnumerable<ShiftsDto> shifts, Guid warehouseId);

        Task TeamsSaveChanges(IEnumerable<TeamsDto> teams, Guid warehouseId);

        Task WorkersSaveChanges(IEnumerable<ResourceWorkerScheduleDto> workers, Guid warehouseId);

        Task RolesProcessSaveChanges(IEnumerable<ResourceRolProcessDto> workers, Guid warehouseId);
        Task EquipmentGroupTypeSaveChanges(IEnumerable<EquipmentGroupsDto> enumerable, Guid warehouseId);

        #endregion

    }
}
