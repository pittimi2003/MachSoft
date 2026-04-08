namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class DockYardTask : GanttTaskBase
    {
        public int MaxEquipments { get; set; }
        public int MaxStockCrossdocking { get; set; }
        public bool AllowInbound { get; set; }
        public bool AllowOutbound { get; set; }
        public string ZoneName { get; set; }
    }
}
