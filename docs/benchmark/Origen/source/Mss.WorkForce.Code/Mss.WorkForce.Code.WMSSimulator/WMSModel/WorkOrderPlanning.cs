namespace Mss.WorkForce.Code.WMSSimulator.WMSModel
{
    public class WorkOrderPlanning
    {
        public Guid Id { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid PlanningId { get; set; }
        public Guid? InputOrderId { get; set; }
        public bool IsOutbound { get; set; }
    }
}
