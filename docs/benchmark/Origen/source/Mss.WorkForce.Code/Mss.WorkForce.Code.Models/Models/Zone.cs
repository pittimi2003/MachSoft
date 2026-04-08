using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Zone : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public double? xInit { get; set; }
        public double? yInit { get; set; }
        public double? xEnd { get; set; }
        public double? yEnd { get; set; }
        public int MaxStockToBook { get; set; }
        public bool IsLimitStock { get; set; }
        public int? MaxStock { get; set; }
        public int? MaxContainers { get; set; }
        public int InitStockToBook { get; set; }
        public Guid AreaId { get; set; }
        public required Area Area { get; set; }
        public string? ViewPort { get; set; }
        public int? MaxEquipments { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Area = context.Areas.FirstOrDefault(x => x.Id == AreaId);
        }
        public object Clone()
        {
            Zone clonedStation = (Zone)this.MemberwiseClone();
            clonedStation.Id = Guid.NewGuid();
            return clonedStation;
        }
    }
}
