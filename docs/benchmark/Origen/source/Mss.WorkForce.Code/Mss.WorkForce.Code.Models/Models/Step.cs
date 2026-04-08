using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Step : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public int? TimeMin { get; set; }
		public int Order { get; set; }
		public bool InitProcess { get; set; }
		public bool EndProcess { get; set; }
		public Guid ProcessId { get; set; }
		public required Process Process { get; set; }
        public string? ViewPort { get; set; }
        public void Fill(ApplicationDbContext context)
        {
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }
        public object Clone()
        {
            Step clonedStep = (Step)this.MemberwiseClone();
            clonedStep.Id = Guid.NewGuid();
            return clonedStep;
        }
    }
}
