using Mss.WorkForce.Code.Models.DBContext;
using System.Collections.Generic;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Strategy : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public bool IsActive { get; set; }
        public Guid WarehouseId { get; set; }
        public required Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
        public object Clone()
        {
            Strategy clonedStrategy = (Strategy)this.MemberwiseClone();
            clonedStrategy.Id = Guid.NewGuid();
            return clonedStrategy;
        }
    }
}
