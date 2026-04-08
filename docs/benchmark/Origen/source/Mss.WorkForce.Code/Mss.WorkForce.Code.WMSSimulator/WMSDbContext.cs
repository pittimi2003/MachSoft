using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.WMSSimulator.WMSModel;
using InputOrder = Mss.WorkForce.Code.WMSSimulator.WMSModel.InputOrder;
using Parameter = Mss.WorkForce.Code.WMSSimulator.WMSModel.Parameter;
using WorkForceTask = Mss.WorkForce.Code.WMSSimulator.WMSModel.WorkForceTask;

namespace Mss.WorkForce.Code.WMSSimulator
{
    public class WMSDbContext : DbContext
    {
        public DbSet<Parameter> Parameters { get { return this.Set<Parameter>(); } }
        public DbSet<InputOrder> InputOrders { get { return this.Set<InputOrder>(); } }
        public DbSet<SimulationResults> SimulationResults { get { return this.Set<SimulationResults>(); } }
        public DbSet<Processes> Processes { get { return this.Set<Processes>(); } }
        public DbSet<WorkForceTask> WorkForceTasks { get { return this.Set<WorkForceTask>(); } }
        public DbSet<Planning> Plannings { get { return this.Set<Planning>(); } }
        public DbSet<WorkOrderPlanning> WorkOrderPlannings { get { return this.Set<WorkOrderPlanning>(); } }
        public DbSet<ItemPlanning> ItemPlannings { get { return this.Set<ItemPlanning>(); } }

        public WMSDbContext(DbContextOptions<WMSDbContext> options) : base(options) { }
    

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            AddParameter(builder);
            AddInpurOrder(builder);
            AddWorkForceTask(builder); 
            AddSimulationResults(builder);
            AddProcesses(builder);
            AddPlanning(builder);
            AddWorkOrderPlanning(builder);
            AddItemPlannig(builder);

            builder.Entity<Parameter>();
            builder.Entity<InputOrder>();
            builder.Entity<WorkForceTask>();
            builder.Entity<SimulationResults>();
            builder.Entity<Processes>();
            builder.Entity<Planning>();
            builder.Entity<WorkOrderPlanning>();
            builder.Entity<ItemPlanning>();
        }

