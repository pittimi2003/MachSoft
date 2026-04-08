using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.DataBaseManager.CloneUtilities;
using Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Mss.WorkForce.Code.Models.WMSCommunications;
using Newtonsoft.Json;
using Npgsql;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Route = Mss.WorkForce.Code.Models.Models.Route;

namespace Mss.WorkForce.Code.DataBaseManager
{
    public class DataBaseUtilities
    {
        public ApplicationDbContext context { get; set; }

        public DataBaseUtilities(ApplicationDbContext context) { this.context = context; }

        public void EnsurePackingModes()
        {
            var modes = new[]
            {
                "OnePackage",
                "ChooseNumPackages",
                "CaptureStock"
            };

            foreach (var mode in modes)
            {
                bool exists = context.PackingMode.Any(x => x.PackingType == mode);

                if (!exists)
                {
                    context.PackingMode.Add(new PackingMode
                    {
                        Id = Guid.NewGuid(),
                        PackingType = mode
                    });
                }
            }

            context.SaveChanges();
        }


        public Guid? GetWarehousePlaceHolder()
        {
            return context.Warehouses.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Deletes plannings with a CreationDate earlier than the given threshold.
        /// Returns the number of records deleted.
        /// </summary>
        public async Task<int> CleanupOldPlanningsAsync(
            DateTime threshold,
            System.Threading.CancellationToken token)
        {
            try
            {
                var deletedCount = await context.Plannings
                    .Where(p => p.CreationDate < threshold)
                    .ExecuteDeleteAsync(token);

                return deletedCount;
            }
            catch
            {
                throw;
            }
        }


        public void setWarehouseConfig(Guid? warehouseId, bool value)
        {
            Warehouse warehouse = context.Warehouses.Where(x => x.Id == warehouseId).FirstOrDefault();

            if (warehouse != null)
            {
                warehouse.IsConfigValid = value;
                context.SaveChanges();
            }

        }

        public void SaveInputOrderStatus(InputOrderStatusChangesInformation s)
        {
            try
            {
                if (!context.Warehouses.Any())
                    return;

                Guid? assignedDockId = !string.IsNullOrWhiteSpace(s.AssignedDock)
                    ? context.Docks
                        .AsNoTracking()
                        .Where(d => d.Zone != null && d.Zone.Name == s.AssignedDock)
                        .Select(d => (Guid?)d.Id)
                        .FirstOrDefault()
                    : null;

                Guid? preferredDockId = !string.IsNullOrWhiteSpace(s.PreferredDock)
                    ? context.Docks
                        .AsNoTracking()
                        .Where(d => d.Zone != null && d.Zone.Name == s.PreferredDock)
                        .Select(d => (Guid?)d.Id)
                        .FirstOrDefault()
                    : null;

                Guid? warehouseId = !string.IsNullOrWhiteSpace(s.Warehouse)
                    ? context.Warehouses
                        .AsNoTracking()
                        .Where(w => w.Name == s.Warehouse)
                        .Select(w => (Guid?)w.Id)
                        .FirstOrDefault()
                    : null;

                var existingOrder = context.InputOrders.FirstOrDefault(m => m.OrderCode == s.InputOrder);

                if (existingOrder != null)
                {
                    existingOrder.IsStarted = s.IsStarted != null ? s.IsStarted : existingOrder.IsStarted;
                    existingOrder.Status = s.Status != null ? s.Status : existingOrder.Status;
                    existingOrder.IsOutbound = s.IsOutbound != null ? s.IsOutbound : existingOrder.IsOutbound;
                    existingOrder.AllowPartialClosed = s.AllowPartialClosed != null ? s.AllowPartialClosed : existingOrder.AllowPartialClosed;
                    existingOrder.AllowGroup = s.AllowGroup != null ? s.AllowGroup : existingOrder.AllowGroup;

                    existingOrder.AppointmentDate = s.AppointmentDate != null
                        ? DateTime.SpecifyKind(s.AppointmentDate, DateTimeKind.Utc)
                        : existingOrder.AppointmentDate;

                    existingOrder.RealArrivalTime = s.RealArrivalTime != null
                        ? DateTime.SpecifyKind(s.RealArrivalTime.Value, DateTimeKind.Utc)
                        : existingOrder.RealArrivalTime;

                    existingOrder.UpdateDate = s.UpdateDate != null
                        ? DateTime.SpecifyKind(s.UpdateDate.Value, DateTimeKind.Utc)
                        : DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                    existingOrder.Priority = s.Priority != null ? s.Priority : existingOrder.Priority;
                    existingOrder.CreationDate = s.CreationDate != null ? s.CreationDate : existingOrder.CreationDate;
                    existingOrder.ReleaseDate = s.ReleaseDate != null ? s.ReleaseDate : existingOrder.ReleaseDate;

                    existingOrder.Carrier = s.Carrier != null ? s.Carrier : existingOrder.Carrier;
                    existingOrder.Account = s.Account != null ? s.Account : existingOrder.Account;
                    existingOrder.Supplier = s.Supplier != null ? s.Supplier : existingOrder.Supplier;
                    existingOrder.Trailer = s.Trailer != null ? s.Trailer : existingOrder.Trailer;
                    existingOrder.IsEstimated = s.IsEstimated != null ? s.IsEstimated : existingOrder.IsEstimated;

                    existingOrder.AssignedDockId = assignedDockId != null ? assignedDockId : existingOrder.AssignedDockId;
                    existingOrder.PreferredDockId = preferredDockId != null ? preferredDockId : existingOrder.PreferredDockId;
                    existingOrder.WarehouseId = warehouseId != null ? warehouseId.Value : existingOrder.WarehouseId;
                }
                else
                {

                    var whId = warehouseId ?? context.Warehouses
                        .AsNoTracking()
                        .Where(w => w.Name == s.Warehouse)
                        .Select(w => (Guid?)w.Id)
                        .FirstOrDefault();

                    if (!whId.HasValue)
                        throw new InvalidOperationException("Warehouse no encontrado para crear InputOrder.");

                    var newOrder = new InputOrder
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = s.InputOrder,

                        IsStarted = s.IsStarted != null ? s.IsStarted : false,
                        Status = s.Status,
                        IsOutbound = s.IsOutbound != null ? s.IsOutbound : false,
                        AllowPartialClosed = s.AllowPartialClosed != null ? s.AllowPartialClosed : false,
                        AllowGroup = s.AllowGroup != null ? s.AllowGroup : false,
                        IsEstimated = s.IsEstimated != null ? s.IsEstimated : false,

                        AppointmentDate = (DateTime)(s.AppointmentDate != null
                            ? DateTime.SpecifyKind(s.AppointmentDate, DateTimeKind.Utc)
                            : (DateTime?)null),

                        RealArrivalTime = s.RealArrivalTime != null
                            ? DateTime.SpecifyKind(s.RealArrivalTime.Value, DateTimeKind.Utc)
                            : (DateTime?)null,

                        Priority = s.Priority,
                        CreationDate = s.CreationDate != null ? s.CreationDate : DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                        ReleaseDate = s.ReleaseDate,
                        UpdateDate = s.UpdateDate != null ? DateTime.SpecifyKind(s.UpdateDate.Value, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),

                        AssignedDockId = assignedDockId,
                        PreferredDockId = preferredDockId,
                        WarehouseId = whId.Value,
                        Carrier = s.Carrier,
                        Account = s.Account,
                        Supplier = s.Supplier,
                        Trailer = s.Trailer
                    };

                    context.InputOrders.Add(newOrder);
                }

                if (!context.InputOrderProcessesClosing.Any(m => m.NotificationId == s.NotificationId))
                {
                    var inputOrderProcessClosing = new InputOrderProcessClosing
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = s.NotificationId,
                        ProcessType = s.ProcessType,
                        Worker = s.Worker,
                        EquipmentGroup = s.EquipmentGroup,
                        InitDate = DateTime.SpecifyKind(s.InitDate, DateTimeKind.Utc),
                        EndDate = DateTime.SpecifyKind(s.EndDate, DateTimeKind.Utc),
                        InputOrder = s.InputOrder,
                        ZoneCode = s.ZoneCode
                    };

                    context.InputOrderProcessesClosing.Add(inputOrderProcessClosing);
                }

                context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }


