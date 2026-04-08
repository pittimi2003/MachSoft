using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Layout : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public double? Height { get; set; }
        public double? Width { get; set; }
        public Guid WarehouseId { get; set; }
        public required Warehouse Warehouse { get; set; }
        public string? Viewport { get; set; }
        private DateTime creationDate;
        public DateTime CreationDate
        {
            get => creationDate;
            set => creationDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }

        public object Clone()
        {
            Layout clonedLayout = (Layout)this.MemberwiseClone();
            clonedLayout.Id = Guid.NewGuid();
            clonedLayout.Name = $"{clonedLayout.Name}_Cloned";
            return clonedLayout;
        }
    }
}
