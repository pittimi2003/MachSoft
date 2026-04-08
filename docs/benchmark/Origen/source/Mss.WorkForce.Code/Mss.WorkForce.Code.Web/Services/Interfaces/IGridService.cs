using DevExpress.Charts.Native;
using global::Mss.WorkForce.Code.Models.ConvertedModel;
using global::Mss.WorkForce.Code.Models.Models;
using System;
using static Mss.WorkForce.Code.Web.Components.Pages.Designer.DesignerDataGrid;
using Buffer = Mss.WorkForce.Code.Models.Models.Buffer;
namespace Mss.WorkForce.Code.Web.Services.Interfaces
{

    public interface IGridService
    {
        void SaveProcesses(IEnumerable<Process> processes);
        void ChangeProcessTypes(IEnumerable<(Process Process, string OldType, string NewType)> changes);
        void SaveProcessDirections(IEnumerable<ProcessDirectionProperty> directions);
        void SaveAreas(IEnumerable<Area> areas);
        void SaveZones(Guid areaId, IEnumerable<Zone> zones);
        void SaveDocks(Guid areaId, IEnumerable<Dock> zones);
        void SaveBuffers(Guid areaId, IEnumerable<Buffer> zones);
        void SaveStages(Guid areaId, IEnumerable<Stage> zones);
        void SaveRacks(Guid areaId, IEnumerable<Rack> zones);
        void SaveDriveIns(Guid areaId, IEnumerable<DriveIn> zones);
        void SaveChaoticStorages(Guid areaId, IEnumerable<ChaoticStorage> zones);
        void SaveAutomaticStorages(Guid areaId, IEnumerable<AutomaticStorage> zones);
        void SaveEquipments(Guid areaId, IEnumerable<EquipmentGroup> equipments);
        void SaveRoutes(Guid areaId, IEnumerable<Models.Models.Route> routes);

        void DeleteProcessDirections(IEnumerable<Guid> guids);
        void DeleteProcesses(IEnumerable<Guid> guids);
        void DeleteRoutes(IEnumerable<Guid> guids);
        void DeleteEquipments(IEnumerable<Guid> guids);
        void DeleteZones(IEnumerable<ZoneRow> zones);
        void DeleteAreas(IEnumerable<Guid> guids);


        OperationDB GetOperationDB();
        Task<bool> ExecuteAsync();
    }
    

}
