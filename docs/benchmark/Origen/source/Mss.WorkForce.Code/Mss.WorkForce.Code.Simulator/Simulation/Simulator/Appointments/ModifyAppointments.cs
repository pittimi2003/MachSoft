using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator.Appointments
{
    public class ModifyAppointments
    {
        #region Variables
        private Simulator simulator;
        #endregion

        #region Constructor
        public ModifyAppointments(Simulator simulator)
        {
            this.simulator = simulator;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Adds a dock to an appointment
        /// </summary>
        /// <param name="process">Process that indicates the dock of the appointment</param>
        public void AddDockToAppointment(Grouping process)
        {
            if (!process.Process.IsWarehouseProcess)
                if (process.Appointment != null && process.Appointment.DockResourceId == null && process.Process.Area.Type == AreaType.Dock)
                {
                    process.Appointment.DockResourceId = process.SelectedStation.Id;
                    this.simulator.Simulation.Resources[process.SelectedStation.Id].IsOccupiedByVehicle = true;
                }
        }

        /// <summary>
        /// Adds a worker and the time working in that appointment to an appointment
        /// </summary>
        /// <param name="process">Process that indicates the worker to add into the appointment</param>
        public void AddWorkersToAppointment(Grouping process)
        {
            var assignedWorkerId = process.AssignedWorker.Id;

            if (process.Process.ProcessType == ProcessType.Inbound && process.OrderId != null)
            {
                if (process.Appointment!.Workers.Select(x => x.WorkerId).Contains(assignedWorkerId))
                {
                    process.Appointment!.Workers.FirstOrDefault(x => x.WorkerId == assignedWorkerId)!.InitDate = process.ItemPlanningReturn!.InitDate;
                    process.Appointment!.Workers.FirstOrDefault(x => x.WorkerId == assignedWorkerId)!.EndDate = process.ItemPlanningReturn.EndDate;
                }
                else
                    process.Appointment!.Workers.Add(new CustomWorkerDoneJobs
                        (
                            process.AssignedWorker.Id,
                            process.ItemPlanningReturn.InitDate,
                            process.ItemPlanningReturn.EndDate
                        )
                    );
            }
        }

        #endregion
    }
}
