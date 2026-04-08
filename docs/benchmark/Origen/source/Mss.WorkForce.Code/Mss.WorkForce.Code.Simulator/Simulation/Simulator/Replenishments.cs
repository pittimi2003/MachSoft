using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator
{
    public class Replenishments
    {
        #region Variables
        private Simulator simulator;
        #endregion

        #region Constructor
        public Replenishments(Simulator simulator)
        {
            this.simulator = simulator;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Checks if a specific picking process triggers a replenishment process.
        /// </summary>
        /// <param name="process">Picking process to compare.</param>
        public void CheckReplenishmentToUnlock(Grouping process)
        {
            for (int i = 0; i < process.Count; i++)
            {
                var associatedReplenishments = this.simulator.Simulation.PausedProcesses
                    .Where(x => x.Process.ProcessType == ProcessType.Replenishment && x.Process.Area.Id == process.Process.Area.Id).OrderBy(x => x.ProcessesLeftToUnlock);
                var replenish = associatedReplenishments.FirstOrDefault();

                if (replenish != null)
                {
                    replenish.ProcessesLeftToUnlock--;
                    if (replenish.ProcessesLeftToUnlock == 0) UnlockReplenishment(replenish);
                }
            }
        }

        /// <summary>
        /// Unlocks the replenishment process if it is triggered by the previous picking process
        /// </summary>
        /// <param name="replenish">Selected replenishment associated to the picking process</param>
        private void UnlockReplenishment(Grouping replenish)
        {
            this.simulator.Simulation.PausedProcesses.Remove(replenish);
            replenish.StartWorkingDate = this.simulator.Simulation.Environment.Now;
            replenish.IsPaused = false;
            this.simulator.Simulation.ActiveProcesses.Add(replenish);
        }

        #endregion
    }
}
