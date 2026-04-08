namespace Mss.WorkForce.Code.Models.DTO
{
    public class ChangePriorityDto
    {
        public List<Guid> WorkOrderId { get; set; }
        public string Priority { get; set; }
    }

}
