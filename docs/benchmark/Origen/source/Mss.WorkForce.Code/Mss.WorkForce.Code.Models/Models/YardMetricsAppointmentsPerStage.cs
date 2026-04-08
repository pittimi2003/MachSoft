using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class YardMetricsAppointmentsPerStage : IFillable
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
        public Stage Stage { get; set; }
        public Guid StageId { get; set; }
        public string VehicleType { get; set; }
        public string License { get; set; }
        public double Progress { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public Planning Planning { get; set; }
        public Guid PlanningId { get; set; }
        public YardMetricsPerStage YardMetricsPerStage { get; set; }
        public Guid YardMetricsPerStageId { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Stage = context.Stages.FirstOrDefault(x => x.Id == StageId)!;
            this.Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
            this.YardMetricsPerStage = context.YardMetricsPerStage.FirstOrDefault(x => x.Id == YardMetricsPerStageId)!;
        }
    }
}
