using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Widget : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Entity { get; set; }
		public required string Type { get; set; }
        private DateTime creationDate;
        public DateTime CreationDate
        {
            get => creationDate;
            set => creationDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
		public required string XML { get; set; }
		public Guid WarehouseId { get; set; }
		public required Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
        public object Clone()
        {
            Widget clonedWidget = (Widget)this.MemberwiseClone();
            clonedWidget.Id = Guid.NewGuid();
            return clonedWidget;
        }
    }
}
