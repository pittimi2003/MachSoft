namespace Mss.WorkForce.Code.WMSSimulator.WMSModel
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
        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { date = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
        }
        public Guid WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
    }
}
