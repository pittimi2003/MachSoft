using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Core.Orders;
using Mss.WorkForce.Code.Simulator.Helper;
using Process = Mss.WorkForce.Code.Simulator.Core.Layout.Process;
using Warehouse = Mss.WorkForce.Code.Simulator.Core.Layout.Warehouse;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.DataOrganization
{
    public class ProcessGenerator
    {
        #region Properties
        private DataSimulatorTablaRequest Data;
        #endregion

        #region Constructor
        public ProcessGenerator(DataSimulatorTablaRequest data)
        {
            this.Data = data;
        }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Assigns the sequence of processes for every order line of the warehouse.
        /// </summary>
        /// <param name="orderLines">List of order lines assigned to the warehouse</param>
        /// <param name="rand">Instance of Random</param>
        /// <param name="warehouse">Data created for the warehouse to simulate</param>
        /// <returns>List of order lines with the processes sequences assigned.</returns>
        public List<OrderLine> ProcessAssignment(List<OrderLine> orderLines, Random rand, Warehouse warehouse)
        {
            foreach (var a in warehouse.Appointments)
            {
                Guid? stageAreaId = null;
                List<Models.Models.Process> ableEnds = this.Data.ProcessDirectionProperty.Where(x => x.EndProcess.IsOut && x.IsEnd).Select(x => x.EndProcess).ToList();

                if (a.IsOut && a.AssignedDockId != null && ableEnds.Any(x => x.Area.Type == AreaType.Dock))
                {
                    var areaId = this.Data.Dock.FirstOrDefault(x => x.Id == a.AssignedDockId)?.Zone.AreaId;
                    ableEnds = ableEnds.Where(x => x.Area.Type != AreaType.Dock).ToList();
                    var possibleProcesses = this.Data.Process.Where(x => x.AreaId == areaId && x.IsOut);
                    ableEnds.AddRange(possibleProcesses);
                }
                
                if (!a.IsOut && a.AssignedDockId != null)
                {
                    var dockAreaId = this.Data.Dock.FirstOrDefault(x => x.Id == a.AssignedDockId).Zone.AreaId;
                    a.SelectedDockProcessId = this.Data.Process.FirstOrDefault(x => x.AreaId == dockAreaId && x.Type == ProcessType.Inbound).Id;
                }

                foreach (var o in a.InputOrders)
                {
                    foreach (var l in orderLines.Where(x => x.OrderId == o.Id))
                    {
                        if (a.SelectedDockProcessId == null) // Esto es lo mismo que no tener dock asignado, porque si lo tengo esto se rellena
                        {
                            // Si no tengo Dock, hago un camino normal para ambos casos
                            if (o.IsOutbound) l.Processes = OutboundFlow(l, this.Data.ProcessDirectionProperty, rand);
                            else l.Processes = InboundFlow(l, rand, a.SelectedDockProcessId, stageAreaId, null);

                            a.SelectedDockProcessId = l.IsOutbound
                                ? (l.Processes.Last().Area.Type == AreaType.Dock ? l.Processes.Last().Id : null)
                                : l.Processes.First().Id;

                            if (l.IsOutbound && a.SelectedDockProcessId != null)
                            {
                                ableEnds = ableEnds.Where(x => x.Area.Type != AreaType.Dock).ToList();
                                var possibleProcesses = this.Data.Process.FirstOrDefault(x => x.Id == a.SelectedDockProcessId);
                                if (possibleProcesses != null) ableEnds.Add(possibleProcesses);
                            }

                            stageAreaId = l.Processes.FirstOrDefault(x => x.Area.Type == AreaType.Stage)?.Area.Id;
                            // En función del área de stage seleccionado, ponemos los possible docks para la cita de entrada
                            if (a.PossibleDockIds == null && !a.IsOut)
                                a.PossibleDockIds = stageAreaId == null ? null : this.Data.Stage.Where(x => x.Zone.AreaId == stageAreaId.Value)
                                                                                    .Join(this.Data.AvailableDocksPerStage, s => s.Id, dps => dps.StageId,
                                                                                    (s, dps) => dps.DockId)
                                                                                    .Join(this.Data.Dock, ds => ds, d => d.Id, (ds, d) => d.ZoneId);
                        }
                        else // Si tengo un SelectedDockProcess es bien o que tengo un assignedDock o que ya he simulado una primera vez y tengo un punto de partida
                        {
                            if (o.IsOutbound)
                            {
                                // Si es de salida, tengo que ver a que area de docks llega y hacer un camino viable para que siempre desemboque en él.
                                // Si además por el camino tengo stages, solo puedo pasar por áreas en las que tengan stages con docks asociados a los possible docks.
                                // Esos docks posibles vendrán dados por el área de stage que haya pasado de primeras, es decir, si paso por el área 1, solo tendré como
                                // docks disponibles el Dock1 y el Dock2, y por tanto solo podré seleccionar áreas que tengan esos docks asociados a sus zonas

                                // Si tengo un assigned dock es más sencillo, pues el problema se resume a mirar los stages asociados a ese dock

                                if (a.AssignedDockId == null)
                                {
                                    // Metemos aquí la condición where porque estamos suponiendo que solo van a ir antes de los docks, en ningún otro lado
                                    var processDirectionPropertiesFromDock = this.Data.ProcessDirectionProperty
                                        .Where(x => ableEnds.Select(x => x.Id).Contains(x.EndProcessId) && x.IsEnd && (stageAreaId != null ? x.InitProcess.AreaId == stageAreaId : true));
                                    var processDirectionProperties = GetCustomProcessDirectionProperties(processDirectionPropertiesFromDock);
                                    l.Processes = OutboundFlow(l, processDirectionProperties, rand);
                                }
                                else
                                {
                                    // Si el assignedDock es no nulo, sacamos un camino desde la zona dock, pudiendo tener varios stages

                                    // Si conozco el dock y no tengo stages, solo tengo que sacar el camino desde ese área de docks
                                    // Si conozco el dock y tengo stages, las areas de stages que podemos seleccionar ya no será solo por la primera que haya pasado, si no
                                    // que serán todas aquellas que tengan al menos un stage relacionado con el dock asignado

                                    var processDirectionPropertiesFromDock = this.Data.ProcessDirectionProperty
                                        .Where(x => ableEnds.Select(x => x.Id).Contains(x.EndProcessId) && x.IsEnd);
                                    if (stageAreaId == null)
                                    {
                                        var availableStagesAreas = this.Data.AvailableDocksPerStage.Where(x => x.DockId == a.AssignedDockId)
                                            .Join(this.Data.Stage, @as => @as.StageId, s => s.Id, (@as, s) => s.Zone.AreaId);

                                        if (availableStagesAreas.Any())
                                        {
                                            var processDirectionPropertiesFromAssignedDock = processDirectionPropertiesFromDock
                                                .Where(x => availableStagesAreas.Contains(x.InitProcess.AreaId));
                                            var processDirectionProperties = GetCustomProcessDirectionProperties(processDirectionPropertiesFromAssignedDock);
                                            l.Processes = OutboundFlow(l, processDirectionProperties, rand);
                                        }
                                        else
                                        {
                                            var processDirectionProperties = GetCustomProcessDirectionProperties(processDirectionPropertiesFromDock);
                                            l.Processes = OutboundFlow(l, processDirectionProperties, rand);
                                        }
                                    }
                                    else
                                    {
                                        var processDirectionProperties = GetCustomProcessDirectionProperties(processDirectionPropertiesFromDock);
                                        l.Processes = OutboundFlow(l, processDirectionProperties, rand);
                                    }
                                }
                            }
                            else
                            {
                                // Si es de entrada, tengo que ver desde que area de docks llega y hacer un camino viable.
                                // Si tengo stages, tengo que seleccionar procesos de áreas de stage que tengan stages asociados a ese área de docks.

                                // Si tengo un assigned dock es más sencillo, pues el problema se resume a mirar los stages asociados a ese dock

                                // Si el assignedDock es nulo, tenemos que buscar los caminos posibles teniendo en cuenta el SelectDockProcessId y el stageArea en caso de stages
                                if (a.AssignedDockId == null) l.Processes = InboundFlow(l, rand, a.SelectedDockProcessId, stageAreaId, null);
                                else
                                {
                                    // Si el assignedDock es no nulo, sacamos un camino desde la zona dock, pudiendo tener varios stages

                                    if (stageAreaId == null)
                                    {
                                        // Si tenemos dock, tenemos que mirar que las áreas seleccionadas sean aquellas
                                        var availableStagesAreas = this.Data.AvailableDocksPerStage.Where(x => x.DockId == a.AssignedDockId)
                                            .Join(this.Data.Stage, @as => @as.StageId, s => s.Id, (@as, s) => s.Zone.AreaId);

                                        l.Processes = InboundFlow(l, rand, a.SelectedDockProcessId, null, availableStagesAreas);
                                    }
                                    else l.Processes = InboundFlow(l, rand, a.SelectedDockProcessId, stageAreaId, null);
                                }
                            }
                        }
                    }
                }
            }

            return orderLines;
        }

        #endregion

        #region Private

        /// <summary>
        /// Gets the custom process direction properties to end in a determined process or area in order to fit the appointment dock selection
        /// </summary>
        /// <param name="processDirectionPropertiesFromDock">Reference processes for the area</param>
        /// <returns>List of the custom process direction properties</returns>
        private List<ProcessDirectionProperty> GetCustomProcessDirectionProperties(IEnumerable<ProcessDirectionProperty> processDirectionPropertiesFromDock)
        {
            List<ProcessDirectionProperty> processDirectionProperties = new List<ProcessDirectionProperty>();
            processDirectionProperties.AddRange(processDirectionPropertiesFromDock);
            List<ProcessDirectionProperty> lastProcessesForPath = new List<ProcessDirectionProperty>();
            lastProcessesForPath.AddRange(processDirectionPropertiesFromDock);

            while (lastProcessesForPath.Any())
            {
                var iteration = lastProcessesForPath.Select(x => x.InitProcessId);
                foreach (var lastProcessId in iteration)
                {
                    var newProcesses = this.Data.ProcessDirectionProperty.Where(x => x.EndProcessId == lastProcessId).ToList();
                    processDirectionProperties.AddRange(newProcesses);
                    lastProcessesForPath = newProcesses.ToList();
                }
            }

            return processDirectionProperties.Distinct().ToList();
        }

        /// <summary>
        /// Assigns the processes sequence to an inbound line.
        /// </summary>
        /// <param name="line">Order line to assign the processes sequence.</param>
        /// <param name="rand">Instance of Random</param>
        /// <param name="initProcessId">Id of the first process to start the flow</param>
        /// <param name="stageAreaId">Area of stages to go through in case of not having an assigned dock</param>
        /// <param name="stageAreasId">Areas that has a stage related to the assigned dock</param>
        /// <returns>List of processes.</returns>
        private List<Process> InboundFlow(OrderLine line, Random rand, Guid? initProcessId, Guid? stageAreaId, IEnumerable<Guid>? stageAreasId)
        {
            List<Process> processes = new List<Process>();
            bool isFirstProcess = true;
            bool isEnd = false;
            Models.Models.Process? nextProcess = null;

            while (true)
            {
                if (isFirstProcess)
                {
                    var possibleProcess = initProcessId.HasValue 
                        ? this.Data.Process.FirstOrDefault(x => x.Id == initProcessId)
                        : RandomSelector.SelectInitProcess(this.Data.Process.Where(x => x.IsInitProcess && x.IsIn).ToList(), this.Data, rand);

                    List<ProcessDirectionProperty> nextProcesses = new List<ProcessDirectionProperty>();

                    if (stageAreasId == null || !stageAreasId.Any())
                        nextProcesses = this.Data.ProcessDirectionProperty.Where(x => x.InitProcessId == possibleProcess.Id)
                            .Where(x => stageAreaId != null ? x.EndProcess.AreaId == stageAreaId : true).ToList();
                    else
                        nextProcesses = this.Data.ProcessDirectionProperty.Where(x => x.InitProcessId == possibleProcess.Id)
                            .Where(x => stageAreasId.Contains(x.EndProcess.AreaId)).ToList();

                    nextProcess = RandomSelector.SelectNextProcess(nextProcesses, this.Data, rand);
                    isEnd = this.Data.ProcessDirectionProperty.FirstOrDefault(x => x.InitProcessId == possibleProcess.Id && x.EndProcessId == nextProcess.Id).IsEnd;

                    processes.Add(new Process(possibleProcess.Id, possibleProcess.Name, possibleProcess.Area, possibleProcess.Type,
                        possibleProcess.IsInitProcess, possibleProcess.IsWarehouseProcess, line.Containers));

                    isFirstProcess = false;
                }
                else
                {
                    if (isEnd)
                    {
                        var area = nextProcess.Area;
                        var code = nextProcess.Name;
                        var processType = nextProcess.Type;
                        var isInitProcess = nextProcess.IsInitProcess;
                        var isWarehouseProcess = nextProcess.IsWarehouseProcess;

                        processes.Add(new Process(nextProcess.Id, code, area, processType, isInitProcess, isWarehouseProcess, line.Containers));
                        break;
                    }
                    else
                    {
                        var area = nextProcess.Area;
                        var code = nextProcess.Name;
                        var processType = nextProcess.Type;
                        var isInitProcess = nextProcess.IsInitProcess;
                        var isWarehouseProcess = nextProcess.IsWarehouseProcess;

                        processes.Add(new Process(nextProcess.Id, code, area, processType, isInitProcess, isWarehouseProcess, line.Containers));

                        try
                        {
                            isEnd = this.Data.ProcessDirectionProperty.FirstOrDefault(x => x.InitProcessId == nextProcess.Id).IsEnd;
                        }
                        catch
                        {
                            Console.WriteLine("WARNING. Error in processes configuration. Not an end process found for inbound flow.");
                            break;
                        }

                        nextProcess = RandomSelector.SelectNextProcess(this.Data.ProcessDirectionProperty.Where(x => x.InitProcessId == nextProcess.Id).ToList(), this.Data, rand);
                    }
                }
            }

            processes = AssignPreviousProcesses(processes);

            return processes;
        }

        /// <summary>
        /// Assigns the processes sequence to an outbound line.
        /// </summary>
        ///<param name = "line" > Order line to assign the processes sequence.</param>
        ///<param name="processDirectionProperties"></param>
        /// <param name="rand">Instance of Random</param>
        /// <returns>List of processes.</returns>
        private List<Process> OutboundFlow(OrderLine line, List<ProcessDirectionProperty> processDirectionProperties, Random rand)
        {
            List<Process> processes = new List<Process>();
            bool isFirstProcess = true;
            bool isEnd = false;
            Models.Models.Process? nextProcess = null;

            while (true)
            {
                if (isFirstProcess)
                {
                    var posibleProcesses = processDirectionProperties.Select(x => x.InitProcess).Where(x => x.IsInitProcess && !x.IsIn).GroupBy(x => x.Id).Select(g => g.First());

                    var possibleProcess = RandomSelector.SelectInitProcess(posibleProcesses.ToList(), this.Data, rand);

                    nextProcess = RandomSelector.SelectNextProcess(processDirectionProperties.Where(x => x.InitProcessId == possibleProcess.Id).ToList(), this.Data, rand);
                    isEnd = processDirectionProperties.FirstOrDefault(x => x.InitProcessId == possibleProcess.Id && x.EndProcessId == nextProcess.Id).IsEnd;

                    processes.Add(new Process(possibleProcess.Id, possibleProcess.Name, possibleProcess.Area, possibleProcess.Type, 
                        possibleProcess.IsInitProcess, possibleProcess.IsWarehouseProcess, line.Containers));

                    isFirstProcess = false;
                }
                else
                {
                    if (isEnd)
                    {
                        var area = nextProcess.Area;
                        var code = nextProcess.Name;
                        var processType = nextProcess.Type;
                        var isInitProcess = nextProcess.IsInitProcess;
                        var isWarehouseProcess = nextProcess.IsWarehouseProcess;

                        processes.Add(new Process(nextProcess.Id, code, area, processType, isInitProcess, isWarehouseProcess, line.Containers));
                        break;
                    }
                    else
                    {
                        var area = nextProcess.Area;
                        var code = nextProcess.Name;
                        var processType = nextProcess.Type;
                        var isInitProcess = nextProcess.IsInitProcess;
                        var isWarehouseProcess = nextProcess.IsWarehouseProcess;

                        processes.Add(new Process(nextProcess.Id, code, area, processType, isInitProcess, isWarehouseProcess, line.Containers));

                        try
                        {
                            isEnd = processDirectionProperties.FirstOrDefault(x => x.InitProcessId == nextProcess.Id).IsEnd;
                        }
                        catch 
                        {
                            Console.WriteLine("WARNING. Error in processes configuration. Not an end process found for outbound flow.");
                            break;
                        }

                        nextProcess = RandomSelector.SelectNextProcess(processDirectionProperties.Where(x => x.InitProcessId == nextProcess.Id).ToList(), this.Data, rand);
                    }
                }
            }

            processes = AssignPreviousProcesses(processes);
            return processes;
        }

        /// <summary>
        /// Assigns the previous process to every process in the list
        /// </summary>
        /// <param name="processes">List of selected processes</param>
        /// <returns>List of processes with the previous processes assigned</returns>
        private List<Process> AssignPreviousProcesses(List<Process> processes)
        {
            for (int i = 0; i < processes.Count(); i++)
                if (i != 0) processes[i].PreviousProcess = processes[i - 1];
            
            return processes;
        }

        #endregion

        #endregion
    }
}
