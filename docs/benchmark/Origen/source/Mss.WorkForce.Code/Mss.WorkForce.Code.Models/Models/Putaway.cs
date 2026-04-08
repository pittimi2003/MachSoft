using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Putaway : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public int? AdditionTmeToPutaway { get; set; }
		public int? MinHour { get; set; }
		public Guid ProcessId { get; set; }
		public required Process Process { get; set; }

        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }
        public object Clone()
        {
            Putaway clonedPutaway = (Putaway)this.MemberwiseClone();
            clonedPutaway.Id = Guid.NewGuid();
            return clonedPutaway;
        }
    }
}
