namespace Mss.WorkForce.Code.Models.Models
{
    public class YardAppointmentsNotifications
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public string AppointmentCode { get; set; }
        public string? YardCode { get; set; }
        public string VehicleCode { get; set; }
        public string VehicleType { get; set; }
        public string? DockCode { get; set; }
        private DateTime appointmentDate;
        public DateTime AppointmentDate
        {
            get => appointmentDate;
            set => appointmentDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime? initDate;
        public DateTime? InitDate
        {
            get => initDate;
            set => initDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        private DateTime? endDate;
        public DateTime? EndDate
        {
            get => endDate;
            set => endDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        public string Customer { get; set; }
        public string License { get; set; }
    }
}
