using System.Globalization;
using System.Text.RegularExpressions;

namespace Mss.WorkForce.Code.Models.Common
{
    public class UserFormatOptions
    {
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public string HourFormat { get; set; } = "hh:mm:ss tt";
        public string UnitSystem { get; set; } = "Metric";
        public double TimeZoneOffSet { get; set; } = 0;
        public string TimeZoneId { get; set; } = "UTC";
        public char DecimalSeparator { get; set; } = '.';
        public string FullDate => $"{DateFormat} {HourFormat}";
        public char GroupSeparator => DecimalSeparator == '.' ? ',' : '.';
        public char ThousandSeparator { get; set; } = ',';
        public string DecimalRegion => DecimalSeparator == '.' ? "en-US" : "es-ES";
        public string CultureCode { get; set; } = "en";



        public string HourFormatWithoutSeconds
        {
            get
            {
                var cleaned = Regex.Replace(HourFormat, @"([:.])?ss", "", RegexOptions.IgnoreCase);
                return Regex.Replace(cleaned, @"\s{2,}", " ").Trim();
            }
        }

        public CultureInfo BuildCultureFromSeparators()
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();

            culture.NumberFormat.NumberGroupSeparator = ThousandSeparator.ToString();
            culture.NumberFormat.NumberDecimalSeparator = DecimalSeparator.ToString();

            return culture;
        }

    }
}
