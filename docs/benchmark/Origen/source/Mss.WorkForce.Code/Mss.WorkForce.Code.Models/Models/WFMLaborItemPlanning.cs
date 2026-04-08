namespace Mss.WorkForce.Code.Models.Models
{
    public class WFMLaborItemPlanning
    {
        public Guid Id { get; set; }
        public Guid WorkerId { get; set; }
        public Worker Worker { get; set; }
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid EquipmentGroupId { get; set; }
        public EquipmentGroup EquipmentGroup { get; set; }
        public Guid InputOrderId { get; set; }
        public InputOrder InputOrder { get; set; }
        public Guid WFMLaborPerProcessTypeId { get; set; }
        public WFMLaborWorkerPerProcessType WFMLaborPerProcessType { get; set; }
        public Guid WFMLaborEquipmentPerProcessTypeId { get; set; }
        public WFMLaborEquipmentPerProcessType WFMLaborEquipmentPerProcessType { get; set; }
        public double Progress { get; set; }
        public double WorkTime { get; set; }
    }
}
