using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Picking : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public int? PickingRoadTime { get; set; }
		public Guid ProcessId { get; set; }
		public required Process Process { get; set; }
        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }
        public object Clone()
        {
            Picking clonedPicking = (Picking)this.MemberwiseClone();
            clonedPicking.Id = Guid.NewGuid();
            return clonedPicking;
        }
    }
}
