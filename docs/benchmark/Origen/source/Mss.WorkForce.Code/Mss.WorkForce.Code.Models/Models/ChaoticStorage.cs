using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class ChaoticStorage : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public int? Capacity { get; set; }
        public Guid ZoneId { get; set; }
        public required Zone Zone { get; set; }
        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Zone = context.Zones.FirstOrDefault(x => x.Id == ZoneId);
        }
        public object Clone()
        {
            ChaoticStorage clonedChaoticStorage = (ChaoticStorage)this.MemberwiseClone();
            clonedChaoticStorage.Id = Guid.NewGuid();
            return clonedChaoticStorage;
        }
    }
}
