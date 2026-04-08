using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Web.Services.Interfaces;
using Buffer = Mss.WorkForce.Code.Models.Models.Buffer;
using Route = Mss.WorkForce.Code.Models.Models.Route;


namespace Mss.WorkForce.Code.Web.Services
{
    public sealed class SaveManager
    {
        // ========================
        //   ÁREAS
        // ========================
        public List<Area> AreasToSave { get; } = new();

        // ========================
        //   ZONAS por Área
        // ========================
        public List<(Guid AreaId, Zone Zone)> ZonesToSave { get; } = new();

        // ========================
        //   SUBTIPOS DE ZONA
        // ========================
        public List<(Guid AreaId, Dock Dock)> DocksToSave { get; } = new();
        public List<(Guid AreaId, Buffer Buffer)> BuffersToSave { get; } = new();
        public List<(Guid AreaId, Stage Stage)> StagesToSave { get; } = new();
        public List<(Guid AreaId, Rack Rack)> RacksToSave { get; } = new();
        public List<(Guid AreaId, DriveIn DriveIn)> DriveInsToSave { get; } = new();
        public List<(Guid AreaId, ChaoticStorage ChaoticToSave)> ChaoticStoragesToSave { get; } = new();
        public List<(Guid AreaId, AutomaticStorage AutoToSave)> AutomaticStoragesToSave { get; } = new();


        // ========================
        //   EQUIPOS por Área
        // ========================
        public List<(Guid AreaId, EquipmentGroup Equipment)> EquipmentsToSave { get; } = new();

        // ========================
        //   RUTAS por Área
        // ========================
        public List<(Guid AreaId, Route Route)> RoutesToSave { get; } = new();

        // ========================
        //   PROCESOS
        // ========================
        public List<Process> ProcessesToSave { get; } = new();

        // ========================
        //   DIRECCIONES
        // ========================
        public List<ProcessDirectionProperty> DirectionsToSave { get; } = new();

        // ========================
        //   CAMBIOS DE TIPO
        // ========================
        public List<(Process Process, string OldType, string NewType)> ProcessTypeChanges { get; } = new();

        // ===================================================================================
        //   APLICACIÓN A GridService (idéntico a tu orden en OnSaveAsync)
        // ===================================================================================
        public void ApplyTo(IGridService gridService)
        {
            // 1) ÁREAS
            if (AreasToSave.Count > 0)
            {
                gridService.SaveAreas(AreasToSave);
            }

            // 2) ZONAS
            foreach (var (areaId, zone) in ZonesToSave)
            {
                gridService.SaveZones(areaId, new List<Zone> { zone });
            }

            // 3) SUBTIPOS DE ZONA
            foreach (var (areaId, dock) in DocksToSave)
            {
                gridService.SaveDocks(areaId, new List<Dock> { dock });
            }

            foreach (var (areaId, buffer) in BuffersToSave)
            {
                gridService.SaveBuffers(areaId, new List<Buffer> { buffer });
            }

            foreach (var (areaId, stage) in StagesToSave)
            {
                gridService.SaveStages(areaId, new List<Stage> { stage });
            }

            foreach (var (areaId, rack) in RacksToSave)
            {
                gridService.SaveRacks(areaId, new List<Rack> { rack });
            }

            foreach (var (areaId, d) in DriveInsToSave)
            {
                gridService.SaveDriveIns(areaId, new List<DriveIn> { d });
            }

            foreach (var (areaId, c) in ChaoticStoragesToSave)
            {
                gridService.SaveChaoticStorages(areaId, new List<ChaoticStorage> { c });
            }

            foreach (var (areaId, a) in AutomaticStoragesToSave)
            {
                gridService.SaveAutomaticStorages(areaId, new List<AutomaticStorage> { a });
            }

            // 4) EQUIPMENTS
            foreach (var (areaId, eq) in EquipmentsToSave)
            {
                gridService.SaveEquipments(areaId, new List<EquipmentGroup> { eq });
            }

            // 5) ROUTES
            foreach (var (areaId, route) in RoutesToSave)
            {
                gridService.SaveRoutes(areaId, new List<Route> { route });
            }

            // 6) PROCESSES
            if (ProcessesToSave.Count > 0)
            {
                gridService.SaveProcesses(ProcessesToSave);
            }

            // 7) DIRECTIONS
            if (DirectionsToSave.Count > 0)
            {
                gridService.SaveProcessDirections(DirectionsToSave);
            }

            // 8) CAMBIOS DE TIPO DEL PROCESO
            if (ProcessTypeChanges.Count > 0)
            {
                gridService.ChangeProcessTypes(ProcessTypeChanges);
            }
        }
    }

}
