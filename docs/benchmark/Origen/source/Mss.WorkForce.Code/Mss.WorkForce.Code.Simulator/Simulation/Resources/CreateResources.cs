using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Helper;

namespace Mss.WorkForce.Code.Simulator.Simulation.Resources
{
    public class CreateResources
    {
        #region Variables
        private DateTime SimulationStartDate { get; set; }
        public DataSimulatorTablaRequest Data { get; set; }
        private Simulation Simulation { get; set; }
        #endregion

        #region Constructor
        public CreateResources(DateTime simulationStartDate, DataSimulatorTablaRequest data, Simulation simulation)
        {
            this.SimulationStartDate = simulationStartDate;
            this.Data = data;
            this.Simulation = simulation;
        }
        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Creates and assigns the different SimSharp resources to each warehouse.
        /// </summary>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        /// <returns>Warehouse with its resources included</returns>
        public void AssignResources(bool whatIf)
        {
            CreateWorkersResources(whatIf);
            CreateEquipmentResources();
            CreateStationResources();
        }
        #endregion

        #region Private
        /// <summary>
        /// Creates the workers SimSharp resources
        /// </summary>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        private void CreateWorkersResources(bool whatIf)
        {
            if (!whatIf)
            {
                foreach (var worker in this.Data.AvailableWorker)
                {
                    var shifts = this.Data.Schedule.Where(x => x.AvailableWorkerId == worker.Id).Select(x => x.Shift).ToList();
                    var breakProfileIds = this.Data.Schedule.Where(x => x.AvailableWorkerId == worker.Id).Select(x => x.BreakProfileId);
                    List<CustomBreak> breaks = new List<CustomBreak>();
                    foreach (var breakProfileId in breakProfileIds)
                    {
                        var _breaks = this.Data.Break.Where(x => x.BreakProfileId == breakProfileId);

                        foreach (var _break in _breaks)
                            breaks.Add(new CustomBreak(_break.Id, _break.InitBreak, _break.EndBreak));
                    }

                    var mergedBreaks = Helper.Methods.MergeBreaks.Merge(breaks);
                    var resource = new Resource(worker.WorkerId, worker.Name, ResourceType.Worker, null, worker.Worker.RolId, shifts, mergedBreaks, 1);
                    this.Simulation.Resources.Add(worker.WorkerId, resource);
                }
            }
        }

        /// <summary>
        /// Creates the equipment SimSharp resources
        /// </summary>
        private void CreateEquipmentResources()
        {
            foreach (var equipmentsInArea in this.Data.EquipmentGroup)
            {
                int equipmentsNumber = equipmentsInArea.Equipments == 0 ? 1 : equipmentsInArea.Equipments;
                var warehouseArea = equipmentsInArea.Area;

                this.Simulation.Resources.Add(equipmentsInArea.Id, new Resource(equipmentsInArea.Id, equipmentsInArea.Name, ResourceType.Equipment, warehouseArea, equipmentsNumber));
            }
        }

        /// <summary>
        /// Creates the station SimSharp resources.
        /// </summary>
        private void CreateStationResources()
        {
            foreach (var zone in this.Data.Zone)
            {
                var area = zone.Area;
                var zoneType = zone.Type;
                var zoneName = zone.Name;
                var zoneId = zone.Id;
                var maxEquipments = zone.MaxEquipments.GetValueOrDefault(int.MaxValue);
                var maxContainers = zone.MaxContainers.GetValueOrDefault(int.MaxValue);

                switch (zoneType)
                {
                    case ZoneType.Rack:
                        var rack = this.Data.Rack.FirstOrDefault(x => x.ZoneId == zoneId);
                        var rackCapacity = rack.NarrowAisle.GetValueOrDefault(false) ? rack.MaxPickers.GetValueOrDefault(int.MaxValue) : maxEquipments;
                        this.Simulation.Resources.Add(zoneId, new Resource(zoneId, zoneName, ResourceType.Zone, zoneType, area, rackCapacity, maxContainers, area.Id));
                        break;

                    case ZoneType.DriveIn:
                        var driveIn = this.Data.DriveIn.FirstOrDefault(x => x.ZoneId == zoneId);
                        var driveInCapacity = driveIn.NarrowAisle.GetValueOrDefault(false) ? driveIn.MaxPickers.GetValueOrDefault(int.MaxValue) : maxEquipments;
                        this.Simulation.Resources.Add(zoneId, new Resource(zoneId, zoneName, ResourceType.Zone, zoneType, area, driveInCapacity, maxContainers, area.Id));
                        break;

                    case ZoneType.Stage:
                        var stage = this.Data.Stage.FirstOrDefault(x => x.ZoneId == zone.Id);
                        var associatedDocks = this.Data.AvailableDocksPerStage.Where(x => x.StageId == stage.Id).Join(this.Data.Dock, ad => ad.DockId, d => d.Id, (ad, d) => new
                        {
                            DockResourceId = d.ZoneId
                        }).Select(x => x.DockResourceId).ToList();
                        this.Simulation.Resources.Add(zone.Id, new Resource(zone.Id, zone.Name, ResourceType.Zone, zoneType, area, maxEquipments, maxContainers, associatedDocks, area.Id));
                        break;

                    default:
                        var dock = zoneType == ZoneType.Dock ? this.Data.Dock.FirstOrDefault(x => x.ZoneId == zoneId) : null;
                        this.Simulation.Resources.Add(zoneId, new Resource(zoneId, zoneName, ResourceType.Zone, zoneType, area, maxEquipments, maxContainers, area.Id,
                            dock?.InboundRange, dock?.OutboundRange));
                        break;
                } 
            }
        }
        #endregion

        #endregion
    }
}
