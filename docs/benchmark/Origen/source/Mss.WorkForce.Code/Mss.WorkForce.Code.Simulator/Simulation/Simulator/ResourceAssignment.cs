using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Helper.Methods;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Appointments;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator
{
    public class ResourceAssignment
    {
        #region Variables
        private Simulator simulator;
        #endregion

        #region Constructor
        public ResourceAssignment(Simulator simulator)
        {
            this.simulator = simulator;
        }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Tries to get the necessary worker for the process
        /// </summary>
        /// <param name="process">Process that needs the resource</param>
        /// <returns>True if found, false if not</returns>
        public bool TryGetWorker(Grouping process)
        {
            IEnumerable<Resource>? resourcesOptions = null;

            if (process.Process.ProcessType == ProcessType.Inbound && process.Appointment != null)
                resourcesOptions = CheckWorkshiftOnTime(
                    process.Appointment.Workers
                        .Where(w => w.EndDate != null && w.EndDate == this.simulator.Simulation.Environment.Now)
                        .Select(w => this.simulator.Simulation.Resources[w.WorkerId])
                );

            if (resourcesOptions == null || !resourcesOptions.Any())
                resourcesOptions = CheckWorkshiftOnTime(GetWorkers(process));

            var freeResource = resourcesOptions.FirstOrDefault(r => r.InUse < r.Capacity);

            if (freeResource != null)
            {
                process.AssignedWorker = freeResource;
                return true;
            }
            else
            {
                QueueProcess(process);
                this.simulator.pausedByWorkers = true;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the necessary station for the process
        /// </summary>
        /// <param name="process">Process that needs the resource</param>
        /// <returns>True if found, false if not</returns>
        public bool TryGetZones(Grouping process)
        {
            var dockResourceId = process.Process.Area.Type == AreaType.Dock ? process.Appointment.DockResourceId : null;

            var dockResource = this.simulator.Simulation.Resources.Values.Where(x => x.Id == dockResourceId);

            var resourcesOptions = dockResourceId == null ? GetZones(process, process.Appointment) : dockResource;

            var freeResources = resourcesOptions.Where(x => x.InUse < x.Capacity && (x.CurrentContainers + process.Containers) <= x.MaxContainers);
            var freeResource = freeResources.FirstOrDefault();

            if (freeResource != null)
            {
                // TODO: Condiciones para no entrar siempre a setear
                SetActualRange(freeResource, process);
                if (!process.Process.IsWarehouseProcess)
                    if (process.Appointment != null && process.Appointment.PossibleDockIds == null && process.Appointment.DockResourceId == null 
                        && freeResource.Type == ResourceType.Zone && freeResource.ZoneType == ZoneType.Stage)
                        process.Appointment.PossibleDockIds = freeResource.AssociatedDocks;
                
                process.SelectedStation = this.simulator.Simulation.Data.Zone.FirstOrDefault(x => x.Id == freeResource.Id);
                return true;
            }
            else
            {
                QueueProcess(process);
                this.simulator.pausedByZoneEquipments = !resourcesOptions.Any(x => x.InUse < x.Capacity);
                if (!this.simulator.pausedByZoneEquipments) 
                    this.simulator.pausedByZoneContainers = !resourcesOptions.Any(x => (x.CurrentContainers + process.Containers) <= x.MaxContainers);
                return false;
            }
        }

        /// <summary>
        /// Tries to get the necessary equipment for the process
        /// </summary>
        /// <param name="process">Process that needs the resource</param>
        /// <returns>True if found, false if not</returns>
        public bool TryGetEquipment(Grouping process)
        {
            var resourcesOptions = GetEquipments(process);

            var freeResources = resourcesOptions.Where(x => x.InUse < x.Capacity);

            if (freeResources.Any())
            {
                process.AssignedEquipment = freeResources.FirstOrDefault();
                return true;
            }
            else
            {
                QueueProcess(process);
                this.simulator.pausedByEquipments = true;
                return false;
            }
        }

        /// <summary>
        /// Releases the resources used for working in a process
        /// </summary>
        /// <param name="process">Process with the resources assigned</param>
        public void ReleaseResources(Grouping process)
        {
            this.simulator.Simulation.Resources[process.AssignedEquipment.Id].InUse--;
            this.simulator.Simulation.Resources[process.AssignedWorker.Id].InUse--;
            this.simulator.Simulation.Resources[process.SelectedStation.Id].InUse--;

            // Si es Loading, cuando termine de cargar habremos liberado el dock
            // Si es Putaway, liberamos el almacenamiento
            if (process.OrderId == null || process.Position == process.AssociatedProcesses.Max(x => x.Position))
                this.simulator.Simulation.Resources[process.SelectedStation.Id].CurrentContainers -= process.Containers;
        }

        /// <summary>
        /// Requests the resources for working in a process
        /// </summary>
        /// <param name="process">Process with the resources assigned</param>
        public void AssignCapacity(Grouping process)
        {
            this.simulator.Simulation.Resources[process.SelectedStation.Id].InUse++;
            this.simulator.Simulation.Resources[process.SelectedStation.Id].CurrentContainers += process.Containers;
            this.simulator.Simulation.Resources[process.AssignedWorker.Id].InUse++;
            this.simulator.Simulation.Resources[process.AssignedEquipment.Id].InUse++;
        }

        /// <summary>
        /// Check if its possible to free the assigned dock of the appointment
        /// </summary>
        /// <param name="process">Process to check if it is possible to free the dock</param>
        public void TryFreeDockFromVehicle(Grouping process)
        {
            if (process.OrderId != null && process.Process.Area.Type == AreaType.Dock)
            {
                var appointmentDockProcesses = this.simulator.Simulation.Processes
                    .Where(x => x.Process.Area.Type == AreaType.Dock)
                    .Where(x => x.Appointment.Id == process.Appointment.Id);

                if (!appointmentDockProcesses.Any(x => x.IsActive))
                {
                    var dockResourceId = process.Appointment.DockResourceId;
                    this.simulator.Simulation.Resources[dockResourceId!.Value].IsOccupiedByVehicle = false;
                }
            }
        }

        #endregion

        #region Private

        #region Workers
        /// <summary>
        /// Gets the worker options for the process
        /// </summary>
        /// <param name="process">Process that needs the equipment</param>
        /// <returns>Enumeration of the possible workers</returns>
        private IEnumerable<Resource> GetWorkers(Grouping process)
        {
            var roles = this.simulator.Simulation.Data.RolProcessSequence
                .Where(x => x.ProcessId == process.Process.Id)
                .Select(x => x.RolId);

            return this.simulator.Simulation.Resources.Values
                .Where(r => r.Type == ResourceType.Worker && roles.Contains(r.RolId.Value));
        }

        /// <summary>
        /// Checks how many resources are within their workshifts times
        /// </summary>
        /// <param name="resourcesOptions">IEnumerable of possible resources</param>
        /// <returns>IEnumerable of the workers that are within their work shift</returns>
        private IEnumerable<Resource> CheckWorkshiftOnTime(IEnumerable<Resource> resourcesOptions)
        {
            var now = this.simulator.Simulation.Environment.Now.TimeOfDay;

            return resourcesOptions.Where(r => r.Shifts.Any(s =>
            {
                var init = TimeSpan.FromHours(s.InitHour);
                var end = TimeSpan.FromHours(s.EndHour);

                return init < end
                    ? now >= init && now < end
                    : now >= init || now < end; // turno cruzando medianoche
            }));
        }

        #endregion

        #region Stations
        /// <summary>
        /// Sets the actual range of the selected resource
        /// </summary>
        /// <param name="freeResource">Free resource</param>
        /// <param name="process">Process that needs the dock</param>
        private void SetActualRange(Resource freeResource, Grouping process)
        {
            if (freeResource.ZoneType == ZoneType.Dock)
            {
                if (process.OrderId != null && this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).IsOutbound)
                    this.simulator.LastOutboundDock = freeResource.OutboundRange;
                else if (process.OrderId != null && !this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).IsOutbound)
                    this.simulator.LastInboundDock = freeResource.InboundRange;
            }
        }

        /// <summary>
        /// Gets the station options for the process
        /// </summary>
        /// <param name="process">Process that needs the station</param>
        /// <returns>Enumeration of the possible stations</returns>
        private IEnumerable<Resource> GetZones(Grouping process, Appointment appointment)
        {
            // Si el area asignada es de tipo Dock
            if (process.Process.Area.Type == AreaType.Dock) 
                return DockSelection(process, appointment);
            else if (process.Process.Area.Type == AreaType.Stage) 
                return StageSelection(process, appointment);
            else 
                return this.simulator.Simulation.Data.Zone.Where(x => x.AreaId == process.Process.Area.Id)
                        .Join(this.simulator.Simulation.Resources.Values.Where(x => x.Type == ResourceType.Zone), so => so.Id, r => r.Id, (so, r) => new Resource
                        {
                            Id = r.Id,
                            Type = r.Type,
                            InUse = r.InUse,
                            Capacity = r.Capacity,
                            ZoneType = r.ZoneType,
                            MaxContainers = r.MaxContainers,
                            CurrentContainers = r.CurrentContainers
                        });
        }

        /// <summary>
        /// Selects the docks depending of the sequence.
        /// </summary>
        /// <param name="process">Process to assign the dock</param>
        /// <returns>Returns a sequence of the docks in the order of the chosen sequence. If there are free docks, these would by ordered from the favourite to the
        /// less wanted one. In case none of them are free, it will return the enumeration of the possible docks.</returns>
        private IEnumerable<Resource> DockSelection(Grouping process, Appointment appointment)
        {
            var assignedDockId = this.simulator.Simulation.Data.Dock.FirstOrDefault(x => x.Id == appointment.AssignedDockId);
            Guid? assignedDockZone = assignedDockId == null ? null : assignedDockId.ZoneId;
            var assignedDockResource = assignedDockZone == null ? null : this.simulator.Simulation.Resources[assignedDockZone.Value];

            Resource? preferedDockResource = null;
            if (assignedDockResource == null)
            {
                var preferedDockId = this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).PreferredDockId;
                Guid? preferedDockStation = preferedDockId == null ? null : this.simulator.Simulation.Data.Dock.FirstOrDefault(x => x.Id == preferedDockId).ZoneId;
                preferedDockResource = preferedDockStation == null ? null : this.simulator.Simulation.Resources[preferedDockStation.Value];
            }

            var possibleDocks = appointment.PossibleDockIds;

            if (assignedDockResource != null)
            {
                return this.simulator.Simulation.Resources.Values.Where(x => x.Id == assignedDockResource.Id);
            }
            else if (preferedDockResource != null && preferedDockResource.InUse < preferedDockResource.Capacity && !preferedDockResource.IsOccupiedByVehicle.Value
                && possibleDocks != null && possibleDocks.Contains(preferedDockResource.Id))
            {
                return this.simulator.Simulation.Resources.Values.Where(x => x.Id ==
                    this.simulator.Simulation.Data.Dock.FirstOrDefault(x => x.Id ==
                    this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).PreferredDockId).ZoneId);
            }
            else
            {
                var processFlow = this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).IsOutbound;
                var strategy = processFlow ? this.simulator.Simulation.OutboundDockSelectionStrategy : this.simulator.Simulation.InboundDockSelectionStrategy;

                switch (strategy.Code)
                {
                    case "Random Selection":
                        return GetDocks.GetDockResources(this.simulator.Simulation, process, possibleDocks).Where(x => !x.IsOccupiedByVehicle.Value);

                    case "Lowest Available":
                        if (process.OrderId == null)
                            return GetDocks.GetDockResources(this.simulator.Simulation, process, possibleDocks).Where(x => !x.IsOccupiedByVehicle.Value);
                        else if (this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).IsOutbound)
                            return GetDocks.GetDockResources(this.simulator.Simulation, process, possibleDocks).OrderBy(x => x.OutboundRange).Where(x => !x.IsOccupiedByVehicle.Value);
                        else
                            return GetDocks.GetDockResources(this.simulator.Simulation, process, possibleDocks).OrderBy(x => x.InboundRange).Where(x => !x.IsOccupiedByVehicle.Value);

                    case "Sequential":
                        var resources = GetDocks.GetDockResources(this.simulator.Simulation, process, possibleDocks).Where(x => !x.IsOccupiedByVehicle.Value).ToList();

                        if (process.OrderId == null)
                        {
                            return resources;
                        }
                        else if (this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).IsOutbound)
                        {
                            // Contamos las posiciones que tenemos desde el final hasta el que corresponda
                            var count = (resources.Count() - 1) - resources.FindIndex(x => x.OutboundRange == this.simulator.LastOutboundDock);
                            // Quitamos el availability range que corresponda
                            var singleTemp = resources.FirstOrDefault(x => x.OutboundRange == this.simulator.LastOutboundDock);
                            // Movemos count veces de posición los elementos restantes
                            List<Resource> temp = resources.Where(x => x.OutboundRange != this.simulator.LastOutboundDock).ToList();
                            temp = MovePositions.Move(temp, count);
                            if (singleTemp != null) temp.Add(singleTemp);

                            return temp;
                        }
                        else
                        {
                            // Contamos las posiciones que tenemos desde el final hasta el que corresponda
                            var count = (resources.Count() - 1) - resources.FindIndex(x => x.InboundRange == this.simulator.LastInboundDock);
                            // Quitamos el availability range que corresponda
                            var singleTemp = resources.FirstOrDefault(x => x.InboundRange == this.simulator.LastInboundDock);
                            // Movemos count veces de posición los elementos restantes
                            List<Resource> temp = resources.Where(x => x.InboundRange != this.simulator.LastInboundDock).ToList();
                            temp = MovePositions.Move(temp, count);
                            if (singleTemp != null) temp.Add(singleTemp);

                            return temp;
                        }

                    default:
                        Console.WriteLine($"Error with the dock selection sequence. The strategy {strategy.Code} is not a configuration.");
                        throw new Exception($"Error with the dock selection sequence. The strategy {strategy.Code} is not a configuration.");
                }
            }
        }

        /// <summary>
        /// Selects the stages depending on the dock and the sequence
        /// </summary>
        /// <param name="process">Process to assign the dock</param>
        /// <returns>Enumerable of availables stages</returns>
        private IEnumerable<Resource> StageSelection(Grouping process, Appointment appointment)
        {
            var preferedDockId = this.simulator.Simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).PreferredDockId;
            var assignedDockId =
                process.Appointment.DockResourceId
                ?? this.simulator.Simulation.Data.Dock
                .FirstOrDefault(x => x.Id == process.Appointment?.AssignedDockId)?.ZoneId;

            var isOut = process.Appointment.IsOut;

            if (assignedDockId != null)
            {
                return this.simulator.Simulation.Data.Stage.Where(st => st.Zone.AreaId == process.Process.Area.Id && (isOut ? st.IsOut : st.IsIn))
                    .Join(
                        this.simulator.Simulation.Resources.Values
                            .Where(r => r.Type == ResourceType.Zone
                                        && r.ZoneType == ZoneType.Stage
                                        && r.AssociatedDocks.Contains(assignedDockId.Value)),
                        sz => sz.ZoneId,
                        r => r.Id,
                        (sz, r) => new Resource
                        {
                            Id = r.Id,
                            Type = r.Type,
                            InUse = r.InUse,
                            Capacity = r.Capacity,
                            ZoneType = r.ZoneType,
                            MaxContainers = r.MaxContainers,
                            CurrentContainers = r.CurrentContainers
                        }
                    );
            }
            else if (preferedDockId != null)
            {
                // TODO: Comprobar antes que el prefered tenga capacidad previa

                var tryPrefered = this.simulator.Simulation.Data.Stage
                .Where(st => st.Zone.AreaId == process.Process.Area.Id && (isOut ? st.IsOut : st.IsIn))
                .Join(
                    this.simulator.Simulation.Resources.Values
                        .Where(r => r.Type == ResourceType.Zone
                                    && r.ZoneType == ZoneType.Stage
                                    && r.AssociatedDocks.Contains(preferedDockId.Value)),
                    st => st.ZoneId,
                    r => r.Id,
                    (st, r) => new Resource
                    {
                        Id = r.Id,
                        Type = r.Type,
                        InUse = r.InUse,
                        Capacity = r.Capacity,
                        ZoneType = r.ZoneType,
                        MaxContainers = r.MaxContainers,
                        CurrentContainers = r.CurrentContainers,
                        AssociatedDocks = r.AssociatedDocks
                    });

                if (tryPrefered.Any())
                    return tryPrefered;

                var stages = this.simulator.Simulation.Data.Stage
                    .Where(st => st.Zone.AreaId == process.Process.Area.Id && (isOut ? st.IsOut : st.IsIn))
                    .Join(
                        this.simulator.Simulation.Resources.Values
                            .Where(r => r.Type == ResourceType.Zone
                                        && r.ZoneType == ZoneType.Stage),
                        st => st.ZoneId,
                        r => r.Id,
                        (st, r) => new Resource
                        {
                            Id = r.Id,
                            Type = r.Type,
                            InUse = r.InUse,
                            Capacity = r.Capacity,
                            ZoneType = r.ZoneType,
                            MaxContainers = r.MaxContainers,
                            CurrentContainers = r.CurrentContainers,
                            AssociatedDocks = r.AssociatedDocks
                        });

                if (appointment.PossibleDockIds == null) return stages;
                else return stages.Where(stage => stage.AssociatedDocks.Any(dockId => appointment.PossibleDockIds.Contains(dockId)));
            }
            else
            {
                var stages = this.simulator.Simulation.Data.Stage
                    .Where(st => st.Zone.AreaId == process.Process.Area.Id && (isOut ? st.IsOut : st.IsIn))
                    .Join(
                        this.simulator.Simulation.Resources.Values
                            .Where(r => r.Type == ResourceType.Zone
                                        && r.ZoneType == ZoneType.Stage),
                        st => st.ZoneId,
                        r => r.Id,
                        (st, r) => new Resource
                        {
                            Id = r.Id,
                            Type = r.Type,
                            InUse = r.InUse,
                            Capacity = r.Capacity,
                            ZoneType = r.ZoneType,
                            MaxContainers = r.MaxContainers,
                            CurrentContainers = r.CurrentContainers,
                            AssociatedDocks = r.AssociatedDocks
                        });

                if (appointment.PossibleDockIds == null) return stages;
                else return stages.Where(stage => stage.AssociatedDocks.Any(dockId => appointment.PossibleDockIds.Contains(dockId)));
            }
        }
        #endregion

        #region Equipments

        /// <summary>
        /// Gets the equipment options for the process
        /// </summary>
        /// <param name="process">Process that needs the equipment</param>
        /// <returns>Enumeration of the possible equipments</returns>
        private IEnumerable<Resource> GetEquipments(Grouping process)
        {
            return this.simulator.Simulation.Data.EquipmentGroup.Where(x => x.AreaId == process.Process.Area.Id)
                .Join(this.simulator.Simulation.Resources.Values.Where(x => x.Type == ResourceType.Equipment), so => so.Id, r => r.Id, (so, r) => new Resource
                {
                    Id = r.Id,
                    Type = r.Type,
                    InUse = r.InUse,
                    Capacity = r.Capacity,
                    ZoneType = r.ZoneType
                });
        }

        #endregion

        /// <summary>
        /// Queue processes that can not select a resource
        /// </summary>
        /// <param name="process">Process to queue</param>
        private void QueueProcess(Grouping process)
        {
            this.simulator.Simulation.WorkingProcesses.Remove(process);
            process.WaitingForResource = this.simulator.Simulation.Environment.Now;
            this.simulator.Simulation.QueuedProcesses.Add(process);
        }

        #endregion

        #endregion
    }
}
