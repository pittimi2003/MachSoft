using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class AvailableWorker : IFillable, ICloneable
	{
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public Guid WorkerId { get; set; }
		public required Worker Worker { get; set; }

        public void Fill(ApplicationDbContext context)
        {
			this.Worker = context.Workers.FirstOrDefault(x => x.Id == WorkerId)!;
        }
        public object Clone()
        {
            AvailableWorker clonedAvailableWorker = (AvailableWorker)this.MemberwiseClone();
            clonedAvailableWorker.Id = Guid.NewGuid();
            return clonedAvailableWorker;
        }
    }
}
