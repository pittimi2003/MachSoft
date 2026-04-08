namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class CustomFlowGraphDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public Guid? FlowId { get; set; }
        public FlowDto? FlowDto { get; set; }
     
        public Guid? WarehouseId { get; set; }
    }
}
