namespace Mss.WorkForce.Code.Simulator.Simulation.PostSimulation
{
    public class ItemPlanningShifts
    {
        #region Variables
        private Simulation simulation;
        #endregion

        #region Constructor
        public ItemPlanningShifts(Simulation simulation)
        {
            this.simulation = simulation;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Calculates the shift of the ItemPlanning
        /// </summary>
        public void CalculateShiftsForItemPlannings()
        {
            var workOrders = this.simulation.PlanningReturn.WorkOrderPlanning.Where(x => x.InputOrderId != null);

            foreach (var w in workOrders)
            {
                foreach (var i in w.ItemPlanning)
                {
                    double init = i.InitDate.TimeOfDay.TotalHours;

                    foreach (var s in this.simulation.Data.Schedule.Where(m => m.AvailableWorker.WorkerId == i.WorkerId))
                    {
                        if (s.Shift.InitHour < s.Shift.EndHour)
                        {
                            if (s.Shift.InitHour <= init && s.Shift.EndHour >= init)
                            {
                                i.Shift = s.Shift;
                                i.ShiftId = s.ShiftId;
                            }
                        } 
                        else
                        {
                            if (s.Shift.InitHour >= init || s.Shift.EndHour < init)
                            {
                                i.Shift = s.Shift;
                                i.ShiftId = s.ShiftId;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
