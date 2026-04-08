using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator
{
    public class Priority
    {
        #region Methods

        /// <summary>
        /// Sets a higher priority to processes that are closed to be delayed
        /// </summary>
        /// <param name="simulation">Specific simulation object</param>
        /// <returns>List of processes with the MarginPriority adjusted</returns>
        public static void SetHurryUpPriority(Simulation simulation)
        {
            foreach (var p in simulation.ActiveProcesses
                .Where(p => p.OnTimePriority != Helper.Constants.Priority.Default && p.MaxMarginDate != null && p.MaxMarginDate <= simulation.Environment.Now)) p.MarginPriority = 0;
        }

        /// <summary>
        /// Sets a lower priority to delayed processes
        /// </summary>
        /// <param name="simulation">Specific simulation object</param>
        /// <returns>List of processes with the OnTimePriority adjusted</returns>
        public static void SetNoOnTimePriority(Simulation simulation)
        {
            foreach (var p in simulation.ActiveProcesses
                .Where(p => p.OnTimePriority != Helper.Constants.Priority.Default && p.MaxOnTimeDate != null && p.MaxOnTimeDate < simulation.Environment.Now))
            {
                p.OnTimePriority = Helper.Constants.Priority.Default;
                p.MarginPriority = Helper.Constants.Priority.Default;
            }
        }

        #endregion
    }
}
