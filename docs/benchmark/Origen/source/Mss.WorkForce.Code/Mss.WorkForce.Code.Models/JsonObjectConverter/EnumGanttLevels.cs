using System.ComponentModel;

namespace Mss.WorkForce.Code.Web
{
    internal enum EnumGanttLevels
    {
        [Description("None")]
        None = 0,
        [Description("Planning")]
        Planning = 1,
        [Description("Warehouse process")]
        WarehouseProcess = 2,
        [Description("Estimated work load")]
        EstimatedWorkLoad = 3,
        [Description("Inbound work")]
        InboundWork = 4,
        [Description("Outbound work")]
        OutboundWork = 5,
    }
}
