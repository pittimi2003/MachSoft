using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class ProcessPriorityOrder : IFillable, ICloneable
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
            ProcessPriorityOrder clonedProcessPriorityOrder = (ProcessPriorityOrder)this.MemberwiseClone();
            clonedProcessPriorityOrder.Id = Guid.NewGuid();
            return clonedProcessPriorityOrder;
        }
    }
}
