namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class VehicleMetricsDto
    {
        public DateTime VehicleAppointment { get; set; }
        public DateTime VehicleAppointmentEnd { get; set; }
        public string VehicleTheoreticalDwellTime { get; set; }
        public string Hour { get; set; }           // eje X del chart
        public Guid orderSheduleId { get; set; }
        public string VehicleCode { get; set; }

        public int IncompleteOrders { get; set; }

        public bool IsClosed { get; set; }

        public bool Status { get; set; }

        public int TotalOrders { get; set; }
        public int TotalProcesses { get; set; }

        public bool IsOut { get; set; }

        public DateTime MinInit { get; set; }
        public DateTime MaxEnd { get; set; }

        public double OTIF { get; set; }

        public string Workload { get; set; }
        public string Delay { get; set; }          // HH:MM:SS
        public string Slack { get; set; }          // HH:MM:SS

        public string AverageDelay { get; set; }
        public string MaxDelay { get; set; }
        public string EarlyStart { get; set; }

        public double LoadTime { get; set; }


        // === NUEVO: valores numéricos en minutos ===

        public double SlackMinutes => ParseTimeToMinutes(Slack);
        public double DelayMinutes => ParseTimeToMinutes(Delay);

        public double SlackSeconds => ParseTimeToSeconds(Slack);

        public double DelaySeconds => ParseTimeToSeconds(Delay);

        private double ParseTimeToMinutes(string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return 0;

            if (TimeSpan.TryParse(t, out var ts))
                return ts.TotalMinutes;

            return 0;
        }

        private double ParseTimeToSeconds(string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return 0;

            if (TimeSpan.TryParse(t, out var ts))
                return ts.TotalSeconds;

            return 0;
        }

        public List<OrderMetricsDto> OrderMetrics { get; set; }


        public VehicleMetricsDto DeepClone()
        {
            return new VehicleMetricsDto
            {
                VehicleAppointment = this.VehicleAppointment,
                VehicleAppointmentEnd = this.VehicleAppointmentEnd,
                VehicleTheoreticalDwellTime = this.VehicleTheoreticalDwellTime,
                Hour = this.Hour,
                orderSheduleId = this.orderSheduleId,
                VehicleCode = this.VehicleCode,
                IncompleteOrders = this.IncompleteOrders,
                IsClosed = this.IsClosed,
                Status = this.Status,
                TotalOrders = this.TotalOrders,
                TotalProcesses = this.TotalProcesses,
                IsOut = this.IsOut,
                MinInit = this.MinInit,
                MaxEnd = this.MaxEnd,
                OTIF = this.OTIF,
                Workload = this.Workload,
                Delay = this.Delay,
                Slack = this.Slack,
                AverageDelay = this.AverageDelay,
                MaxDelay = this.MaxDelay,
                EarlyStart = this.EarlyStart,
                LoadTime = this.LoadTime,
                OrderMetrics = this.OrderMetrics != null
                    ? this.OrderMetrics.Select(o => o.DeepClone()).ToList()
                    : new List<OrderMetricsDto>()
            };
        }
    }
}


