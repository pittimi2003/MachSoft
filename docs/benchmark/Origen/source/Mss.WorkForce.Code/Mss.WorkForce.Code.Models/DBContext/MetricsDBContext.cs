using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.MetricssModel;

namespace Mss.WorkForce.Code.Models.DBContext
{
    public class MetricsDBContext : DbContext
    {
        public DbSet<PerformanceMetrics> PerformanceMetrics { get; set; }

        public MetricsDBContext(DbContextOptions<MetricsDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PerformanceMetrics>()
                .HasKey(e => e.Id);
        }
    }
}
