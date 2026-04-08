namespace Mss.WorkForce.Code.Models.ModelUpdate
{
    public class OrderScheduleUpdate
    {
        public Guid Id { get; set; }
        public TimeSpan? InitHour { get; set; }
        public TimeSpan? EndHour { get; set; }
        public Guid? VehicleId { get; set; }
        public double? NumberVehicles { get; set; }
        public Guid? LoadId { get; set; }
        public bool? IsOut { get; set; }
        public Guid? WarehouseId { get; set; }
    }
}
