using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class CustomProcess : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public double? InitHour { get; set; }
		public double? EndHour { get; set; }
		public double? Percentage { get; set; }
		public double? NumPossibleTimes { get; set; }
		public Guid ProcessId { get; set; }
		public required Process Process { get; set; }
        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }
        public object Clone()
        {
            CustomProcess clonedCustomProcess = (CustomProcess)this.MemberwiseClone();
            clonedCustomProcess.Id = Guid.NewGuid();
            return clonedCustomProcess;
        }
    }
}
