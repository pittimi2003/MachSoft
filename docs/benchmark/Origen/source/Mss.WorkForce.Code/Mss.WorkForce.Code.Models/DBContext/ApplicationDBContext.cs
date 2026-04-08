using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models.DBContext
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Layout> Layouts { get; set; }
        public DbSet<Widget> Widgets { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<TypeEquipment> TypeEquipment { get; set; }
        public DbSet<Objects> Objects { get; set; }
        public DbSet<EquipmentGroup> EquipmentGroups { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Dock> Docks { get; set; }
        public DbSet<Models.Buffer> Buffers { get; set; }
        public DbSet<Rack> Racks { get; set; }
        public DbSet<Aisle> Aisles { get; set; }
        public DbSet<InputOrder> InputOrders { get; set; }
        public DbSet<InputOrderLine> InputOrderLines { get; set; }
        public DbSet<InboundFlowGraph> InboundFlowGraphs { get; set; }
        public DbSet<OutboundFlowGraph> OutboundFlowGraphs { get; set; }
        public DbSet<BreakProfile> BreakProfiles { get; set; }
        public DbSet<Break> Breaks { get; set; }
        public DbSet<LoadProfile> LoadProfiles { get; set; }
        public DbSet<PostprocessProfile> PostprocessProfiles { get; set; }
        public DbSet<PreprocessProfile> PreprocessProfiles { get; set; }
        public DbSet<PutawayProfile> PutawayProfiles { get; set; }
        public DbSet<VehicleProfile> VehicleProfiles { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<Inbound> Inbounds { get; set; }
        public DbSet<Reception> Receptions { get; set; }
        public DbSet<Putaway> Putaways { get; set; }
        public DbSet<Picking> Pickings { get; set; }
        public DbSet<Replenishment> Replenishments { get; set; }
        public DbSet<Shipping> Shippings { get; set; }
        public DbSet<Loading> Loadings { get; set; }
        public DbSet<CustomProcess> CustomProcesses { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<ProcessDirectionProperty> ProcessDirectionProperties { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<RolProcessSequence> RolProcessSequences { get; set; }
        public DbSet<AvailableWorker> AvailableWorkers { get; set; }
        public DbSet<ProcessHour> ProcessHours { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Planning> Plannings { get; set; }
        public DbSet<WorkOrderPlanning> WorkOrdersPlanning { get; set; }
        public DbSet<ItemPlanning> ItemsPlanning { get; set; }
        public DbSet<WarehouseProcessPlanning> WarehouseProcessPlanning { get; set; }
        public DbSet<Strategy> Strategies { get; set; }
        public DbSet<StrategySequence> StrategySequences { get; set; }
        public DbSet<ConfigurationSequenceHeader> ConfigurationSequenceHeaders { get; set; }
        public DbSet<ConfigurationSequence> ConfigurationSequences { get; set; }
        public DbSet<OrderSchedule> OrderSchedules { get; set; }
        public DbSet<OrderLoadRatio> OrderLoadRatios { get; set; }
        public DbSet<SystemOfMeasurement> SystemOfMeasurements { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<DecimalSeparator> DecimalSeparators { get; set; }
        public DbSet<ThousandsSeparator> ThousandsSeparators { get; set; }
        public DbSet<DateFormat> DateFormats { get; set; }
        public DbSet<HourFormat> HourFormats { get; set; }
        public DbSet<DockSelectionStrategy> DockSelectionStrategies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Models.TimeZone> TimeZones { get; set; }
        public DbSet<InputOrderProcessClosing> InputOrderProcessesClosing { get; set; }
        public DbSet<WarehouseProcessClosing> WarehouseProcessClosing { get; set; }
        public DbSet<ObjectsCanvas> ObjectsCanvas { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<AlertConfiguration> AlertConfigurations { get; set; }
        public DbSet<AlertResponse> AlertResponses { get; set; }
        public DbSet<AlertMail> AlertMails { get; set; }
        public DbSet<AlertFilter> AlertFilters { get; set; }
        public DbSet<SLAConfig> SLAConfigs { get; set; }
        public DbSet<ProcessPriorityOrder> ProcessPriorityOrder { get; set; }
        public DbSet<OrderPriority> OrderPriority { get; set; }
        public DbSet<WFMLaborEquipment> WFMLaborEquipment { get; set; }
        public DbSet<WFMLaborEquipmentPerProcessType> WFMLaborEquipmentPerProcessType { get; set; }
        public DbSet<WFMLaborItemPlanning> WFMLaborItemPlanning { get; set; }
        public DbSet<WFMLaborWorkerPerProcessType> WFMLaborWorkerPerProcessType { get; set; }
        public DbSet<WFMLaborWorkerPerFlow> WFMLaborWorkerPerFlow { get; set; }
        public DbSet<WFMLaborEquipmentPerFlow> WFMLaborEquipmentPerFlow { get; set; }
        public DbSet<WFMLaborWorker> WFMLaborWorker { get; set; }
        public DbSet<DriveIn> DriveIns { get; set; }
        public DbSet<ChaoticStorage> ChaoticStorages { get; set; }
        public DbSet<AutomaticStorage> AutomaticStorages { get; set; }
        public DbSet<YardAppointmentsNotifications> YardAppointmentsNotifications { get; set; }
        public DbSet<YardMetricsPerDock> YardMetricsPerDock { get; set; }
        public DbSet<YardMetricsPerStage> YardMetricsPerStage { get; set; }
        public DbSet<YardMetricsAppointmentsPerDock> YardMetricsAppointmentsPerDock { get; set; }
        public DbSet<YardMetricsAppointmentsPerStage> YardMetricsAppointmentsPerStage { get; set; }
        public DbSet<YardDockUsagePerHour> YardDockUsagePerHour { get; set; }
        public DbSet<YardStageUsagePerHour> YardStageUsagePerHour { get; set; }
        public DbSet<Flow> Flow { get; set; }
        public DbSet<CustomFlowGraph> CustomFlowGraphs { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<AvailableDocksPerStage> AvailableDocksPerStages { get; set; }
        public DbSet<Transaction> TransactionLog { get; set; }
        public DbSet<Deliveries> Deliveries { get; set; }
        public DbSet<Packing> Packing { get; set; }
        public DbSet<PackingPacksMode> PackingPacksMode { get; set; }
        public DbSet<PackingMode> PackingMode { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>() //Crea tanto organization como warehouse
                .HasMany(l => l.Warehouses)
                .WithOne(l => l.Organization)
                .HasForeignKey(e => e.OrganizationId)
                .HasPrincipalKey(e => e.Id)
                .IsRequired();

            modelBuilder.Entity<Organization>()
               .HasOne(l => l.SystemOfMeasurement)
               .WithMany()
               .HasForeignKey(l => l.SystemOfMeasurementId);

            modelBuilder.Entity<Organization>()
                .HasOne(l => l.Language)
                .WithMany()
                .HasForeignKey(l => l.LanguageId);

            modelBuilder.Entity<Organization>()
                .HasOne(l => l.DecimalSeparator)
                .WithMany()
                .HasForeignKey(l => l.DecimalSeparatorId);

            modelBuilder.Entity<Organization>()
                .HasOne(l => l.ThousandsSeparator)
                .WithMany()
               .HasForeignKey(l => l.ThousandsSeparatorId);

            modelBuilder.Entity<Organization>()
                .HasOne(l => l.DateFormat)
                .WithMany()
               .HasForeignKey(l => l.DateFormatId);

            modelBuilder.Entity<Organization>()
                .HasOne(l => l.HourFormat)
                .WithMany()
               .HasForeignKey(l => l.HourFormatId);

            modelBuilder.Entity<User>() //Crea tanto usuario como warehouse
                .HasMany(l => l.Warehouses)
                .WithMany(l => l.Users);

            modelBuilder.Entity<User>()
                .HasOne(l => l.WarehouseDefault)
                .WithMany()
                .HasForeignKey(l => l.WarehouseDefaultId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Team>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Worker>()
                .HasOne(l => l.Team)
                .WithMany()
                .HasForeignKey(l => l.TeamId);

            modelBuilder.Entity<Worker>()
                .HasOne(h => h.Rol)
                .WithMany()
                .HasForeignKey(h => h.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shift>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Rol>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Process>()
                .HasOne(l => l.Area)
                .WithMany()
                .HasForeignKey(l => l.AreaId);

            modelBuilder.Entity<RolProcessSequence>()
                .HasOne(l => l.Process)
                .WithMany()
                .HasForeignKey(l => l.ProcessId);

            modelBuilder.Entity<RolProcessSequence>()
                .HasOne(l => l.Rol)
                .WithMany()
                .HasForeignKey(l => l.RolId);

            modelBuilder.Entity<Inbound>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<Inbound>(l => l.ProcessId);

            modelBuilder.Entity<Reception>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<Reception>(l => l.ProcessId);

            modelBuilder.Entity<Putaway>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<Putaway>(l => l.ProcessId);

            modelBuilder.Entity<Picking>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<Picking>(l => l.ProcessId);

            modelBuilder.Entity<Replenishment>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<Replenishment>(l => l.ProcessId);

            modelBuilder.Entity<Shipping>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<Shipping>(l => l.ProcessId);

            modelBuilder.Entity<Loading>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<Loading>(l => l.ProcessId);

            modelBuilder.Entity<CustomProcess>()
                .HasOne(l => l.Process)
                .WithOne()
                .HasForeignKey<CustomProcess>(l => l.ProcessId);

            modelBuilder.Entity<Step>()
                .HasOne(l => l.Process)
                .WithMany()
                .HasForeignKey(l => l.ProcessId);

            modelBuilder.Entity<ProcessDirectionProperty>()
                .HasOne(l => l.InitProcess)
                .WithMany(x => x.ProcessDirectionPropertiesEntry)
                .HasForeignKey(l => l.InitProcessId);

            modelBuilder.Entity<ProcessDirectionProperty>()
                .HasOne(l => l.EndProcess)
                .WithMany(x => x.ProcessDirectionPropertiesExit)
                .HasForeignKey(l => l.EndProcessId);

            modelBuilder.Entity<OutboundFlowGraph>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<InboundFlowGraph>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<BreakProfile>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Break>()
                .HasOne(l => l.BreakProfile)
                .WithMany()
                .HasForeignKey(l => l.BreakProfileId);

            modelBuilder.Entity<LoadProfile>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<PostprocessProfile>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<PreprocessProfile>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<PutawayProfile>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<VehicleProfile>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Layout>()
                .HasOne(l => l.Warehouse)
                .WithOne()
                .HasForeignKey<Layout>(l => l.WarehouseId);

            modelBuilder.Entity<InputOrder>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<InputOrder>()
                .HasOne(l => l.Delivery)
                .WithMany()
                .HasForeignKey(l => l.DeliveryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WarehouseProcessClosing>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<InputOrderLine>()
                .HasOne(l => l.InputOutboundOrder)
                .WithMany()
                .HasForeignKey(l => l.InputOutboundOrderId);

            modelBuilder.Entity<AvailableWorker>()
                .HasOne(l => l.Worker)
                .WithOne()
                .HasForeignKey<AvailableWorker>(l => l.WorkerId);

            modelBuilder.Entity<Schedule>()
                .HasOne(l => l.AvailableWorker)
                .WithMany()
                .HasForeignKey(l => l.AvailableWorkerId);

            modelBuilder.Entity<Schedule>()
                .HasOne(l => l.Shift)
                .WithMany(l => l.Schedules)
                .HasForeignKey(l => l.ShiftId);

            modelBuilder.Entity<Schedule>()
                .HasOne(l => l.BreakProfile)
                .WithMany()
                .HasForeignKey(l => l.BreakProfileId);

            modelBuilder.Entity<ProcessHour>()
                .HasOne(l => l.Process)
                .WithMany()
                .HasForeignKey(l => l.ProcessId);

            modelBuilder.Entity<Area>()
               .HasOne(l => l.Layout)
               .WithMany()
               .HasForeignKey(l => l.LayoutId);

            modelBuilder.Entity<Area>()
               .HasOne(l => l.AlternativeArea)
               .WithMany()
               .HasForeignKey(l => l.AlternativeAreaId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Objects>()
               .HasOne(l => l.Layout)
               .WithMany()
               .HasForeignKey(l => l.LayoutId);

            modelBuilder.Entity<TypeEquipment>()
               .HasOne(l => l.Warehouse)
               .WithMany()
               .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Widget>()
               .HasOne(l => l.Warehouse)
               .WithMany()
               .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Route>()
               .HasOne(l => l.DepartureArea)
               .WithMany()
               .HasForeignKey(l => l.DepartureAreaId);

            modelBuilder.Entity<Route>()
               .HasOne(l => l.ArrivalArea)
               .WithMany()
               .HasForeignKey(l => l.ArrivalAreaId);

            modelBuilder.Entity<EquipmentGroup>()
               .HasOne(l => l.Area)
               .WithMany()
               .HasForeignKey(l => l.AreaId);

            modelBuilder.Entity<EquipmentGroup>()
               .HasOne(l => l.TypeEquipment)
               .WithMany()
               .HasForeignKey(l => l.TypeEquipmentId);

            modelBuilder.Entity<Zone>()
               .HasOne(l => l.Area)
               .WithMany()
               .HasForeignKey(l => l.AreaId);

            modelBuilder.Entity<Dock>()
               .HasOne(l => l.Zone)
               .WithOne()
               .HasForeignKey<Dock>(l => l.ZoneId);

            modelBuilder.Entity<Models.Buffer>()
               .HasOne(l => l.Zone)
               .WithOne()
               .HasForeignKey<Models.Buffer>(l => l.ZoneId);

            modelBuilder.Entity<Rack>()
               .HasOne(l => l.Zone)
               .WithOne()
               .HasForeignKey<Rack>(l => l.ZoneId);

            modelBuilder.Entity<Rack>()
                .Property(l => l.IsVertical)
                .HasDefaultValue(true);

            modelBuilder.Entity<Aisle>()
               .HasOne(l => l.Zone)
               .WithOne()
               .HasForeignKey<Aisle>(l => l.ZoneId);


            modelBuilder.Entity<Planning>()
               .HasOne(l => l.Warehouse)
               .WithMany()
               .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<WorkOrderPlanning>()
               .HasOne(l => l.Planning)
               .WithMany()
               .HasForeignKey(l => l.PlanningId);

            modelBuilder.Entity<WorkOrderPlanning>()
               .HasOne(l => l.InputOrder)
               .WithMany()
               .HasForeignKey(l => l.InputOrderId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkOrderPlanning>()
               .HasOne(l => l.AssignedDock)
               .WithMany()
               .HasForeignKey(l => l.AssignedDockId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ItemPlanning>()
               .HasOne(l => l.WorkOrderPlanning)
               .WithMany()
               .HasForeignKey(l => l.WorkOrderPlanningId);

            modelBuilder.Entity<ItemPlanning>()
               .HasOne(l => l.Worker)
               .WithMany()
               .HasForeignKey(l => l.WorkerId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ItemPlanning>()
               .HasOne(l => l.EquipmentGroup)
               .WithMany()
               .HasForeignKey(l => l.EquipmentGroupId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ItemPlanning>()
               .HasOne(l => l.Shift)
               .WithMany()
               .HasForeignKey(l => l.ShiftId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ItemPlanning>()
               .HasOne(l => l.Stage)
               .WithMany()
               .HasForeignKey(l => l.StageId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WarehouseProcessPlanning>()
               .HasOne(l => l.Planning)
               .WithMany()
               .HasForeignKey(l => l.PlanningId);

            modelBuilder.Entity<WarehouseProcessPlanning>()
                .HasOne(l => l.Process)
                .WithMany()
                .HasForeignKey(l => l.ProcessId);

            modelBuilder.Entity<WarehouseProcessPlanning>()
                .HasOne(l => l.Worker)
                .WithMany()
                .HasForeignKey(l => l.WorkerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WarehouseProcessPlanning>()
                .HasOne(l => l.EquipmentGroup)
                .WithMany()
                .HasForeignKey(l => l.EquipmentGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InputOrder>()
                .HasOne(l => l.PreferredDock)
                .WithMany()
                .HasForeignKey(l => l.PreferredDockId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InputOrder>()
                .HasOne(l => l.AssignedDock)
                .WithMany()
                .HasForeignKey(l => l.AssignedDockId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Strategy>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<DockSelectionStrategy>()
                .HasOne(l => l.Organization)
                .WithMany()
                .HasForeignKey(l => l.OrganizationId);

            modelBuilder.Entity<InboundFlowGraph>()
                .HasOne(l => l.DockSelectionStrategy)
                .WithMany()
                .HasForeignKey(l => l.DockSelectionStrategyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OutboundFlowGraph>()
                .HasOne(l => l.DockSelectionStrategy)
                .WithMany()
                .HasForeignKey(l => l.DockSelectionStrategyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StrategySequence>()
                .HasOne(l => l.Strategy)
                .WithMany()
                .HasForeignKey(l => l.StrategyId);

            modelBuilder.Entity<Organization>()
               .HasOne(l => l.Country)
               .WithMany()
               .HasForeignKey(l => l.CountryId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Warehouse>()
               .HasOne(l => l.Country)
               .WithMany()
               .HasForeignKey(l => l.CountryId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Warehouse>()
              .HasOne(l => l.TimeZone_)
              .WithMany()
              .HasForeignKey(l => l.TimeZoneId)
              .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
               .HasOne(l => l.DecimalSeparator)
               .WithMany()
               .HasForeignKey(l => l.DecimalSeparatorId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
               .HasOne(l => l.ThousandsSeparator)
               .WithMany()
               .HasForeignKey(l => l.ThousandsSeparatorId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
               .HasOne(l => l.DateFormat)
               .WithMany()
               .HasForeignKey(l => l.DateFormatId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
               .HasOne(l => l.HourFormat)
               .WithMany()
               .HasForeignKey(l => l.HourFormatId)
               .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<User>()
               .HasOne(l => l.Language)
               .WithMany()
               .HasForeignKey(l => l.LanguageId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProcessInterleavingView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("processinterleavingview");
                entity.Property(e => e.When).HasColumnName("When");
                entity.Property(e => e.InboundSec).HasColumnName("InboundSec");
                entity.Property(e => e.OutboundSec).HasColumnName("OutboundSec");
                entity.Property(e => e.InterleavingSec).HasColumnName("InterleavingSec");
            });

            modelBuilder.Entity<PlanningResourcesView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("planningresourcesview");
                entity.Property(e => e.When).HasColumnName("When");
                entity.Property(e => e.InitDate).HasColumnName("InitDate");
                entity.Property(e => e.EndDate).HasColumnName("EndDate");
                entity.Property(e => e.ProcessId).HasColumnName("ProcessId");
                entity.Property(e => e.ProcessName).HasColumnName("ProcessName");
                entity.Property(e => e.ResourceType).HasColumnName("ResourceType");
                entity.Property(e => e.ResourceId).HasColumnName("ResourceId");
                entity.Property(e => e.ResourceName).HasColumnName("ResourceName");
                entity.Property(e => e.AvailableResources).HasColumnName("AvailableResources");
            });

            modelBuilder.Entity<AreaView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("areaview");
                entity.Property(e => e.When).HasColumnName("When");
                entity.Property(e => e.AreaName).HasColumnName("AreaName");
                entity.Property(e => e.AreasPercUt).HasColumnName("AreasPercUt");
            });
            modelBuilder.Entity<ResourcesUsageView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("resourcesusageview");
                entity.Property(e => e.When).HasColumnName("When");
                entity.Property(e => e.ResourceName).HasColumnName("ResourceName");
                entity.Property(e => e.ResourcesUt).HasColumnName("ResourcesUt");
            });


            modelBuilder.Entity<ConfigurationSequenceHeader>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<ConfigurationSequence>()
                .HasOne(l => l.ConfigurationSequenceHeader)
                .WithMany(l => l.ConfigurationSequences)
                .HasForeignKey(l => l.ConfigurationSequenceHeaderId);

            modelBuilder.Entity<OrderSchedule>()
                .HasOne(l => l.Load)
                .WithMany()
                .HasForeignKey(l => l.LoadId);

            modelBuilder.Entity<OrderSchedule>()
                .HasOne(l => l.Vehicle)
                .WithMany()
                .HasForeignKey(l => l.VehicleId);

            modelBuilder.Entity<OrderSchedule>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<OrderLoadRatio>()
                .HasOne(l => l.Load)
                .WithMany()
                .HasForeignKey(l => l.LoadId);

            modelBuilder.Entity<OrderLoadRatio>()
                .HasOne(l => l.Vehicle)
                .WithMany()
                .HasForeignKey(l => l.VehicleId);

            modelBuilder.Entity<Alert>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<AlertConfiguration>()
                .HasOne(ac => ac.Alert)
                .WithMany(a => a.Configurations)
                .HasForeignKey(ac => ac.AlertId);

            modelBuilder.Entity<AlertFilter>()
                .HasOne(l => l.Alert)
                .WithMany()
                .HasForeignKey(l => l.AlertId);

            modelBuilder.Entity<AlertMail>()
                .HasOne(l => l.Alert)
                .WithMany()
                .HasForeignKey(l => l.AlertId);

            modelBuilder.Entity<AlertResponse>()
                .HasOne(l => l.Alert)
                .WithMany()
                .HasForeignKey(l => l.AlertId);

            modelBuilder.Entity<AlertResponse>()
                .HasOne(l => l.Planning)
                .WithMany()
                .HasForeignKey(l => l.PlanningId);

            modelBuilder.Entity<SLAConfig>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<WFMLaborEquipment>()
                .HasOne(l => l.EquipmentGroup)
                .WithMany()
                .HasForeignKey(l => l.EquipmentGroupId);

            modelBuilder.Entity<WFMLaborEquipment>()
                .HasOne(l => l.TypeEquipment)
                .WithMany()
                .HasForeignKey(l => l.TypeEquipmentId);

            modelBuilder.Entity<WFMLaborEquipment>()
                .HasOne(l => l.Planning)
                .WithMany()
                .HasForeignKey(l => l.PlanningId);

            modelBuilder.Entity<WFMLaborEquipmentPerProcessType>()
                .HasOne(l => l.EquipmentGroup)
                .WithMany()
                .HasForeignKey(l => l.EquipmentGroupId);

            modelBuilder.Entity<WFMLaborEquipmentPerProcessType>()
                .HasOne(l => l.TypeEquipment)
                .WithMany()
                .HasForeignKey(l => l.TypeEquipmentId);

            modelBuilder.Entity<WFMLaborEquipmentPerProcessType>()
                .HasOne(l => l.Planning)
                .WithMany()
                .HasForeignKey(l => l.PlanningId);

            modelBuilder.Entity<DriveIn>()
               .HasOne(l => l.Zone)
               .WithMany()
               .HasForeignKey(l => l.ZoneId);

            modelBuilder.Entity<DriveIn>()
                .Property(l => l.IsVertical)
                .HasDefaultValue(true);

            modelBuilder.Entity<ChaoticStorage>()
               .HasOne(l => l.Zone)
               .WithMany()
               .HasForeignKey(l => l.ZoneId);

            modelBuilder.Entity<AutomaticStorage>()
               .HasOne(l => l.Zone)
               .WithMany()
               .HasForeignKey(l => l.ZoneId);

            modelBuilder.Entity<AutomaticStorage>()
                .Property(l => l.IsVertical)
                .HasDefaultValue(true);

            modelBuilder.Entity<YardMetricsAppointmentsPerDock>()
               .HasOne(l => l.YardMetricsPerDock)
               .WithMany()
               .HasForeignKey(l => l.YardMetricsPerDockId);

            modelBuilder.Entity<YardMetricsAppointmentsPerStage>()
               .HasOne(l => l.YardMetricsPerStage)
               .WithMany()
               .HasForeignKey(l => l.YardMetricsPerStageId);

            modelBuilder.Entity<WFMLaborEquipment>()
                .Property(l => l.Progress)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<WFMLaborEquipmentPerFlow>()
                .Property(l => l.Progress)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<WFMLaborEquipmentPerProcessType>()
                .Property(l => l.Progress)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<WFMLaborItemPlanning>()
                .Property(l => l.Progress)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<WFMLaborItemPlanning>()
                .Property(l => l.WorkTime)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<WFMLaborWorker>()
                .Property(l => l.Progress)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<WFMLaborWorkerPerFlow>()
                .Property(l => l.Progress)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<WFMLaborWorkerPerProcessType>()
                .Property(l => l.Progress)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<Flow>()
                .HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<OutboundFlowGraph>()
                .HasOne(l => l.Flow)
                .WithMany()
                .HasForeignKey(l => l.FlowId);

            modelBuilder.Entity<InboundFlowGraph>()
               .HasOne(l => l.Flow)
               .WithMany()
               .HasForeignKey(l => l.FlowId);

            modelBuilder.Entity<CustomFlowGraph>()
               .HasOne(l => l.Flow)
               .WithMany()
               .HasForeignKey(l => l.FlowId);

            modelBuilder.Entity<Process>()
              .HasOne(l => l.Flow)
              .WithMany()
              .HasForeignKey(l => l.FlowId)
              .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Stage>()
               .HasOne(l => l.Zone)
               .WithMany()
               .HasForeignKey(l => l.ZoneId);

            modelBuilder.Entity<Stage>()
                .Property(l => l.MixCarriers)
                .HasDefaultValue(false);

            modelBuilder.Entity<Stage>()
                .Property(l => l.IsOut)
                .HasDefaultValue(true);

            modelBuilder.Entity<Stage>()
                .Property(l => l.IsIn)
                .HasDefaultValue(true);

            modelBuilder.Entity<AvailableDocksPerStage>()
               .HasOne(l => l.Dock)
               .WithMany()
               .HasForeignKey(l => l.DockId);

            modelBuilder.Entity<AvailableDocksPerStage>()
               .HasOne(l => l.Stage)
               .WithMany()
               .HasForeignKey(l => l.StageId);

            modelBuilder.Entity<YardMetricsPerStage>()
               .HasOne(l => l.Stage)
               .WithMany()
               .HasForeignKey(l => l.StageId);

            modelBuilder.Entity<YardMetricsPerStage>()
               .HasOne(l => l.Planning)
               .WithMany()
               .HasForeignKey(l => l.PlanningId);

            modelBuilder.Entity<YardStageUsagePerHour>()
               .HasOne(l => l.Stage)
               .WithMany()
               .HasForeignKey(l => l.StageId);

            modelBuilder.Entity<YardStageUsagePerHour>()
               .HasOne(l => l.Planning)
               .WithMany()
               .HasForeignKey(l => l.PlanningId);

            modelBuilder.Entity<YardStageUsagePerHour>()
               .HasOne(l => l.Warehouse)
               .WithMany()
               .HasForeignKey(l => l.WarehouseId);

            modelBuilder.Entity<Packing>()
                .HasOne(l => l.Process)
                .WithMany()
                .HasForeignKey(l => l.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PackingPacksMode>()
                .HasOne(l => l.Packing)
                .WithMany()
                .HasForeignKey(l => l.PackingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PackingPacksMode>()
                .HasOne(l => l.PackingMode)
                .WithMany()
                .HasForeignKey(l => l.PackingModeId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
