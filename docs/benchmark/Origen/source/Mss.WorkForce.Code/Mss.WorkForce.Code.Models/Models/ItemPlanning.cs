using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class ItemPlanning : IFillable
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public Process Process { get; set; }
        public bool IsOutbound { get; set; }
        private DateTime limitDate;
        public DateTime LimitDate
        {
            get => limitDate;
            set => limitDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
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
        public bool IsStored { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsStarted { get; set; }
        public Guid WorkOrderPlanningId { get; set; }
        public WorkOrderPlanning WorkOrderPlanning { get; set; }
        public Guid? WorkerId { get; set; }
        public Worker? Worker { get; set; }
        public Guid? EquipmentGroupId { get; set; }
        public EquipmentGroup? EquipmentGroup { get; set; }
        public bool? IsFaked { get; set; }
        public double Progress { get; set; } = 0;
        public Guid? ShiftId { get; set; }
        public Shift? Shift { get; set; }
        public Guid? StageId { get; set; }
        public Stage? Stage { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
            WorkOrderPlanning = context.WorkOrdersPlanning.FirstOrDefault(x => x.Id == WorkOrderPlanningId)!;
            Worker = context.Workers.FirstOrDefault(x => x.Id == WorkerId);
            EquipmentGroup = context.EquipmentGroups.FirstOrDefault(x => x.Id == EquipmentGroupId);
            Shift = context.Shifts.FirstOrDefault(x => x.Id == ShiftId);
            this.Stage = context.Stages.FirstOrDefault(x => x.Id == StageId);
        }
    }
}
