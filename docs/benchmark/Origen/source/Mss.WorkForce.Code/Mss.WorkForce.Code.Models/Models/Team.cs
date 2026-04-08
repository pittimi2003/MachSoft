using Mss.WorkForce.Code.Models.DBContext;
using System.Collections.Generic;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Team : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
        public string? Description { get; set; }
		public Guid WarehouseId { get; set; }
		public required Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
        public object Clone()
        {
            Team clonedTeam = (Team)this.MemberwiseClone();
            clonedTeam.Id = Guid.NewGuid();
            return clonedTeam;
        }
    }
}
