namespace Mss.WorkForce.Code.Models.DTO
{
    public class OrderScheduleDto : PenelEditorOperations, ICloneable
    {

        #region Properties

        public OperationType DataOperationType { get; set; }
        public TimeSpan EndHour { get; set; }
        public Guid Id { get; set; }
        public TimeSpan InitHour { get; set; }
        public Guid LoadId { get; set; }
        public string LoadName { get; set; } = string.Empty;
        public string Name {  get; set; } = string.Empty;
        public double NumberVehicles { get; set; } = 0;
        public Guid VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;

        #endregion

        #region Methods

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not OrderScheduleDto order)
                return false;

            return Id == order.Id &&
                LoadId == order.LoadId &&
                VehicleId == order.VehicleId && 
                NumberVehicles == order.NumberVehicles &&
                InitHour == order.InitHour && 
                EndHour == order.EndHour;
        }

        public object GetProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
