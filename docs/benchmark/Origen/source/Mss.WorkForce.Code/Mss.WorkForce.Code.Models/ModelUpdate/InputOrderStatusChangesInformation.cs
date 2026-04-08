namespace Mss.WorkForce.Code.Models.ModelUpdate
{
    public class InputOrderStatusChangesInformation
    {
        public Guid NotificationId { get; set; }
        public string ProcessType { get; set; }
        public string Worker { get; set; }
        public string? Priority { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string EquipmentGroup { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public string InputOrder { get; set; }
        public bool IsStarted { get; set; }
        public string Status { get; set; }
        public bool IsOutbound { get; set; }
        public bool AllowPartialClosed { get; set; }
        public bool AllowGroup { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? RealArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? RealDepartureTime { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Carrier { get; set; }
        public string Account { get; set; }
        public string Supplier { get; set; }
        public string Trailer { get; set; }
        public bool IsEstimated { get; set; }
        public string AssignedDock { get; set; }
        public string PreferredDock { get; set; }
        public string Warehouse { get; set; }
        public string ZoneCode { get; set; }
    }
}
