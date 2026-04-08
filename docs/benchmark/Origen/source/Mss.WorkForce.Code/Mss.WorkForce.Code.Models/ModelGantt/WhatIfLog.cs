namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class WhatIfLog
    {
        public string Reason { get; set; }
        public string Log { get; set; }
    }

    public class WorkerWhatIf
    {
        public string Name { get; set; }
        public string Rol { get; set; }
        public double WorkTime { get; set; }
        public double Percentage { get; set; }
        public string Shift { get; set; }
        public DateTime? Init { get; set; }
        public DateTime? End { get; set; }
        public Guid RolId { get; set; }
        public Guid ShiftId { get; set; }
        public Dictionary<string, List<string>> LogMessages { get; set; }

        private int daysDifference => Convert.ToInt32(
         (End.GetValueOrDefault(DateTime.UtcNow).Date -
          Init.GetValueOrDefault(DateTime.UtcNow).Date).TotalDays);

        // campo numérico para filtros
        public int DaysDifferenceValue => daysDifference;

        // campo visual
        public string DaysDifference => $"+ {daysDifference}";


        // segundos desde las 00:00 del día (Init)
        public int? InitSecondsOfDay =>
            Init == null ? null : (int)Init.Value.TimeOfDay.TotalSeconds;

        // segundos desde las 00:00 del día (End)
        public int? EndSecondsOfDay =>
            End == null ? null : (int)End.Value.TimeOfDay.TotalSeconds;

        public IEnumerable<DictionaryItem> LogItems =>
            LogMessages.Select(x => new DictionaryItem
            {
                Key = x.Key,
                Values = x.Value
            }).ToList();
    }


    public class DictionaryItem
    {
        public string Key { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
    }

    public enum WhatIfResults
    {
    }
}