        private static void AddParameter(ModelBuilder builder)
        {
            builder.Entity<Parameter>(e =>
            {
                e.ToTable("Parameters");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.Code).HasColumnName("Code");
                e.Property(e => e.Value).HasColumnName("Value");
            });
        }

        private static void AddWorkForceTask(ModelBuilder builder)
        {
            builder.Entity<WorkForceTask>(e =>
            {
                e.ToTable("WorkForceTasks");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.InitHour).HasColumnName("InitHour");
                e.Property(e => e.EndHour).HasColumnName("EndHour");
                e.Property(e => e.NumOrders).HasColumnName("NumOrders");
                e.Property(e => e.NumOrdersCompleted).HasColumnName("NumOrdersCompleted");
                e.Property(e => e.LinesPerOrder).HasColumnName("LinesPerOrder");
                e.Property(e => e.IsOut).HasColumnName("IsOut");
                e.Property(e => e.Date).HasColumnName("Date");
                e.Property(e => e.WarehouseId).HasColumnName("WarehouseId");
            });
        }

        private static void AddInpurOrder(ModelBuilder builder)
        {
            builder.Entity<InputOrder>(e =>
            {
                e.ToTable("InputOrder");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.OrderCode).HasColumnName("OrderCode");
                e.Property(e => e.IsOut).HasColumnName("IsOut");
                e.Property(e => e.PreferedDockCode).HasColumnName("PreferedDockCode");
                e.Property(e => e.Status).HasColumnName("Status");
                e.Property(e => e.Progress).HasColumnName("Progress");
                e.Property(e => e.NumLines).HasColumnName("NumLines");
                e.Property(e => e.UpdateDate).HasColumnName("UpdateDate");
                e.Property(e => e.MarginTime).HasColumnName("MarginTime");
                e.Property(e => e.WarehouseId).HasColumnName("WarehouseId");
                e.Property(e => e.WarehouseCode).HasColumnName("WarehouseCode");
                e.Property(e => e.WorkForceTaskId).HasColumnName("WorkForceTaskId"); 
                e.Property(e => e.AppointmentDate).HasColumnName("AppointmentDate");
                e.Property(e => e.CreationDate).HasColumnName("CreationDate");
                e.Property(e => e.ReleaseDate).HasColumnName("ReleaseDate");
            });
        }
        private static void AddSimulationResults(ModelBuilder builder)
        {
            builder.Entity<SimulationResults>(e =>
            {
                e.ToTable("SimulationResults");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.Code).HasColumnName("Code");
                e.Property(e => e.ClientCode).HasColumnName("ClientCode");
                e.Property(e => e.ProviderCode).HasColumnName("ProviderCode");
                e.Property(e => e.TypeCode).HasColumnName("TypeCode");
                e.Property(e => e.AppointmentDate).HasColumnName("AppointmentDate");
                e.Property(e => e.ExpeditionEstimatedDate).HasColumnName("ExpeditionEstimatedDate");
                e.Property(e => e.ArrivalEstimatedDate).HasColumnName("ArrivalEstimatedDate");
                e.Property(e => e.LiberationDate).HasColumnName("LiberationDate");
                e.Property(e => e.DownloadDate).HasColumnName("DownloadDate");
                e.Property(e => e.StartDate).HasColumnName("StartDate");
                e.Property(e => e.EndDate).HasColumnName("EndDate");
                e.Property(e => e.RealTruckArrivalDate).HasColumnName("RealTruckArrivalDate");
                e.Property(e => e.AssignedDockCode).HasColumnName("AssignedDockCode");
                e.Property(e => e.ActionCode).HasColumnName("ActionCode");
            });
        }
        private static void AddProcesses(ModelBuilder builder)
        {
            builder.Entity<Processes>(e =>
            {
                e.ToTable("Processes");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.SimulationResultsId).HasColumnName("SimulationResultsId");
                e.Property(e => e.UserCode).HasColumnName("UserCode");
                e.Property(e => e.EquipmentTypeCode).HasColumnName("EquipmentTypeCode");
                e.Property(e => e.StartDate).HasColumnName("StartDate");
                e.Property(e => e.EndDate).HasColumnName("EndDate");

            });
        }
        private static void AddPlanning(ModelBuilder builder)
        {
            builder.Entity<Planning>(e =>
            {
                e.ToTable("Plannings");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.Date).HasColumnName("Date");
                e.Property(e => e.CreationDate).HasColumnName("CreationDate");
                e.Property(e => e.WarehouseId).HasColumnName("WarehouseId");
            });
        }
        private static void AddWorkOrderPlanning(ModelBuilder builder)
        {
            builder.Entity<WorkOrderPlanning>(e =>
            {
                e.ToTable("WorkOrdersPlanning");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.InitDate).HasColumnName("InitDate");
                e.Property(e => e.EndDate).HasColumnName("EndDate");
                e.Property(e => e.PlanningId).HasColumnName("PlanningId");
                e.Property(e => e.InputOrderId).HasColumnName("InputOrderId");
                e.Property(e => e.IsOutbound).HasColumnName("IsOutbound");
            });
        }
        private static void AddItemPlannig(ModelBuilder builder)
        {
            builder.Entity<ItemPlanning>(e =>
            {
                e.ToTable("ItemsPlanning");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.ProcessId).HasColumnName("ProcessId");
                e.Property(e => e.InitDate).HasColumnName("InitDate");
                e.Property(e => e.EndDate).HasColumnName("EndDate");
                e.Property(e => e.WorkOrderPlanningId).HasColumnName("WorkOrderPlanningId");
                e.Property(e => e.WorkerId).HasColumnName("WorkerId");
                e.Property(e => e.EquipmentGroupId).HasColumnName("EquipmentGroupId");
                e.Property(e => e.IsDrawn).HasColumnName("IsDrawn");
            });
        }
    }
}
