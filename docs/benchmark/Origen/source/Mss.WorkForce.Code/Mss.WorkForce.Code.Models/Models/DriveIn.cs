using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class DriveIn : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public int? Capacity { get; set; }
        public Guid ZoneId { get; set; }
        public required Zone Zone { get; set; }
        public string? ViewPort { get; set; }
        public int? NumCrossAisles { get; set; }
        public int? NumShelves { get; set; }
        public bool IsVertical { get; set; }
        public bool? Bidirectional { get; set; }
        public bool? NarrowAisle { get; set; }
        public int? MaxPickers { get; set; }
        public int? MaxEquipments { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Zone = context.Zones.FirstOrDefault(x => x.Id == ZoneId);
        }
        public object Clone()
        {
            DriveIn clonedDriveIn = (DriveIn)this.MemberwiseClone();
            clonedDriveIn.Id = Guid.NewGuid();
            return clonedDriveIn;
        }
    }
}
