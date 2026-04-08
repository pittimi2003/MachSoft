using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator.Processes
{
    public class ModifyProcesses
    {
        #region Variables
        private Simulator simulator;
        #endregion

        #region Constructor
        public ModifyProcesses(Simulator simulator) 
        {
            this.simulator = simulator;
        }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Start working in a process when its StartWorkingDate arrived.
        /// </summary>
        /// <param name="process">Process to work in</param>
        public void StartWorkingInProcess(Grouping process)
        {
            // Si no es el primero, y por tanto no tiene proceso previos no tenemos que restar nada.
            // Vale igual para procesos de Replenishment
            if (process.Position > 0)
            {
                var previousProcess = process.AssociatedProcesses.FirstOrDefault(x => x.Position == (process.Position - 1));
                this.simulator.Simulation.Resources[previousProcess.SelectedStation.Id].CurrentContainers -= previousProcess.Containers;
            }

            if (process.Process.ProcessType == ProcessType.Picking)
                new Replenishments(this.simulator).CheckReplenishmentToUnlock(process);
        }

        /// <summary>
        /// Closses a process and free the previously assigned worker
        /// </summary>
        /// <param name="process">Process to close</param>
        public void CloseProcess(Grouping process)
        {
            this.simulator.Simulation.WorkingProcesses.Remove(process);
            process.IsActive = false;
            this.simulator.Simulation.DoneProcesses.Add(process);

            if (!process.Process.IsWarehouseProcess) StartNextWorkingTimeProcesses(process);
            EndProcessInPlanning(process);
        }

        /// <summary>
        /// Pauses a group of processes related to the just checked process
        /// </summary>
        /// <param name="process">Process checked for pausing</param>
        public void QueueProcesses(Grouping process)
        {
            HashSet<Grouping> processesToPause = new();

            if (this.simulator.pausedByEquipments || this.simulator.pausedByWorkers || this.simulator.pausedByZoneEquipments)
            {
                processesToPause = this.simulator.Simulation.ActiveProcesses
                    .Where(x => x.Process.Id == process.Process.Id).ToHashSet();
            }
            else if (this.simulator.pausedByZoneContainers)
            {
                var maxCapacity = this.simulator.Simulation.Resources.Values
                    .Where(x => x.Type == ResourceType.Zone && x.AreaId == process.Process.Area.Id)
                    .Max(x => x.MaxContainers - x.CurrentContainers);

                processesToPause = this.simulator.Simulation.ActiveProcesses
                    .Where(x => x.Process.Id == process.Process.Id && x.Containers > maxCapacity).ToHashSet();
            }

            if (process.Process.ProcessType == ProcessType.Loading || process.Process.ProcessType == ProcessType.Inbound)
                processesToPause = processesToPause.Where(x => x.Appointment.Id == process.Appointment.Id).ToHashSet();

            foreach (var p in processesToPause)
            {
                this.simulator.Simulation.ActiveProcesses.Remove(p);
                p.WaitingForResource = this.simulator.Simulation.Environment.Now;
                this.simulator.Simulation.QueuedProcesses.Add(p);
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Checks if the next processes can start
        /// </summary>
        /// <param name="process">Last process to be closed</param>
        private void StartNextWorkingTimeProcesses(Grouping process)
        {
            bool allDone = process.AssociatedProcesses
                .Where(x => x.Position == process.Position).All(x => !x.IsActive);

            if (allDone)
            {
                if (process.Position < process.AssociatedProcesses.Max(X => X.Position))
                {
                    // Procesos de la siguiente posición
                    var nextProcesses = process.AssociatedProcesses.Where(x => x.Position == (process.Position + 1));

                    foreach (var p in nextProcesses)
                    {
                        this.simulator.Simulation.PausedProcesses.Remove(p);
                        p.IsPaused = false;
                        this.simulator.Simulation.ActiveProcesses.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// Finish a specific process in the simulation planning
        /// </summary>
        /// <param name="process">Process to start</param>
        private void EndProcessInPlanning(Grouping process)
        {
            if (!process.Process.IsWarehouseProcess)
            {
                process.ItemPlanningReturn.EndDate = this.simulator.Simulation.Environment.Now;
                this.simulator.Simulation.PlanningReturn.WorkOrderPlanning.FirstOrDefault(x => x.InputOrderId == process.OrderId).ItemPlanning.Add(process.ItemPlanningReturn);
            }
            else
            {
                process.WarehouseProcessPlanningReturn.EndDate = this.simulator.Simulation.Environment.Now;
                this.simulator.Simulation.PlanningReturn.WarehouseProcessPlanning.Add(process.WarehouseProcessPlanningReturn);
            }
        }

        #endregion

        #endregion
    }
}
