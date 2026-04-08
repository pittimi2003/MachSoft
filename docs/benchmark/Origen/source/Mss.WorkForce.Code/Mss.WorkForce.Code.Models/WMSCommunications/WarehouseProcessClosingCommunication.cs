using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.WMSCommunications
{
    public class WarehouseProcessClosingCommunicaation
    {
        public string ProcessType { get; set; } = string.Empty;
        public string Worker { get; set; } = string.Empty;
        public string EquipmentGroup { get; set; } = string.Empty;
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Task { get; set; } = string.Empty;
        public int NumProcesses { get; set; }
        public string Zone { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public Guid NotificationId { get; set; }
    }
}
