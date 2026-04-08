using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Helper;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator.Appointments
{
    public class Appointment
    {
        #region Variables
        public Guid Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime? RealArrivalTime { get; set; }
        public string VehicleCode { get; set; }
        public Guid? DockResourceId { get; set; }
        public Guid? AssignedDockId { get; set; }
        public Guid? SelectedDockProcessId { get; set; }
        public List<CustomWorkerDoneJobs> Workers { get; set; }
        public List<InputOrder> InputOrders { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<Guid>? PossibleDockIds { get; set; }
        public bool IsOut { get; set; }
        #endregion

        #region Constructor
        public Appointment() { }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Generates the Appointments, with the StartDate and EndDate of each of them to be used during the simulation 
        /// </summary>
        /// <param name="data">Simulation data</param>
        /// <returns>List of the appointments in the simulation</returns>
        public static List<Appointment> Generate(DataSimulatorTablaRequest data)
        {
            List<Appointment> appointments = GenerateAppointments(data.InputOrder);

            var warehouseId = data.Warehouse!.Id;
            var outboundTime = data.OutboundFlowGraph.Any() ? data.OutboundFlowGraph.FirstOrDefault(x => x.WarehouseId == warehouseId)!.MaxVehicleTime.GetValueOrDefault(0) : 0;
            var inboundTime = data.InboundFlowGraph.Any() ? data.InboundFlowGraph.FirstOrDefault(x => x.WarehouseId == warehouseId)!.MaxVehicleTime.GetValueOrDefault(0) : 0;

            var hasNotifications = data.YardAppointmentsNotifications.Any();
            var notifications = hasNotifications
                ? data.YardAppointmentsNotifications
                    .GroupBy(x => (x.VehicleCode, x.AppointmentDate))
                    .ToDictionary(g => g.Key, g => g.First())
                : null;

            foreach (var a in appointments)
            {
                var isOutbound = a.InputOrders.FirstOrDefault()!.IsOutbound;
                var maxTimeInDock = isOutbound ? outboundTime : inboundTime;

                if (hasNotifications && notifications.TryGetValue((a.VehicleCode, a.AppointmentDate), out var dataAppointment))
                {
                    a.StartDate = dataAppointment.InitDate.GetValueOrDefault(a.AppointmentDate);
                    a.EndDate = dataAppointment.EndDate.GetValueOrDefault(a.AppointmentDate.AddSeconds(maxTimeInDock));
                }
                else
                {
                    var arrivalTime = a.RealArrivalTime.GetValueOrDefault(a.AppointmentDate);
                    a.StartDate = arrivalTime;
                    a.EndDate = arrivalTime.AddSeconds(maxTimeInDock);
                }
            }

            return appointments;
        }

        #endregion

        #region Private

        /// <summary>
        /// Generates the grouping for the different appointments.
        /// </summary>
        /// <param name="orders">List of the orders given from the warehouse</param>
        /// <returns>List of appointments and their orders</returns>
        private static List<Appointment> GenerateAppointments(List<InputOrder> orders)
        {
            return orders.GroupBy(x => (x.VehicleCode, x.AppointmentDate, x.RealArrivalTime, x.AssignedDockId, x.IsOutbound))
                .Select(x => new Appointment()
                {
                    Id = Guid.NewGuid(),
                    AppointmentDate = x.Key.AppointmentDate,
                    RealArrivalTime = x.Key.RealArrivalTime,
                    VehicleCode = x.Key.VehicleCode,
                    AssignedDockId = x.Key.AssignedDockId,
                    InputOrders = x.Select(m => m).ToList(),
                    IsOut = x.Key.IsOutbound,
                    Workers = new List<CustomWorkerDoneJobs>()
                }).ToList();
        }

        #endregion

        #endregion
    }

    #region Auxiliar Class

    public class CustomWorkerDoneJobs(Guid workerId, DateTime initDate, DateTime endDate)
    {
        public Guid WorkerId { get; internal set; } = workerId;
        public DateTime InitDate { get; internal set; } = initDate;
        public DateTime EndDate { get; internal set; } = endDate;
    }

    #endregion

}
