namespace Mss.WorkForce.Code.WMSSimulator.WMSModel
{
    public class InputOrder
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; }
        public bool IsOut { get; set; }
        public string? PreferedDockCode { get; set; }
        public string Status { get; set; }
        public double Progress { get; set; }
        public int NumLines { get; set; }
        public DateTime UpdateDate { get; set; }
        public TimeSpan MarginTime { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public Guid? WorkForceTaskId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
