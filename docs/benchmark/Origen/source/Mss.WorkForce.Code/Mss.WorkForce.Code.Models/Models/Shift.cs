using Mss.WorkForce.Code.Models.DBContext;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Shift : IFillable,ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public double InitHour { get; set; }
        public double EndHour { get; set; }
        public Guid WarehouseId { get; set; }
		public required Warehouse Warehouse { get; set; }
        [NotMapped]
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<Schedule> Schedules { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId);
        }
        public object Clone()
        {
            Shift clonedShift = (Shift)this.MemberwiseClone();
            clonedShift.Id = Guid.NewGuid();
            return clonedShift;
        }
    }
}
