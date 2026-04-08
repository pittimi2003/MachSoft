namespace Mss.WorkForce.Code.Models.Models
{
    public class PlanningResourcesView
    {
        public DateTime When { get; set; }
        private DateTime initDate;
        public DateTime InitDate
        {
            get => initDate;
            set => initDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime endDate;
        public DateTime EndDate
        {
            get => endDate;
            set => endDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public Guid ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string ResourceType { get; set; }
        public Guid ResourceId { get; set; }
        public int ResourceName { get; set; }
        public int AvailableResources { get; set; }

    }
}
