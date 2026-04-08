using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Model
{
    public class PriorityStateResult
    {
        public List<ProcessPriorityOrderDto> ProcessItems { get; set; } = new();
        public List<OrderPriorityDto> PriorityItems { get; set; } = new();
    }
}
