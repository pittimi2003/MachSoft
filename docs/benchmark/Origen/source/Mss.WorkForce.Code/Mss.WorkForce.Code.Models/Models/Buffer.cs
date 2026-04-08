using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Buffer : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public int EntryCapacity { get; set; }
        public int ExitCapacity { get; set; }
        public bool CapacityByMaterial { get; set; }
        public bool Excess { get; set; }
        public int MaxEquipments { get; set; }
        public Guid ZoneId { get; set; }
        public required Zone Zone { get; set; }
        public string? ViewPort { get; set; }
        public string? Type { get; set; }
        public int? NumCrossAisles { get; set; }
        public int? NumShelves { get; set; }
        public bool? IsVertical { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Zone = context.Zones.FirstOrDefault(x => x.Id == ZoneId)!;
        }
        public object Clone()
        {
            Buffer clonedBuffer = (Buffer)this.MemberwiseClone();
            clonedBuffer.Id = Guid.NewGuid();
            return clonedBuffer;
        }
    }
}
