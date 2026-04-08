using Mss.WorkForce.Code.Models.Enums;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class GanttFilterSettingsDto
    {
        public GanttFilterSettingsDto()
        {
            Reference = EnumDateField.StartDate;
            TimeUntil = TimeSpan.MinValue;
        }

        public EnumDateField Reference { get; set; }

        public TimeSpan TimeUntil { get; set; }
    }
}
