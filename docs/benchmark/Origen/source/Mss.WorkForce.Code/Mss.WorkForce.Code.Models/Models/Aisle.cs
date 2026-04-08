using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Aisle : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public int? MaxMHE { get; set; }
        public int? AdditionalTimePerUnitEntry { get; set; }
        public int? AdditionalTimePerUnitExit { get; set; }
        public int? MaxTasks { get; set; }
        public int? AisleChangeTime { get; set; }
        public bool NarrowAisle { get; set; }
        public int? MaxPickers { get; set; }
        public bool EndPicking { get; set; }
        public bool Bidirectional { get; set; }
        public int? WidthPerDirection { get; set; }
        public int? MaxMHEPickOrPutaway { get; set; }
        public bool ReplenishmentControl { get; set; }
        public int? MaxMovement { get; set; }
        public Guid ZoneId { get; set; }
        public required Zone Zone { get; set; }

        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Zone = context.Zones.FirstOrDefault(x => x.Id == ZoneId)!;
        }
        public object Clone()
        {
            Aisle clonedAisle = (Aisle)this.MemberwiseClone();
            clonedAisle.Id = Guid.NewGuid();
            return clonedAisle;
        }
    }
}
