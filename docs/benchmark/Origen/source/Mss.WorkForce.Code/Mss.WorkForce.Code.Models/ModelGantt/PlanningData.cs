using Mss.WorkForce.Code.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class PlanningData
    {
        public List<OrderMetricsDto> OrderMetrics { get; set; }
        public List<VehicleMetricsDto> VehicleMetrics { get; set; }
        
    }
}
