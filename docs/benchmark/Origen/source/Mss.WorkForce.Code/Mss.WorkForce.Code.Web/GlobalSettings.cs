using Mss.WorkForce.Code.Web.Model.Enums;
namespace Mss.WorkForce.Code.Web
{
    public class GlobalSettings
    {
        public bool widgetOrientation { get; set; }
        public int AlertIntervalSeconds { get; set; }
        public eGroupWidgets GroupWidgets { get; set; }
        public Guid CurrentPlanning { get; set; }
        public bool IsReloadData { get; set; }
        public double UtcWarehouse { get; set; }
        public string HourFormat { get; set; } = string.Empty;
    }
}
