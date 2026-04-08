using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class YardMetricsAppointmentsPerDock : IFillable
    {
        public Guid Id { get; set; }
        public string AppointmentCode { get; set; }
        public string ProcessType { get; set; }
        private DateTime appointmentDate;
        public DateTime AppointmentDate 
        { 
            get => appointmentDate; 
            set => appointmentDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
        }
        private DateTime startDate;
        public DateTime StartDate 
        { 
            get => startDate; 
            set => startDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
        }
        private DateTime endDate;
        public DateTime EndDate 
        { 
            get => endDate; 
            set => endDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
        }
        public string Customer { get; set; }
        public string YardCode { get; set; }
        public Dock Dock { get; set; }
        public Guid DockId { get; set; }
        public string VehicleType { get; set; }
        public string License { get; set; }
        public double Progress { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public Planning Planning { get; set; }
        public Guid PlanningId { get; set; }
        public YardMetricsPerDock YardMetricsPerDock { get; set; }
        public Guid YardMetricsPerDockId { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Dock = context.Docks.FirstOrDefault(x => x.Id == DockId)!;
            Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
            YardMetricsPerDock = context.YardMetricsPerDock.FirstOrDefault(x => x.Id == YardMetricsPerDockId)!;
        }
    }
}
