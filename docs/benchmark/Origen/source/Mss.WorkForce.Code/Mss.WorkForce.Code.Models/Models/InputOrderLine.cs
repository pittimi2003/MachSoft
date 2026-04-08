using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class InputOrderLine : IFillable
    {
        public Guid Id { get; set; }
        public string? Product { get; set; }
        public double? Quantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool? IsClosed { get; set; }
        public Guid InputOutboundOrderId { get; set; }
        public required InputOrder InputOutboundOrder { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.InputOutboundOrder = context.InputOrders.FirstOrDefault(x => x.Id == InputOutboundOrderId)!;
        }
    }
}


