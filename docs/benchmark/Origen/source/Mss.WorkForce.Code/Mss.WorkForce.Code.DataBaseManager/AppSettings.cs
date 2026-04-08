using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.DataBaseManager
{
    public class AppSettings
    {
        public string DefaultInsert { get; set; }
        public List<User> Users { get; set; }
        public List<Language> Languages { get; set; }
        public List<SystemOfMeasurement> SystemOfMeasurements { get; set; }
        public List<DecimalSeparator> DecimalSeparators { get; set; }
        public List<ThousandsSeparator> ThousandsSeparators { get; set; }
        public List<DateFormat> DateFormats { get; set; }
        public List<HourFormat> HourFormats { get; set; }
        public List<Organization> Organizations { get; set; }
        public List<DockSelectionStrategy> DockSelectionStrategies { get; set; }
    }
}
