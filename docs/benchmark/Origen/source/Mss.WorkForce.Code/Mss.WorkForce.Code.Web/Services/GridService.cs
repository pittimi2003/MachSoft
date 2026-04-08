using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.Interfaces;
using static Mss.WorkForce.Code.Web.Components.Pages.Designer.DesignerDataGrid;
using Buffer = Mss.WorkForce.Code.Models.Models.Buffer;

namespace Mss.WorkForce.Code.Web.Services
{
    public class GridService : IGridService
    {
        private OperationDB operationDB;
        private readonly DataAccess da;
        private readonly ISimulateService _simulateService;
        private readonly DeleteManager deleteManager;

        public GridService(DataAccess dataAccess, ISimulateService simulateService)
        {
            da = dataAccess;
            _simulateService = simulateService;
            operationDB = new OperationDB();

            // DeleteManager se crea aquí y reutiliza el mismo OperationDB
            deleteManager = new DeleteManager(operationDB, da);

        }

        // ===================================================
        // ========== PROCESOS ===============================
        // ===================================================

        public void SaveProcesses(IEnumerable<Process> processes)
        {
            foreach (var process in processes)
            {
                process.Area = null;
                process.ProcessDirectionPropertiesEntry = null;

                bool exists = da.ExistsProcess(process.Id);

                if (!exists)
                {
                    operationDB.AddNew("Processes", process);
                    CreateProcessTypeEntity(process);
                }
                else
                {
                    operationDB.AddUpdate("Processes", process);
                }
            }
        }

        public void ChangeProcessTypes(IEnumerable<(Process Process, string OldType, string NewType)> changes)
        {
            foreach (var c in changes)
            {
                if (c.OldType == c.NewType)
                    continue; // No hacer nada si no hubo cambio real

                DeleteProcessTypeEntity(c.Process.Id, c.OldType);
                CreateProcessTypeEntity(c.Process);
            }
        }


        private void DeleteProcessTypeEntity(Guid processId, string? oldType)
        {
            if (string.IsNullOrWhiteSpace(oldType))
                return;

            switch (oldType)
            {
                case "CustomProcess":
                    var cp = da.GetCustomeProcesById(processId);
                    if (cp != null) operationDB.AddDelete("CustomProcesses", cp);
                    break;

                case "Inbound":
                    var inbound = da.GetInboundByProcessId(processId);
                    if (inbound != null) operationDB.AddDelete("Inbounds", inbound);
                    break;

                case "Loading":
                    var load = da.GetLoadingByProcessId(processId);
                    if (load != null) operationDB.AddDelete("Loadings", load);
                    break;

                case "Picking":
                    var pick = da.GetPickingByProcessId(processId);
                    if (pick != null) operationDB.AddDelete("Pickings", pick);
                    break;

                case "Putaway":
                    var put = da.GetPutawayByIdWithProcess(processId);
                    if (put != null) operationDB.AddDelete("Putaways", put);
                    break;

                case "Shipping":
                    var ship = da.GetShippingByIdProcess(processId);
                    if (ship != null) operationDB.AddDelete("Shippings", ship);
                    break;

                case "Reception":
                    var rec = da.GetReceptionByIdProcess(processId);
                    if (rec != null) operationDB.AddDelete("Receptions", rec);
                    break;

                case "Replenishment":
                    var rep = da.GetReplenishmentByIdProcess(processId);
                    if (rep != null) operationDB.AddDelete("Replenishments", rep);
                    break;
            }
        }

        private void CreateProcessTypeEntity(Process process)
        {
            if (string.IsNullOrWhiteSpace(process.Type))
                return;

            switch (process.Type)
            {
                case "CustomProcess":
                    operationDB.AddNew("CustomProcesses", new CustomProcess
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process
                    });
                    break;

                case "Inbound":
                    operationDB.AddNew("Inbounds", new Inbound
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process,
                        Quantity = 0,
                        VehiclePerHour = 0,
                        TruckPerDay = 0
                    });
                    break;

