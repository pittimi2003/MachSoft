using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Loading : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public string? Dock { get; set; }
		public int? AutomaticLoadingTime { get; set; }
		public int? AdditionalTimeInBuffer { get; set; }
		public Guid ProcessId { get; set; }
		public required Process Process { get; set; }
        public string? Viewport { get; set; }
        public void Fill(ApplicationDbContext context)
        {
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }
        public object Clone()
        {
            Loading clonedLoading = (Loading)this.MemberwiseClone();
            clonedLoading.Id = Guid.NewGuid();
            return clonedLoading;
        }
    }
}
