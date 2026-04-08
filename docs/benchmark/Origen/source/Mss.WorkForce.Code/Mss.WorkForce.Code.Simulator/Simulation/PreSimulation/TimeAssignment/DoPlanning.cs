using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Warehouse = Mss.WorkForce.Code.Simulator.Core.Layout.Warehouse;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.TimeAssignment
{
    public class DoPlanning
    {
        #region Variables
        private Warehouse warehouse;
        private DataSimulatorTablaRequest data;
        private DateTime simulationDate;
        #endregion

        #region Constructor
        public DoPlanning(Warehouse warehouse, DataSimulatorTablaRequest data, DateTime simulationDate)
        {
            this.warehouse = warehouse;
            this.data = data;
            this.simulationDate = simulationDate;
        }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Make the times planning fot the processes
        /// </summary>
        /// <returns>Yields a SimSharp Event</returns>
        public Warehouse MakePlanner()
        {
            foreach (var order in this.warehouse.Orders)
            {
                foreach (var process in order.Grouping)
                {
                    var steps = this.data.Step.Where(x => x.ProcessId == process.Process.Id);
                    var maxPosition = order.Grouping.Max(x => x.Position);

                    if (process.Position == 0)
                    {
                        // No tendremos en cuenta el tiempo de ruta, pues es de donde parte

                        process.Duration = CalculateProcessDuration(steps, process, ProcessType.Inbound);
                        var inputOrder = this.data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId);
      
                        if (inputOrder.IsOutbound)
                        {
                            if (inputOrder.IsBlocked.GetValueOrDefault() && inputOrder.Status == OrderStatus.Waiting)
                            {
                                process.StartWorkingDate = inputOrder.BlockDate.Value;
                            }
                            else
                            {
                                process.StartWorkingDate = this.simulationDate;
                            }
                        }
                        else
                        {
                            var appointment = this.warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == inputOrder.AppointmentDate && x.VehicleCode == inputOrder.VehicleCode);
                            var date = appointment.StartDate;
                            if (date > this.simulationDate)
                            {
                                if (inputOrder.IsBlocked.GetValueOrDefault() && inputOrder.Status == OrderStatus.Waiting)
                                {
                                    process.StartWorkingDate = inputOrder.BlockDate.Value;
                                }
                                else
                                {
                                    process.StartWorkingDate = date;
                                }
                            }
                            else
                            {
                                process.StartWorkingDate = this.simulationDate;
                            }
                        }
                    }
                    else
                    {
                        // Procesos de posición anterior
                        int maxPreviousPosition = order.Grouping.Where(x => x.Position < process.Position).Max(x => x.Position);
                        var maxPreviousProcesses = order.Grouping.Where(x => x.Position == maxPreviousPosition);

                        var previousMaxDuration = maxPreviousProcesses.Max(x => x.Duration);
                        var previousStartWorkingDate = maxPreviousProcesses.FirstOrDefault().StartWorkingDate;

                        var previousProcesses = order.Grouping.Where(x => x.Position < process.Position);

                        // Comprueba si el proceso actual o alguno de los anteriores es de tipo Packing para inicializar el valor Skipping Process
                        process.Duration = CalculateProcessDuration(steps, process, 
                            (process.Process.ProcessType == ProcessType.Loading && !previousProcesses.Any(x => x.Process.ProcessType == ProcessType.Packing)) ? ProcessType.Loading : null) 
                            + CalculateRouteTime(process);

                        if (process.Position == maxPosition && process.Process.Area.Type == AreaType.Dock)
                        {
                            var inputOrder = this.data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId);
                            var appointment = this.warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == inputOrder.AppointmentDate && x.VehicleCode == inputOrder.VehicleCode);

                            process.StartWorkingDate = appointment.StartDate;
                        }
                        else
                            process.StartWorkingDate = previousStartWorkingDate.AddSeconds(previousMaxDuration);
                    }

                    process.AssociatedProcesses.AddRange(order.Grouping);
                }
            }

            return this.warehouse;
        }

        /// <summary>
        /// Calculates the duration of the process, taking into account the pre and post process times, and the specific steps.
        /// </summary>
        /// <param name="steps">List of steps assigned to the process.</param>
        /// <param name="process">Process to calculate the time for</param>
        /// <param name="skippingProcess">Process that will only have one time per process without considering the number of processes grouped.</param>
        /// <returns>The duration of the process</returns>
        public int CalculateProcessDuration(IEnumerable<Models.Models.Step> steps, Grouping process, string? skippingProcess = null)
        {
            int totalWorkingPeriod = 0;

            var processCount = (process.Process.ProcessType == ProcessType.Loading || process.Process.ProcessType == ProcessType.Inbound) ? 1 : process.Count;

            if (steps.Any(x => x.InitProcess)) totalWorkingPeriod += steps.Where(x => x.InitProcess).Sum(x => x.TimeMin).GetValueOrDefault(0);
            totalWorkingPeriod += (processCount * this.data.Process.FirstOrDefault(x => x.Id == process.Process.Id).PreprocessTime.GetValueOrDefault(0));
            if (process.Process.ProcessType == skippingProcess) totalWorkingPeriod += this.data.Process.FirstOrDefault(x => x.Id == process.Process.Id).MinTime;
            else if (process.Process.ProcessType == ProcessType.Picking) totalWorkingPeriod += ((process.Count * this.data.Process.FirstOrDefault(x => x.Id == process.Process.Id).MinTime) + this.data.Picking.FirstOrDefault(x => x.ProcessId == process.Process.Id).PickingRoadTime.GetValueOrDefault(0));
            else totalWorkingPeriod += (process.Count * this.data.Process.FirstOrDefault(x => x.Id == process.Process.Id).MinTime);
            totalWorkingPeriod += (processCount * this.data.Process.FirstOrDefault(x => x.Id == process.Process.Id).PostprocessTime.GetValueOrDefault(0));
            if (steps.Any(x => x.EndProcess)) totalWorkingPeriod += steps.Where(x => x.EndProcess).Sum(x => x.TimeMin).GetValueOrDefault(0);

            return totalWorkingPeriod;
        }

        #endregion

        #region Private

        /// <summary>
        /// Calculates the route time from one area to another different one.
        /// </summary>
        /// <param name="process">Process that will give us the actual process and the previous one assigned to it</param>
        /// <returns>The time for travelling from one area to another.</returns>
        private int CalculateRouteTime(Grouping process)
        {
            int totalWorkingPeriod = 0;

            if (process.Process.PreviousProcess != null)
            {
                var arrivalArea = process.Process.Area.Id;
                var departureArea = process.Process.PreviousProcess.Area.Id;

                if(this.data.Route.Any(x => x.DepartureAreaId == departureArea && x.ArrivalAreaId == arrivalArea))
                {
                    totalWorkingPeriod += this.data.Route.FirstOrDefault(x => x.DepartureAreaId == departureArea && x.ArrivalAreaId == arrivalArea).TimeMin.GetValueOrDefault(0);
                }
                else if (this.data.Route.Any(x => x.DepartureAreaId == arrivalArea && x.ArrivalAreaId == departureArea && x.Bidirectional))
                {
                    totalWorkingPeriod += this.data.Route.FirstOrDefault(x => x.DepartureAreaId == arrivalArea && x.ArrivalAreaId == departureArea && x.Bidirectional).TimeMin.GetValueOrDefault(0);
                }
            }
            
            return totalWorkingPeriod;
        }

        #endregion

        #endregion
    }
}
