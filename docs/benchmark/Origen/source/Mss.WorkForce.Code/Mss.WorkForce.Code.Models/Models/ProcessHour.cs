using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class ProcessHour : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public Guid ProcessId { get; set; }
		public required Process Process { get; set; }
		public int Hour { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }

        public object Clone()
        {
            ProcessHour clonedProcessHour = (ProcessHour)this.MemberwiseClone();
            clonedProcessHour.Id = Guid.NewGuid();
            return clonedProcessHour;
        }
    }
}
