using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class RolProcessSequence : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public Guid RolId { get; set; }
		public required Rol Rol { get; set; }
		public Guid ProcessId { get; set; }
		public required Process Process { get; set; }
		public int Sequence { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Rol = context.Roles.FirstOrDefault(x => x.Id == RolId)!;
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }
        public object Clone()
        {
            RolProcessSequence clonedRolProcessSequence = (RolProcessSequence)this.MemberwiseClone();
            clonedRolProcessSequence.Id = Guid.NewGuid();
            return clonedRolProcessSequence;
        }
    }
}
