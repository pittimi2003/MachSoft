using System.Globalization;

namespace Mss.WorkForce.Code.Models.Common
{
    public static class NumberFormatExtensions
    {
        public static string ToUserNumber(this double value, UserFormatOptions format)
        {
            var decimalSeparator = format.DecimalSeparator.ToString();
            var groupSeparator = format.GroupSeparator.ToString();

            var numberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = decimalSeparator,
                NumberGroupSeparator = groupSeparator
            };

            if (value % 1 == 0)
                return ((int)value).ToString("N0", numberFormat);

            return value.ToString("N2", numberFormat);
        }

        public static string ToUserNumber(this double? value, UserFormatOptions format)
        {
            return value.HasValue ? value.Value.ToUserNumber(format) : string.Empty;
        }
    }
}
