using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Planning : IFillable
    {
        public Guid Id { get; set; }
        private DateTime date;
        public DateTime Date
        {
            get => date;
            set => date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime creationDate;
        public DateTime CreationDate
        {
            get => creationDate;
            set => creationDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public bool IsStored { get; set; }
        public bool IsWorkforcePlanning { get; set; }
        public double? SLAWorkOrdersOnTimePercentage { get; set; }
        public double? SLAShippedStock { get; set; }
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
    }
}