        public void UpdateConfig(Actions Actions)
        {
            try
            {
                // NEW
                foreach (var tabla in Actions.New)
                {
                    if (tabla.Value.Any())
                    {
                        var dbSetProperty = context.GetType().GetProperty(tabla.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        var DbSet = dbSetProperty.GetValue(context);
                        var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];

                        foreach (var value in tabla.Value)
                        {
                            var entityInstance = ConvertToType(value, entityType);
                            Fill(entityInstance, context);
                            DbSet.GetType().GetMethod("Add")?.Invoke(DbSet, new[] { entityInstance });
                        }
                    }
                }

                // UPDATE
                foreach (var tabla in Actions.Update)
                {
                    if (tabla.Value.Any())
                    {
                        var dbSetProperty = context.GetType().GetProperty(tabla.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        var DbSet = dbSetProperty.GetValue(context);
                        var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];

                        foreach (var value in tabla.Value)
                        {
                            var entityInstance = ConvertToType(value, entityType);

                            var idValue = entityInstance.GetType().GetProperty("Id")?.GetValue(entityInstance);

                            var findMethod = DbSet.GetType().GetMethod("Find");
                            var existingItem = findMethod?.Invoke(DbSet, new object[] { new object[] { idValue } });

                            if (existingItem != null)
                            {
                                UpdateItem(existingItem, entityInstance);
                            }
                        }
                    }
                }

                // DELETE
                foreach (var tabla in Actions.Delete)
                {
                    if (tabla.Value.Any())
                    {
                        // caso especial Warehouses
                        if (tabla.Key.Equals("Warehouses", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var value in tabla.Value)
                            {
                                if (value.ContainsKey("Id") && Guid.TryParse(value["Id"].ToString(), out var warehouseId))
                                {
                                    var workerIds = context.Roles
                                        .Where(r => r.WarehouseId == warehouseId)
                                        .Join(context.Workers, r => r.Id, w => w.RolId, (r, w) => w.Id)
                                        .ToList();

                                    context.RemoveRange(context.Workers.Where(w => workerIds.Contains(w.Id)));
                                    context.SaveChanges();
                                }
                            }
                        }

                        var dbSetProperty = context.GetType().GetProperty(tabla.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        var DbSet = dbSetProperty.GetValue(context);
                        var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
                        var findMethod = DbSet.GetType().GetMethod("Find");
                        var removeMethod = DbSet.GetType().GetMethod("Remove");

                        foreach (var value in tabla.Value)
                        {
                            if (value.ContainsKey("Id") && Guid.TryParse(value["Id"].ToString(), out var id))
                            {
                                var existingItem = findMethod?.Invoke(DbSet, new object[] { new object[] { id } });
                                if (existingItem != null)
                                    removeMethod?.Invoke(DbSet, new[] { existingItem });
                            }
                        }
                    }
                }

                context.SaveChanges();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
            {
                Console.WriteLine($"PG SqlState={pg.SqlState} Constraint={pg.ConstraintName} Message={pg.MessageText} Exception={ex.ToString()}");
                throw;
            }
        }

        public object ConvertToType(object value, Type targetType)
        {
            try
            {
                var serializeValue = JsonConvert.SerializeObject(value);
                return JsonConvert.DeserializeObject(serializeValue, targetType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public Actions CreateJson(Actions? json)
        {
            if (json == null)
            {
                Actions jsonNull = new Actions
                {
                    New = new Dictionary<string, List<Dictionary<string, object>>>(),
                    Delete = new Dictionary<string, List<Dictionary<string, object>>>(),
                    Update = new Dictionary<string, List<Dictionary<string, object>>>()
                };

                return jsonNull;
            }
            else
            {
                Actions jsonNotNull = new Actions
                {
                    New = json.New == null ? new Dictionary<string, List<Dictionary<string, object>>>() : json.New,
                    Delete = json.Delete == null ? new Dictionary<string, List<Dictionary<string, object>>>() : json.Delete,
                    Update = json.Update == null ? new Dictionary<string, List<Dictionary<string, object>>>() : json.Update
                };
                return jsonNotNull;
            }
        }

        public DataSimulatorTablaRequest SelectDataForSimulatorPlanning(Actions? postActions, Guid warehouseId)
        {
            try
            {
                Dictionary<string, IList> response = new Dictionary<string, IList>();

                var utcToday = DateTime.UtcNow.Date;

                //1-Read from database
                var dataSimulatorTableRequest = new DataSimulatorTablaRequest
                {
                    Aisle = context.Aisles.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Area = context.Areas.AsNoTracking().Where(m => m.Layout.WarehouseId == warehouseId).ToList(),
                    AutomaticStorage = context.AutomaticStorages.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    AvailableDocksPerStage = context.AvailableDocksPerStages.AsNoTracking().Where(x => x.Stage.Zone.Area.Layout.WarehouseId == warehouseId && x.Dock.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    AvailableWorker = context.AvailableWorkers.AsNoTracking().Where(m => m.Worker.Team.WarehouseId == warehouseId).Include(x => x.Worker).ThenInclude(w => w.Team).Include(x => x.Worker).ThenInclude(w => w.Rol).ToList(),
                    Break = context.Breaks.AsNoTracking().Where(m => m.BreakProfile.WarehouseId == warehouseId).ToList(),
                    Buffer = context.Buffers.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    ChaoticStorage = context.ChaoticStorages.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    CustomProcess = context.CustomProcesses.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).Include(x => x.Process).ThenInclude(x => x.Area).ToList(),
                    Date = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    Dock = context.Docks.AsNoTracking().Include(x => x.Zone).Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    DockSelectionStrategy = context.DockSelectionStrategies.AsNoTracking().ToList(),
                    DriveIn = context.DriveIns.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    EquipmentGroup = context.EquipmentGroups.AsNoTracking().Where(m => m.Area.Layout.WarehouseId == warehouseId).ToList(),
                    InboundFlowGraph = context.InboundFlowGraphs.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    InputOrder = context.InputOrders.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.AppointmentDate.Date == utcToday).Include(x => x.Delivery).ToList(),
                    InputOrderLine = context.InputOrderLines.AsNoTracking().Where(m => m.InputOutboundOrder.WarehouseId == warehouseId && m.InputOutboundOrder.AppointmentDate.Date == utcToday).Include(x => x.InputOutboundOrder).ToList(),
                    InputOrderProcessClosing = context.InputOrderProcessesClosing.AsNoTracking().Where(m => m.EndDate.Date == utcToday)
                        .Join(context.InputOrders.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.AppointmentDate.Date == utcToday), 
                        ipc => ipc.InputOrder, i => i.OrderCode, (ipc, i) => new InputOrderProcessClosing 
                        { 
                            Id = ipc.Id, 
                            NotificationId = ipc.NotificationId, 
                            ProcessType = ipc.ProcessType, 
                            Worker = ipc.Worker, 
                            EquipmentGroup = ipc.EquipmentGroup, 
                            InitDate = ipc.InitDate, 
                            EndDate = ipc.EndDate, 
                            InputOrder = ipc.InputOrder, 
                            NumProcesses = ipc.NumProcesses,
                            ZoneCode = ipc.ZoneCode
                        }).ToList(),
                    Layout = context.Layouts.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    LoadProfile = context.LoadProfiles.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    OrderLoadRatio = context.OrderLoadRatios.AsNoTracking().Where(m => m.Load.WarehouseId == warehouseId && m.Vehicle.WarehouseId == warehouseId).ToList(),
                    OrderPriority = context.OrderPriority.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.IsActive).ToList(),
                    OrderSchedule = context.OrderSchedules.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    OutboundFlowGraph = context.OutboundFlowGraphs.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    Picking = context.Pickings.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Process = context.Processes.AsNoTracking().Include(x => x.Area).ThenInclude(x => x.Layout).Where(m => m.Area.Layout.WarehouseId == warehouseId).ToList(),
                    ProcessDirectionProperty = context.ProcessDirectionProperties.AsNoTracking().Where(m => m.InitProcess.Area.Layout.WarehouseId == warehouseId && m.EndProcess.Area.Layout.WarehouseId == warehouseId).Include(x => x.InitProcess).ThenInclude(x => x.Area).Include(x => x.EndProcess).ThenInclude(x => x.Area).ToList(),
                    ProcessPriorityOrder = context.ProcessPriorityOrder.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.IsActive).ToList(),
                    Rack = context.Racks.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Replenishment = context.Replenishments.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Rol = context.Roles.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    RolProcessSequence = context.RolProcessSequences.AsNoTracking().Where(m => m.Rol.WarehouseId == warehouseId).ToList(),
                    Route = context.Routes.AsNoTracking().Where(m => m.ArrivalArea.Layout.WarehouseId == warehouseId && m.DepartureArea.Layout.WarehouseId == warehouseId).ToList(),
                    Schedule = context.Schedules.AsNoTracking().Where(m => m.AvailableWorker.Worker.Team.WarehouseId == warehouseId && m.Shift.WarehouseId == warehouseId).Include(s => s.AvailableWorker).ThenInclude(aw => aw.Worker).ThenInclude(r => r.Rol).Include(s => s.Shift).ToList(),
                    Shift = context.Shifts.AsNoTracking().Where(m => m.WarehouseId == warehouseId).Include(x => x.Schedules).ToList(),
                    Stage = context.Stages.AsNoTracking().Where(x => x.Zone.Area.Layout.WarehouseId == warehouseId).Include(x => x.Zone).ToList(),
                    Step = context.Steps.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Strategy = context.Strategies.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    TypeEquipment = context.TypeEquipment.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    VehicleProfile = context.VehicleProfiles.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    Warehouse = context.Warehouses.AsNoTracking().FirstOrDefault(m => m.Id == warehouseId),
                    Worker = context.Workers.AsNoTracking().Where(m => m.Team.WarehouseId == warehouseId).Include(x => x.Rol).ToList(),
                    YardAppointmentsNotifications = context.YardAppointmentsNotifications.AsNoTracking().Join(context.InputOrders.AsNoTracking().Where(x => x.WarehouseId == warehouseId && x.AppointmentDate.Date == utcToday).GroupBy(x => new { x.AppointmentDate, x.VehicleCode }).Select(x => new { x.Key.AppointmentDate, x.Key.VehicleCode }), yn => new { yn.VehicleCode, yn.AppointmentDate }, i => new { i.VehicleCode, i.AppointmentDate }, (yn, i) => new YardAppointmentsNotifications { Id = yn.Id, NotificationId = yn.NotificationId, AppointmentCode = yn.AppointmentCode, YardCode = yn.YardCode, VehicleCode = yn.VehicleCode, VehicleType = yn.VehicleType, DockCode = yn.DockCode, AppointmentDate = yn.AppointmentDate, InitDate = yn.InitDate, EndDate = yn.EndDate, Customer = yn.Customer, License = yn.License }).ToList(),
                    Zone = context.Zones.AsNoTracking().Include(x => x.Area).Where(m => m.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Deliveries = context.InputOrders.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.AppointmentDate.Date == utcToday && m.DeliveryId != null)
                    .Select(x => x.DeliveryId).Distinct()
                        .Join(context.Deliveries.AsNoTracking(), i => i, d => d.Id, (i,d) => new Deliveries()
                        {
                            Id = d.Id,
                            Code = d.Code,
                            PackagingType = d.PackagingType,
                            NumPackages = d.NumPackages,
                        }).ToList(),
                    Packings = context.Packing.AsNoTracking().Include(x => x.Process).Where(x => x.Process.Area.Layout.WarehouseId == warehouseId).ToList(),
                    PackingPacksModes = context.Packing.AsNoTracking().Include(x => x.Process).Where(x => x.Process.Area.Layout.WarehouseId == warehouseId)
                        .Join(context.PackingPacksMode.AsNoTracking().Include(x => x.Packing).Include(x => x.PackingMode), 
                        p => p.Id, ppm => ppm.PackingId, (p,ppm) => new PackingPacksMode()
                        {
                            Id = ppm.Id,
                            PackingId = ppm.PackingId,
                            Packing = ppm.Packing,
                            PackingModeId = ppm.PackingModeId,
                            PackingMode = ppm.PackingMode,
                            NumPackages = ppm.NumPackages
                        }).ToList()
                };


                var sequences = context.ConfigurationSequences.AsNoTracking().Where(m => m.Date.Date == utcToday && m.ConfigurationSequenceHeader.WarehouseId == warehouseId).Include(cs => cs.ConfigurationSequenceHeader).AsEnumerable().OrderBy(m => m.Sequence);

                //2-ConfigSequence
                foreach (var configurationSequence in sequences)
                {
                    var configSequenceData = System.Text.Json.JsonSerializer.Deserialize<PreviewDtoData>(configurationSequence.ConfigurationSequenceHeader.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    ProcessActions(dataSimulatorTableRequest, configSequenceData);
                }

                //3-Create orders
                var nowUtc = dataSimulatorTableRequest.Date?.ToUniversalTime() ?? DateTime.UtcNow;
                var today = nowUtc.Date;
                var nowTod = nowUtc.TimeOfDay;

                var orderSchedule = dataSimulatorTableRequest.OrderSchedule
                    .Where(s => (s.InitHour <= nowTod && s.EndHour >= nowTod) || s.InitHour >= nowTod)
                    .ToList();

                var loadRatio = dataSimulatorTableRequest.OrderLoadRatio.ToList();

                var todayOrdersExisting = dataSimulatorTableRequest.InputOrder.Where(o => o.AppointmentDate.Date == today).ToList();

                var parsedOrders = Process(todayOrdersExisting, orderSchedule, loadRatio);

                var avgOutbound = context.OutboundFlowGraphs.AsNoTracking().Select(x => x.AverageLinesPerOrder).FirstOrDefault();
                var avgInbound = context.InboundFlowGraphs.AsNoTracking().Select(x => x.AverageLinesPerOrder).FirstOrDefault();

                // Genera líneas para los pedidos procesados
                var newLines = CreateLinesForOrders(parsedOrders, avgOutbound, avgInbound);

                dataSimulatorTableRequest.InputOrder = parsedOrders.Concat(dataSimulatorTableRequest.InputOrder.Where(x => x.AppointmentDate.Date == DateTime.UtcNow.Date)).ToList();
                dataSimulatorTableRequest.InputOrderLine = newLines.Concat(dataSimulatorTableRequest.InputOrderLine.Where(x => x.InputOutboundOrder.AppointmentDate.Date == DateTime.UtcNow.Date)).ToList();

                return dataSimulatorTableRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSimulatorTablaRequest SelectDataForSimulatorPreview(Actions? postActions, Guid warehouseId, PreviewDto parametersRequest)
        {
            try
            {
                Dictionary<string, IList> response = new Dictionary<string, IList>();

                var utcToday = DateTime.UtcNow.Date;

                //1-Read from database
                var dataSimulatorTableRequest = new DataSimulatorTablaRequest
                {
                    Aisle = context.Aisles.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Area = context.Areas.AsNoTracking().Where(m => m.Layout.WarehouseId == warehouseId).ToList(),
                    AutomaticStorage = context.AutomaticStorages.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    AvailableDocksPerStage = context.AvailableDocksPerStages.AsNoTracking().Where(x => x.Stage.Zone.Area.Layout.WarehouseId == warehouseId && x.Dock.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    AvailableWorker = new List<AvailableWorker>(),
                    Break = context.Breaks.AsNoTracking().Where(m => m.BreakProfile.WarehouseId == warehouseId).ToList(),
                    Buffer = context.Buffers.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    ChaoticStorage = context.ChaoticStorages.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    CustomProcess = context.CustomProcesses.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).Include(x => x.Process).ThenInclude(x => x.Area).ToList(),
                    Date = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    Dock = context.Docks.AsNoTracking().Include(x => x.Zone).Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    DockSelectionStrategy = context.DockSelectionStrategies.AsNoTracking().ToList(),
                    DriveIn = context.DriveIns.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    EquipmentGroup = context.EquipmentGroups.AsNoTracking().Where(m => m.Area.Layout.WarehouseId == warehouseId).ToList(),
                    InboundFlowGraph = context.InboundFlowGraphs.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    InputOrder = new List<InputOrder>(),
                    InputOrderLine = new List<InputOrderLine>(),
                    InputOrderProcessClosing = context.InputOrderProcessesClosing.AsNoTracking().Where(m => m.EndDate.Date == utcToday).Join(context.InputOrders.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.AppointmentDate.Date == utcToday), ipc => ipc.InputOrder, i => i.OrderCode, (ipc, i) => new InputOrderProcessClosing { Id = ipc.Id, NotificationId = ipc.NotificationId, ProcessType = ipc.ProcessType, Worker = ipc.Worker, EquipmentGroup = ipc.EquipmentGroup, InitDate = ipc.InitDate, EndDate = ipc.EndDate, InputOrder = ipc.InputOrder, NumProcesses = ipc.NumProcesses }).ToList(),
                    Layout = context.Layouts.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    LoadProfile = context.LoadProfiles.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    OrderLoadRatio = context.OrderLoadRatios.AsNoTracking().Where(m => m.Load.WarehouseId == warehouseId && m.Vehicle.WarehouseId == warehouseId).ToList(),
                    OrderPriority = context.OrderPriority.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.IsActive).ToList(),
                    OrderSchedule = new List<OrderSchedule>(),
                    OutboundFlowGraph = context.OutboundFlowGraphs.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    Picking = context.Pickings.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Process = context.Processes.AsNoTracking().Include(x => x.Area).ThenInclude(x => x.Layout).Where(m => m.Area.Layout.WarehouseId == warehouseId).ToList(),
                    ProcessDirectionProperty = context.ProcessDirectionProperties.AsNoTracking().Where(m => m.InitProcess.Area.Layout.WarehouseId == warehouseId && m.EndProcess.Area.Layout.WarehouseId == warehouseId).Include(x => x.InitProcess).ThenInclude(x => x.Area).Include(x => x.EndProcess).ThenInclude(x => x.Area).ToList(),
                    ProcessPriorityOrder = context.ProcessPriorityOrder.AsNoTracking().Where(m => m.WarehouseId == warehouseId && m.IsActive).ToList(),
                    Rack = context.Racks.AsNoTracking().Where(m => m.Zone.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Replenishment = context.Replenishments.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Rol = context.Roles.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    RolProcessSequence = context.RolProcessSequences.AsNoTracking().Where(m => m.Rol.WarehouseId == warehouseId).ToList(),
                    Route = context.Routes.AsNoTracking().Where(m => m.ArrivalArea.Layout.WarehouseId == warehouseId && m.DepartureArea.Layout.WarehouseId == warehouseId).ToList(),
                    Schedule = new List<Schedule>(),
                    Shift = new List<Shift>(),
                    Stage = context.Stages.AsNoTracking().Where(x => x.Zone.Area.Layout.WarehouseId == warehouseId).Include(x => x.Zone).ToList(),
                    Step = context.Steps.AsNoTracking().Where(m => m.Process.Area.Layout.WarehouseId == warehouseId).ToList(),
                    Strategy = context.Strategies.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    TypeEquipment = context.TypeEquipment.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    VehicleProfile = context.VehicleProfiles.AsNoTracking().Where(m => m.WarehouseId == warehouseId).ToList(),
                    Warehouse = context.Warehouses.AsNoTracking().FirstOrDefault(m => m.Id == warehouseId),
                    Worker = new List<Worker>(),
                    YardAppointmentsNotifications = context.YardAppointmentsNotifications.AsNoTracking().Join(context.InputOrders.AsNoTracking().Where(x => x.WarehouseId == warehouseId && x.AppointmentDate.Date == utcToday).GroupBy(x => new { x.AppointmentDate, x.VehicleCode }).Select(x => new { x.Key.AppointmentDate, x.Key.VehicleCode }), yn => new { yn.VehicleCode, yn.AppointmentDate }, i => new { i.VehicleCode, i.AppointmentDate }, (yn, i) => new YardAppointmentsNotifications { Id = yn.Id, NotificationId = yn.NotificationId, AppointmentCode = yn.AppointmentCode, YardCode = yn.YardCode, VehicleCode = yn.VehicleCode, VehicleType = yn.VehicleType, DockCode = yn.DockCode, AppointmentDate = yn.AppointmentDate, InitDate = yn.InitDate, EndDate = yn.EndDate, Customer = yn.Customer, License = yn.License }).ToList(),
                    Zone = context.Zones.AsNoTracking().Include(x => x.Area).Where(m => m.Area.Layout.WarehouseId == warehouseId).ToList()
                };

                //2-Preview window information
                foreach (var Loadinput in parametersRequest.Loadinput)
                {
                    dataSimulatorTableRequest.OrderSchedule.Add(new OrderSchedule
                    {
                        Id = Loadinput.id,
                        InitHour = Loadinput.hour,
                        EndHour = Loadinput.endHour,
                        VehicleId = Loadinput.vehicleId,
                        Vehicle = dataSimulatorTableRequest.VehicleProfile.First(m => m.Id == Loadinput.vehicleId),
                        LoadId = Loadinput.loadId,
                        Load = dataSimulatorTableRequest.LoadProfile.First(m => m.Id == Loadinput.loadId),
                        NumberVehicles = Loadinput.numberVehicle,
                        IsOut = false,
                        WarehouseId = warehouseId,
                        Warehouse = dataSimulatorTableRequest.Warehouse!
                    });
                }

                foreach (var LoadOutput in parametersRequest.LoadOutput)
                {
                    dataSimulatorTableRequest.OrderSchedule.Add(new OrderSchedule
                    {
                        Id = LoadOutput.id,
                        InitHour = LoadOutput.hour,
                        EndHour = LoadOutput.endHour,
                        VehicleId = LoadOutput.vehicleId,
                        Vehicle = dataSimulatorTableRequest.VehicleProfile.First(m => m.Id == LoadOutput.vehicleId),
                        LoadId = LoadOutput.loadId,
                        Load = dataSimulatorTableRequest.LoadProfile.First(m => m.Id == LoadOutput.loadId),
                        NumberVehicles = LoadOutput.numberVehicle,
                        IsOut = true,
                        WarehouseId = warehouseId,
                        Warehouse = dataSimulatorTableRequest.Warehouse!
                    });
                }

                if (parametersRequest.ShiftRolDto.Any())
                {
                    dataSimulatorTableRequest.Rol = new List<Rol>();
                }

                foreach (var shiftrol in parametersRequest.ShiftRolDto)
                {
                    var shift = new Shift
                    {
                        Id = shiftrol.id,
                        Name = shiftrol.name,
                        InitHour = shiftrol.initHour.TotalHours,
                        EndHour = shiftrol.endHour.TotalHours,
                        WarehouseId = warehouseId,
                        Warehouse = dataSimulatorTableRequest.Warehouse!
                    };

                    foreach (var workersinrol in shiftrol.workersInRol)
                    {
                        var rol = new Rol
                        {
                            Id = workersinrol.id,
                            Name = workersinrol.name,
                            WarehouseId = warehouseId,
                            Warehouse = dataSimulatorTableRequest.Warehouse!
                        };

                        var counter = 0;
                        for (int i = 0; i < workersinrol.Workers; i++)
                        {
                            var equipo = context.Teams.FirstOrDefault(x => x.Id == context.Workers.First(x => x.Rol.WarehouseId == warehouseId && x.Rol.Name == workersinrol.name).TeamId && x.WarehouseId == warehouseId);

                            Worker worker = null;
                            if (equipo == null)
                            {
                                worker = new Worker
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"{workersinrol.name}_{counter}",
                                    TeamId = Guid.NewGuid(),
                                    Team = null,
                                    RolId = rol.Id,
                                    Rol = rol,
                                    WorkerNumber = counter
                                };
                            }
                            else
                            {
                                worker = new Worker
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"{workersinrol.name}_{counter}",
                                    TeamId = equipo.Id,
                                    Team = equipo,
                                    RolId = rol.Id,
                                    Rol = rol,
                                    WorkerNumber = counter
                                };
                            }
                            dataSimulatorTableRequest.Worker.Add(worker);

                            counter += 1;

                            var aworker = new AvailableWorker
                            {
                                Id = Guid.NewGuid(),
                                Name = $"{workersinrol.name}_{counter}",
                                WorkerId = worker.Id,
                                Worker = worker,
                            };

                            dataSimulatorTableRequest.AvailableWorker.Add(aworker);

                            dataSimulatorTableRequest.Schedule.Add(new Schedule()
                            {
                                Id = Guid.NewGuid(),
                                AvailableWorker = aworker,
                                BreakProfile = null,
                                Date = DateTime.Now,
                                Name = "schedule" + counter,
                                Shift = shift,
                                BreakProfileId = Guid.NewGuid(),
                                AvailableWorkerId = aworker.Id,
                                ShiftId = shift.Id,
                                CustomInitHour = null,
                                CustomEndHour = null,
                            });
                        }

                        dataSimulatorTableRequest.Rol.Add(rol);
                    }
                    dataSimulatorTableRequest.Shift.Add(shift);
                }

                //3-Create orders
                var OrdersAndLines = CreateOrdersAndLines(dataSimulatorTableRequest);
                dataSimulatorTableRequest.InputOrder.AddRange(OrdersAndLines.Orders);
                dataSimulatorTableRequest.InputOrderLine.AddRange(OrdersAndLines.Lines);


                return dataSimulatorTableRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSimulatorTablaRequest IntroduceUpdatedShiftInSchedule(DataSimulatorTablaRequest dataSimulatorTableRequest, SimulationParametrizedDto parametersRequest)
        {
            List<Shift> newShiftsToAdd = new List<Shift>();
            List<Schedule> newSchedulesToAdd = new List<Schedule>();


            var calendarios = parametersRequest.shiftDtos;

            foreach (var schedule in dataSimulatorTableRequest.Schedule)
            {
                var paramSchedule = calendarios.Where(x => schedule.ShiftId == x.id).FirstOrDefault();

                if (paramSchedule == null || paramSchedule.schedules == null) { continue; }

                var paramShift = paramSchedule.schedules.Where(x => schedule.Id == x.id).FirstOrDefault();

                if (paramShift == null || paramShift.initHour == null || paramShift.endHour == null) { continue; }

                if (paramShift.initHour?.TimeOfDay.TotalHours == schedule.Shift.InitHour
                    && paramShift.endHour?.TimeOfDay.TotalHours == schedule.Shift.EndHour)
                    continue;


                Guid NewShiftId = Guid.NewGuid();

                newShiftsToAdd.Add(new Shift
                {
                    Id = NewShiftId,
                    Name = schedule.Shift.Name + schedule.AvailableWorker.Name,
                    InitHour = paramShift.initHour?.TimeOfDay.TotalHours ?? 0,
                    EndHour = paramShift.endHour?.TimeOfDay.TotalHours ?? 0,
                    WarehouseId = schedule.Shift.WarehouseId,
                    Warehouse = schedule.Shift.Warehouse
                });

                //Schedule OldSchedule = dataSimulatorTableRequest.Schedule.First(m => m.Id == schedule.Id);
                //dataSimulatorTableRequest.Schedule.Remove(OldSchedule);
                newSchedulesToAdd.Add(new Schedule
                {
                    Id = schedule.Id,
                    Name = schedule.Name,
                    Date = schedule.Date,
                    AvailableWorkerId = schedule.AvailableWorkerId,
                    AvailableWorker = schedule.AvailableWorker,
                    ShiftId = NewShiftId,
                    Shift = newShiftsToAdd.First(m => m.Id == NewShiftId),
                    BreakProfileId = schedule.BreakProfileId,
                    BreakProfile = schedule.BreakProfile,
                    CustomInitHour = null,
                    CustomEndHour = null,
                });
            }

            dataSimulatorTableRequest.Schedule.RemoveAll(z => newSchedulesToAdd.Select(x => x.Id).Contains(z.Id));
            dataSimulatorTableRequest.Shift.AddRange(newShiftsToAdd);
            dataSimulatorTableRequest.Schedule.AddRange(newSchedulesToAdd);


            return dataSimulatorTableRequest;
        }
        public void ProcessActions(DataSimulatorTablaRequest response, PreviewDtoData? data)
        {
            try
            {
                // Input Loads
                foreach (var inputLoad in data.Loadinput)
                {

                    response.OrderSchedule.Add(new OrderSchedule
                    {
                        Id = Guid.NewGuid(),
                        InitHour = inputLoad.hour,
                        EndHour = inputLoad.endHour,
                        VehicleId = inputLoad.vehicleId,
                        Vehicle = response.VehicleProfile.Where(x => x.Id == inputLoad.vehicleId).FirstOrDefault(),
                        NumberVehicles = inputLoad.numberVehicle,
                        LoadId = inputLoad.loadId,
                        Load = response.LoadProfile.Where(x => x.Id == inputLoad.loadId).FirstOrDefault(),
                        IsOut = false,
                        WarehouseId = response.Warehouse.Id,
                        Warehouse = response.Warehouse
                    });
                }

                // Output Loads
                foreach (var inputLoad in data.LoadOutput)
                {
                    response.OrderSchedule.Add(new OrderSchedule
                    {
                        Id = Guid.NewGuid(),
                        InitHour = inputLoad.hour,
                        EndHour = inputLoad.endHour,
                        VehicleId = response.VehicleProfile.Where(x => x.Name == inputLoad.vehicle).Select(x => x.Id).FirstOrDefault(),
                        Vehicle = response.VehicleProfile.Where(x => x.Name == inputLoad.vehicle).FirstOrDefault(),
                        NumberVehicles = inputLoad.numberVehicle,
                        LoadId = response.LoadProfile.Where(x => x.Name == inputLoad.load).Select(x => x.Id).FirstOrDefault(),
                        Load = response.LoadProfile.Where(x => x.Name == inputLoad.load).FirstOrDefault(),
                        IsOut = true,
                        WarehouseId = response.Warehouse.Id,
                        Warehouse = response.Warehouse
                    });
                }
            }
            catch
            {
                throw;
            }
        }

        /* ---------------- Helpers ---------------- */

        public void UpdateItem(object existingItem, object newItem)
        {
            var type = existingItem.GetType();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite || prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    continue;

                var newVal = prop.GetValue(newItem);
                if (newVal != null)
                    prop.SetValue(existingItem, newVal);
            }
        }

        public void Fill(object existingItem, ApplicationDbContext context)
        {
            Type itemType = existingItem.GetType();

            if (itemType.GetInterface(nameof(IFillable)) != null)
            {
                existingItem.GetType().GetMethod("Fill").Invoke(existingItem, new object[] { context });
            }
        }

        private (List<InputOrder> Orders, List<InputOrderLine> Lines) CreateOrdersAndLines(DataSimulatorTablaRequest response)
        {
            try
            {
                List<InputOrder> orders = new List<InputOrder>();
                List<InputOrderLine> lines = new List<InputOrderLine>();
                int vehicleCount = 0;

                foreach (var parameter in response.OrderSchedule)
                {
                    var endHour = parameter.EndHour - TimeSpan.FromSeconds((parameter.IsOut ? response.OutboundFlowGraph.First().MaxVehicleTime : response.InboundFlowGraph.First().MaxVehicleTime) ?? 0);
                    var timeSpans = GetTimeSpans(parameter.InitHour, (parameter.InitHour < endHour) ? endHour : parameter.InitHour, (int) parameter.NumberVehicles);
                    for (int i = 0; i < parameter.NumberVehicles; i++)
                    {
                        var load = response.OrderLoadRatio.FirstOrDefault(m => m.VehicleId == parameter.VehicleId && m.LoadId == parameter.LoadId);

                        double loadPerVehicleRatio = load != null ? load.LoadInVehicle : 1;
                        double orderPerLoadRatio = load != null ? load.OrderInLoad : 1;

                        double totalOrdersCount = loadPerVehicleRatio * orderPerLoadRatio;

                        string flow = parameter.IsOut ? "Outbound" : "Inbound";
                        vehicleCount++;
                        string vehicleCode = $"{flow}_" + parameter.Vehicle.Name + $"_{vehicleCount}";

                        DateTime appointmentDate = DateTime.UtcNow.Date.Add(timeSpans[i]);

                        for (int j = 0; j < totalOrdersCount; j++)
                        {
                            InputOrder inputOrder = new InputOrder()
                            {
                                Id = Guid.NewGuid(),
                                OrderCode = string.Empty,
                                Warehouse = response.Warehouse,
                                WarehouseId = response.Warehouse.Id,
                                IsOutbound = parameter.IsOut,
                                IsEstimated = true,
                                Status = InputOrderStatus.Waiting,
                                RealArrivalTime = null,
                                AppointmentDate = appointmentDate,
                                VehicleCode = vehicleCode,
                                Progress = 0,
                                OrderSchedule = parameter
                            };
                            orders.Add(inputOrder);

                            var outGraph = response.OutboundFlowGraph.FirstOrDefault();
                            var inGraph = response.InboundFlowGraph.FirstOrDefault();

                            var avgLines = inputOrder.IsOutbound ? (outGraph == null ? 0 : outGraph.AverageLinesPerOrder) : (inGraph == null ? 0 : inGraph.AverageLinesPerOrder);

                            for (int k = 0; k < avgLines; k++)
                            {
                                lines.Add(new InputOrderLine()
                                {
                                    Id = Guid.NewGuid(),
                                    InputOutboundOrder = inputOrder,
                                    InputOutboundOrderId = inputOrder.Id
                                });
                            }
                        }
                    }
                }

                return (orders, lines);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<TimeSpan> GetTimeSpans(TimeSpan from, TimeSpan to, int fragments)
        {
            if (fragments < 2)
                return new List<TimeSpan> { to }; // Caso excepcional

            var totalTicks = to.Ticks - from.Ticks;

            return Enumerable.Range(0, fragments)
                .Select(i => from + TimeSpan.FromTicks((totalTicks * i) / (fragments - 1)))
                .ToList();
        }

        public void SavePreview(PreviewDto config, Guid WarehouseId)
        {
            try
            {
                var configSequences = context.ConfigurationSequenceHeaders.FirstOrDefault(x => x.Id == config.Temporalidad.Id);

                var previewData = new
                {
                    config.Loadinput,
                    config.LoadOutput,
                    config.ShiftRolDto
                };

                if (configSequences == null)
                {
                    double differenceDate = (config.Temporalidad.FechaHasta.Date - config.Temporalidad.FechaDesde.Date).TotalDays;

                    var NewTemporalidad = new ConfigurationSequenceHeader()
                    {
                        Id = config.Temporalidad.Id,
                        Code = config.Temporalidad.Nombre,
                        WarehouseId = config.Temporalidad.WarehouseId,
                        Warehouse = context.Warehouses.First(x => x.Id == config.Temporalidad.WarehouseId),
                        Data = JsonConvert.SerializeObject(previewData),
                    };

                    context.ConfigurationSequenceHeaders.Add(NewTemporalidad);


                    for (int i = 0; i <= differenceDate; i++)
                    {
                        var fecha = DateTime.SpecifyKind(config.Temporalidad.FechaDesde.AddDays(i), DateTimeKind.Utc);

                        var tempSequence = new ConfigurationSequence
                        {
                            ConfigurationSequenceHeaderId = config.Temporalidad.Id,
                            Date = fecha,
                            Id = Guid.NewGuid(),
                            Sequence = context.ConfigurationSequences.Any(x => x.Date == fecha.Date)
                                ? context.ConfigurationSequences.Where(x => x.Date == fecha.Date).Max(x => x.Sequence) + 1 : 0,
                            ConfigurationSequenceHeader = NewTemporalidad
                        };

                        context.ConfigurationSequences.Add(tempSequence);
                    }
                }
                else
                {
                    configSequences.Id = config.Temporalidad.Id;
                    configSequences.Code = config.Temporalidad.Nombre;
                    configSequences.WarehouseId = config.Temporalidad.WarehouseId;
                    configSequences.Warehouse = context.Warehouses.First(x => x.Id == config.Temporalidad.WarehouseId);
                    configSequences.Data = JsonConvert.SerializeObject(previewData);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<InputOrder> Process(List<InputOrder> orders, List<OrderSchedule> orderSchedulers, List<OrderLoadRatio> orderLoadRatio)
        {
            try
            {
                List<InputOrder> tmpOrders = new List<InputOrder>();
                tmpOrders.AddRange(GenerateOrders(orders, orderSchedulers, orderLoadRatio, true));
                tmpOrders.AddRange(GenerateOrders(orders, orderSchedulers, orderLoadRatio, false));

                return tmpOrders;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<InputOrder> GenerateOrders(List<InputOrder> orders, List<OrderSchedule> orderSchedulers, List<OrderLoadRatio> orderLoadRatio, bool isOut)
        {
            try
            {
                var finalOrders = new List<InputOrder>();

                // Filtra por flujo y ordena horarios
                var sched = orderSchedulers.Where(x => x.IsOut == isOut)
                                           .OrderBy(s => s.InitHour)
                                           .ToList();
                if (sched.Count == 0) return finalOrders;

                Guid schedWarehouseId = sched.Select(s => s.WarehouseId).Distinct().FirstOrDefault();

                // Diccionario de ratios (acceso O(1))
                var ratioMap = (orderLoadRatio ?? new List<OrderLoadRatio>())
                    .ToDictionary(r => (r.VehicleId, r.LoadId),
                                  r => (LoadInVehicle: r.LoadInVehicle, OrderInLoad: r.OrderInLoad));

                // Merge de intervalos sumando OrderCount
                var merged = new List<(TimeSpan InitHour, TimeSpan EndHour, int OrderCount)>();
                foreach (var s in sched)
                {
                    var ratios = ratioMap.TryGetValue((s.VehicleId, s.LoadId), out var v)
                        ? v
                        : (LoadInVehicle: 1d, OrderInLoad: 1d);

                    int estimated = (int)Math.Round(
                        s.NumberVehicles * ratios.LoadInVehicle * ratios.OrderInLoad,
                        MidpointRounding.AwayFromZero);

                    if (merged.Count == 0)
                    {
                        merged.Add((s.InitHour, s.EndHour, estimated));
                    }
                    else
                    {
                        var last = merged[^1];
                        if (s.InitHour < last.EndHour) // solapa
                        {
                            var newEnd = s.EndHour > last.EndHour ? s.EndHour : last.EndHour;
                            merged[^1] = (last.InitHour, newEnd, last.OrderCount + estimated);
                        }
                        else
                        {
                            merged.Add((s.InitHour, s.EndHour, estimated));
                        }
                    }
                }

                // Datos de almacén (para las órdenes nuevas)
                var firstSched = sched[0];
                var warehouse = firstSched.Warehouse;
                var warehouseId = firstSched.WarehouseId;

                var times = orders
                    .Where(o => o.IsOutbound == isOut)
                    .Select(o => o.AppointmentDate.TimeOfDay)
                    .OrderBy(t => t)
                    .ToArray();


                // Two-pointers O(n + k) 
                int iStart = 0, iEnd = 0;
                int iteration = 0;
                string flow = isOut ? "Outbound" : "Inbound";

                Console.WriteLine($"[GenerateOrders] isOut={isOut} schedWh={schedWarehouseId} timesLen={times.Length} mergedBlocks={merged.Count}");

                foreach (var m in merged)
                {
                    while (iStart < times.Length && times[iStart] < m.InitHour) iStart++;
                    if (iEnd < iStart) iEnd = iStart;
                    while (iEnd < times.Length && times[iEnd] <= m.EndHour) iEnd++;

                    int inputOrders = iEnd - iStart;
                    int restOfOrders = m.OrderCount - inputOrders;

                    if (restOfOrders < 0) restOfOrders = 0;

                    if (restOfOrders == 0) { iteration++; continue; }

                    string vehicleCode = $"{flow}_Vehicle_{iteration}";
                    DateTime appointmentDate = RandomDateTime(m.InitHour, m.EndHour);

                    for (int j = 0; j < restOfOrders; j++)
                    {
                        finalOrders.Add(new InputOrder
                        {
                            Id = Guid.NewGuid(),
                            OrderCode = string.Empty,
                            Warehouse = warehouse,
                            WarehouseId = warehouseId,
                            IsOutbound = isOut,
                            IsEstimated = true,
                            Status = InputOrderStatus.Waiting,
                            RealArrivalTime = null,
                            AppointmentDate = appointmentDate,
                            VehicleCode = vehicleCode,
                            Progress = 0
                        });
                    }

                    iteration++;
                }

                return finalOrders;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<InputOrderLine> CreateLinesForOrders(List<InputOrder> orders, int avgLinesOut, int avgLinesIn)
        {
            try
            {
                avgLinesOut = avgLinesOut > 0 ? avgLinesOut : 1;
                avgLinesIn = avgLinesIn > 0 ? avgLinesIn : 1;
                var lines = new List<InputOrderLine>();
                foreach (var order in orders)
                {
                    var avgLines = order.IsOutbound ? avgLinesOut : avgLinesIn;

                    for (var i = 0; i < avgLines; i++)
                        lines.Add(new InputOrderLine()
                        {
                            InputOutboundOrder = order,
                            Id = new Guid(),
                            InputOutboundOrderId = order.Id,
                            IsClosed = false,
                            Product = null,
                            Quantity = 10,
                            UnitOfMeasure = null,
                        });
                }

                return lines;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static DateTime RandomDateTime(TimeSpan initHour, TimeSpan endHour)
        {
            try
            {
                DateTime start = DateTime.UtcNow.Date.Add(initHour);
                DateTime end = DateTime.UtcNow.Date.Add(endHour);

                Random rnd = new Random();
                long rangoTicks = (end - start).Ticks;
                long ticksAleatorios = (long)(rnd.NextDouble() * rangoTicks);
                return start.AddTicks(ticksAleatorios);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GenerateSequentialName(ApplicationDbContext context, string baseName)
        {
            var match = Regex.Match(baseName, @"^(.*?)(\d+)?$");

            string namePart = match.Groups[1].Value;
            string numberPart = match.Groups[2].Value;

            int startingNumber = 1;

            if (!string.IsNullOrEmpty(numberPart))
                startingNumber = int.Parse(numberPart);

            var existing = context.Warehouses
                .Where(w => w.Code.StartsWith(namePart))
                .Select(w => w.Code)
                .ToList();

            int maxNumber = 0;

            foreach (var name in existing)
            {
                var m = Regex.Match(name, @"^" + Regex.Escape(namePart) + @"(\d+)$");
                if (m.Success)
                {
                    int num = int.Parse(m.Groups[1].Value);
                    if (num > maxNumber)
                        maxNumber = num;
                }
            }

            int nextNumber = Math.Max(startingNumber, maxNumber + 1);

            return $"{namePart}{nextNumber}";
        }

        public Guid? CloneLayout(Guid layoutId)
        {
            try
            {
                var cloner = new Cloner();

                var layout = context.Layouts
                    .Include(x => x.Warehouse)
                    .FirstOrDefault(x => x.Id == layoutId);

                if (layout == null) return null;

                var globalIdMap = new Dictionary<Guid, Guid>(capacity: 1024);

                // Layout y Warehouse
                Layout clonedLayout = (Layout)layout.Clone();
                Warehouse clonedWarehouse = (Warehouse)layout.Warehouse.Clone();
                clonedLayout.WarehouseId = clonedWarehouse.Id;
                clonedWarehouse.Code = GenerateSequentialName(context, layout.Warehouse.Code);
                clonedLayout.Warehouse = clonedWarehouse;

                // Remapear Viewport del layout
                clonedLayout.Viewport = ViewPortIdRemapper.Remap(layout.Viewport, clonedLayout.Id, globalIdMap);

                // Mapear ids globales
                globalIdMap[layout.Id] = clonedLayout.Id;
                globalIdMap[layout.Warehouse.Id] = clonedWarehouse.Id;

                var clonedAreas = new List<Area>();
                var clonedAisles = new List<Aisle>();
                var clonedAutomaticStorages = new List<AutomaticStorage>();
                var clonedAvailableDocksPerStage = new List<AvailableDocksPerStage>();
                var clonedAvailableWorkers = new List<AvailableWorker>();
                var clonedObjects = new List<Objects>();
                var clonedBreakProfiles = new List<BreakProfile>();
                var clonedBreaks = new List<Break>();
                var clonedBuffers = new List<Models.Models.Buffer>();
                var clonedChaoticStorages = new List<ChaoticStorage>();
                var clonedCustomProcesses = new List<CustomProcess>();
                var clonedDocks = new List<Dock>();
                var clonedDockSelectionStrategies = new List<DockSelectionStrategy>();
                var clonedDriveIns = new List<DriveIn>();
                var clonedEquipmentGroups = new List<EquipmentGroup>();
                var clonedInbounds = new List<Inbound>();
                var clonedInboundFlowGraphs = new List<InboundFlowGraph>();
                var clonedFlow = new List<Flow>();
                var clonedCustomFlowGraph = new List<CustomFlowGraph>();
                var clonedLoadings = new List<Loading>();
                var clonedLoadProfiles = new List<LoadProfile>();
                var clonedOrderSchedules = new List<OrderSchedule>();
                var clonedOutboundFlowGraphs = new List<OutboundFlowGraph>();
                var clonedOrderLoadRatios = new List<OrderLoadRatio>();
                var clonedOrderPriorities = new List<OrderPriority>();
                var clonedPickings = new List<Picking>();
                var clonedPostprocessProfiles = new List<PostprocessProfile>();
                var clonedPreprocessProfiles = new List<PreprocessProfile>();
                var clonedProcesses = new List<Models.Models.Process>();
                var clonedProcessHours = new List<ProcessHour>();
                var clonedProcessDirectionProperties = new List<ProcessDirectionProperty>();
                var clonedProcessPriorityOrder = new List<ProcessPriorityOrder>();
                var clonedPutawayProfiles = new List<PutawayProfile>();
                var clonedPutaways = new List<Putaway>();
                var clonedRacks = new List<Rack>();
                var clonedReceptions = new List<Reception>();
                var clonedReplenishments = new List<Replenishment>();
                var clonedRolProcessSequences = new List<RolProcessSequence>();
                var clonedRols = new List<Rol>();
                var clonedRoutes = new List<Route>();
                var clonedSchedules = new List<Schedule>();
                var clonedShifts = new List<Shift>();
                var clonedShippings = new List<Shipping>();
                var clonedSLAConfig = new List<SLAConfig>();
                var clonedStages = new List<Stage>();
                var clonedSteps = new List<Step>();
                var clonedStrategies = new List<Strategy>();
                var clonedStrategySequences = new List<StrategySequence>();
                var clonedTeams = new List<Team>();
                var clonedTypeEquipments = new List<TypeEquipment>();
                var clonedVehicleProfiles = new List<VehicleProfile>();
                var clonedWidgets = new List<Widget>();
                var clonedWorkers = new List<Worker>();
                var clonedZones = new List<Zone>();

                // Relación (old -> clonedEntity)
                var oldflows = new Dictionary<Guid, Flow>();
                var oldnewprocess = new Dictionary<Guid, Models.Models.Process>();
                var oldnewareas = new Dictionary<Guid, Area>();
                var oldnewrols = new Dictionary<Guid, Rol>();
                var oldnewshifts = new Dictionary<Guid, Shift>();
                var oldnewteams = new Dictionary<Guid, Team>();
                var oldnewtypeequipments = new Dictionary<Guid, TypeEquipment>();
                var oldnewvehicleprofiles = new Dictionary<Guid, VehicleProfile>();
                var oldnewbreakprofiles = new Dictionary<Guid, BreakProfile>();
                var oldnewavailableworkers = new Dictionary<Guid, AvailableWorker>();
                var oldnewloadprofiles = new Dictionary<Guid, LoadProfile>();

                // Flow
                var flows = context.Flow.Where(x => x.WarehouseId == layout.WarehouseId).ToList();
                foreach (var flow in flows)
                {
                    var cf = (Flow)flow.Clone();
                    cf.WarehouseId = clonedWarehouse.Id;
                    cf.Warehouse = clonedWarehouse;

                    oldflows.Add(flow.Id, cf);
                    clonedFlow.Add(cf);

                    globalIdMap[flow.Id] = cf.Id;
                }

                // Áreas
                var areas = context.Areas.Where(x => x.LayoutId == layout.Id).ToList();
                foreach (var area in areas)
                {
                    var ca = (Area)area.Clone();
                    ca.LayoutId = clonedLayout.Id;
                    ca.Layout = clonedLayout;

                    // Remapear ViewPort del área con el mapa global actual (se irá completando)
                    ca.ViewPort = ViewPortIdRemapper.Remap(area.ViewPort, ca.Id, globalIdMap);

                    oldnewareas.Add(area.Id, ca);
                    clonedAreas.Add(ca);

                    globalIdMap[area.Id] = ca.Id;
                }

                // Procesos por área
                foreach (var area in areas)
                {
                    var ca = oldnewareas[area.Id];

                    var processes = context.Processes.Where(x => x.AreaId == area.Id).ToList();
                    foreach (var process in processes)
                    {
                        var cp = (Models.Models.Process)process.Clone();
                        cp.AreaId = ca.Id;
                        cp.Area = ca;

                        if (process.FlowId.HasValue && oldflows.TryGetValue(process.FlowId.Value, out var cf))
                        {
                            cp.FlowId = cf.Id;
                            cp.Flow = cf;
                        }

                        // Remap ViewPort del proceso
                        cp.ViewPort = ViewPortIdRemapper.Remap(process.ViewPort, cp.Id, globalIdMap);

                        oldnewprocess.Add(process.Id, cp);
                        clonedProcesses.Add(cp);

                        globalIdMap[process.Id] = cp.Id;

                        // ------------- Hijos del proceso -------------
                        var inb = context.Inbounds.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (inb != null)
                        {
                            var cin = (Inbound)inb.Clone();
                            cin.ProcessId = cp.Id;
                            cin.Process = cp;
                            cin.Viewport = ViewPortIdRemapper.Remap(inb.Viewport, cin.Id, globalIdMap);
                            clonedInbounds.Add(cin);
                            globalIdMap[inb.Id] = cin.Id;
                        }

                        var rec = context.Receptions.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (rec != null)
                        {
                            var cr = (Reception)rec.Clone();
                            cr.ProcessId = cp.Id;
                            cr.Process = cp;
                            cr.ViewPort = ViewPortIdRemapper.Remap(rec.ViewPort, cr.Id, globalIdMap);
                            clonedReceptions.Add(cr);
                            globalIdMap[rec.Id] = cr.Id;
                        }

                        var put = context.Putaways.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (put != null)
                        {
                            var cput = (Putaway)put.Clone();
                            cput.ProcessId = cp.Id;
                            cput.Process = cp;
                            cput.ViewPort = ViewPortIdRemapper.Remap(put.ViewPort, cput.Id, globalIdMap);
                            clonedPutaways.Add(cput);
                            globalIdMap[put.Id] = cput.Id;
                        }

                        var pick = context.Pickings.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (pick != null)
                        {
                            var cpk = (Picking)pick.Clone();
                            cpk.ProcessId = cp.Id;
                            cpk.Process = cp;
                            cpk.ViewPort = ViewPortIdRemapper.Remap(pick.ViewPort, cpk.Id, globalIdMap);
                            clonedPickings.Add(cpk);
                            globalIdMap[pick.Id] = cpk.Id;
                        }

                        var repl = context.Replenishments.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (repl != null)
                        {
                            var crepl = (Replenishment)repl.Clone();
                            crepl.ProcessId = cp.Id;
                            crepl.Process = cp;
                            crepl.ViewPort = ViewPortIdRemapper.Remap(repl.ViewPort, crepl.Id, globalIdMap);
                            clonedReplenishments.Add(crepl);
                            globalIdMap[repl.Id] = crepl.Id;
                        }

                        var ship = context.Shippings.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (ship != null)
                        {
                            var csh = (Shipping)ship.Clone();
                            csh.ProcessId = cp.Id;
                            csh.Process = cp;
                            csh.ViewPort = ViewPortIdRemapper.Remap(ship.ViewPort, csh.Id, globalIdMap);
                            clonedShippings.Add(csh);
                            globalIdMap[ship.Id] = csh.Id;
                        }

                        var load = context.Loadings.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (load != null)
                        {
                            var cld = (Loading)load.Clone();
                            cld.ProcessId = cp.Id;
                            cld.Process = cp;
                            cld.Viewport = ViewPortIdRemapper.Remap(load.Viewport, cld.Id, globalIdMap);
                            clonedLoadings.Add(cld);
                            globalIdMap[load.Id] = cld.Id;
                        }

                        var cproc = context.CustomProcesses.FirstOrDefault(x => x.ProcessId == process.Id);
                        if (cproc != null)
                        {
                            var ccp = (CustomProcess)cproc.Clone();
                            ccp.ProcessId = cp.Id;
                            ccp.Process = cp;
                            ccp.ViewPort = ViewPortIdRemapper.Remap(cproc.ViewPort, ccp.Id, globalIdMap);
                            clonedCustomProcesses.Add(ccp);
                            globalIdMap[cproc.Id] = ccp.Id;
                        }

                        var steps = context.Steps.Where(x => x.ProcessId == process.Id).ToList();
                        foreach (var step in steps)
                        {
                            var cst = (Step)step.Clone();
                            cst.ProcessId = cp.Id;
                            cst.Process = cp;
                            cst.ViewPort = ViewPortIdRemapper.Remap(step.ViewPort, cst.Id, globalIdMap);
                            clonedSteps.Add(cst);
                            globalIdMap[step.Id] = cst.Id;
                        }

                        var phs = context.ProcessHours.Where(x => x.ProcessId == process.Id).ToList();
                        foreach (var ph in phs)
                        {
                            var cph = (ProcessHour)ph.Clone();
                            cph.ProcessId = cp.Id;
                            cph.Process = cp;
                            clonedProcessHours.Add(cph);
                            globalIdMap[ph.Id] = cph.Id;
                        }
                    }

                    // Zonas (Stations)
                    var zones = context.Zones.Where(x => x.AreaId == area.Id).ToList();
                    foreach (var zone in zones)
                    {
                        var cz = (Zone)zone.Clone();
                        cz.AreaId = ca.Id;
                        cz.Area = ca;
                        cz.ViewPort = ViewPortIdRemapper.Remap(zone.ViewPort, cz.Id, globalIdMap);
                        clonedZones.Add(cz);

                        globalIdMap[zone.Id] = cz.Id;

                        // Hijos de Zone
                        var dock = context.Docks.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (dock != null)
                        {
                            var cd = (Dock)dock.Clone();
                            cd.ZoneId = cz.Id;
                            cd.Zone = cz;
                            cd.ViewPort = ViewPortIdRemapper.Remap(dock.ViewPort, cd.Id, globalIdMap);
                            clonedDocks.Add(cd);
                            globalIdMap[dock.Id] = cd.Id;
                        }

                        var buff = context.Buffers.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (buff != null)
                        {
                            var cb = (Models.Models.Buffer)buff.Clone();
                            cb.ZoneId = cz.Id;
                            cb.Zone = cz;
                            cb.ViewPort = ViewPortIdRemapper.Remap(buff.ViewPort, cb.Id, globalIdMap);
                            clonedBuffers.Add(cb);
                            globalIdMap[buff.Id] = cb.Id;
                        }

                        var stage = context.Stages.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (stage != null)
                        {
                            var cs = (Models.Models.Stage)stage.Clone();
                            cs.ZoneId = cz.Id;
                            cs.Zone = cz;
                            cs.ViewPort = ViewPortIdRemapper.Remap(stage.ViewPort, cs.Id, globalIdMap);
                            clonedStages.Add(cs);
                            globalIdMap[stage.Id] = cs.Id;
                        }

                        var aut = context.AutomaticStorages.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (aut != null)
                        {
                            var caa = (AutomaticStorage)aut.Clone();
                            caa.ZoneId = cz.Id;
                            caa.Zone = cz;
                            caa.ViewPort = ViewPortIdRemapper.Remap(aut.ViewPort, caa.Id, globalIdMap);
                            clonedAutomaticStorages.Add(caa);
                            globalIdMap[aut.Id] = caa.Id;
                        }

                        var chs = context.ChaoticStorages.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (chs != null)
                        {
                            var cchs = (ChaoticStorage)chs.Clone();
                            cchs.ZoneId = cz.Id;
                            cchs.Zone = cz;
                            cchs.ViewPort = ViewPortIdRemapper.Remap(chs.ViewPort, cchs.Id, globalIdMap);
                            clonedChaoticStorages.Add(cchs);
                            globalIdMap[chs.Id] = cchs.Id;
                        }

                        var di = context.DriveIns.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (di != null)
                        {
                            var cdi = (DriveIn)di.Clone();
                            cdi.ZoneId = cz.Id;
                            cdi.Zone = cz;
                            cdi.ViewPort = ViewPortIdRemapper.Remap(di.ViewPort, cdi.Id, globalIdMap);
                            clonedDriveIns.Add(cdi);
                            globalIdMap[di.Id] = cdi.Id;
                        }

                        var rack = context.Racks.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (rack != null)
                        {
                            var crk = (Rack)rack.Clone();
                            crk.ZoneId = cz.Id;
                            crk.Zone = cz;
                            crk.ViewPort = ViewPortIdRemapper.Remap(rack.ViewPort, crk.Id, globalIdMap);
                            clonedRacks.Add(crk);
                            globalIdMap[rack.Id] = crk.Id;
                        }

                        var aisle = context.Aisles.FirstOrDefault(x => x.ZoneId == zone.Id);
                        if (aisle != null)
                        {
                            var cai = (Aisle)aisle.Clone();
                            cai.ZoneId = cz.Id;
                            cai.Zone = cz;
                            cai.ViewPort = ViewPortIdRemapper.Remap(aisle.ViewPort, cai.Id, globalIdMap);
                            clonedAisles.Add(cai);
                            globalIdMap[aisle.Id] = cai.Id;
                        }
                    }
                }

                context.Docks.AddRange(clonedDocks);
                context.Stages.AddRange(clonedStages);

                // AvailableDocksPerStage
                var availableDocksPerStage = context.AvailableDocksPerStages.Where(x => x.Stage.Zone.Area.LayoutId == layoutId).ToList();
                foreach (var ad in availableDocksPerStage)
                {
                    var cad = (Models.Models.AvailableDocksPerStage)ad.Clone();
                    cad.StageId = globalIdMap[ad.StageId];
                    cad.Stage = context.Stages.FirstOrDefault(x => x.Id == globalIdMap[ad.StageId]);
                    cad.DockId = globalIdMap[ad.DockId];
                    cad.Dock = context.Docks.FirstOrDefault(x => x.Id == globalIdMap[ad.DockId]);
                    clonedAvailableDocksPerStage.Add(cad);
                    globalIdMap[ad.Id] = cad.Id;
                }

                // Objetos de Layout
                var objects = context.Objects.Where(x => x.LayoutId == layout.Id).ToList();
                foreach (var item in objects)
                {
                    var co = (Objects)item.Clone();
                    co.LayoutId = clonedLayout.Id;
                    co.Layout = clonedLayout;
                    // Remap ViewPort con el mapa global y su propio Id nuevo
                    co.ViewPort = ViewPortIdRemapper.Remap(item.ViewPort, co.Id, globalIdMap);
                    clonedObjects.Add(co);
                    globalIdMap[item.Id] = co.Id;
                }

                // Break profiles + breaks
                var breakprofiles = context.BreakProfiles.Where(x => x.WarehouseId == layout.WarehouseId).ToList();
                foreach (var bp in breakprofiles)
                {
                    var cbp = (BreakProfile)bp.Clone();
                    cbp.WarehouseId = clonedWarehouse.Id;
                    cbp.Warehouse = clonedWarehouse;
                    oldnewbreakprofiles.Add(bp.Id, cbp);
                    clonedBreakProfiles.Add(cbp);
                    globalIdMap[bp.Id] = cbp.Id;

                    var breaks = context.Breaks.Where(x => x.BreakProfileId == bp.Id).ToList();
                    foreach (var b in breaks)
                    {
                        var cb = (Break)b.Clone();
                        cb.BreakProfileId = cbp.Id;
                        cb.BreakProfile = cbp;
                        clonedBreaks.Add(cb);
                        globalIdMap[b.Id] = cb.Id;
                    }
                }

                // Inbound / Outbound flow graphs, custom flow graphs
                var inboundFlowGraphs = context.InboundFlowGraphs.Where(x => x.WarehouseId == layout.WarehouseId).ToList();
                foreach (var ifg in inboundFlowGraphs)
                {
                    var cifg = (InboundFlowGraph)ifg.Clone();
                    cifg.WarehouseId = clonedWarehouse.Id;
                    cifg.Warehouse = clonedWarehouse;
                    if (ifg.FlowId.HasValue && oldflows.TryGetValue(ifg.FlowId.Value, out var cf))
                    {
                        cifg.FlowId = cf.Id;
                        cifg.Flow = cf;
                    }

                    cifg.ViewPort = ViewPortIdRemapper.Remap(ifg.ViewPort, cifg.Id, globalIdMap);

                    clonedInboundFlowGraphs.Add(cifg);
                    globalIdMap[ifg.Id] = cifg.Id;
                }

                var customFlowGraphs = context.CustomFlowGraphs
                    .Include(x => x.Flow)
                    .Where(x => x.Flow.WarehouseId == layout.WarehouseId)
                    .ToList();

                foreach (var cfg in customFlowGraphs)
                {
                    var ccfg = (CustomFlowGraph)cfg.Clone();

                    if (cfg.FlowId.HasValue && globalIdMap.TryGetValue(cfg.FlowId.Value, out var newFlowId))
                    {
                        ccfg.FlowId = newFlowId;

                        if (oldflows.TryGetValue(cfg.FlowId.Value, out var newFlow))
                            ccfg.Flow = newFlow;
                    }

                    clonedCustomFlowGraph.Add(ccfg);
                    globalIdMap[cfg.Id] = ccfg.Id;
                }

                var loadProfiles = context.LoadProfiles.Where(x => x.WarehouseId == layout.WarehouseId).ToList();
                foreach (var lp in loadProfiles)
                {
                    var clp = (LoadProfile)lp.Clone();
                    clp.WarehouseId = clonedWarehouse.Id;
                    clp.Warehouse = clonedWarehouse;
                    oldnewloadprofiles.Add(lp.Id, clp);
                    clonedLoadProfiles.Add(clp);
                    globalIdMap[lp.Id] = clp.Id;
                }

                var outboundFlowGraphs = context.OutboundFlowGraphs.Where(x => x.WarehouseId == layout.WarehouseId).ToList();
                foreach (var ofg in outboundFlowGraphs)
                {
                    var cofg = (OutboundFlowGraph)ofg.Clone();
                    if (ofg.FlowId.HasValue && oldflows.TryGetValue(ofg.FlowId.Value, out var cf))
                    {
                        cofg.FlowId = cf.Id;
                        cofg.Flow = cf;
                    }

                    cofg.WarehouseId = clonedWarehouse.Id;
                    cofg.Warehouse = clonedWarehouse;
                    cofg.ViewPort = ViewPortIdRemapper.Remap(ofg.ViewPort, cofg.Id, globalIdMap);

                    clonedOutboundFlowGraphs.Add(cofg);
                    globalIdMap[ofg.Id] = cofg.Id;
                }

                // Post/Pre/Putaway profiles
                foreach (var p in context.PostprocessProfiles.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var cp = (PostprocessProfile)p.Clone();
                    cp.WarehouseId = clonedWarehouse.Id;
                    cp.Warehouse = clonedWarehouse;
                    clonedPostprocessProfiles.Add(cp);
                    globalIdMap[p.Id] = cp.Id;
                }
                foreach (var p in context.PreprocessProfiles.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var cp = (PreprocessProfile)p.Clone();
                    cp.WarehouseId = clonedWarehouse.Id;
                    cp.Warehouse = clonedWarehouse;
                    clonedPreprocessProfiles.Add(cp);
                    globalIdMap[p.Id] = cp.Id;
                }
                foreach (var p in context.PutawayProfiles.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var cp = (PutawayProfile)p.Clone();
                    cp.WarehouseId = clonedWarehouse.Id;
                    cp.Warehouse = clonedWarehouse;
                    clonedPutawayProfiles.Add(cp);
                    globalIdMap[p.Id] = cp.Id;
                }

                // Roles, Shifts, Teams, TypeEquipment, VehicleProfiles, Widgets
                foreach (var rol in context.Roles.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (Rol)rol.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    oldnewrols.Add(rol.Id, c);
                    clonedRols.Add(c);
                    globalIdMap[rol.Id] = c.Id;
                }
                foreach (var sh in context.Shifts.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (Shift)sh.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    oldnewshifts.Add(sh.Id, c);
                    clonedShifts.Add(c);
                    globalIdMap[sh.Id] = c.Id;
                }
                foreach (var team in context.Teams.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (Team)team.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    oldnewteams.Add(team.Id, c);
                    clonedTeams.Add(c);
                    globalIdMap[team.Id] = c.Id;
                }
                foreach (var te in context.TypeEquipment.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (TypeEquipment)te.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    oldnewtypeequipments.Add(te.Id, c);
                    clonedTypeEquipments.Add(c);
                    globalIdMap[te.Id] = c.Id;
                }
                foreach (var vp in context.VehicleProfiles.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (VehicleProfile)vp.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    oldnewvehicleprofiles.Add(vp.Id, c);
                    clonedVehicleProfiles.Add(c);
                    globalIdMap[vp.Id] = c.Id;
                }
                foreach (var w in context.Widgets.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (Widget)w.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    clonedWidgets.Add(c);
                    globalIdMap[w.Id] = c.Id;
                }

                // Workers & AvailableWorkers
                var roleIds = oldnewrols.Keys.ToList();
                var teamIds = oldnewteams.Keys.ToList();

                var workers = context.Workers
                    .Where(x => roleIds.Contains(x.RolId) || teamIds.Contains(x.TeamId))
                    .ToList();

                foreach (var worker in workers)
                {
                    var cw = (Worker)worker.Clone();

                    if (oldnewrols.TryGetValue(worker.RolId, out var newRol))
                    {
                        cw.RolId = newRol.Id;
                        cw.Rol = newRol;
                    }
                    if (oldnewteams.TryGetValue(worker.TeamId, out var newTeam))
                    {
                        cw.TeamId = newTeam.Id;
                        cw.Team = newTeam;
                    }

                    clonedWorkers.Add(cw);
                    globalIdMap[worker.Id] = cw.Id;

                    var av = context.AvailableWorkers.FirstOrDefault(x => x.WorkerId == worker.Id);
                    if (av != null)
                    {
                        var cav = (AvailableWorker)av.Clone();
                        cav.WorkerId = cw.Id;
                        cav.Worker = cw;
                        clonedAvailableWorkers.Add(cav);
                        oldnewavailableworkers.Add(av.Id, cav);
                        globalIdMap[av.Id] = cav.Id;
                    }
                }

                // RolProcessSequence
                var rpsList = context.RolProcessSequences
                    .Where(x => oldnewprocess.Keys.Contains(x.ProcessId) || oldnewrols.Keys.Contains(x.RolId))
                    .ToList();
                foreach (var rps in rpsList)
                {
                    var crps = (RolProcessSequence)rps.Clone();

                    if (oldnewrols.TryGetValue(rps.RolId, out var nr))
                    {
                        crps.RolId = nr.Id;
                        crps.Rol = nr;
                    }
                    if (oldnewprocess.TryGetValue(rps.ProcessId, out var np))
                    {
                        crps.ProcessId = np.Id;
                        crps.Process = np;
                    }

                    clonedRolProcessSequences.Add(crps);
                    globalIdMap[rps.Id] = crps.Id;
                }

                // Schedules
                var breakProfileIds = oldnewbreakprofiles.Keys.ToList();
                var shiftIds = oldnewshifts.Keys.ToList();
                var availIds = oldnewavailableworkers.Keys.ToList();

                var schedules = context.Schedules
                    .Where(x => breakProfileIds.Contains(x.BreakProfileId)
                             || shiftIds.Contains(x.ShiftId)
                             || availIds.Contains(x.AvailableWorkerId))
                    .ToList();

                foreach (var sch in schedules)
                {
                    var csch = (Schedule)sch.Clone();

                    if (oldnewbreakprofiles.TryGetValue(sch.BreakProfileId, out var nbp))
                    {
                        csch.BreakProfileId = nbp.Id;
                        csch.BreakProfile = nbp;
                    }
                    if (oldnewshifts.TryGetValue(sch.ShiftId, out var nsh))
                    {
                        csch.ShiftId = nsh.Id;
                        csch.Shift = nsh;
                    }
                    if (oldnewavailableworkers.TryGetValue(sch.AvailableWorkerId, out var nav))
                    {
                        csch.AvailableWorkerId = nav.Id;
                        csch.AvailableWorker = nav;
                    }

                    clonedSchedules.Add(csch);
                    globalIdMap[sch.Id] = csch.Id;
                }

                // OrderSchedule, OrderLoadRatio
                foreach (var os in context.OrderSchedules.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var cos = (OrderSchedule)os.Clone();
                    cos.WarehouseId = clonedWarehouse.Id;
                    cos.Warehouse = clonedWarehouse;

                    if (oldnewvehicleprofiles.TryGetValue(os.VehicleId, out var nveh))
                    {
                        cos.VehicleId = nveh.Id;
                        cos.Vehicle = nveh;
                    }
                    if (oldnewloadprofiles.TryGetValue(os.LoadId, out var nload))
                    {
                        cos.LoadId = nload.Id;
                        cos.Load = nload;
                    }

                    clonedOrderSchedules.Add(cos);
                    globalIdMap[os.Id] = cos.Id;
                }

                var olrList = context.OrderLoadRatios
                    .Where(x => oldnewloadprofiles.Keys.Contains(x.LoadId) || oldnewvehicleprofiles.Keys.Contains(x.VehicleId))
                    .ToList();
                foreach (var olr in olrList)
                {
                    var colr = (OrderLoadRatio)olr.Clone();

                    if (oldnewvehicleprofiles.TryGetValue(olr.VehicleId, out var nveh))
                    {
                        colr.VehicleId = nveh.Id;
                        colr.Vehicle = nveh;
                    }
                    if (oldnewloadprofiles.TryGetValue(olr.LoadId, out var nload))
                    {
                        colr.LoadId = nload.Id;
                        colr.Load = nload;
                    }

                    clonedOrderLoadRatios.Add(colr);
                    globalIdMap[olr.Id] = colr.Id;
                }

                // ProcessDirectionProperty
                var pdps = context.ProcessDirectionProperties
                    .Where(x => oldnewprocess.Keys.Contains(x.InitProcessId) || oldnewprocess.Keys.Contains(x.EndProcessId))
                    .ToList();

                foreach (var pdp in pdps)
                {
                    var c = (ProcessDirectionProperty)pdp.Clone();

                    if (oldnewprocess.TryGetValue(pdp.InitProcessId, out var nip))
                    {
                        c.InitProcessId = nip.Id;
                        c.InitProcess = nip;
                    }
                    if (oldnewprocess.TryGetValue(pdp.EndProcessId, out var nep))
                    {
                        c.EndProcessId = nep.Id;
                        c.EndProcess = nep;
                    }

                    c.ViewPort = ViewPortIdRemapper.Remap(pdp.ViewPort, c.Id, globalIdMap);

                    clonedProcessDirectionProperties.Add(c);
                    globalIdMap[pdp.Id] = c.Id;
                }

                // Routes (Departure/Arrival Area) + ViewPort
                var routes = context.Routes
                    .Where(x => oldnewareas.Keys.Contains(x.DepartureAreaId) || oldnewareas.Keys.Contains(x.ArrivalAreaId))
                    .ToList();

                foreach (var r in routes)
                {
                    var cr = (Route)r.Clone();

                    if (oldnewareas.TryGetValue(r.DepartureAreaId, out var da))
                    {
                        cr.DepartureAreaId = da.Id;
                        cr.DepartureArea = da;
                    }
                    if (oldnewareas.TryGetValue(r.ArrivalAreaId, out var aa))
                    {
                        cr.ArrivalAreaId = aa.Id;
                        cr.ArrivalArea = aa;
                    }

                    cr.ViewPort = ViewPortIdRemapper.Remap(r.ViewPort, cr.Id, globalIdMap);

                    clonedRoutes.Add(cr);
                    globalIdMap[r.Id] = cr.Id;
                }

                // EquipmentGroups (ojo: mapear TypeEquipmentId con oldnewtypeequipments)
                var eqGroups = context.EquipmentGroups
                    .Where(x => oldnewareas.Keys.Contains(x.AreaId) || oldnewtypeequipments.Keys.Contains(x.TypeEquipmentId))
                    .ToList();
                foreach (var eg in eqGroups)
                {
                    var ceg = (EquipmentGroup)eg.Clone();

                    if (oldnewareas.TryGetValue(eg.AreaId, out var na))
                    {
                        ceg.AreaId = na.Id;
                        ceg.Area = na;
                    }
                    if (oldnewtypeequipments.TryGetValue(eg.TypeEquipmentId, out var nte))
                    {
                        ceg.TypeEquipmentId = nte.Id;
                        ceg.TypeEquipment = nte;
                    }

                    ceg.ViewPort = ViewPortIdRemapper.Remap(eg.ViewPort, ceg.Id, globalIdMap);

                    clonedEquipmentGroups.Add(ceg);
                    globalIdMap[eg.Id] = ceg.Id;
                }

                // Usuarios: relacionar warehouse clonado 
                var usersToLink = context.Users
                    .Where(u => u.Warehouses.Any(w => w.Id == layout.WarehouseId))
                    .ToList();

                foreach (var user in usersToLink)
                {
                    context.Entry(user).Collection(u => u.Warehouses).Load();
                    user.Warehouses ??= new List<Warehouse>();

                    if (!user.Warehouses.Any(w => w.Id == clonedWarehouse.Id))
                        user.Warehouses.Add(clonedWarehouse);
                }

                // ProcessPriorityOrder / SLAConfigs / OrderPriority
                foreach (var ppo in context.ProcessPriorityOrder.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (ProcessPriorityOrder)ppo.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    clonedProcessPriorityOrder.Add(c);
                    globalIdMap[ppo.Id] = c.Id;
                }
                foreach (var sla in context.SLAConfigs.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (SLAConfig)sla.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    clonedSLAConfig.Add(c);
                    globalIdMap[sla.Id] = c.Id;
                }
                foreach (var op in context.OrderPriority.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var c = (OrderPriority)op.Clone();
                    c.WarehouseId = clonedWarehouse.Id;
                    c.Warehouse = clonedWarehouse;
                    clonedOrderPriorities.Add(c);
                    globalIdMap[op.Id] = c.Id;
                }

                // Strategy and strategy sequence
                foreach (var s in context.Strategies.Where(x => x.WarehouseId == layout.WarehouseId))
                {
                    var cs = (Strategy)s.Clone();
                    cs.WarehouseId = clonedWarehouse.Id;
                    cs.Warehouse = clonedWarehouse;
                    clonedStrategies.Add(cs);
                    globalIdMap[s.Id] = cs.Id;
                }
                context.Strategies.AddRange(clonedStrategies);
                var strategySequences = context.StrategySequences.Where(x => x.Strategy.WarehouseId == layout.WarehouseId).ToList();
                foreach (var ss in strategySequences)
                {
                    var css = (StrategySequence)ss.Clone();
                    css.StrategyId = globalIdMap[ss.StrategyId];
                    css.Strategy = context.Strategies.FirstOrDefault(x => x.Id == globalIdMap[ss.StrategyId])!;
                    clonedStrategySequences.Add(css);
                    globalIdMap[ss.Id] = css.Id;
                }
                context.StrategySequences.AddRange(clonedStrategySequences);

                // Organización
                var org = context.Organizations
                    .Include(o => o.Warehouses)
                    .FirstOrDefault(o => o.Warehouses.Any(w => w.Id == layout.WarehouseId));

                if (org != null)
                {
                    org.Warehouses ??= new List<Warehouse>();
                    if (!org.Warehouses.Any(w => w.Id == clonedWarehouse.Id))
                        org.Warehouses.Add(clonedWarehouse);
                }


                context.Layouts.Add(clonedLayout);
                context.Warehouses.Add(clonedLayout.Warehouse);
                context.Flow.AddRange(clonedFlow);

                context.Aisles.AddRange(clonedAisles);
                context.Areas.AddRange(clonedAreas);
                context.AvailableWorkers.AddRange(clonedAvailableWorkers);
                context.Breaks.AddRange(clonedBreaks);
                context.BreakProfiles.AddRange(clonedBreakProfiles);
                context.Buffers.AddRange(clonedBuffers);
                context.CustomFlowGraphs.AddRange(clonedCustomFlowGraph);
                context.CustomProcesses.AddRange(clonedCustomProcesses);
                context.DockSelectionStrategies.AddRange(clonedDockSelectionStrategies);
                context.EquipmentGroups.AddRange(clonedEquipmentGroups);
                context.Inbounds.AddRange(clonedInbounds);
                context.InboundFlowGraphs.AddRange(clonedInboundFlowGraphs);
                context.Loadings.AddRange(clonedLoadings);
                context.LoadProfiles.AddRange(clonedLoadProfiles);
                context.Objects.AddRange(clonedObjects);
                context.OrderSchedules.AddRange(clonedOrderSchedules);
                context.OrderLoadRatios.AddRange(clonedOrderLoadRatios);
                context.OutboundFlowGraphs.AddRange(clonedOutboundFlowGraphs);
                context.Processes.AddRange(clonedProcesses);
                context.ProcessHours.AddRange(clonedProcessHours);
                context.Pickings.AddRange(clonedPickings);
                context.PostprocessProfiles.AddRange(clonedPostprocessProfiles);
                context.ProcessDirectionProperties.AddRange(clonedProcessDirectionProperties);
                context.PreprocessProfiles.AddRange(clonedPreprocessProfiles);
                context.PutawayProfiles.AddRange(clonedPutawayProfiles);
                context.Putaways.AddRange(clonedPutaways);
                context.Racks.AddRange(clonedRacks);
                context.Replenishments.AddRange(clonedReplenishments);
                context.Receptions.AddRange(clonedReceptions);
                context.Roles.AddRange(clonedRols);
                context.RolProcessSequences.AddRange(clonedRolProcessSequences);
                context.Routes.AddRange(clonedRoutes);
                context.Schedules.AddRange(clonedSchedules);
                context.Shifts.AddRange(clonedShifts);
                context.Shippings.AddRange(clonedShippings);
                context.Zones.AddRange(clonedZones);
                context.Steps.AddRange(clonedSteps);
                context.Teams.AddRange(clonedTeams);
                context.TypeEquipment.AddRange(clonedTypeEquipments);
                context.VehicleProfiles.AddRange(clonedVehicleProfiles);
                context.Widgets.AddRange(clonedWidgets);
                context.Workers.AddRange(clonedWorkers);
                context.ProcessPriorityOrder.AddRange(clonedProcessPriorityOrder);
                context.SLAConfigs.AddRange(clonedSLAConfig);
                context.OrderPriority.AddRange(clonedOrderPriorities);
                context.AutomaticStorages.AddRange(clonedAutomaticStorages);
                context.ChaoticStorages.AddRange(clonedChaoticStorages);
                context.DriveIns.AddRange(clonedDriveIns);
                context.AvailableDocksPerStages.AddRange(clonedAvailableDocksPerStage);

                context.SaveChanges();

                return clonedLayout.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void BlockOrder(List<WorkOrderBlock> workOrderBlockList)
        {
            if (workOrderBlockList == null || workOrderBlockList.Count == 0)
            {
                Console.WriteLine("[BlockOrder] Empty request");
                return;
            }

            string logError = string.Empty;

            var workOrderIds = workOrderBlockList
                .Select(x => x.Id)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var workOrders = context.WorkOrdersPlanning
                .Where(w => workOrderIds.Contains(w.Id))
                .ToList();

            var inputOrderIds = workOrders
                .Where(w => w.InputOrderId.HasValue)
                .Select(w => w.InputOrderId!.Value)
                .Distinct()
                .ToList();

            var inputOrders = context.InputOrders
                .Where(io => inputOrderIds.Contains(io.Id))
                .ToList();

            foreach (var req in workOrderBlockList)
            {
                try
                {
                    var workOrder = workOrders.FirstOrDefault(w => w.Id == req.Id);
                    if (workOrder == null)
                        throw new Exception("WorkOrder not found");

                    if (!workOrder.InputOrderId.HasValue)
                        throw new Exception("WorkOrder has no InputOrderId");

                    var inputOrder = inputOrders
                        .FirstOrDefault(io => io.Id == workOrder.InputOrderId.Value);

                    if (inputOrder == null)
                        throw new Exception("InputOrder not found");

                    if (!req.IsBlocked.HasValue)
                        throw new Exception("IsBlocked missing");

                    inputOrder.IsBlocked = req.IsBlocked.Value;

                    if (req.IsBlocked.Value)
                    {
                        if (!req.BlockDate.HasValue || !req.Duration.HasValue)
                            throw new Exception("BlockDate or Duration missing");

                        inputOrder.BlockDate = req.BlockDate.Value;
                        inputOrder.EndBlockDate = req.BlockDate.Value.AddSeconds(req.Duration.Value);
                    }
                    else
                    {
                        inputOrder.BlockDate = null;
                        inputOrder.EndBlockDate = null;
                    }
                }
                catch (Exception ex)
                {
                    var textError = $"[BlockOrder] WorkOrder {req.Id}: {ex.Message}";
                    Console.WriteLine(textError);
                    logError += textError;
                }
            }

            if (context.SaveChanges() == 0)
                throw new Exception(logError ?? "[BlockOrder] There are no changes to save");
        }

        public void ChangePriority(ChangePriorityDto changePriorityDto, Guid warehouseId)
        {
            if (context.OrderPriority.Where(x => x.WarehouseId == warehouseId).Select(x => x.Code).Contains(changePriorityDto.Priority))
            {

                var inputOrderIds = context.WorkOrdersPlanning
                    .Where(wp => changePriorityDto.WorkOrderId.Contains(wp.Id))
                    .Select(wp => wp.InputOrderId);

                context.InputOrders
                    .Where(io =>
                        inputOrderIds.Contains(io.Id) &&
                        io.WarehouseId == warehouseId)
                    .ExecuteUpdate(y => y
                        .SetProperty(x => x.Priority, changePriorityDto.Priority)
                        .SetProperty(x => x.UpdateDate, DateTime.UtcNow));
            }
            else
            {
                var values = string.Join(", ", context.OrderPriority.Where(x => x.WarehouseId == warehouseId).Select(x => x.Code).ToList());
                throw new ArgumentException($"The specified priority does not match the warehouse criteria: {changePriorityDto.Priority} is not contained in {values}");
            }
        }

        public int CancelOrders(List<Guid> workOrderIds)
        {
            // Select the orders-to-cancel IDs
            var workOrdersToCancell = context.WorkOrdersPlanning
                .Where(order => workOrderIds.Contains(order.Id))
                .Select(order => order.InputOrderId);

            // Updating input orders with "Cancelled" on the Waiting-Status Orders 
            int cancelledOrdersCount = context.InputOrders
                                        .Where(order => workOrdersToCancell.Contains(order.Id)) // Se presupone que todas las órdenes que llegan tienen estado Waiting --> NO se pone order.Status == "Waiting"
                                        .ExecuteUpdate(order => order
                                            .SetProperty(o => o.Status, InputOrderStatus.Cancelled)
                                            .SetProperty(o => o.UpdateDate, DateTime.UtcNow));
            context.SaveChanges();
            return cancelledOrdersCount;
        }

        public Guid getLastPlanning(Guid warehouseId)
        {
            var lastPlanning = context.Plannings
                .AsNoTracking()
                .Where(p => p.WarehouseId == warehouseId)
                .OrderByDescending(p => p.CreationDate)
                .Select(p => new { p.Id, p.CreationDate })
                .FirstOrDefault();

            return (lastPlanning?.Id != null) ? lastPlanning.Id : Guid.Empty;
        }

        public DateTime? GetLastUpdateDateInputOrderProcessesClosing(string WarehouseCode)
        {
            if (context.Warehouses.Any(m => m.Code == WarehouseCode))
            {
                var processes = context.InputOrders
                    .Where(m => m.Warehouse.Code == WarehouseCode)
                    .Join(context.InputOrderProcessesClosing, io => io.OrderCode, ip => ip.InputOrder, (io, ip) => new
                    {
                        Enddate = ip.EndDate
                    })
                    .AsNoTracking();

                if (processes.Any())
                {
                    DateTime? lastUpdateDate = processes.Max(m => m.Enddate);


                    // Return de max wms update date of the inputorderprocessesclosing, if everyone has null wms update date, return minus three days
                    return lastUpdateDate.HasValue ? DateTime.SpecifyKind(lastUpdateDate.Value, DateTimeKind.Unspecified)
                        : DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-3), DateTimeKind.Unspecified); ;
                }

                else
                {
                    // If that warehouse don't have any inputorderprocessesclosing information yet, we will use what's been updated/created in the last three days.
                    return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-3), DateTimeKind.Unspecified);
                }
            }
            else
            {
                //If the warehouse does not exist, we will return null
                return null;
            }
        }

        public DateTime? GetLastUpdateDateWarehouseProcesses(string WarehouseCode)
        {
            if (!context.Warehouses.Any(x => x.Code == WarehouseCode))
                return null;

            return DateTime.SpecifyKind(context.WarehouseProcessClosing.Where(m => m.Warehouse.Code == WarehouseCode).AsNoTracking().Select(x => (DateTime?)x.EndDate).Max() ?? DateTime.UtcNow.Date.AddDays(-3), DateTimeKind.Unspecified);
        }

        public async Task UpdateWarehouseProcessesClosingAsync(List<WarehouseProcessClosingCommunicaation> processes, string? user, string? sender)
        {
            var inputOrderProcessesContent = await JsonContent.Create(processes, mediaType: null).ReadAsStringAsync();

            var warehouse = processes.Select(x => x.WarehouseCode).FirstOrDefault();

            var warehouseId = context.Warehouses.Where(w => w.Code == warehouse).Select(w => w.Id).FirstOrDefault();

            try
            {
                foreach (var process in processes)
                {

                    if (context.WarehouseProcessClosing.Any(x => x.ProcessType == process.ProcessType && x.NotificationId == process.NotificationId))
                        continue;

                        context.Add(new WarehouseProcessClosing
                        {
                            Id = Guid.NewGuid(),
                            WarehouseId = warehouseId,
                            NotificationId = process.NotificationId,
                            ProcessType = process.ProcessType,
                            Worker = process.Worker,
                            EquipmentGroup = process.EquipmentGroup,
                            InitDate = DateTime.SpecifyKind(process.InitDate, DateTimeKind.Utc),
                            EndDate = DateTime.SpecifyKind(process.EndDate, DateTimeKind.Utc),
                            NumProcesses = process.NumProcesses,
                            ZoneCode = process.Zone
                        });
                }

                context.TransactionLog.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.UtcNow,
                    WarehouseCode = warehouse,
                    Type = TransactionType.UpsertWarehouseProcesses,
                    Content = inputOrderProcessesContent,
                    Status = MessageStatus.Success,
                    Sender = sender ?? ConstStrings.UnknownSender,
                    Recipient = ConstStrings.WFM,
                    UserName = user ?? ConstStrings.UnknownUser
                });

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                try
                {
                    context.ChangeTracker.Clear();
                    context.TransactionLog.Add(new Transaction
                    {
                        Id = Guid.NewGuid(),
                        CreationDate = DateTime.UtcNow,
                        WarehouseCode = warehouse,
                        Type = TransactionType.UpsertWarehouseProcesses,
                        Content = inputOrderProcessesContent,
                        Status = MessageStatus.Failure,
                        FailureMessage = $"{TransactionErrorMessage.UpsertWarehouseProcesses(warehouse)} {ex}",
                        Sender = sender ?? ConstStrings.UnknownSender,
                        Recipient = ConstStrings.WFM,
                        UserName = user ?? ConstStrings.UnknownUser
                    });

                    context.SaveChanges();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Error trying to register an entry in TransactionLog while executing method WarehouseProcessesClosingAsync. Exception: {ex2}");
                    throw;
                }
            }
        }

        public List<ReleaseDateOrder> GetReleaseDateOutboundOrders(string warehouseCode)
        {
            List<ReleaseDateOrder> releaseDateOutboundOrders = new List<ReleaseDateOrder>();

            if (context.Warehouses.Where(m => m.Code == warehouseCode).Any())
            {
                var workOrders = context.WorkOrdersPlanning
                    .Where(m => 
                        !m.IsEstimated && 
                        m.Planning.Warehouse.Code == warehouseCode && 
                        m.PlanningId == context.Plannings.Where(m => m.Warehouse.Code == warehouseCode).OrderByDescending(m => m.CreationDate).Select(x => x.Id).FirstOrDefault() && 
                        m.Status == InputOrderStatus.Waiting).Include(m => m.InputOrder);

                foreach (var workOrder in workOrders)
                {
                    releaseDateOutboundOrders.Add(new ReleaseDateOrder { OrderCode = workOrder.InputOrder.OrderCode, ReleaseDate = workOrder.InitDate });
                }
            }

            return releaseDateOutboundOrders;
        }

        public DateTime? GetLastUpdateDateInputOrders(string WarehouseCode)
        {
            if (context.Warehouses.AsNoTracking().Any(m => m.Code == WarehouseCode))
            {
                var inputOrders = context.InputOrders.Where(m => m.Warehouse.Code == WarehouseCode).AsNoTracking();

                if (inputOrders.Any())
                {
                    DateTime? lastUpdateDate = inputOrders.Max(m => m.UpdateDateWMS);


                    // Return de max wms update date of the inputorders, if everyone has null wms update date, return minus three days
                    return lastUpdateDate.HasValue ? DateTime.SpecifyKind(lastUpdateDate.Value, DateTimeKind.Unspecified) 
                        : DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-3), DateTimeKind.Unspecified); ;
                }

                else
                {
                    // If that warehouse don't have any inputorder information yet, we will use what's been updated/created in the last three days.
                    return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-3), DateTimeKind.Unspecified);
                }
            }
            else
            {
                //If the warehouse does not exist, we will return null
                return null;
            }
        }

        public async Task UpdateInputOrderProcessesClosingAsync(List<InputOrderProcessesClosingCommunication> processes, string warehouse, string? user, string? sender)
        {
            var inputOrderProcessesContent = await JsonContent.Create(processes, mediaType: null).ReadAsStringAsync();
            try
            {
                foreach (var process in processes)
                {

                    if (context.InputOrderProcessesClosing.Any(x => x.ProcessType == process.ProcessType && x.NotificationId == process.NotificationId))
                        continue;

                    if (context.InputOrders.Any(x => x.OrderCode == process.InputOrder && x.Warehouse.Code == warehouse))
                    {
                        context.Add(new InputOrderProcessClosing
                        {
                            Id = Guid.NewGuid(),
                            NotificationId = process.NotificationId,
                            ProcessType = process.ProcessType,
                            Worker = process.Worker,
                            EquipmentGroup = process.EquipmentGroup,
                            InitDate = DateTime.SpecifyKind(process.InitDate, DateTimeKind.Utc),
                            EndDate = DateTime.SpecifyKind(process.EndDate, DateTimeKind.Utc),
                            InputOrder = process.InputOrder,
                            NumProcesses = process.NumProcesses,
                            ZoneCode = process.Zone
                        });
                    }

                    else
                    {
                        context.Add(new InputOrder
                        {
                            Id = Guid.NewGuid(),
                            OrderCode = process.InputOrder,
                            IsStarted = false,
                            Status = InputOrderStatus.Released,
                            Priority = "Normal",
                            IsOutbound = process.ProcessType == ProcessType.Picking || process.ProcessType == ProcessType.Shipping || process.ProcessType == ProcessType.Loading ? true : false,
                            AllowPartialClosed = false,
                            AllowGroup = false,
                            AppointmentDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Unspecified),
                            RealArrivalTime = null,
                            UpdateDate = null,
                            UpdateDateWMS = null,
                            IsEstimated = false,
                            AssignedDockId = null,
                            AssignedDock = null,
                            PreferredDockId = null,
                            PreferredDock = null,
                            WarehouseId = context.Warehouses.First(x => x.Code == warehouse).Id,
                            Warehouse = context.Warehouses.First(x => x.Code == warehouse),
                            IsBlocked = false,
                            BlockDate = null,
                            EndBlockDate = null,
                            Progress = 0,
                            CreationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                            ReleaseDate = null,
                            VehicleCode = null
                        });

                        context.Add(new InputOrderProcessClosing
                        {
                            Id = Guid.NewGuid(),
                            NotificationId = process.NotificationId,
                            ProcessType = process.ProcessType,
                            Worker = process.Worker,
                            EquipmentGroup = process.EquipmentGroup,
                            InitDate = DateTime.SpecifyKind(process.InitDate, DateTimeKind.Utc),
                            EndDate = DateTime.SpecifyKind(process.EndDate, DateTimeKind.Utc),
                            InputOrder = process.InputOrder,
                            NumProcesses = process.NumProcesses,
                            ZoneCode = process.Zone
                        });
                    }
                }

                context.TransactionLog.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.UtcNow,
                    WarehouseCode = warehouse,
                    Type = TransactionType.UpsertInputOrderProcesses,
                    Content = inputOrderProcessesContent,
                    Status = MessageStatus.Success,
                    Sender = sender ?? ConstStrings.UnknownSender,
                    Recipient = ConstStrings.WFM,
                    UserName = user ?? ConstStrings.UnknownUser
                });

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                try
                {
                    context.ChangeTracker.Clear();
                    context.TransactionLog.Add(new Transaction
                    {
                        Id = Guid.NewGuid(),
                        CreationDate = DateTime.UtcNow,
                        WarehouseCode = warehouse,
                        Type = TransactionType.UpsertInputOrderProcesses,
                        Content = inputOrderProcessesContent,
                        Status = MessageStatus.Failure,
                        FailureMessage = $"{TransactionErrorMessage.UpsertInputOrderProcesses(warehouse)} {ex}",
                        Sender = sender ?? ConstStrings.UnknownSender,
                        Recipient = ConstStrings.WFM,
                        UserName = user ?? ConstStrings.UnknownUser
                    });

                    context.SaveChanges();
                }
                catch (Exception ex2) 
                {
                    Console.WriteLine($"Error trying to register an entry in TransactionLog while executing method UpdateInputOrderProcessesClosingAsync. Exception: {ex2}");
                    throw;
                }
            }
        }

        public async Task<Guid> UpdateInputOrdersAsync(List<InputOrderCommunication> inputOrderCommunications, string? user, string? sender)
        {
            var inputOrderCommunicationsContent = await JsonContent.Create(inputOrderCommunications, mediaType: null).ReadAsStringAsync();
            try
            {
                foreach (var order in inputOrderCommunications)
                {
                    if (context.InputOrders.Any(x => x.OrderCode == order.Code && x.Warehouse.Code == order.WarehouseCode))
                    {

                        var row = context.InputOrders.First(x => x.OrderCode == order.Code && x.Warehouse.Code == order.WarehouseCode);

                        row.Status = order.Status;
                        row.Priority = order.Priority;
                        row.AppointmentDate = order.AppointmentDate == null ? DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Unspecified) : DateTime.SpecifyKind(order.AppointmentDate!.Value, DateTimeKind.Utc);
                        row.UpdateDateWMS = DateTime.SpecifyKind(order.UpdateDateWMS, DateTimeKind.Utc);
                        row.CreationDate = DateTime.SpecifyKind(order.CreationDate, DateTimeKind.Utc);
                        row.AssignedDockId = context.Docks.FirstOrDefault(x => x.Zone.Name == order.AssignedDockCode)?.Id;
                        row.AssignedDock = context.Docks.FirstOrDefault(x => x.Zone.Name == order.AssignedDockCode);
                        row.PreferredDockId = context.Docks.FirstOrDefault(x => x.Zone.Name == order.PreferredDockCode)?.Id;
                        row.PreferredDock = context.Docks.FirstOrDefault(x => x.Zone.Name == order.PreferredDockCode);
                        row.IsOutbound = order.IsOutbound;
                        row.WarehouseId = context.Warehouses.First(x => x.Code == order.WarehouseCode).Id;
                        row.Warehouse = context.Warehouses.First(x => x.Code == order.WarehouseCode);
                        row.Progress = order.Progress;
                        row.ReleaseDate = order.ReleaseDate == null ? order.ReleaseDate : DateTime.SpecifyKind(order.ReleaseDate!.Value, DateTimeKind.Utc);
                        row.VehicleCode = order.VehicleCode;

                        if (order.Status != InputOrderStatus.Cancelled && order.Status != InputOrderStatus.Paused)
                        {
                            var linesToDelete = context.InputOrderLines.Where(x => x.InputOutboundOrder.OrderCode == order.Code);

                            context.InputOrderLines.RemoveRange(linesToDelete);

                            for (int i = 0; i < order.NumLines; i++)
                            {
                                context.InputOrderLines.Add(new InputOrderLine
                                {
                                    Id = Guid.NewGuid(),
                                    Product = null,
                                    Quantity = null,
                                    UnitOfMeasure = null,
                                    IsClosed = false,
                                    InputOutboundOrderId = context.InputOrders.First(x => x.OrderCode == order.Code).Id,
                                    InputOutboundOrder = context.InputOrders.First(x => x.OrderCode == order.Code)
                                });

                            }
                        }
                    }
                    else
                    {
                        var inputOrder = new InputOrder
                        {
                            Id = Guid.NewGuid(),
                            OrderCode = order.Code,
                            IsStarted = false,
                            Status = order.Status,
                            Priority = order.Priority,
                            IsOutbound = order.IsOutbound,
                            AllowPartialClosed = false,
                            AllowGroup = false,
                            AppointmentDate = order.AppointmentDate == null ? DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Unspecified) : DateTime.SpecifyKind(order.AppointmentDate!.Value, DateTimeKind.Utc),
                            RealArrivalTime = null,
                            UpdateDate = null,
                            UpdateDateWMS = DateTime.SpecifyKind(order.UpdateDateWMS, DateTimeKind.Utc),
                            IsEstimated = false,
                            AssignedDockId = context.Docks.FirstOrDefault(x => x.Zone.Name == order.AssignedDockCode)?.Id,
                            AssignedDock = context.Docks.FirstOrDefault(x => x.Zone.Name == order.AssignedDockCode),
                            PreferredDockId = context.Docks.FirstOrDefault(x => x.Zone.Name == order.PreferredDockCode)?.Id,
                            PreferredDock = context.Docks.FirstOrDefault(x => x.Zone.Name == order.PreferredDockCode),
                            WarehouseId = context.Warehouses.First(x => x.Code == order.WarehouseCode).Id,
                            Warehouse = context.Warehouses.First(x => x.Code == order.WarehouseCode),
                            IsBlocked = false,
                            BlockDate = null,
                            EndBlockDate = null,
                            Progress = order.Progress,
                            CreationDate = DateTime.SpecifyKind(order.CreationDate, DateTimeKind.Utc),
                            ReleaseDate = order.ReleaseDate == null ? order.ReleaseDate : DateTime.SpecifyKind(order.ReleaseDate!.Value, DateTimeKind.Utc),
                            VehicleCode = order.VehicleCode
                        };

                        context.InputOrders.Add(inputOrder);

                        for (int i = 0; i < order.NumLines; i++)
                        {
                            context.InputOrderLines.Add(new InputOrderLine
                            {
                                Id = Guid.NewGuid(),
                                Product = null,
                                Quantity = null,
                                UnitOfMeasure = null,
                                IsClosed = false,
                                InputOutboundOrderId = inputOrder.Id,
                                InputOutboundOrder = inputOrder
                            });
                        }
                    }
                }

                context.TransactionLog.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.UtcNow,
                    WarehouseCode = inputOrderCommunications.Select(x => x.WarehouseCode).FirstOrDefault(),
                    Type = TransactionType.UpsertInputOrder,
                    Content = inputOrderCommunicationsContent,
                    Status = MessageStatus.Success,
                    Sender = sender ?? ConstStrings.UnknownSender,
                    Recipient = ConstStrings.WFM,
                    UserName = user ?? ConstStrings.UnknownUser
                });

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                try
                {
                    var warehouse = inputOrderCommunications.Select(x => x.WarehouseCode).FirstOrDefault();
                    context.ChangeTracker.Clear();
                    context.TransactionLog.Add(new Transaction
                    {
                        Id = Guid.NewGuid(),
                        CreationDate = DateTime.UtcNow,
                        WarehouseCode = warehouse,
                        Type = TransactionType.UpsertInputOrder,
                        Content = inputOrderCommunicationsContent,
                        Status = MessageStatus.Failure,
                        FailureMessage = $"{TransactionErrorMessage.UpsertInputOrder(warehouse)} {ex}",
                        Sender = sender ?? ConstStrings.UnknownSender,
                        Recipient = ConstStrings.WFM,
                        UserName = user ?? ConstStrings.UnknownUser
                    });

                    context.SaveChanges();
                    return Guid.Empty;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Error trying to register an entry in TransactionLog while executing method UpdateInputOrdersAsync. Exception: {ex2}");
                    throw;
                }
            }
            // TODO: revisar
            Guid warehouseId = context.Warehouses.First(x => x.Code == inputOrderCommunications.First().WarehouseCode).Id;
            return warehouseId;
        }

        public bool AddTransaction(Transaction transaction)
        {
            context.TransactionLog.Add(transaction);
            context.SaveChanges();

            return true;
        }

        public Warehouse? GetWarehouse(string warehouseCode)
        {
            var warehouse = context.Warehouses.FirstOrDefault(x => x.Code == warehouseCode);

            return warehouse;
        }
    }
}
