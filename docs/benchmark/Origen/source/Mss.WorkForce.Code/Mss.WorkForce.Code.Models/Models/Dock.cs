using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Dock : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public int MaxEquipments { get; set; }
        public bool OperatesFromBuffer { get; set; }
        public double? OverloadHandling { get; set; }
        public int? InboundRange { get; set; }
        public int? OutboundRange { get; set; }
        public int MaxStockCrossdocking { get; set; }
        public bool AllowInbound { get; set; }
        public bool AllowOutbound { get; set; }
        public Guid ZoneId { get; set; }
        public required Zone Zone { get; set; }
        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Zone = context.Zones.FirstOrDefault(x => x.Id == ZoneId)!;
        }
        public object Clone()
        {
            Dock clonedDock = (Dock)this.MemberwiseClone();
            clonedDock.Id = Guid.NewGuid();
            return clonedDock;
        }
    }
}
