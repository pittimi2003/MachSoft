
using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class OrderLoadRatio: IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public Guid LoadId { get; set; }
        public LoadProfile Load { get; set; }
        public Guid VehicleId { get; set; }
        public VehicleProfile Vehicle { get; set; }
        public int LoadInVehicle { get; set; }
        public double OrderInLoad { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Load = context.LoadProfiles.FirstOrDefault(x => x.Id == LoadId)!;
            this.Vehicle = context.VehicleProfiles.FirstOrDefault(x => x.Id == VehicleId)!;
        }
        public object Clone()
        {
            OrderLoadRatio clonedOrderLoadRatio = (OrderLoadRatio)this.MemberwiseClone();
            clonedOrderLoadRatio.Id = Guid.NewGuid();
            return clonedOrderLoadRatio;
        }
    }    
}
