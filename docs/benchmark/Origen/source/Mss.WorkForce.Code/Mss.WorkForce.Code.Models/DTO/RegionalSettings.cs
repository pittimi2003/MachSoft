using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class RegionalSettingsOrganization:RegionalSettings
    {

        [DisplayAttributes(1, "System of measurement", true, ComponentType.DropDownLocalized)]
        public SystemOfMeasurement? SystemOfMeasurement { get; set; }
 
    }

    public class RegionalSettings
    {
        [DisplayAttributes(7, "Default language", true, ComponentType.DropDownLocalized, IsVisibleDefault = true )]
        public Language? Language { get; set; }

        [DisplayAttributes(8, "Decimal separator", true, ComponentType.DropDownLocalized)]
        public DecimalSeparator? DecimalSeparator { get; set; }

        [DisplayAttributes(9, "Thousand separator", true, ComponentType.DropDownLocalized)]
        public ThousandsSeparator? ThousandsSeparator { get; set; }

        [DisplayAttributes(10, "Default date format", true, ComponentType.DropDown)]
        public DateFormat? DateFormat { get; set; }

        [DisplayAttributes(11, "Default hour format", true, ComponentType.DropDown)]
        public HourFormat? HourFormat { get; set; }
    }
}
