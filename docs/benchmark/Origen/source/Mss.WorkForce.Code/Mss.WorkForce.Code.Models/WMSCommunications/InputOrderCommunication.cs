namespace Mss.WorkForce.Code.Models.WMSCommunications
{
    public class InputOrderCommunication
    {
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsOutbound { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? AssignedDockCode { get; set; }
        public string? PreferredDockCode { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public double Progress { get; set; } = 0;
        public DateTime CreationDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? VehicleCode { get; set; }
        public DateTime UpdateDateWMS { get; set; }
        public int NumLines { get; set; }
    }
}
