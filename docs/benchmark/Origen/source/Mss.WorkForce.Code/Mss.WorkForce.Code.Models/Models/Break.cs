using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Break : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public double  InitBreak { get; set; }
		public double  EndBreak { get; set; }
		public bool? IsPaid { get; set; }
		public bool? IsRequiered { get; set; }
		public Guid BreakProfileId { get; set; }
		public required BreakProfile BreakProfile { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.BreakProfile = context.BreakProfiles.FirstOrDefault(x => x.Id == BreakProfileId)!;
        }
        public object Clone()
        {
            Break clonedBreak = (Break)this.MemberwiseClone();
            clonedBreak.Id = Guid.NewGuid();
            return clonedBreak;
        }
    }
}
