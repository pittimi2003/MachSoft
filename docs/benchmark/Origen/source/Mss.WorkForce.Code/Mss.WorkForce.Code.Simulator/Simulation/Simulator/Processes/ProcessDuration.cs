using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator.Processes
{
    public class ProcessDuration
    {
        #region Variables
        private Simulator simulator;
        #endregion

        #region Constructor
        public ProcessDuration(Simulator simulator)
        {
            this.simulator = simulator;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Calculates the waiting time in order of the break corresponging to a specific moment for a worker
        /// </summary>
        /// <param name="worker">Worker assigned to the process</param>
        /// <param name="equipment">Equipment assigned to the process</param>
        /// <param name="process">Selected process</param>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        /// <returns>Yields a SimSharp event</returns>
        public IEnumerable<SimSharp.Event> Calculate(Resource worker, Resource equipment, Grouping process, bool whatIf)
        {
            var initProcess = this.simulator.Simulation.Environment.Now;
            var endProcess = this.simulator.Simulation.Environment.Now.AddSeconds(process.Duration);

            var workerBreaks = worker.Breaks.Select(x => new
            {
                Id = x.Id,
                InitBreakDate = this.simulator.Simulation.Environment.Now.Date.AddHours(x.InitBreak),
                EndBreakDate = this.simulator.Simulation.Environment.Now.Date.AddHours(x.EndBreak)
            });

            if (!workerBreaks.Any())
            {
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration));
            }
            else if (workerBreaks.Any(x => initProcess < x.InitBreakDate && endProcess > x.InitBreakDate && endProcess < x.EndBreakDate))
            {
                var _break = workerBreaks.FirstOrDefault(x => initProcess < x.InitBreakDate && endProcess > x.InitBreakDate && endProcess < x.EndBreakDate);
                var breakDuration = Convert.ToInt32((_break.EndBreakDate - _break.InitBreakDate).TotalSeconds);
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration + breakDuration));
            }
            else if (workerBreaks.Any(x => initProcess == x.InitBreakDate && endProcess > x.InitBreakDate && endProcess < x.EndBreakDate))
            {
                var _break = workerBreaks.FirstOrDefault(x => initProcess == x.InitBreakDate && endProcess > x.InitBreakDate && endProcess < x.EndBreakDate);
                var breakDuration = Convert.ToInt32((_break.EndBreakDate - _break.InitBreakDate).TotalSeconds);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(breakDuration));
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration));
            }
            else if (workerBreaks.Any(x => initProcess > x.InitBreakDate && initProcess < x.EndBreakDate && endProcess > x.InitBreakDate && endProcess < x.EndBreakDate))
            {
                var _break = workerBreaks.FirstOrDefault(x => initProcess > x.InitBreakDate && initProcess < x.EndBreakDate && endProcess > x.InitBreakDate && endProcess < x.EndBreakDate);
                var restOfBreak = Convert.ToInt32((_break.EndBreakDate - initProcess).TotalSeconds);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(restOfBreak));
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration));
            }
            else if (workerBreaks.Any(x => initProcess > x.InitBreakDate && initProcess < x.EndBreakDate && endProcess == x.EndBreakDate))
            {
                var _break = workerBreaks.FirstOrDefault(x => initProcess > x.InitBreakDate && initProcess < x.EndBreakDate && endProcess == x.EndBreakDate);
                var restOfBreak = Convert.ToInt32((_break.EndBreakDate - initProcess).TotalSeconds);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(restOfBreak));
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration));
            }
            else if (workerBreaks.Any(x => initProcess == x.InitBreakDate && endProcess == x.EndBreakDate))
            {
                var _break = workerBreaks.FirstOrDefault(x => initProcess == x.InitBreakDate && endProcess == x.EndBreakDate);
                var breakDuration = Convert.ToInt32((_break.EndBreakDate - _break.InitBreakDate).TotalSeconds);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(breakDuration));
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration));
            }
            else if (workerBreaks.Any(x => initProcess > x.InitBreakDate && initProcess < x.EndBreakDate && endProcess > x.InitBreakDate && endProcess > x.EndBreakDate))
            {
                var _break = workerBreaks.FirstOrDefault(x => initProcess > x.InitBreakDate && initProcess < x.EndBreakDate && endProcess > x.InitBreakDate && endProcess > x.EndBreakDate);
                var restOfBreak = Convert.ToInt32((_break.EndBreakDate - initProcess).TotalSeconds);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(restOfBreak));
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration));
            }
            else
            {
                StartProcessInPlanning(process, whatIf);
                yield return this.simulator.Simulation.Environment.Timeout(TimeSpan.FromSeconds(process.Duration));
            }
        }

        /// <summary>
        /// Starts a specific process in the simulation planning
        /// </summary>
        /// <param name="process">Process to start</param>
        /// <param name="whatIf">Boolean that indicates if the what if configuration applies to the simulation.</param>
        private void StartProcessInPlanning(Grouping process, bool whatIf)
        {
            if (!process.Process.IsWarehouseProcess && process.OrderId != null)
            {
                if (!this.simulator.Simulation.PlanningReturn.WorkOrderPlanning.Select(x => x.InputOrderId).Contains(process.OrderId))
                {
                    this.simulator.Simulation.PlanningReturn.WorkOrderPlanning.Add(PlanningData.CreateWorkOrderPlanning(this.simulator, process));
                    process.ItemPlanningReturn = PlanningData.CreateItemPlanning(this.simulator, process, whatIf);
                }
                else process.ItemPlanningReturn = PlanningData.CreateItemPlanning(this.simulator, process, whatIf); 
            }
            else process.WarehouseProcessPlanningReturn = PlanningData.CreateWarehouseProcessPlanning(this.simulator, process);
        }

        #endregion
    }
}
