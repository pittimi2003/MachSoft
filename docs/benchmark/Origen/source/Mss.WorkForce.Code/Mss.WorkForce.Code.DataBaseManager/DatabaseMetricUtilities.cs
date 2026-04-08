using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.MetricssModel;

namespace Mss.WorkForce.Code.DataBaseManager
{
    public class DatabaseMetricUtilities
    {
        public MetricsDBContext context { get; set; }

        public DatabaseMetricUtilities(MetricsDBContext context) { this.context = context; }

        public void SendMetrics(PerformanceMetrics metrics)
        {
            context.PerformanceMetrics.Add(metrics);
            context.SaveChanges();
        }
    }
}
