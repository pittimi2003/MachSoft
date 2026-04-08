namespace Mss.WorkForce.Code.Models.Models
{
    public class WarehouseProcessClosing
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public Warehouse Warehouse { get; set; }
        public Guid WarehouseId { get; set; }
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
        public int? NumProcesses { get; set; }
        public string? ZoneCode { get; set; }
    }
}
