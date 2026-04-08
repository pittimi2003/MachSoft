
namespace Mss.WorkForce.Code.Web.Common
{
    public static class TimeToString
    {
        /// <summary>
        /// Function extension to transform time span o range in hour format string
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string ToTransform(this TimeSpan timeSpan, string format)
        {
            string Format12Hour(TimeSpan ts)
            {
                int hour12 = ts.Hours % 12;
                if (hour12 == 0) hour12 = 12;
                string period = ts.Hours >= 12 ? "PM" : "AM";
                return $"{hour12:D2}:{ts.Minutes:D2} {period}";
            }

            switch (format)
            {
                case "HH:mm:ss":
                case "HH:mm":
                    return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}";

                case "hh:mm:ss tt":
                case "hh:mm tt":
                    return Format12Hour(timeSpan);

                case "HH.mm.ss":
                case "HH.mm":
                    return $"{timeSpan.Hours:D2}.{timeSpan.Minutes:D2}";

                default:
                    return Format12Hour(timeSpan);
            }
        }
    }
}
