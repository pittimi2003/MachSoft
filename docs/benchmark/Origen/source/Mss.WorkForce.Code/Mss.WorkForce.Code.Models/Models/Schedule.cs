using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Schedule : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
        private DateTime date;
        public DateTime Date
        {
            get => date;
            set => date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
		public Guid AvailableWorkerId { get; set; }
		public required AvailableWorker AvailableWorker { get; set; }
		public Guid ShiftId { get; set; }
		public required Shift Shift { get; set; }
		public Guid BreakProfileId { get; set; }
		public required BreakProfile BreakProfile { get; set; }
        public double? CustomInitHour { get; set; }
        public double? CustomEndHour { get; set; }
        public bool? Available { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.AvailableWorker = context.AvailableWorkers.FirstOrDefault(x => x.Id == AvailableWorkerId)!;
            this.Shift = context.Shifts.FirstOrDefault(x => x.Id == ShiftId)!;
            this.BreakProfile = context.BreakProfiles.FirstOrDefault(x => x.Id == BreakProfileId)!;
        }

        public object Clone()
        {
            Schedule clonedSchedule = (Schedule)this.MemberwiseClone();
            clonedSchedule.Id = Guid.NewGuid();
            return clonedSchedule;
        }
    }
}
