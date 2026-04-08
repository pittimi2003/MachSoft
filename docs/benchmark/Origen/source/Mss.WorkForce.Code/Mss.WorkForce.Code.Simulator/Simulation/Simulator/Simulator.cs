using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Helper.Checker;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Appointments;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Processes;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator
{
    public class Simulator
    {
        #region Variables
        internal Simulation Simulation { get; set; }
        internal int? LastInboundDock { get; set; }
        internal int? LastOutboundDock { get; set; }
        internal Dictionary<(Guid, Guid), double> routeCache { get; set; }
        internal ResourceAssignment resourceAssignment { get; set; }
        internal ModifyProcesses modifyProcess { get; set; }
        internal ModifyAppointments modifyAppointments { get; set; }
        internal bool pausedByEquipments { get; set; }
        internal bool pausedByWorkers { get; set; }
        internal bool pausedByZoneContainers { get; set; }
        internal bool pausedByZoneEquipments { get; set; }
        internal class CustomShift
        {
            internal DateTime InitDate { get; set; }
            internal DateTime EndDate { get; set; }
        }
        #endregion

        #region Constructor
        internal Simulator(Simulation simulation)
        {
            this.Simulation = simulation;
            this.routeCache = new Dictionary<(Guid, Guid), double>();
            this.resourceAssignment = new ResourceAssignment(this);
            this.modifyProcess = new ModifyProcesses(this);
            this.modifyAppointments = new ModifyAppointments(this);
            this.pausedByEquipments = false;
            this.pausedByWorkers = false;
            this.pausedByZoneContainers = false;
            this.pausedByZoneEquipments = false;
        }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Starts the simulation for evaluate the selection of the genetic algorithm
        /// </summary>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        public void Simulate(bool whatIf)
        {
            // Crea un PlanningReturn en cada simulación de almacén
            this.Simulation.PlanningReturn = PlanningData.InitialCreate(this.Simulation, this.Simulation.Warehouse);
            
            this.Simulation.Environment.Process(SimulationSchema(whatIf));
            this.Simulation.Environment.Run();

            // Se asigna los valores necesarios al WorkOrderPlanning en función de los procesos que contenga
            this.Simulation.PlanningReturn = PlanningData.CompleteDatesAndWorkTimeForWorkOrders(this.Simulation.PlanningReturn);
        }

        #endregion

        #region Private

        /// <summary>
        /// Creates the schema of the processes simulation
        /// </summary>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        /// <returns>Yields a SimSharp event</returns>
        private IEnumerable<SimSharp.Event> SimulationSchema(bool whatIf)
        {
            if (!this.Simulation.Data.ProcessPriorityOrder.Any()) Console.WriteLine($"Warning. No priority order for this warehouse configuration. Done default ordering.");

            DateTime? lastStep = null;
            bool drainEventQueue = false;
            var orderedPriorities = this.Simulation.Data.ProcessPriorityOrder.OrderBy(x => x.Priority);
            var processes = this.Simulation.ActiveProcesses.Where(x => x.StartWorkingDate <= this.Simulation.Environment.Now).OrderBy(x => x.EstimatedPriority);
            var shifts = this.Simulation.Data.Shift.Select(x => new CustomShift
            {
                InitDate = this.Simulation.Environment.Now.Date.Add(TimeSpan.FromHours(x.InitHour)),
                EndDate = this.Simulation.Environment.Now.Date.Add(TimeSpan.FromHours(x.EndHour))
            });

            while (true)
            {
                if (drainEventQueue)
                {
                    drainEventQueue = false;
                    yield return this.Simulation.Environment.Timeout(TimeSpan.Zero);
                }
                //this.Simulation.Iterations++;

                if (lastStep == null || lastStep < this.Simulation.Environment.Now)
                {
                    CleanQueueForResource();

                    Priority.SetHurryUpPriority(this.Simulation);
                    Priority.SetNoOnTimePriority(this.Simulation);

                    processes = this.Simulation.ActiveProcesses.Where(x => x.StartWorkingDate <= this.Simulation.Environment.Now).OrderBy(x => x.EstimatedPriority);

                    if (orderedPriorities.Any()) foreach (var priority in orderedPriorities) processes = OrderProcesses.Dinamic(processes, priority.Code, this.Simulation);
                    else processes = OrderProcesses.Static(processes, this.Simulation);

                    lastStep = this.Simulation.Environment.Now;
                }

                var process = processes.FirstOrDefault();
                if (process != null)
                {
                    this.Simulation.ActiveProcesses.Remove(process);
                    this.Simulation.WorkingProcesses.Add(process);

                    if (resourceAssignment.TryGetEquipment(process) && resourceAssignment.TryGetZones(process) && resourceAssignment.TryGetWorker(process))
                    {
                        resourceAssignment.AssignCapacity(process);
                        modifyAppointments.AddDockToAppointment(process);
                        modifyProcess.StartWorkingInProcess(process);

                        if (WorkInProcess(process, whatIf)) yield return this.Simulation.Environment.Timeout(TimeSpan.Zero);
                        else Console.WriteLine($"{this.Simulation.Environment.Now} - Not available resources for the warehouse: {this.Simulation.Warehouse.Name}");
                    }
                    else
                    {
                        modifyProcess.QueueProcesses(process);
                        CleanPauseFlags();
                    }
                }
                else
                {
                    drainEventQueue = true;
                    if (this.Simulation.Processes.Count == this.Simulation.DoneProcesses.Count) 
                    {
                        this.Simulation.LogChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ResultOk.ToString(), Log = "Finished OK." });
                        break;
                    }
                    else
                    {
                        var timeOut = CalculateTimeout(shifts);

                        if (timeOut == null)
                        {
                            var message = new BadSimulationChecker(this.Simulation).Check();
                            Console.WriteLine(message);
                            break;
                        }
                        else yield return this.Simulation.Environment.Timeout(timeOut.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates timeout for the simulation
        /// </summary>
        /// <param name="shifts">Shifts with the actual simulation date</param>
        /// <returns>Returns a timespan with the seconds neccesaries to yield</returns>
        private TimeSpan? CalculateTimeout(IEnumerable<CustomShift> shifts)
        {
            var peek = this.Simulation.Environment.Peek();
            var now = this.Simulation.Environment.Now;

            if (now >= this.Simulation.StartSimulationDate.Date.AddDays(2).AddSeconds(-1))
                return null; // señal de ruptura del while

            var intermediateProcesses = this.Simulation.ActiveProcesses.Where(x => x.StartWorkingDate > now && x.StartWorkingDate < peek);

            var nextShift = shifts.Any(x => x.InitDate > now && x.InitDate < peek)
                ? shifts.Where(x => x.InitDate > now && x.InitDate < peek).Min(x => x.InitDate)
                : shifts.Min(x => x.InitDate).AddDays(1);

            // Caso peek distinto de MaxValue
            if (peek != DateTime.MaxValue)
            {
                if (intermediateProcesses.Any())
                    return MinTime(
                        intermediateProcesses.Min(x => x.StartWorkingDate) - now,
                        nextShift - now);

                if (nextShift < peek)
                    return nextShift - now;

                return peek == now ? TimeSpan.Zero : peek - now;
            }

            // Caso peek == DateTime.MaxValue, no hay nada en la cola DES
            if (intermediateProcesses.Any())
                return MinTime(
                    intermediateProcesses.Min(x => x.StartWorkingDate) - now,
                    nextShift - now);

            return nextShift - now;
        }

        /// <summary>
        /// Returns the minimum TimeSpan from two given
        /// </summary>
        /// <param name="a">Timespan a</param>
        /// <param name="b">Timespan b</param>
        /// <returns>Returns the minimum TimeSpan from two given</returns>
        private static TimeSpan MinTime(TimeSpan a, TimeSpan b) => a > b ? b : a;

        /// <summary>
        /// Sets all the flags to false after pausing the processes for a specific reason
        /// </summary>
        private void CleanPauseFlags()
        {
            this.pausedByEquipments = false;
            this.pausedByWorkers = false;
            this.pausedByZoneContainers = false;
            this.pausedByZoneEquipments = false;
        }

        /// <summary>
        /// Cleans the queue for processes if the moment has passed
        /// </summary>
        private void CleanQueueForResource()
        {
            foreach (var p in this.Simulation.QueuedProcesses.Where(p => p.WaitingForResource < this.Simulation.Environment.Now).ToHashSet())
            {
                this.Simulation.QueuedProcesses.Remove(p);
                p.WaitingForResource = null;
                this.Simulation.ActiveProcesses.Add(p);
            }
        }

        /// <summary>
        /// Simulates the work in a process, assigning the necessary resources.
        /// </summary>
        /// <param name="process">Process to work in</param>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        /// <returns>Boolean. True if exists workers, false if does not</returns>
        private bool WorkInProcess(Grouping process, bool whatIf)
        {
            try
            {
                this.Simulation.Environment.Process(RequestResource(process, whatIf));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Request the availability of the assigned resource, or stays in queue until the resource gets released.
        /// </summary>
        /// <param name="process">Process to start working in</param>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        /// <returns>Yields a SimSharp event</returns>
        private IEnumerable<SimSharp.Event> RequestResource(Grouping process, bool whatIf)
        {
            if (process.AssignedWorker.CurrentArea == null)
                this.Simulation.Resources[process.AssignedWorker.Id].CurrentArea = process.Process.Area;

            double? routeTime = 0;
            if (this.Simulation.Resources[process.AssignedWorker.Id].CurrentArea.Id != process.Process.Area.Id)
            {
                routeTime = GetRouteTime(process.AssignedWorker.CurrentArea.Id, process.Process.Area.Id);
                if (routeTime > 0) yield return this.Simulation.Environment.Timeout(TimeSpan.FromSeconds(routeTime.GetValueOrDefault(0)));
                this.Simulation.Resources[process.AssignedWorker.Id].CurrentArea = process.Process.Area;
            }

            yield return this.Simulation.Environment.Process(new ProcessDuration(this).Calculate(process.AssignedWorker, process.AssignedEquipment, process, whatIf));

            if (process.Process.ProcessType == ProcessType.Loading)
                this.Simulation.LoadingDuration.Add((process.ItemPlanningReturn.WorkOrderPlanningId, process.ItemPlanningReturn.Id, routeTime.GetValueOrDefault(0)));

            if (process.SelectedStation.Type == ZoneType.Stage) 
                process.ItemPlanningReturn.StageId = this.Simulation.Data.Stage.FirstOrDefault(x => x.ZoneId == process.SelectedStation.Id).Id;

            resourceAssignment.ReleaseResources(process);
            modifyProcess.CloseProcess(process);
            modifyAppointments.AddWorkersToAppointment(process);
            resourceAssignment.TryFreeDockFromVehicle(process);
        }

        /// <summary>
        /// Gets and caches the times for the used routes.
        /// </summary>
        /// <param name="from">Id of the area where the route starts</param>
        /// <param name="to">Id of the area where the route ends</param>
        /// <returns>The time of the route from an area to another</returns>
        private double GetRouteTime(Guid from, Guid to)
        {
            if (routeCache.TryGetValue((from, to), out var time))
                return time;
            time = this.Simulation.Warehouse.Routes
                .FirstOrDefault(x => x.DepartureAreaId == from && x.ArrivalAreaId == to)?.Time ?? 0;
            routeCache[(from, to)] = time;
            return time;
        }

        #endregion

        #endregion
    }
}
