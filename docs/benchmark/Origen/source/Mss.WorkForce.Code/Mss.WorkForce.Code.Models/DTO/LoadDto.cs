namespace Mss.WorkForce.Code.Models.DTO
{
    public class LoadDto : IDataOperation, ICloneable
    {
        public Guid id { get; set; }
        public string load { get; set; }
        public Guid loadId { get; set; }
        public string vehicle { get; set; }
        public Guid vehicleId { get; set; }
        public double numberVehicle { get; set; }
        public TimeSpan hour { get; set; }
        public TimeSpan endHour { get; set; }
        public OperationType DataOperationType { get; set; }

        public HashSet<string> ModifiedFields { get; set; } = new();

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not LoadDto order)
                return false;

            return id == order.id &&
                load == order.load &&
                loadId == order.loadId &&
                vehicle == order.vehicle &&
                vehicleId == order.vehicleId &&
                numberVehicle == order.numberVehicle &&
                hour == order.hour &&
                endHour == order.endHour;
        }
        
    }
    
}
