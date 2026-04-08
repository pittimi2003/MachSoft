using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;

namespace Mss.WorkForce.Code.Simulator.Helper.Checker
{
    public class BadSimulationChecker
    {
        #region Properties
        private DataSimulatorTablaRequest data;
        private IEnumerable<Grouping> processes;
        private Dictionary<Guid, Resource> resources;
        private List<SimulationLogCheck> logChecks;
        #endregion

        #region Constructor

        public BadSimulationChecker(Simulation.Simulation simulation) 
        {
            this.data = simulation.Data;
            this.processes = simulation.Processes;
            this.resources = simulation.Resources;
            this.logChecks = simulation.LogChecks;
        }

        #endregion

        #region Auxiliary Classes

        internal class CustomNotDoneProcesses
        {
            internal Guid ProcessId { get; set; }
            internal required string Name { get; set; }
            internal Guid AreaId { get; set; }
            internal double MinContainers { get; set; }
            internal double MaxContainers { get; set; }
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Checks the simulation in order to find the problem for which it didn't finish correctly.
        /// </summary>
        /// <returns>Message with the errors found</returns>
        public string Check()
        {
            string message = string.Empty;
            var info = $"\nAborting simulation. An error ocurred while working in the simulation processes.";
            message += $"\n[{data.Warehouse.Name}] Aborting simulation. An error ocurred while working in the simulation processes."; ;
            logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.General.ToString(), Log = info });

            // ME DICE QUE PROCESOS ESTÁN ACTIVOS PERO SIN FINALIZAR, ES DECIR, LOS QUE NO PUEDEN TERMINAR
            var activeNotDoneProcesses = processes.Where(x => x.IsActive && !x.IsPaused)
                .OrderBy(x => x.StartWorkingDate).ThenBy(x => x.Position)
                .GroupBy(x => (x.Process.Id, x.Process.Code, x.Process.Area.Id))
                .Select(x => new CustomNotDoneProcesses
                {
                    ProcessId = x.Key.Item1,
                    Name = x.Key.Item2,
                    AreaId = x.Key.Item3,
                    MinContainers = x.Min(m => m.Containers),
                    MaxContainers = x.Max(m => m.Containers)
                });

