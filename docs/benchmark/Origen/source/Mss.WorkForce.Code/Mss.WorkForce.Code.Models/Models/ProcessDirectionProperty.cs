using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class ProcessDirectionProperty : IFillable,ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public double Percentage { get; set; }
		public Guid InitProcessId { get; set; }
		public required Process InitProcess { get; set; }
		public Guid EndProcessId { get; set; }
		public required Process EndProcess { get; set; }
		public bool IsEnd { get; set; }

        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.InitProcess = context.Processes.FirstOrDefault(x => x.Id == InitProcessId)!;
            this.EndProcess = context.Processes.FirstOrDefault(x => x.Id == EndProcessId)!;
        }
        public object Clone()
        {
            ProcessDirectionProperty clonedProcessDirectionProperty = (ProcessDirectionProperty)this.MemberwiseClone();
            clonedProcessDirectionProperty.Id = Guid.NewGuid();
            return clonedProcessDirectionProperty;
        }
    }
}
