using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class PreprocessProfile : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Type { get; set; }
		public bool AllowInInboundFlow { get; set; }
		public bool AllowInOutboundFlow { get; set; }
		public Guid WarehouseId { get; set; }
		public required Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
        public object Clone()
        {
            PreprocessProfile clonedPreprocessProfile = (PreprocessProfile)this.MemberwiseClone();
            clonedPreprocessProfile.Id = Guid.NewGuid();
            return clonedPreprocessProfile;
        }
    }
}
