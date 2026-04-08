using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Area : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public double? xInit { get; set; }
        public double? yInit { get; set; }
        public double? xEnd { get; set; }
        public double? yEnd { get; set; }
        public Guid? AlternativeAreaId { get; set; }
        public Area? AlternativeArea { get; set; }
        public int? DelayedTimePerUnit { get; set; }
        public bool NarrowAisle { get; set; }
        public bool IsAutomatic { get; set; }
        public Guid LayoutId { get; set; }
        public required Layout Layout { get; set; } 

        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.AlternativeArea = context.Areas.FirstOrDefault(x => x.Id == AlternativeAreaId);
            this.Layout = context.Layouts.FirstOrDefault(x => x.Id == LayoutId)!;
        }
        public object Clone()
        {
            Area clonedArea = (Area)this.MemberwiseClone();
            clonedArea.Id = Guid.NewGuid();            
            return clonedArea;
        }
    }
}