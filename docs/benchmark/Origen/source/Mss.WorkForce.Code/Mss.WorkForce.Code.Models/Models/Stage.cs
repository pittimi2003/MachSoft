using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Stage : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public Guid ZoneId { get; set; }
        public Zone Zone { get; set; }
        public int? EntryCapacity { get; set; }
        public int? ExitCapacity { get; set; }
        public int MaxEquipments { get; set; }
        public bool MixCarriers { get; set; }
        public bool IsOut { get; set; }
        public bool IsIn { get; set; }
        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Zone = context.Zones.FirstOrDefault(x => x.Id == ZoneId)!;
        }

        public object Clone()
        {
            Stage clonedStage = (Stage)this.MemberwiseClone();
            clonedStage.Id = Guid.NewGuid();
            return clonedStage;
        }
    }
}
