namespace Mss.WorkForce.Code.WMSSimulator.WMSModel
{
    public class ItemPlanning
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid WorkOrderPlanningId { get; set; }
        public Guid? WorkerId { get; set; }
        public Guid? EquipmentGroupId { get; set; }
        public bool IsDrawn { get; set; }
        public Guid ZoneId { get; set; }
    }
}
