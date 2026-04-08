using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class WorkOrderPlanning : IFillable
    {
        public Guid Id { get; set; }
        public bool IsOutbound { get; set; }
        private DateTime appointmentDate;
        public DateTime AppointmentDate
        {
            get => appointmentDate;
            set => appointmentDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime initDate;
        public DateTime InitDate
        {
            get => initDate;
            set => initDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime endDate;
        public DateTime EndDate
        {
            get => endDate;
            set => endDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public double WorkTime { get; set; }
        public bool IsEstimated { get; set; }
        public bool IsStored { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsStarted { get; set; }
        public string Status { get; set; }
        public string? Priority { get; set; }

        [MeasureAttributes(Enums.MeasuresType.Percent)]
        public double Progress { get; set; }
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid? InputOrderId { get; set; }
        public InputOrder? InputOrder { get; set; }
        public Guid? AssignedDockId { get; set; }
        public Dock? AssignedDock { get; set; }
        public bool IsOnTime { get; set; }
        public bool IsInVehicleTime { get; set; }
        public double? OrderDelay { get; set; }
        public DateTime? SLATarget { get; set; }
        public bool SLAMet { get; set; }
        public DateTime? AppointmentEndDate { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
            this.InputOrder = context.InputOrders.FirstOrDefault(x => x.Id == InputOrderId)!;
            this.Priority = this.InputOrder.Priority;
            this.AssignedDock = context.Docks.FirstOrDefault(x => x.Id != AssignedDockId);
        }
    }
}
