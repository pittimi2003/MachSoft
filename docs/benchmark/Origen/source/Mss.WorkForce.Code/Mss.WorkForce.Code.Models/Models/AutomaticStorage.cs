using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class AutomaticStorage : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public int? Capacity { get; set; }
        public Guid ZoneId { get; set; }
        public required Zone Zone { get; set; }
        public string? ViewPort { get; set; }
        public int? NumCrossAisles { get; set; }
        public int? NumShelves { get; set; }
        public bool IsVertical { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Zone = context.Zones.FirstOrDefault(x => x.Id == ZoneId);
        }
        public object Clone()
        {
            AutomaticStorage clonedAutomatic = (AutomaticStorage)this.MemberwiseClone();
            clonedAutomatic.Id = Guid.NewGuid();
            return clonedAutomatic;
        }
    }
}
