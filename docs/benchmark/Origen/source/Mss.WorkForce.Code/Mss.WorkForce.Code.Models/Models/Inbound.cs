using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Inbound : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public int Quantity { get; set; }
		public string? UnitOfMeasure { get; set; }
		public int VehiclePerHour { get; set; }
		public int TruckPerDay { get; set; }
		public int? MinTimeInBuffer { get; set; }
        public int? LoadTime { get; set; }
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
            Inbound clonedInbound = (Inbound)this.MemberwiseClone();
            clonedInbound.Id = Guid.NewGuid();
            return clonedInbound;
        }
    }
}
