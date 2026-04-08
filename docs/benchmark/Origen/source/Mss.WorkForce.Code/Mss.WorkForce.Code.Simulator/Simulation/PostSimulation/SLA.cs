using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Simulator.Helper;

namespace Mss.WorkForce.Code.Simulator.Simulation.PostSimulation
{
    public class SLA
    {
        #region Variables
        private Simulation simulation;
        #endregion

        #region Constructor
        public SLA(Simulation simulation)
        {
            this.simulation = simulation;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calculates the SLA of the simulation
        /// </summary>
        public void CalculateSLAWorkOrdersOnTimePercentage()
        {
            if (this.simulation.PlanningReturn.WorkOrderPlanning.Any(x => x.IsOutbound))
            {
                var totalWorkOrders = this.simulation.PlanningReturn.WorkOrderPlanning.Count(x => x.IsOutbound);
                var workOrdersOnTime = this.simulation.PlanningReturn.WorkOrderPlanning.Count(x => x.IsOutbound && x.IsInVehicleTime);

                this.simulation.PlanningReturn.SLAWorkOrdersOnTimePercentage = Math.Round((Convert.ToDouble(workOrdersOnTime) / Convert.ToDouble(totalWorkOrders)) * 100, 2);
            }  
            else 
                this.simulation.PlanningReturn.SLAWorkOrdersOnTimePercentage = 0;
        }

        /// <summary>
        /// Calculates the SLA
        /// </summary>
        public void CalculateSLATargetAndOrderDelay()
        {
            var workOrders = this.simulation.PlanningReturn.WorkOrderPlanning.Where(x => x.InputOrderId != null && x.Status != OrderStatus.Cancelled);

            if (workOrders.Any())
            {
                foreach (var w in workOrders)
                {
                    w.SLATarget = w.AppointmentEndDate == null? w.AppointmentDate: w.AppointmentEndDate.Value ;
                    var delayInfo = CalculateOrderDelay(w, w.AppointmentEndDate.Value);
                    w.OrderDelay = delayInfo.Item1;
                    w.SLAMet = delayInfo.Item2;                    
                }
            }
        }

        private (double?, bool) CalculateOrderDelay(WorkOrderPlanningReturn workOrder, DateTime endAppointment)
        {
            if (workOrder.IsOutbound)
                return workOrder.EndDate > endAppointment ? ((workOrder.EndDate - endAppointment).TotalSeconds, false) : (0, true);
            else
            {
                if(this.simulation.Processes.LastOrDefault(x => x.OrderId == workOrder.InputOrderId && x.Process.ProcessType == ProcessType.Inbound)?.ItemPlanningReturn?.EndDate == null)
                {
                    return (0, false);
                }
                return this.simulation.Processes.LastOrDefault(x => x.OrderId == workOrder.InputOrderId && x.Process.ProcessType == ProcessType.Inbound).ItemPlanningReturn.EndDate > endAppointment ? ((this.simulation.Processes.LastOrDefault(x => x.OrderId == workOrder.InputOrderId && x.Process.ProcessType == ProcessType.Inbound).ItemPlanningReturn.EndDate - endAppointment).TotalSeconds, false) : (0, true);
                
            }

        }
        #endregion
    }
}
