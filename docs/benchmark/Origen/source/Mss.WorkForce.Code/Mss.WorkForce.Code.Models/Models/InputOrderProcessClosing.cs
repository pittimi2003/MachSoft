namespace Mss.WorkForce.Code.Models.Models
{
    public class InputOrderProcessClosing
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public string ProcessType { get; set; }
        public string Worker { get; set; }
        public string EquipmentGroup { get; set; }
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
        public string InputOrder { get; set; }
        public int? NumProcesses { get; set; }
        public string? ZoneCode { get; set; }
    }
}
