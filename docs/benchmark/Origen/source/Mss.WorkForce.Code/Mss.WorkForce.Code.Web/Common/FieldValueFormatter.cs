namespace Mss.WorkForce.Code.Web.Common
{
    public static class FieldValueFormatter
    {
        public static Dictionary<string, string> Format(Dictionary<string, string> fields)
        {
            var formatted = new Dictionary<string, string>(fields);

            if (formatted.TryGetValue("Flow", out var flowVal) && bool.TryParse(flowVal, out var flowBool))
            {
                formatted["Flow"] = flowBool ? "Output profile" : "Input profile";
            }

            if (formatted.TryGetValue("OrderDelay", out var delayVal) && bool.TryParse(delayVal, out var isOnTime))
            {
                formatted["OrderDelay"] = isOnTime ? "Not delayed order" : "Delayed order";
            }

            if (formatted.TryGetValue("Progress", out var progressRaw))
            {
                var cleanValue = progressRaw.Replace(" %", "").Trim();

                if (double.TryParse(cleanValue, out double progressDecimal))
                {
                    var percent = Math.Round(progressDecimal);
                    formatted["Progress"] = $"{percent}%";
                }
            }

            if (formatted.TryGetValue("Committedtime", out var committedRaw))
            {
                if (DateTime.TryParse(committedRaw, out var committedDateTime))
                {
                    formatted["Committedtime"] = committedDateTime.ToString("hh:mm:ss tt");
                }
            }

            return formatted;
        }
    }
}
