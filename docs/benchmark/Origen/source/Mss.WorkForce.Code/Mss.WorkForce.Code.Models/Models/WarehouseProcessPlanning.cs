using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class WarehouseProcessPlanning : IFillable
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid ProcessId { get; set; }
        public Process Process { get; set; }
        private DateTime? limitDate;
        public DateTime? LimitDate
        {
            get => limitDate;
            set => limitDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
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
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid? WorkerId { get; set; }
        public Worker? Worker { get; set; }
        public Guid? EquipmentGroupId { get; set; }
        public EquipmentGroup? EquipmentGroup { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
            Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
            Worker = context.Workers.FirstOrDefault(x => x.Id == WorkerId);
            EquipmentGroup = context.EquipmentGroups.FirstOrDefault(x => x.Id == EquipmentGroupId);
        }
    }
}
