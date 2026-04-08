
namespace Mss.WorkForce.Code.Models.Common
{
    public static class TimeFormatter
    {
        public static string GetFormattedWorkTime(double totalSeconds)
        {
            int hours;
            int minutes;
            int seconds;

            if (totalSeconds < 0)
                return "0 s";

            int roundedSeconds = (int)Math.Round(totalSeconds);

            hours = roundedSeconds / 3600;
            minutes = (roundedSeconds % 3600) / 60;
            seconds = roundedSeconds % 60;

            var parts = new List<string>();
            if (hours > 0) parts.Add($"{hours:D2} h");
            if (minutes > 0) parts.Add($"{minutes:D2} min");
            if (seconds > 0) parts.Add($"{seconds:D2} s");

            return string.Join(" ", parts);
        }

        public static double? GetSecondsToMinutes(this double? seconds) => seconds is double s ? s / 60 >= 1 ? Math.Round((s / 60), 2) : 0:null;

        public static double? GetSecondsToMinutes(this double seconds) => seconds is double s ? s / 60 >= 1 ? Math.Round((s / 60),2) : 0 : null;


        public static string GetSLAMetString(this bool val) => val ? "On time" : "Out of time";
    }

    
}
