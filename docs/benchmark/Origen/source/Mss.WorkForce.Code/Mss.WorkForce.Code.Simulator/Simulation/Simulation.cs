using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Simulation.PostSimulation;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.DataOrganization;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.Processes;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.TimeAssignment;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.WhatIf;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.WRP;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Appointments;
using System.Diagnostics;
using Deliveries = Mss.WorkForce.Code.Simulator.Simulation.Simulator.Deliveries.Deliveries;
using Warehouse = Mss.WorkForce.Code.Simulator.Core.Layout.Warehouse;

namespace Mss.WorkForce.Code.Simulator.Simulation
{

    /// <summary>
    /// Contains the simulation schema.
    /// </summary>
    public class Simulation : IDisposable
    {
        #region Properties
        public SimSharp.Simulation Environment { get; set; }
        public DateTime StartSimulationDate { get; set; }
        public DataSimulatorTablaRequest Data { get; set; }
        public Warehouse Warehouse { get; set; }
        public HashSet<Grouping> Processes { get; set; }
        public HashSet<Grouping> ActiveProcesses { get; set; }
        public HashSet<Grouping> PausedProcesses { get; set; }
        public HashSet<Grouping> DoneProcesses { get; set; }
        public HashSet<Grouping> WorkingProcesses { get; set; }
        public HashSet<Grouping> QueuedProcesses { get; set; }
        public PlanningReturn PlanningReturn { get; set; }
        public DockSelectionStrategy InboundDockSelectionStrategy { get; set; }
        public DockSelectionStrategy OutboundDockSelectionStrategy { get; set; }
        public List<(Guid WorkOrderPlanningId, Guid ItemPlanningId, double Duration)> LoadingDuration { get; set; }
        public Dictionary<Guid, Resource> Resources { get; set; }
        public List<SimulationLogCheck> LogChecks { get; set; }
        internal class CustomClosed
        {
            internal string Type { get; set; }
            internal int Total { get; set; }
        }
        #endregion

