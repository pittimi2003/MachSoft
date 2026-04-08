using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Worker : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public Guid TeamId { get; set; }
		public required Team Team { get; set; }
		public Guid RolId { get; set; }
		public required Rol Rol { get; set; }
		public int? WorkerNumber { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Team = context.Teams.FirstOrDefault(x => x.Id == TeamId);
            this.Rol = context.Roles.FirstOrDefault(x => x.Id == RolId);
        }
        public object Clone()
        {
            Worker clonedWorker = (Worker)this.MemberwiseClone();
            clonedWorker.Id = Guid.NewGuid();
            return clonedWorker;
        }
    }
}
