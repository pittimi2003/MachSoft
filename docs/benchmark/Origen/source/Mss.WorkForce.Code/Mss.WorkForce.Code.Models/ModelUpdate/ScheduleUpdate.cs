namespace Mss.WorkForce.Code.Models.ModelUpdate
{
    public class ScheduleUpdate
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public Guid? AvailableWorkerId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? BreakProfileId { get; set; }
        public double? CustomInitHour { get; set; }
        public double? CustomEndHour { get; set; }
    }
}
