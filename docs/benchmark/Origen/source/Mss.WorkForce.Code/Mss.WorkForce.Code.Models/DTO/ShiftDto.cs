namespace Mss.WorkForce.Code.Models.DTO
{
    public class ShiftDto : IDataOperation, ICloneable
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public TimeSpan initHour { get; set; }
        public TimeSpan endHour { get; set; }
        public OperationType DataOperationType { get; set; }
        public List<ShiftSheduleDto> schedules { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public ShiftDto DeepClone()
        {
            return new ShiftDto
            {
                id = this.id,
                name = this.name,
                initHour = this.initHour,
                endHour = this.endHour,
                DataOperationType = this.DataOperationType,
                schedules = new (this.schedules.Select(x => x.DeepClone())),
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ShiftDto shift)
                return false;

            return id == shift.id &&
                name == shift.name &&
                initHour == shift.initHour &&
                endHour == shift.endHour;
        }
    }
}
