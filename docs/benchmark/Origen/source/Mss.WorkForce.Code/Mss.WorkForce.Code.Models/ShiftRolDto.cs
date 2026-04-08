using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Models
{
    public class ShiftRolDto : ICloneable
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public TimeSpan initHour { get; set; }
        public TimeSpan endHour { get; set; }
        public List<WorkersInRolDto> workersInRol { get; set; }

        public bool IsNew { get; set; }
        public HashSet<string> ModifiedFields { get; set; } = new();


        public object Clone()
        {
            return MemberwiseClone();
        }

        public ShiftRolDto DeepClone()
        {
            return new ShiftRolDto
            {
                id = this.id,
                name = this.name,
                initHour = this.initHour,
                endHour = this.endHour,
                workersInRol = this.workersInRol?.Select(x => x.DeepClone()).ToList() ?? new List<WorkersInRolDto>(),
                IsNew = this.IsNew,
                ModifiedFields = this.ModifiedFields != null
                ? new HashSet<string>(ModifiedFields)
                : new HashSet<string>()
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
