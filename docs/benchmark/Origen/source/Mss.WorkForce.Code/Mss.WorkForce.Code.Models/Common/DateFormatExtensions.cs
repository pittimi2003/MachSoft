using System;
using System.Globalization;
using Mss.WorkForce.Code.Models.Enums;

namespace Mss.WorkForce.Code.Models.Common
{
    public static class DateFormatExtensions
    {
        public static string ToUserDate(this DateTime value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
