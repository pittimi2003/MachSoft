using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class OrderMetricsDto
    {
        public string VehicleCode { get; set; }
        public string OrderCode { get; set; }
        public double OTIF { get; set; }
        public string Delay { get; set; }
        public string Slack { get; set; }
        public bool IsClosed { get; set; }
        public double DelaySeconds => ParseTimeToSeconds(Delay);
        public double SlackSeconds => ParseTimeToSeconds(Slack);

        private double ParseTimeToSeconds(string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return 0;

            if (TimeSpan.TryParse(t, out var ts))
                return ts.TotalSeconds;

            return 0;
        }

        public OrderMetricsDto DeepClone()
        {
            return new OrderMetricsDto
            {
                VehicleCode = this.VehicleCode,
                OrderCode = this.OrderCode,
                OTIF = this.OTIF,
                Delay = this.Delay,
                Slack = this.Slack,
                IsClosed = this.IsClosed
            };
        }

    }
}