            info = $"\nActive processes but not done are:";
            message += $"\n[{data.Warehouse.Name}] Active processes but not done are:";
            logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Processes.ToString(), Log = info });
            foreach (var p in activeNotDoneProcesses)
            {
                info = $"\n    - {p.Name}";
                message += info;
                logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Processes.ToString(), Log = info });
            }

            message += "\n";

            var workersAndRoles = CheckWorkerAndRoles(activeNotDoneProcesses, data, resources);
            var equipments = CheckEquipmentsInArea(activeNotDoneProcesses, data);
            var containersCapacity = CheckContainersCapacity(activeNotDoneProcesses, resources);
            var zonesCapacity = CheckZonesCapacity(activeNotDoneProcesses, resources, data);

            if (!workersAndRoles.IsFailed && !equipments.IsFailed && !containersCapacity.IsFailed && !zonesCapacity.IsFailed)
            {
                info = "\nThe workload configured for the simulation exceeds the warehouse current operational capacity.";
                message += $"\n[{data.Warehouse.Name}] The workload configured for the simulation exceeds the warehouse current operational capacity.";
                logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.WorkLoad.ToString(), Log = info });
                message += "\n";
            }
            else
            {
                message += workersAndRoles.Message;
                message += equipments.Message;
                message += containersCapacity.Message;
                message += zonesCapacity.Message;
            }

            return message;
        }

        #endregion

        #region Private

        /// <summary>
        /// Checks if the problem is related with workers or roles
        /// </summary>
        /// <param name="activeNotDoneProcesses">Processes that are not able to be done after breaking the simulation</param>
        /// <param name="data">Data of the simulation</param>
        /// <param name="resources">Resources used to simulate</param>
        /// <returns>String containing the workers and roles errors</returns>
        private (bool IsFailed, string Message) CheckWorkerAndRoles(IEnumerable<CustomNotDoneProcesses> activeNotDoneProcesses, DataSimulatorTablaRequest data, Dictionary<Guid,Resource> resources)
        {
            string message = string.Empty;
            bool isFailed = false;

            var processesWithNoRoles = from p in activeNotDoneProcesses
                                       join r in data.RolProcessSequence
                                       on p.ProcessId equals r.ProcessId into pr
                                       from result in pr.DefaultIfEmpty()
                                       select new
                                       {
                                           ProcessId = p.ProcessId,
                                           Name = p.Name,
                                           RolId = result?.RolId ?? Guid.Empty,
                                       };

            if (processesWithNoRoles.Any(x => x.RolId == Guid.Empty))
            {
                // Mensaje de que el proceso X no tiene roles asociados

                var info = "\nProcesses with no roles assigned:";
                message += info;
                logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Roles.ToString(), Log = info });
                foreach (var p in processesWithNoRoles.Where(x => x.RolId == Guid.Empty))
                {
                    info = $"\n    - {p.Name}";
                    message += info;
                    logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Roles.ToString(), Log = info });
                }
                
                message += "\n";
                isFailed = true;
            }

            if (processesWithNoRoles.Any(x => x.RolId != Guid.Empty))
            {
                // Tenemos procesos sin terminar con roles, por lo que tenemos que ver que tengamos trabajadores con ese rol asignado

                var workerResources = resources.Values.Where(x => x.Type == ResourceType.Worker).Select(x => new
                {
                    WorkerId = x.Id,
                    RolId = x.RolId
                });

                var workersForTheProcess = from r in processesWithNoRoles.Where(x => x.RolId != Guid.Empty)
                                           join w in workerResources
                                           on r.RolId equals w.RolId into rw
                                           from result in rw.DefaultIfEmpty()
                                           select new
                                           {
                                               ProcessId = r.ProcessId,
                                               ProcessName = r.Name,
                                               Workers = result?.WorkerId ?? Guid.Empty,
                                           };

                var processesWithNoWorkers = workersForTheProcess.Where(x => x.Workers == Guid.Empty).Select(x => x.ProcessName).Distinct();

                if (processesWithNoWorkers.Any())
                {
                    var info = "\nProcesses with no workers assigned:";
                    message += info;
                    logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Workers.ToString(), Log = info });
                    foreach (var p in processesWithNoWorkers)
                    {
                        info = $"\n    - {p}";
                        message += info;
                        logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Workers.ToString(), Log = info });
                    }

                    message += "\n";
                    isFailed = true;
                }
            }

            return (isFailed, message);
        }

        /// <summary>
        /// Checks if the problem is related with the equipments in areas
        /// </summary>
        /// <param name="activeNotDoneProcesses">Processes that are not able to be done after breaking the simulation</param>
        /// <param name="data">Data of the simulation</param>
        /// <returns>String containing the equipments in areas errors</returns>
        private (bool IsFailed, string Message) CheckEquipmentsInArea(IEnumerable<CustomNotDoneProcesses> activeNotDoneProcesses, DataSimulatorTablaRequest data)
        {
            // ME DICE QUE AREAS NO TIENEN EQUIPAMIENTOS O SI ES 0, PARA PODER TRABAJAR EN ELLA

            string message = string.Empty;
            bool isFailed = false;

            var processesAreas = data.Area.Join(activeNotDoneProcesses, a => a.Id, p => p.AreaId, (a, p) => new
            {
                Id = a.Id,
                Name = a.Name
            }).GroupBy(x => (x.Id, x.Name)).Select(x => new { Id = x.Key.Id, Name = x.Key.Name });

            var c = from a in processesAreas
                    join e in data.EquipmentGroup
                    on a.Id equals e.AreaId into ea
                    from result in ea.DefaultIfEmpty()
                    select new
                    {
                        Area = a.Name,
                        Equipments = result?.Equipments ?? 0
                    };

            var areasWithoutEquipments = c.Where(x => x.Equipments <= 0);

            if (areasWithoutEquipments.Any())
            {
                var info = "\nAreas with no equipments assigned:";
                message += info;
                logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Equipments.ToString(), Log = info });
                foreach (var a in areasWithoutEquipments)
                {
                    info = $"\n    - {a.Area}";
                    message += info;
                    logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.Equipments.ToString(), Log = info });
                }
                
                message += "\n";
                isFailed = true;
            }

            return (isFailed, message);
        }

        /// <summary>
        /// Checks if the problem is related with containers capacity in zones
        /// </summary>
        /// <param name="activeNotDoneProcesses">Processes that are not able to be done after breaking the simulation</param>
        /// <param name="resources">Resources used to simulate</param>
        /// <returns>String containing the containers capacity errors</returns>
        private (bool IsFailed, string Message) CheckContainersCapacity(IEnumerable<CustomNotDoneProcesses> activeNotDoneProcesses, Dictionary<Guid,Resource> resources)
        {
            // ME DICE QUE ZONAS ESTÁN LLENAS DE CONTENEDORES

            string message = string.Empty;
            bool isFailed = false;

            var zonesFullOfContainers = resources.Values.Where(x => x.MaxContainers != null && x.CurrentContainers != null)
                .Where(x => x.CurrentContainers == x.MaxContainers).GroupBy(x => (x.Name, x.MaxContainers, x.CurrentContainers, x.AreaId));

            if (zonesFullOfContainers.Any())
            {
                var info = "\nZones with full containers capacity:";
                message += info;
                logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersFull.ToString(), Log = info });
                foreach (var z in zonesFullOfContainers)
                {
                    info = $"\n    - {z.Key.Name} -> Current: {z.Key.CurrentContainers} / Max: {z.Key.MaxContainers} (100% occupied). Processes affected:";
                    message += info;
                    logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersFull.ToString(), Log = info });
                    var processesAffected = activeNotDoneProcesses.Where(x => x.AreaId == z.Key.AreaId).Select(x => x.Name).Distinct();
                    foreach (var p in processesAffected)
                    {
                        info = $"\n        - {p}";
                        message += info;
                        logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersFull.ToString(), Log = info });
                    }
                }

                message += "\n";
                isFailed = true;
            }

            // ME DICE CUELLOS DE BOTELLA

            var zoneResources = resources.Values.Where(x => x.Type == ResourceType.Zone && x.CurrentContainers < x.MaxContainers);
            var zonesForProcesses = zoneResources.Join(activeNotDoneProcesses,
                z => z.AreaId, p => p.AreaId, (z, p) => new
                {
                    ZoneName = z.Name,
                    Process = p.Name,
                    ZoneCurrentCapacity = z.MaxContainers - z.CurrentContainers,
                    ZoneMaxContainers = z.MaxContainers,
                    MinProcessContainers = p.MinContainers
                }).GroupBy(x => (x.ZoneName, x.ZoneCurrentCapacity, x.ZoneMaxContainers))
                .Select(x => new
                {
                    ZoneName = x.Key.ZoneName,
                    Processes = x.Select(p => p.Process).Distinct(),
                    ZoneMaxContainers = x.Key.ZoneMaxContainers,
                    ZoneCurrentCapacity = x.Key.ZoneCurrentCapacity,
                    MinProcessContainers = x.Min(m => m.MinProcessContainers)
                }).Where(x => x.MinProcessContainers > x.ZoneCurrentCapacity && x.ZoneCurrentCapacity < x.ZoneMaxContainers);

            if (zonesForProcesses.Any())
            {
                var info = "\nZones blocking process execution due to insufficient free capacity:";
                message += info;
                logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersBlock.ToString(), Log = info });
                foreach (var z in zonesForProcesses)
                {
                    info = $"\n    - {z.ZoneName} -> " +
                        $"Zone current containers capacity: {z.ZoneCurrentCapacity} ({Math.Round((1 - (z.ZoneCurrentCapacity.Value / z.ZoneMaxContainers.Value)) * 100, 2)}% occupied). " +
                        $"Minimum containers among pending processes: {z.MinProcessContainers}. Processes affected:";
                    message += info;
                    logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersBlock.ToString(), Log = info });
                    foreach (var p in z.Processes)
                    { 
                        info = $"\n      - {p}";
                        message += info;
                        logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersBlock.ToString(), Log = info });
                    }
                }

                message += "\n";
                isFailed = true;
            }

            return (isFailed, message);
        }

        /// <summary>
        /// Checks if the problem is related with no enough containers capacity
        /// </summary>
        /// <param name="activeNotDoneProcesses">Processes that are not able to be done after breaking the simulation</param>
        /// <param name="resources">Resources used to simulate</param>
        /// <param name="data">Data of the simulation</param>
        /// <returns></returns>
        private (bool IsFailed, string Message) CheckZonesCapacity(IEnumerable<CustomNotDoneProcesses> activeNotDoneProcesses, Dictionary<Guid, Resource> resources, DataSimulatorTablaRequest data)
        {
            // ME DICE QUE ZONAS ESTÁN LLENAS DE CONTENEDORES

            string message = string.Empty;
            bool isFailed = false;

            var zonesWithCapacityNotEnough = resources.Values.Where(x => x.Type == ResourceType.Zone)
                .Join(activeNotDoneProcesses, r => r.AreaId, p => p.AreaId, (r, p) => new
                {
                    Zone = r.Name,
                    Process = p.Name,
                    ZoneCapacity = r.MaxContainers,
                    ProcessContainers = p.MinContainers,
                })
                .Where(x => x.ProcessContainers > x.ZoneCapacity)
                .GroupBy(x => (x.Zone, x.ZoneCapacity, x.ProcessContainers))
                .Select(x => new
                {
                    Zone = x.Key.Zone,
                    ZoneCapacity = x.Key.ZoneCapacity,
                    ProcessContainers = x.Key.ProcessContainers,
                    Processes = x.Select(m => m.Process).Distinct()
                });

            if (zonesWithCapacityNotEnough.Any())
            {
                var info = "\nZones with insufficient total container capacity for required processes:";
                message += info;
                logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersInsufficient.ToString(), Log = info });
                foreach (var z in zonesWithCapacityNotEnough)
                {
                    info = $"\n    - {z.Zone} -> Zone containers capacity: {z.ZoneCapacity}. Process containers: {z.ProcessContainers}. Processes affected:";
                    message += info;
                    logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersInsufficient.ToString(), Log = info });
                    foreach (var p in z.Processes)
                    { 
                        info = $"\n      - {p}";
                        message += info;
                        logChecks.Add(new SimulationLogCheck() { SimulationResult = SimulationResults.ContainersInsufficient.ToString(), Log = info });
                    }
                }

                message += "\n";
                isFailed = true;
            }

            return (isFailed, message);
        }

        #endregion

        #endregion
    }
}