                case "Loading":
                    operationDB.AddNew("Loadings", new Loading
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process
                    });
                    break;

                case "Picking":
                    operationDB.AddNew("Pickings", new Picking
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process
                    });
                    break;

                case "Putaway":
                    operationDB.AddNew("Putaways", new Putaway
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process
                    });
                    break;

                case "Shipping":
                    operationDB.AddNew("Shippings", new Shipping
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process
                    });
                    break;

                case "Reception":
                    operationDB.AddNew("Receptions", new Reception
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process
                    });
                    break;

                case "Replenishment":
                    operationDB.AddNew("Replenishments", new Replenishment
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = process.Id,
                        Process = process
                    });
                    break;
            }
        }

        public void SaveProcessDirections(IEnumerable<ProcessDirectionProperty> directions)
        {
            foreach (var dir in directions)
            {
                dir.InitProcess = null;
                dir.EndProcess = null;

                bool exists = da.ExistsProcessDirection(dir.Id);

                if (!exists)
                    operationDB.AddNew("ProcessDirectionProperties", dir);
                else
                    operationDB.AddUpdate("ProcessDirectionProperties", dir);
            }
        }

        // ===================================================
        // ========== DELETE =======
        // ===================================================

        public void DeleteProcesses(IEnumerable<Guid> ids)
            => deleteManager.DeleteProcesses(ids);

        public void DeleteProcessDirections(IEnumerable<Guid> ids)
            => deleteManager.DeleteProcessDirections(ids);

        public void DeleteRoutes(IEnumerable<Guid> ids)
            => deleteManager.DeleteRoutes(ids);

        public void DeleteEquipments(IEnumerable<Guid> ids)
            => deleteManager.DeleteEquipments(ids);        

        public void DeleteAreas(IEnumerable<Guid> ids)
            => deleteManager.DeleteAreas(ids);

        // ===================================================
        // ========== DELETES ESPECÍFICOS ====================
        // ===================================================

        public void DeleteZones(IEnumerable<ZoneRow> zones)
        {
            foreach (var zr in zones)
            {
                // 1) Borrar el hijo según el tipo
                switch (zr.Type)
                {
                    case "Dock":
                        {
                            var dock = da.GetDockByIdNoTracking(zr.Id);
                            if (dock != null)
                                operationDB.AddDelete("Docks", dock);
                            break;
                        }

                    case "Buffer":
                        {
                            var buffer = da.GetBufferByIdNoTracking(zr.Id);
                            if (buffer != null)
                                operationDB.AddDelete("Buffers", buffer);
                            break;
                        }

                    case "Stage":
                        {
                            var stage = da.GetStageByIdNoTracking(zr.Id);
                            if (stage != null)
                                operationDB.AddDelete("Stages", stage);
                            break;
                        }
                }

                // 2) Borrar siempre la Zone
                var zone = da.GetZoneByIdNoTracking(zr.Id);
                if (zone != null)
                    operationDB.AddDelete("Zones", zone);
            }
        }

        // ===================================================
        // ========== AREAS Y SUBENTIDADES ===================
        // ===================================================

        public void SaveAreas(IEnumerable<Area> areas)
        {
            foreach (var area in areas)
            {
                bool exists = da.ExistsArea(area.Id);

                if (!exists)
                    operationDB.AddNew("Areas", area);
                else
                    operationDB.AddUpdate("Areas", area);
            }
        }

        public void SaveZones(Guid areaId, IEnumerable<Zone> zones)
        {
            foreach (var zone in zones)
            {
                zone.AreaId = areaId;

                bool exists = da.ExistsZone(zone.Id);

                if (!exists)
                    operationDB.AddNew("Zones", zone);
                else
                    operationDB.AddUpdate("Zones", zone);
            }
        }

        public void SaveDocks(Guid areaId, IEnumerable<Dock> docks)
        {
            foreach (var dock in docks)
            {
                bool exists = da.ExistsDock(dock.Id);

                if (!exists)
                    operationDB.AddNew("Docks", dock);
                else
                    operationDB.AddUpdate("Docks", dock);
            }
        }

        public void SaveBuffers(Guid areaId, IEnumerable<Buffer> buffers)
        {
            foreach (var buffer in buffers)
            {
                bool exists = da.ExistsBuffer(buffer.Id);

                if (!exists)
                    operationDB.AddNew("Buffers", buffer);
                else
                    operationDB.AddUpdate("Buffers", buffer);
            }
        }

        public void SaveStages(Guid areaId, IEnumerable<Stage> stages)
        {
            foreach (var stage in stages)
            {
                bool exists = da.ExistsStage(stage.Id);

                if (!exists)
                    operationDB.AddNew("Stages", stage);
                else
                    operationDB.AddUpdate("Stages", stage);
            }
        }

        public void SaveRacks(Guid areaId, IEnumerable<Rack> racks)
        {
            foreach (var rack in racks)
            {
                bool exists = da.ExistsRack(rack.Id);

                if (!exists)
                    operationDB.AddNew("Racks", rack);
                else
                    operationDB.AddUpdate("Racks", rack);
            }
        }

        public void SaveDriveIns(Guid areaId, IEnumerable<DriveIn> driveIns)
        {
            foreach (var driveIn in driveIns)
            {
                bool exists = da.ExistsDriveIn(driveIn.Id);

                if (!exists)
                    operationDB.AddNew("DriveIns", driveIn);
                else
                    operationDB.AddUpdate("DriveIns", driveIn);
            }
        }

        public void SaveChaoticStorages(Guid areaId, IEnumerable<ChaoticStorage> chaoticStorages)
        {
            foreach (var chaoticStorage in chaoticStorages)
            {
                bool exists = da.ExistsChaotic(chaoticStorage.Id);

                if (!exists)
                    operationDB.AddNew("ChaoticStorages", chaoticStorage);
                else
                    operationDB.AddUpdate("ChaoticStorages", chaoticStorage);
            }
        }

        public void SaveAutomaticStorages(Guid areaId, IEnumerable<AutomaticStorage> automaticStorages)
        {
            foreach (var automaticStorage in automaticStorages)
            {
                bool exists = da.ExistsAutomatic(automaticStorage.Id);

                if (!exists)
                    operationDB.AddNew("AutomaticStorages", automaticStorage);
                else
                    operationDB.AddUpdate("AutomaticStorages", automaticStorage);
            }
        }       

        public void SaveEquipments(Guid areaId, IEnumerable<EquipmentGroup> equipments)
        {
            foreach (var eq in equipments)
            {
                eq.AreaId = areaId;

                bool exists = da.ExistsEquipment(eq.Id);

                if (!exists)
                    operationDB.AddNew("EquipmentGroups", eq);
                else
                    operationDB.AddUpdate("EquipmentGroups", eq);
            }
        }

        public void SaveRoutes(Guid areaId, IEnumerable<Models.Models.Route> routes)
        {
            foreach (var route in routes)
            {
                route.DepartureArea = null;
                route.ArrivalArea = null;
                route.DepartureAreaId = areaId;

                bool exists = da.ExistsRoute(route.Id);

                if (!exists)
                    operationDB.AddNew("Routes", route);
                else
                    operationDB.AddUpdate("Routes", route);
            }
        }

        public OperationDB GetOperationDB() => operationDB;


        // ===================================================
        // ========== EJECUCIÓN DE GUARDADO ==================
        // ===================================================

        public async Task<bool> ExecuteAsync()
        {
            if (operationDB == null)
                return false;

            await _simulateService.SaveChangesInDataBase(operationDB);

            operationDB.Clear();          

            return true;
        }
    }
}
