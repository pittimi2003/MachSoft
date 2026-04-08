using Mss.WorkForce.Code.Simulator.Helper.Constants;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator.Processes
{
    public class OrderProcesses
    {
        #region Methods

        /// <summary>
        /// Orders the processes as in the database is specified
        /// </summary>
        /// <param name="processes">List of processes to order</param>
        /// <param name="priorityCode">Code of the priority to order for</param>
        /// <param name="simulation">Simulation data</param>
        /// <returns>List of the processes ordered</returns>
        /// <exception cref="NotImplementedException">If the code is not present in the catalog, throws a not implemented exception</exception>
        public static IOrderedEnumerable<Grouping> Dinamic(IOrderedEnumerable<Grouping> processes, string priorityCode, Simulation simulation)
        {
            if (processes.Any())
            {
                switch (priorityCode)
                {
                    case PriorityOrderCode.CommittedTime:
                        processes = processes.ThenBy(x => x.OnTimePriority.GetValueOrDefault(Helper.Constants.Priority.Default))
                                            .ThenBy(x => x.MarginPriority.GetValueOrDefault(Helper.Constants.Priority.Default))
                                            .ThenBy(x => x.AppointmentDate.GetValueOrDefault(simulation.Environment.Now));
                        break;

                    case PriorityOrderCode.Priority:
                        processes = processes.ThenBy(x => x.OrderPriority.GetValueOrDefault(Helper.Constants.Priority.Default));
                        break;

                    case PriorityOrderCode.ReleaseTime:
                        processes = processes.ThenBy(x => x.ReleaseDate.GetValueOrDefault(simulation.Environment.Now));
                        break;

                    case PriorityOrderCode.CreationTime:
                        processes = processes.ThenBy(x => x.CreationDate.GetValueOrDefault(simulation.Environment.Now));
                        break;

                    default:
                        throw new NotImplementedException($"Priority {priorityCode} does not exists in the catalog. Please, check the configuration.");
                }
                
            }

            return processes;
        }

        /// <summary>
        /// Orders the processes in the default order
        /// </summary>
        /// <param name="processes">List of processes to order</param>
        /// <param name="simulation">Simulation data</param>
        /// <returns>List of the processes ordered</returns>
        public static IOrderedEnumerable<Grouping> Static(IOrderedEnumerable<Grouping> processes, Simulation simulation)
        {
            return processes.ThenBy(x => x.OnTimePriority.GetValueOrDefault(Helper.Constants.Priority.Default))
                            .ThenBy(x => x.MarginPriority.GetValueOrDefault(Helper.Constants.Priority.Default))
                            .ThenBy(x => x.AppointmentDate.GetValueOrDefault(simulation.Environment.Now))
                            .ThenBy(x => x.OrderPriority.GetValueOrDefault(Helper.Constants.Priority.Default))
                            .ThenBy(x => x.ReleaseDate.GetValueOrDefault(simulation.Environment.Now))
                            .ThenBy(x => x.CreationDate.GetValueOrDefault(simulation.Environment.Now));
        }

        #endregion
    }
}
