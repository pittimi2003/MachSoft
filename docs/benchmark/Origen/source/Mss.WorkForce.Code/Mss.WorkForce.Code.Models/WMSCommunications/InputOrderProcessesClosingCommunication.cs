namespace Mss.WorkForce.Code.Models.WMSCommunications
{
    public class InputOrderProcessesClosingCommunication
    {
        public string ProcessType { get; set; } = string.Empty;
        public string Worker { get; set; } = string.Empty;
        public string EquipmentGroup { get; set; } = string.Empty;
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public string InputOrder { get; set; } = string.Empty;
        public int NumProcesses { get; set; }
        public string Zone { get; set; } = string.Empty;
        public Guid NotificationId { get; set; }
    }
}
