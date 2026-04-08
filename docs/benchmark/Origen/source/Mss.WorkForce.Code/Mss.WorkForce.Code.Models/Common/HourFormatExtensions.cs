using System.Globalization;

namespace Mss.WorkForce.Code.Models.Common
{
    public static class HourFormatExtensions
    {
        public static string ToUserTime(this DateTime value, string format) => value.ToString(format, CultureInfo.InvariantCulture);        

        public static string ToUserTime(this DateTimeOffset value, string format) => value.DateTime.ToUserTime(format);

        public static string ToUserTime(this TimeSpan value, string format) => DateTime.Today.Add(value).ToUserTime(format);


	}
}