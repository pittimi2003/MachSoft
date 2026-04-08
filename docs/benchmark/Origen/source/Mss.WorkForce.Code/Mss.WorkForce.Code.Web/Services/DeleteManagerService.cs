using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.DTO.Designer;

namespace Mss.WorkForce.Code.Web.Services
{
    public sealed class DeleteManager
    {
        private readonly OperationDB op;
        private readonly DataAccess da;

        public DeleteManager(OperationDB op, DataAccess da)
        {
            this.op = op;
            this.da = da;
        }

        public void DeleteAreas(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                var entity = da.GetAreaByAreaId(id);
                if (entity != null)
                    op.AddDelete("Areas", entity);
            }
        }

        public void DeleteProcesses(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                var entity = da.GetProcessByIdNoTracking(id);
                if (entity != null)
                    op.AddDelete("Processes", entity);
            }
        }

        public void DeleteProcessDirections(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                var entity = da.GetProcessDirectionPropertyByIdNoTracking(id);
                if (entity != null)
                    op.AddDelete("ProcessDirectionProperties", entity);
            }
        }

        public void DeleteZones(IEnumerable<Guid> zoneIds)
        {
            foreach (var zoneId in zoneIds)
            {
                var zone = da.GetZoneByIdNoTracking(zoneId);
                if (zone == null)
                    continue;

                // Primero: eliminar subtipo asociado
                switch (ZoneTypeGetMethods.GetStringZoneType(zone.Type))
                {
                    case ZoneType.Dock:
                        var dock = da.GetDockByIdNoTracking(zoneId);
                        if (dock != null)
                            op.AddDelete("Docks", dock);
                        break;

                    case ZoneType.Buffer:
                        var buffer = da.GetBufferByIdNoTracking(zoneId);
                        if (buffer != null)
                            op.AddDelete("Buffers", buffer);
                        break;

                    case ZoneType.Stage:
                        var stage = da.GetStageByIdNoTracking(zoneId);
                        if (stage != null)
                            op.AddDelete("Stages", stage);
                        break;

                    case ZoneType.Rack:
                        var rack = da.GetRackByIdNoTracking(zoneId);
                        if (rack != null)
                            op.AddDelete("Racks", rack);
                        break;

                    case ZoneType.DriveIn:
                        var driveIn = da.GetDriveInByIdNoTracking(zoneId);
                        if (driveIn != null)
                            op.AddDelete("DriveIns", driveIn);
                        break;

                    case ZoneType.ChaoticStorage:
                        var chaotic = da.GetChaoticByIdNoTracking(zoneId);
                        if (chaotic != null)
                            op.AddDelete("ChaoticStorages", chaotic);
                        break;

                    case ZoneType.AutomaticStorage:
                        var auto = da.GetAutomaticStorageByIdNoTracking(zoneId);
                        if (auto != null)
                            op.AddDelete("AutomaticStorages", auto);
                        break;
                }

                // Finalmente: eliminar la zona
                op.AddDelete("Zones", zone);
            }
        }


        public void DeleteEquipments(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                var entity = da.GetEquipmentGroupsByIdAsNoTracking(id);
                if (entity != null)
                    op.AddDelete("EquipmentGroups", entity);
            }
        }

        public void DeleteRoutes(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                var entity = da.GetRouteByIdAsNoTracking(id);
                if (entity != null)
                    op.AddDelete("Routes", entity);
            }
        }
    }
}
