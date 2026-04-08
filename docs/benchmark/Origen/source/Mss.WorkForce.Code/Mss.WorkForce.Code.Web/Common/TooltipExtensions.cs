using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Web.Common
{
    public static class TooltipExtensions
    {
        //Tooltip text content
        public static readonly List<(AreaType Type, string Name, string Description)> AreaTypes = new()
        {
            (AreaType.Dock, "Dock", "Area where goods are received or shipped, usually linked to inbound or outbound flows."),
            (AreaType.Storage, "Storage", "Area used to store products or materials for medium or long-term periods."),
            (AreaType.Buffer, "Buffer", "Temporary holding area used to manage flow imbalances between processes."),
            (AreaType.Stage, "Stage", "Preparation area where goods are organized before the next process step (e.g., packing, loading).")
        };
    }
}
