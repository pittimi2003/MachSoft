using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class SLAConfig : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public double Value { get; set; }
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }

        public object Clone()
        {
            SLAConfig clonedSLAConfig = (SLAConfig)this.MemberwiseClone();
            clonedSLAConfig.Id = Guid.NewGuid();
            return clonedSLAConfig;
        }
    }
}
