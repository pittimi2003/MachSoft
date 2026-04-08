namespace Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared
{
    public class WorkForceTask
    {
        public Guid Id { get; set; }
        public TimeSpan InitHour { get; set; }
        public TimeSpan EndHour { get; set; }
        public double NumOrders { get; set; }
        public int NumOrdersCompleted { get; set; }
        public int LinesPerOrder { get; set; }
        public bool IsOut { get; set; }
        public DateTime Date { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
    }
}
