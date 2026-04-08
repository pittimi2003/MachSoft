using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class OrderPriority : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public int Priority { get; set; }
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public bool IsActive { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }

        public object Clone()
        {
            OrderPriority clonedOrderPriority = (OrderPriority)this.MemberwiseClone();
            clonedOrderPriority.Id = Guid.NewGuid();
            return clonedOrderPriority;
        }
    }
}