        #region Constructor
        public Simulation(DataSimulatorTablaRequest data)
        {
            this.Data = data;
            var minShift = data.Shift.Any() ? data.Shift.Min(x => x.InitHour) : 0;
            this.StartSimulationDate = DateTime.SpecifyKind(data.Date.GetValueOrDefault(DateTime.UtcNow.Date.AddHours(minShift)), DateTimeKind.Utc);
            this.Environment = new SimSharp.Simulation(this.StartSimulationDate);
            this.LoadingDuration = new();
            this.Resources = new Dictionary<Guid, Resource>();
            this.Processes = new HashSet<Grouping>();
            this.LogChecks = new List<SimulationLogCheck>();
            //this.Iterations = 0;
            //this.broken = false;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Fires the simulation.
        /// </summary>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        public void Simulate(bool whatIf)
        {
            try
            {
                //this.Data.InputOrder = this.Data.InputOrder.Where(x => !string.IsNullOrEmpty(x.OrderCode)).ToList();

                Random rand = new Random();
                Stopwatch sw = new Stopwatch();

                //total.Start();
                Console.WriteLine($"--------------- Simulation started: {this.Data.Warehouse.Name} ---------------");

                // Reorganiza los datos por almacén para poder simular
                sw.Restart();
                DataGenerator dataGenerator = new DataGenerator(this.Data, StartSimulationDate);
                this.Warehouse = dataGenerator.WarehousesGenerator(rand);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Data reorganization OK. {sw.ElapsedMilliseconds} ms.");

                // Agrupa las citas y las asigna para cada una de sus órdenes
                sw.Restart();
                this.Warehouse.Appointments = Appointment.Generate(this.Data);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Appointments calculations OK. {sw.ElapsedMilliseconds} ms");

                // Genera la lista de deliveries de las órdenes, pudiendo estar vacía si no tenemos empaquetado
                sw.Restart();
                this.Warehouse.Deliveries = Deliveries.CreateGroupingDelivery(this.Data);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Deliveries calculations OK. {sw.ElapsedMilliseconds} ms");

                // Genera los procesos por líneas
                sw.Restart();
                this.Warehouse.OrderLines = new ProcessGenerator(this.Data).ProcessAssignment(this.Warehouse.OrderLines, rand, this.Warehouse);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Processes generated OK. {sw.ElapsedMilliseconds} ms.");

                // Agrupa los procesos de las ordenes
                sw.Restart();
                DoGrouping doGrouping = new DoGrouping(this);
                this.Warehouse = doGrouping.GetGroupings();
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Processes grouping OK. {sw.ElapsedMilliseconds} ms.");

                // Asigna momentos temporales a las agrupaciones
                sw.Restart();
                DoPlanning doPlanning = new DoPlanning(this.Warehouse, this.Data, this.StartSimulationDate);
                this.Warehouse = doPlanning.MakePlanner();
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Time periods assignment OK. {sw.ElapsedMilliseconds} ms.");

                // Calcula lo necesario para el step-back del WRP
                sw.Restart();
                WRP wrp = new WRP(this.Warehouse, this.Data);
                this.Warehouse = wrp.CalculateLimitDate(0.2);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] WRP step-back OK. {sw.ElapsedMilliseconds} ms.");

                // Crea los recursos de SimSharp necesarios
                sw.Restart();
                CreateResources createResources = new CreateResources(this.Environment.StartDate, this.Data, this);
                createResources.AssignResources(whatIf);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] SimSharp resources OK. {sw.ElapsedMilliseconds} ms.");

                // Selecciona la estrategia que se seguirá para la elección de Docks
                sw.Restart();
                SelectDockStrategies(this.Data);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Selected dock strategies. {sw.ElapsedMilliseconds} ms.");

                // Ajusta para los procesos cerrados y añade los procesos de almacén
                sw.Restart();
                ProcessesOrganization processesOrganization = new ProcessesOrganization(this);
                processesOrganization.AdjustPendingProcesses();
                processesOrganization.AddOrderProcesses(doPlanning, rand);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Adjusted processes. {sw.ElapsedMilliseconds} ms.");

                // Divide la lista general de procesos en las pequeñas listas que usamos para simular
                sw.Restart();
                DivideProcesses();
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Divided processes. {sw.ElapsedMilliseconds} ms.");

                // Calcula y crea los trabajadores mínimos necesarios para simular el WhatIf
                if (whatIf)
                {
                    sw.Restart();
                    this.Resources = WorkersNumber.CreateWorkers(this.Data, this.Resources, this.Processes);
                    sw.Stop();
                    Console.WriteLine($"[{this.Data.Warehouse.Name}] WhatIf workers calculated and created. {sw.ElapsedMilliseconds} ms.");
                }

                // Ejecución de la simulación
                sw.Restart();
                new Simulator.Simulator(this).Simulate(whatIf);
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Simulator ended. {sw.ElapsedMilliseconds} ms.");

                // Añade los procesos cerrados y las órdenes fake cerradas
                sw.Restart();
                Recalculate recalculate = new Recalculate(this);
                recalculate.AddClosedProcessesToPlanning();
                recalculate.FakeWorkOrders();
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Adjusted recalculate processes. {sw.ElapsedMilliseconds} ms.");

                // Calcula el IsOnTime de las órdenes y vehículos
                sw.Restart();
                new IsOnTime(this).Check();
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Calculated IsOnTime and IsVehicleOnTime. {sw.ElapsedMilliseconds} ms.");

                // Calcula el SLA y el OrderDelay
                sw.Restart();
                SLA sla = new SLA(this);
                sla.CalculateSLAWorkOrdersOnTimePercentage();
                sla.CalculateSLATargetAndOrderDelay();
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Calculated SLA. {sw.ElapsedMilliseconds} ms.");

                // Añade los shifts de los operarios para los ItemPlannings
                sw.Restart();
                ItemPlanningShifts its = new ItemPlanningShifts(this);
                its.CalculateShiftsForItemPlannings();
                sw.Stop();
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Calculated Shifts per ItemPlannings. {sw.ElapsedMilliseconds} ms.");

                if (whatIf && this.PlanningReturn.WorkOrderPlanning.All(x => x.IsOnTime)) Console.WriteLine("WHATIF: ALL ORDERS ON TIME");
                if (whatIf && this.PlanningReturn.WorkOrderPlanning.All(x => x.IsInVehicleTime)) Console.WriteLine("WHATIF: ALL VEHICLES ON TIME");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{this.Data.Warehouse.Name}] Error while simulating. {ex.Message}");
                Console.WriteLine(ex.StackTrace.ToString());
            }
        }

        /// <summary>
        /// Selects the strategies for the different flows
        /// </summary>
        /// <param name="data">Simulation data</param>
        private void SelectDockStrategies(DataSimulatorTablaRequest data)
        {
            var inboundFlowGraph = data.InboundFlowGraph.FirstOrDefault(x => x.WarehouseId == data.Warehouse.Id);
            var inboundDockSelection = data.DockSelectionStrategy.FirstOrDefault(x => x.Id == inboundFlowGraph?.DockSelectionStrategyId);
            this.InboundDockSelectionStrategy = inboundDockSelection ?? data.DockSelectionStrategy.FirstOrDefault(x => x.OrganizationId == data.Warehouse.OrganizationId)!;

            var outboundFlowGraph = data.OutboundFlowGraph.FirstOrDefault(x => x.WarehouseId == data.Warehouse.Id);
            var outboundDockSelection = data.DockSelectionStrategy.FirstOrDefault(x => x.Id == outboundFlowGraph?.DockSelectionStrategyId);
            this.OutboundDockSelectionStrategy = outboundDockSelection ?? data.DockSelectionStrategy.FirstOrDefault(x => x.OrganizationId == data.Warehouse.OrganizationId)!;
        }

        /// <summary>
        /// Divides all the processes in the different lists
        /// </summary>
        private void DivideProcesses()
        {
            this.ActiveProcesses = this.Processes.Where(x => !x.IsPaused).ToHashSet();
            this.PausedProcesses = this.Processes.Where(x => x.IsPaused).ToHashSet();
            this.DoneProcesses = new HashSet<Grouping>();
            this.WorkingProcesses = new HashSet<Grouping>();
            this.QueuedProcesses = new HashSet<Grouping>();
        }

        #endregion

        #region Disposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                this.Environment = null;
                this.Data = null;
                this.StartSimulationDate = default;
                this.Warehouse = null;
                this.Processes = null;
                this.PlanningReturn = null;
                this.InboundDockSelectionStrategy = null;
                this.OutboundDockSelectionStrategy = null;
                this.DoneProcesses = null;
                this.QueuedProcesses = null;
                this.PausedProcesses = null;
                this.WorkingProcesses = null;
                this.LoadingDuration = null;
                this.Resources = null;
                this.LogChecks = null;
                this.ActiveProcesses = null;
            }

            _disposed = true;
        }

        ~Simulation()
        {
            Dispose(false);
        }
        #endregion
    }
}
