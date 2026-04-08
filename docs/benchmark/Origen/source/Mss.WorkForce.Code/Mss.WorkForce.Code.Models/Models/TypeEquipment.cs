using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class TypeEquipment : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int Quantity { get; set; }
        public int? LoadingWaitTime { get; set; }
        public Guid WarehouseId { get; set; }
        public required Warehouse Warehouse { get; set; }
        public string? ViewPort { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId);
        }
        public object Clone()
        {
            TypeEquipment clonedTypeEquipment = (TypeEquipment)this.MemberwiseClone();
            clonedTypeEquipment.Id = Guid.NewGuid();
            return clonedTypeEquipment;
        }
    }
}