using Mss.WorkForce.Code.Models.ModelGantt;

namespace Mss.WorkForce.Code.Models.DTO.Preview
{
    public class VehicleLoadDto: IDataOperation, ICloneable
    {
        public Guid id { get; set; }
        public bool isOut {  get; set; }
        public string load { get; set; }
        public Guid loadId { get; set; }
        public string vehicle { get; set; }
        public Guid vehicleId { get; set; }
        public double numberVehicle { get; set; }
        public TimeSpan hour { get; set; }
        public TimeSpan endHour { get; set; }
        public OperationType DataOperationType { get; set; }

        public bool IsNew { get; set; }
        public HashSet<string> ModifiedFields { get; set; } = new();

        
        public List<VehicleMetricsDto> Errors { get; set; } = new();

        public List<VehicleMetricsDto> AllMetrics { get; set; } = new();

        public string MetricsStatus => (Errors != null && Errors.Count > 0) ? "Error" : "Success";

        public object Clone()
        {
            return MemberwiseClone();
        }

        public VehicleLoadDto DeepClone()
        {
            return new VehicleLoadDto
            {
                id = this.id,
                isOut = this.isOut,
                load = this.load,
                loadId = this.loadId,
                vehicle = this.vehicle,
                vehicleId = this.vehicleId,
                numberVehicle = this.numberVehicle,
                hour = this.hour,
                endHour = this.endHour,
                DataOperationType = this.DataOperationType,
                IsNew = this.IsNew,
                ModifiedFields = this.ModifiedFields != null ? new HashSet<string>(this.ModifiedFields) : new HashSet<string>(),
                Errors = this.Errors != null ? Errors.Select(e => e.DeepClone()).ToList() : new List<VehicleMetricsDto>(),
                AllMetrics = this.AllMetrics != null ? AllMetrics.Select(e => e.DeepClone()).ToList() : new List<VehicleMetricsDto>(),               
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not LoadDto order)
                return false;

            return id == order.id &&
                load == order.load &&
                vehicle == order.vehicle &&
                numberVehicle == order.numberVehicle &&
                hour == order.hour &&
                endHour == order.endHour;
        }
    }
}
