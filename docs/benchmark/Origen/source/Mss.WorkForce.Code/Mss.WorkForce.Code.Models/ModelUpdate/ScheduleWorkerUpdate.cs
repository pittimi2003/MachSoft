namespace Mss.WorkForce.Code.Models.ModelUpdate
{
    public class ScheduleWorkerUpdate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid WorkerId { get; set; }
        public Guid AvailableWorkerId { get; set; }
        public Guid ShiftId { get; set; }
        public Guid BreakProfileId { get; set; }

    }
}
