using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.DTO;
namespace Mss.WorkForce.Code.Models.Models
{
    public class OrderSchedule : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required TimeSpan InitHour { get; set; }
        public required TimeSpan EndHour { get; set; }
        public required Guid VehicleId { get; set; }
        public required VehicleProfile Vehicle { get; set; }
        public required double NumberVehicles { get; set; }
        public required Guid LoadId { get; set; }
        public required LoadProfile Load { get; set; }
        public bool IsOut { get; set; }
        public Guid WarehouseId { get; set; }
        public required Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
            this.Vehicle = context.VehicleProfiles.FirstOrDefault(x => x.Id == VehicleId)!;
            this.Load = context.LoadProfiles.FirstOrDefault(x => x.Id == LoadId)!;
        }
        public object Clone()
        {
            OrderSchedule clonedOrderSchedule = (OrderSchedule)this.MemberwiseClone();
            clonedOrderSchedule.Id = Guid.NewGuid();
            return clonedOrderSchedule;
        }
    }
}
