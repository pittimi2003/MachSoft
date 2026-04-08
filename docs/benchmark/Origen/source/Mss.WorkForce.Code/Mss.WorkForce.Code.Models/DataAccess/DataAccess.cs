using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Calendar;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.DTO.Designer.DesignerTabDto;
using Mss.WorkForce.Code.Models.DTO.DisgnerTabDto;
using Mss.WorkForce.Code.Models.DTO.Preview;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Newtonsoft.Json;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;
using Buffer = Mss.WorkForce.Code.Models.Models.Buffer;
using TimeZone = Mss.WorkForce.Code.Models.Models.TimeZone;
namespace Mss.WorkForce.Code.Models.DataAccess
{
    /// <summary>
    /// Clase de acceso a datos que interactúa con la base de datos mediante el uso de ApplicationDbContext.
    /// Proporciona métodos para obtener información sobre widgets, operadores, equipos, 
    /// y datos de carga de entrada y salida en función del sitio especificado.
    /// </summary>
    public class DataAccess
    {
        // Contexto de la base de datos para realizar consultas y operaciones.
        public ApplicationDbContext _context;

        /// <summary>
        /// Constructor de la clase DataAccess.
        /// Inicializa el contexto de base de datos.
        /// </summary>
        /// <param name="context">Instancia de ApplicationDbContext utilizada para acceder a los datos.</param>
        public DataAccess(ApplicationDbContext context)
        {
            _context = context;
        }

        public ApplicationDbContext getContext()
        {
            return _context;
        }

        /// <summary>
        /// Método para obtener una lista de widgets disponibles.
        /// </summary>
        /// <returns>Enumeración de strings que representa los widgets.</returns>
        public static List<string> GetWidgetsTitles()
        {
            List<string> Widgets = new List<string>();
            Widgets.Add(widgets.view1.Split("<Title Text=")[1].Split(@" />")[0]);
            Widgets.Add(widgets.view2.Split("<Title Text=")[1].Split(@" />")[0]);
            Widgets.Add(widgets.view3.Split("<Title Text=")[1].Split(@" />")[0]);
            Widgets.Add(widgets.view4.Split("<Title Text=")[1].Split(@" />")[0]);
            Widgets.Add(widgets.view5.Split("<Title Text=")[1].Split(@" />")[0]);
            Widgets.Add(widgets.view6.Split("<Title Text=")[1].Split(@" />")[0]);

            return Widgets;
        }

        public static List<string> GetWidgets()
        {
            List<string> Widgets = new List<string>();
            Widgets.Add(widgets.view1);
            //Widgets.Add(widgets.view2);
            //Widgets.Add(widgets.view3);
            //Widgets.Add(widgets.view4);
            //Widgets.Add(widgets.view5);
            //Widgets.Add(widgets.view6);
            return Widgets;
        }

        public static List<string> GetWidgetsWorkers()
        {
            return new List<string> { widgets.WFMLaborWorkerPerProcessType };
        }

        public static List<string> GetWidgetsTitlesworkers()
        {
            List<string> Widgets = new List<string>();
            Widgets.Add(widgets.WFMLaborWorkerPerProcessType.Split("<Title Text=")[1].Split(@" />")[0]);


            return Widgets;
        }

        /// <summary>
        /// Método para obtener una lista de metricas disponibles.
        /// </summary>
        /// <returns>Enumeración de strings que representa los widgets.</returns>
        public static List<string> GetMetricsTitles()
        {
            List<string> Metrics = new List<string>();
            Metrics.Add(PerformanceMetricsDashBoard.dashboard.Split("<Title Text=")[1].Split(@" />")[0]);

            return Metrics;
        }

        public static List<string> GetMetrics()
        {
            List<string> metrics = new List<string>();
            metrics.Add(PerformanceMetricsDashBoard.dashboard);
            //Widgets.Add(widgets.view2);
            //Widgets.Add(widgets.view3);
            //Widgets.Add(widgets.view4);
            //Widgets.Add(widgets.view5);
            //Widgets.Add(widgets.view6);
            return metrics;
        }

        /// <summary>
        /// Método para obtener la lista de operadores disponibles en un sitio específico.
        /// Filtra los operadores por el nombre del almacén asignado al equipo del operador.
        /// </summary>
        /// <param name="warehosueId">Guid del site (almacén) para el cual se obtienen los operadores.</param>
        /// <returns>Enumeración de objetos AvailableWorker que representan a los operadores disponibles.</returns>
        public IEnumerable<Schedule> GetOperators(Guid warehosueId)
        {
            return _context.Schedules
                           .Where(x => x.AvailableWorker.Worker.Team.Warehouse.Id == warehosueId && x.BreakProfile.WarehouseId == warehosueId)
                           .Include(p => p.BreakProfile)
                           .Include(p => p.AvailableWorker).ThenInclude(q => q.Worker).ThenInclude(t => t.Team)
                           .Include(p => p.AvailableWorker).ThenInclude(q => q.Worker).ThenInclude(t => t.Rol)
                           .Include(p => p.Shift)
                           .AsNoTracking()
                           .ToList();
        }



        /// <summary>
        /// Método para obtener la lista de grupos de equipos en un sitio específico.
        /// Filtra los equipos por el id del almacén asociado al tipo de equipo.
        /// </summary>
        /// <param name="warehouseId">Guid del site (almacén) para el cual se obtienen los equipos.</param>
        /// <returns>Enumeración de objetos EquipmentGroup que representan los grupos de equipos disponibles.</returns>
        public List<EquipmentGroup> GetEquipments(Guid warehouseId)
        {
            List<EquipmentGroup> EquipmentGroups = _context.EquipmentGroups
                           .Where(x => x.TypeEquipment.Warehouse.Id == warehouseId)
                           .Include(p => p.Area)
                           .Include(p => p.TypeEquipment).ToList();

            var site = _context.Warehouses.FirstOrDefault(x => x.Id == warehouseId)?.Name ?? string.Empty;

            foreach (var configurationSequence in _context.ConfigurationSequences.Where(m => m.Date == DateTime.UtcNow.Date && m.ConfigurationSequenceHeader.Warehouse.Name == site).AsEnumerable().OrderBy(m => m.Sequence))
            {
                var Json = System.Text.Json.JsonSerializer.Deserialize<Get>(configurationSequence.ConfigurationSequenceHeader.Data);

                //NEW
                foreach (var item in Json.New.EquipmentGroup.Join(_context.TypeEquipment, e => e.TypeEquipmentId, t => t.Id, (e, t) => new { Id = e.Id, Name = e.Name, Equipments = e.Equipments, TypeEquipmentId = e.TypeEquipmentId, AreaId = e.AreaId, Warehouse = t.Warehouse.Name }).Where(m => m.Warehouse == site))
                {
                    EquipmentGroups.Add(new EquipmentGroup
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Equipments = item.Equipments.Value,
                        TypeEquipmentId = item.TypeEquipmentId.Value,
                        TypeEquipment = _context.TypeEquipment.FirstOrDefault(x => x.Id == item.TypeEquipmentId),
                        AreaId = item.AreaId.Value,
                        Area = _context.Areas.FirstOrDefault(x => x.Id == item.AreaId),
                    });
                }

                //UPDATE
                foreach (var item in Json.Update.EquipmentGroup)
                {
                    if (EquipmentGroups.Select(m => m.Id).Contains(item.Id))
                    {
                        var TypeEquipmentId = item.TypeEquipmentId == null ? EquipmentGroups.FirstOrDefault(m => m.Id == item.Id).TypeEquipmentId : item.TypeEquipmentId.Value;
                        var AreaId = item.AreaId == null ? EquipmentGroups.FirstOrDefault(m => m.Id == item.Id).AreaId : item.AreaId.Value;

                        var EquipmentGroupUpdate = EquipmentGroups.Where(m => m.Id == item.Id).Select(m => new EquipmentGroup
                        {
                            Id = item.Id,
                            Name = item.Name == null ? EquipmentGroups.FirstOrDefault(m => m.Id == item.Id).Name : item.Name,
                            Equipments = item.Equipments == null ? EquipmentGroups.FirstOrDefault(m => m.Id == item.Id).Equipments : item.Equipments.Value,
                            TypeEquipmentId = TypeEquipmentId,
                            TypeEquipment = _context.TypeEquipment.FirstOrDefault(m => m.Id == TypeEquipmentId),
                            AreaId = AreaId,
                            Area = _context.Areas.FirstOrDefault(m => m.Id == AreaId)
                        }).First();

                        EquipmentGroups.Remove(EquipmentGroups.FirstOrDefault(m => m.Id == item.Id));
                        EquipmentGroups.Add(EquipmentGroupUpdate);
                    }
                }

                //DELETE
                foreach (var item in Json.Delete.EquipmentGroup)
                {
                    if (EquipmentGroups.Select(m => m.Id).Contains(item.Id))
                    {
                        EquipmentGroups.Remove(EquipmentGroups.FirstOrDefault(m => m.Id == item.Id));
                    }
                }

            }

            return EquipmentGroups;
        }

        /// <summary>
        /// Método para obtener la lista de cargas de entrada en un sitio específico.
        /// </summary>
        /// <param name="site">Nombre del site para el cual se obtienen las cargas de entrada.</param>
        /// <returns>Diccionario cuya clave es el nombre de la carga y cuyo valor son datos referentes a la carga de entrada.</returns>
        public List<OrderSchedule> GetInboundLoad(Guid site, Guid? configId)
        {
            return GetLoad(site, false, configId);
        }

        /// <summary>
        /// Método para obtener la lista de cargas de salida en un sitio específico.
        /// </summary>
        /// <param name="site">Nombre del site para el cual se obtienen las cargas de salida.</param>
        /// <returns>Diccionario cuya clave es el nombre de la carga y cuyo valor son datos referentes a la carga de salida.</returns>
        public List<OrderSchedule> GetOutboundLoad(Guid site, Guid? configId)
        {
            return GetLoad(site, true, configId);
        }

        /// <summary>
        /// Método para obtener la lista de cargas de entrada o de salida en un sitio específico.
        /// </summary>
        /// <param name="site">Nombre del site para el cual se obtienen las cargas de salida.</param>
        /// /// <param name="AllowInInboundFlow">Booleano que indica si las cargas son de entrada.</param>
        /// /// <param name="AllowInOutboundFlow">Booleano que indica si las cargas son de salida.</param>
        /// <returns>Diccionario cuya clave es el nombre de la carga y cuyo valor son datos referentes a la carga de entrada o de salida.</returns>
        public List<OrderSchedule> GetLoad(Guid site, bool IsOut, Guid? configId)
        {
            List<OrderSchedule> OrderSchedules = _context
                .OrderSchedules
                .Where(m => m.IsOut == IsOut && m.WarehouseId == site)
                .Include(m => m.Load)
                .Include(m => m.Vehicle)
                .AsNoTracking()
                .ToList();
            if (configId != Guid.Empty)
            {
                foreach (var configurationSequence in _context.ConfigurationSequences.Include(cs => cs.ConfigurationSequenceHeader).Where(m => m.ConfigurationSequenceHeaderId == configId).AsEnumerable().OrderBy(m => m.Sequence))
                {
                    var Json = System.Text.Json.JsonSerializer.Deserialize<Get>(configurationSequence.ConfigurationSequenceHeader.Data);

                    //NEW
                    foreach (var item in Json.New.OrderSchedule.Where(m => m.IsOut == IsOut && m.WarehouseId == site))
                    {
                        OrderSchedules.Add(new OrderSchedule
                        {
                            Id = item.Id,
                            InitHour = item.InitHour.Value,
                            EndHour = item.EndHour.Value,
                            VehicleId = item.VehicleId.Value,
                            Vehicle = _context.VehicleProfiles.FirstOrDefault(x => x.Id == item.VehicleId),
                            NumberVehicles = item.NumberVehicles.Value,
                            LoadId = item.LoadId.Value,
                            Load = _context.LoadProfiles.FirstOrDefault(x => x.Id == item.LoadId),
                            IsOut = item.IsOut.Value,
                            WarehouseId = item.WarehouseId.Value,
                            Warehouse = _context.Warehouses.FirstOrDefault(x => x.Id == item.WarehouseId)
                        });

                    }

                    //UPDATE
                    foreach (var item in Json.Update.OrderSchedule)
                    {
                        if (OrderSchedules.Select(m => m.Id).Contains(item.Id))
                        {
                            var a = OrderSchedules.Where(m => m.Id == item.Id).Select(m => m.InitHour);
                            var OrderScheduleUpdate = OrderSchedules.Where(m => m.Id == item.Id).Select(m => new OrderSchedule
                            {
                                Id = item.Id,
                                InitHour = item.InitHour == null ? new TimeSpan() : item.InitHour.Value,
                                EndHour = item.EndHour == null ? new TimeSpan() : item.EndHour.Value,
                                VehicleId = item.VehicleId == null ? OrderSchedules.FirstOrDefault(m => m.Id == item.Id).VehicleId : item.VehicleId.Value,
                                Vehicle = item.VehicleId == null ? OrderSchedules.FirstOrDefault(m => m.Id == item.Id).Vehicle : _context.VehicleProfiles.FirstOrDefault(m => m.Id == item.VehicleId),
                                NumberVehicles = item.NumberVehicles == null ? 0 : item.NumberVehicles.Value,
                                LoadId = item.LoadId == null ? OrderSchedules.FirstOrDefault(m => m.Id == item.Id).LoadId : item.LoadId.Value,
                                Load = item.LoadId == null ? OrderSchedules.FirstOrDefault(m => m.Id == item.Id).Load : _context.LoadProfiles.FirstOrDefault(m => m.Id == item.LoadId),
                                IsOut = item.IsOut == null ? OrderSchedules.FirstOrDefault(m => m.Id == item.Id).IsOut : item.IsOut.Value,
                                WarehouseId = item.WarehouseId == null ? OrderSchedules.FirstOrDefault(m => m.Id == item.Id).WarehouseId : item.WarehouseId.Value,
                                Warehouse = item.WarehouseId == null ? OrderSchedules.FirstOrDefault(m => m.Id == item.Id).Warehouse : _context.Warehouses.FirstOrDefault(m => m.Id == item.WarehouseId)
                            }).First();

                            OrderSchedules.Remove(OrderSchedules.FirstOrDefault(m => m.Id == item.Id));
                            OrderSchedules.Add(OrderScheduleUpdate);
                        }
                    }

                    //DELETE
                    foreach (var item in Json.Delete.OrderSchedule)
                    {
                        if (OrderSchedules.Select(m => m.Id).Contains(item.Id))
                        {
                            OrderSchedules.Remove(OrderSchedules.FirstOrDefault(m => m.Id == item.Id));
                        }
                    }

                }
            }
            return OrderSchedules;
        }

        /// <summary>
        /// Método para obtener la organización.
        /// </summary>
        /// <returns>Objeto Organization con lista de objetos que representan sus correspondientes warehouses.</returns>
        public IEnumerable<Organization> GetOrganization()
        {
            return _context.Organizations.Include(m => m.Warehouses);
        }

        /// <summary>
        /// Método para obtener los almacenes a partir de una organización.
        /// </summary>
        /// /// <param name="OrganizationId">Identificador de la organización.</param>
        /// <returns>IEnumerable de la lista de almacenes que corresponden a una organización concreta.</returns>
        public IEnumerable<Warehouse> GetWarehouse(Guid OrganizationId)
        {
            return _context.Warehouses.Include(m => m.Organization).Include(m => m.Users).Include(m => m.TimeZone_).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener el almacén a partir de un PlanningIs.
        /// </summary>
        /// /// <param name="PlanningId">Identificador del planning.</param>
        /// <returns>Guid WarehouseId del PlanningId correspondiente.</returns>
        public Guid GetWarehouseIdByPlanningId(Guid? PlanningId)
        {
            if (PlanningId != null)
                return _context.Plannings.Where(m => m.Id == PlanningId).FirstOrDefault()!.WarehouseId;
            else return Guid.Empty;
        }

        /// <summary>
        /// Método para obtener los usuarios a partir de una organización.
        /// </summary>
        /// /// <param name="OrganizationId">Identificador de la organización.</param>
        /// <returns>IEnumerable de la lista de usuarios que corresponden a una organización concreta.</returns>
        public IEnumerable<User> GetUser(Guid OrganizationId)
        {
            return _context.Users
                .Include(m => m.Organization)
                .Include(m => m.DecimalSeparator)
                .Include(m => m.ThousandsSeparator)
                .Include(m => m.DateFormat)
                .Include(m => m.HourFormat)
                .Include(m => m.Language)
                .Include(m => m.Warehouses)
                     .ThenInclude(w => w.TimeZone_).AsNoTracking(); ;
        }

        /// <summary>
        /// Método para obtener la lista de procesos a partir de las órdenes planificadas.
        /// </summary>
        /// <param name="WorkOrderPlanningReturn">Órdenes planificadas para las cuales se obtienen los procesos asociados.</param>
        /// <returns>Diccionario con clave de las órdenes planificadas y lista de objetos que representa los procesos asociados.</returns>
        public Dictionary<Guid, List<ItemPlanning>> GetProcesses(List<Guid> WorkOrdersPlanningId)
        {
            Dictionary<Guid, List<ItemPlanning>> ItemsPlanning = new Dictionary<Guid, List<ItemPlanning>>();

            foreach (var WorkOrderPlanningId in WorkOrdersPlanningId)
            {
                ItemsPlanning.Add(WorkOrderPlanningId, _context.ItemsPlanning.Where(m => m.WorkOrderPlanningId == WorkOrderPlanningId).Select(m => m).ToList());
            }

            return ItemsPlanning;
        }
        /// <summary>
        /// Método para obtener la lista de órdenes y procesos a partir de la planificación.
        /// </summary>
        /// <param name="PlanningId">Identificador de la planificación para las cuales se obtienen las órdenes y los procesos asociados.</param>
        /// <returns>Objeto PlanningReturn con listas de objetos que representa las órdenes y los procesos asociados.</returns>
        public IEnumerable<ItemPlanning> GetPlannings(Guid PlanningId)
        {
            //GEt all Items planning for planning 
            var itemPlannings = _context.ItemsPlanning
                .Include(x => x.Process)
                .Include(x => x.EquipmentGroup).ThenInclude(x => x.TypeEquipment)
                .Include(x => x.Worker)
                .Include(x => x.Shift)
                .Include(x => x.WorkOrderPlanning).ThenInclude(x => x.InputOrder)
                .Include(x => x.WorkOrderPlanning).ThenInclude(x => x.Planning)
                .Include(x => x.WorkOrderPlanning).ThenInclude(x => x.AssignedDock).ThenInclude(s => s.Zone)
                .Where(x => x.WorkOrderPlanning.PlanningId == PlanningId).Select(m => m)
                .OrderBy(x => x.WorkOrderPlanning.InitDate).AsNoTracking();

            return itemPlannings;
        }

        /// <summary>
        /// Método para obtener la lista de órdenes a partir de la planificación.
        /// </summary>
        /// <param name="PlanningId">Identificador de la planificación para las cuales se obtienen las órdenes.</param>
        /// <returns>Objeto WorkOrderPlanning con listas de objetos que representa las órdenes asociados.</returns>
        public IEnumerable<WorkOrderPlanning> GetOrdersPlanning(Guid PlanningId)
        {
            return _context.WorkOrdersPlanning.Where(x => x.PlanningId == PlanningId).Include(x => x.InputOrder).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de procesos de almacen a partir de la planificación.
        /// </summary>
        /// <param name="PlanningId">Identificador de la planificación para las cuales se obtienen las órdenes y los procesos asociados.</param>
        /// <returns>Objeto PlanningReturn con listas de objetos que representa las órdenes y los procesos asociados.</returns>
        public IEnumerable<WarehouseProcessPlanning> GetWarehouseProcessPlannings(Guid PlanningId)
        {
            var Plannings = _context.WarehouseProcessPlanning
                .Include(x => x.Process)
                .Include(x => x.Worker)
                .Where(x => x.PlanningId == PlanningId).Select(m => m)
                .OrderBy(x => x.InitDate).AsNoTracking();

            return Plannings;
        }

        /// <summary>
        /// Método para obtener la lista de tipos de equipamientos 
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de tipos de equipamientos</returns>
        public IEnumerable<TypeEquipment> GetEquipmentTypes(Guid warehosueId)
        {
            return _context.TypeEquipment.Where(x => x.WarehouseId == warehosueId).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de tipos de equipamientos 
        /// </summary>
        /// <returns>IEnumerable de la lista de tipos de equipamientos</returns>
        public IEnumerable<TypeEquipment> GetEquipmentTypesNoFiltered()
        {
            return _context.TypeEquipment.AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de los grupos equipamientos 
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de tipos de grupos de equipamientos</returns>
        public IEnumerable<EquipmentGroup> GetEquipmentGroupsWithType(Guid warehosueId)
        {
            return _context.EquipmentGroups
                   .Include(e => e.TypeEquipment)
                   .Include(e => e.Area)
                   .Where(e => e.TypeEquipment.WarehouseId == warehosueId)
                   .AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de los grupos equipamientos 
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de tipos de grupos de equipamientos</returns>
        public IEnumerable<EquipmentGroup> GetEquipmentGroups()
        {
            return _context.EquipmentGroups.AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de areas
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de areas</returns>
        public IEnumerable<Area> GetAreas()
        {
            return _context.Areas.AsNoTracking();
        }


        /// <summary>
        /// Método para obtener la lista de areas por almacen 
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de areas</returns>
        public IEnumerable<Area> GetAreasByIdWarehouse(Guid IdWarehouse) => _context.Areas.Include(x => x.Layout).Where(x => x.Layout.WarehouseId == IdWarehouse).AsNoTracking();



        /// <summary>
        /// Método para obtener la lista de stations
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de areas</returns>
        public IEnumerable<Zone> GetZones()
        {
            return _context.Zones.Include(x => x.Area).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de BreakProfile
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de BreakProfile</returns>
        public IEnumerable<BreakProfile> GetBreakProfiles(Guid warehosueId)
        {
            return _context.BreakProfiles.Where(x => x.WarehouseId == warehosueId).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de Workers
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de Workers</returns>
        public IEnumerable<Worker> GetWorkers()
        {
            return _context.Workers
                .Include(x => x.Rol)
                .Include(x => x.Team).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de Workers
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de Workers</returns>
        public IEnumerable<RolProcessSequence> GetRolProcess()
        {
            return _context.RolProcessSequences
                .Include(x => x.Rol)
                .Include(x => x.Process).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de Shift
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de Shift</returns>
        public IEnumerable<Shift> GetShifts(Guid warehosueId)
        {
            return _context.Shifts.Where(x => x.WarehouseId == warehosueId)
                .Include(x => x.Schedules)
                .ThenInclude(x => x.AvailableWorker)
                .AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de Roles
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de Roles</returns>
        public IEnumerable<Rol> GetRoles(Guid warehouseId)
        {
            return _context.RolProcessSequences
                .Include(x => x.Rol)
                .Where(x => x.Rol != null && x.Rol.WarehouseId == warehouseId)
                .Select(x => x.Rol)
                .Distinct();
        }

        /// <summary>
        /// Método para obtener la lista de VehicleProfiles
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de VehicleProfiles</returns>
        public IEnumerable<VehicleProfile> GetVehicleProfiles(Guid warehosueId) => _context.VehicleProfiles.Where(x => x.WarehouseId == warehosueId).AsNoTracking();

        /// <summary>
        /// Método para obtener la lista de VehicleProfiles
        /// </summary>
        /// <returns>IEnumerable de la lista de VehicleProfiles</returns>
        public IEnumerable<VehicleProfile> GetVehicleProfilesNoFiltered()
        {
            return _context.VehicleProfiles.AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de LoadProfile
        /// </summary>
        /// <returns>IEnumerable de la lista de LoadProfile</returns>
        public IEnumerable<LoadProfile> GetLoadProfiles2()
        {
            return _context.LoadProfiles.AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de VehicleProfiles
        /// </summary>
        /// <returns>IEnumerable de la lista de VehicleProfiles</returns>
        public IEnumerable<VehicleProfile> GetVehicleProfiles2()
        {
            return _context.VehicleProfiles.AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de LoadProfile
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de LoadProfile</returns>
        public IEnumerable<LoadProfile> GetLoadProfiles(Guid warehosueId)
        {
            return _context.LoadProfiles.Where(x => x.WarehouseId == warehosueId).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de LoadProfile
        /// </summary>
        /// <returns>IEnumerable de la lista de LoadProfile</returns>
        public IEnumerable<LoadProfile> GetLoadProfilesNoFiltered()
        {
            return _context.LoadProfiles.AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de PutawayProfile
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de PutawayProfile</returns>
        public IEnumerable<PutawayProfile> GetPutawayProfiles(Guid warehosueId)
        {
            return _context.PutawayProfiles.Where(x => x.WarehouseId == warehosueId).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de PostprocessProfile
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de PostprocessProfile</returns>
        public IEnumerable<PostprocessProfile> GetPostprocessProfiles(Guid warehosueId)
        {
            return _context.PostprocessProfiles.Where(x => x.WarehouseId == warehosueId).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de PreprocessProfile
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de PreprocessProfile</returns>
        public IEnumerable<PreprocessProfile> GetPreprocessProfiles(Guid warehosueId)
        {
            return _context.PreprocessProfiles.Where(x => x.WarehouseId == warehosueId).AsNoTracking();
        }


        /// <summary>
        /// Método para obtener la lista de OrderSchedules
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de PreprocessProfile</returns>
        public IEnumerable<OrderSchedule> GetOrderSchedules(Guid warehosueId)
        {
            return _context.OrderSchedules.Where(x => x.WarehouseId == warehosueId)
                .Include(o => o.Vehicle)
                .Include(o => o.Load)
                .AsNoTracking();
        }

        /// <summary>
        /// Método para obtener la lista de OrderProfiles
        /// TODO: recibir parámetro del sitio ó almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de PreprocessProfile</returns>
        public IEnumerable<PreprocessProfile> GetOrderProfiles(Guid warehosueId)
        {
            return _context.PreprocessProfiles.Where(x => x.WarehouseId == warehosueId).AsNoTracking();
        }


        /// <summary>
        /// Método para obtener la lista de workers habilitados para cada tipo de proceso
        /// </summary>
        /// <returns>Lista de workers habilitados por tipo de proceso</returns>
        public List<OperatorsByProcess> GetWorkersByProcess(Guid site)
        {
            return _context.AvailableWorkers.Where(x => x.Worker.Rol.WarehouseId == site)
                .GroupBy(x => x.Worker.Rol.Name)
                .Select(x => new OperatorsByProcess
                {
                    Process = x.Key,
                    Quantity = x.Count(),
                }).ToList();
        }

        /// <summary>        
        /// Método para obtener el guid del almacén, se obtiene el primero por defecto en la base de datos
        /// </summary>
        /// <returns>Guid del almacén por defecto</returns>
        public Guid GetWarehouseId()
        {
            return _context.Warehouses.FirstOrDefault()?.Id ?? Guid.Empty;
        }

        public void RemoveTemporality(Guid Id)
        {
            if (_context.ConfigurationSequenceHeaders.Any(x => x.Id == Id))
            {
                _context.ConfigurationSequenceHeaders.Remove(_context.ConfigurationSequenceHeaders.First(x => x.Id == Id));
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Método para obtener una lista de almacenes de la organización
        /// </summary>
        /// <returns>Lista de almacenes</returns>
        public IEnumerable<Warehouse> GetWarehouses()
        {
            return _context.Warehouses.Include(m => m.Country)
                .Include(m => m.TimeZone_).AsNoTracking();
        }


        /// <summary>
        /// Método para obtener una lista de usuarios de la organización
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        public IEnumerable<User> GetUsers()
        {
            return _context.Users.AsNoTracking();
        }

        /// <summary>
        /// Gets the DockSelectionStrategies elements
        /// </summary>
        /// <returns>IEnumerable of the DockSelectionStrategies for the organization</returns>
        public IEnumerable<DockSelectionStrategy> GetDockSelectionStrategies()
        {
            return _context.DockSelectionStrategies.Include(m => m.Organization).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener una lista de zona horaria
        /// </summary>
        /// <returns>Lista de zonas horarias</returns>
        public IEnumerable<TimeZone> GetTimeZones() => _context.TimeZones;

        /// <summary>
        /// Método para obtener la primera organización
        /// </summary>
        /// <returns>Organización</returns>
        public Organization GetOnlyOrganization()
        {
            return _context.Organizations
                .Include(m => m.DecimalSeparator)
                .Include(m => m.Country)
                .Include(m => m.ThousandsSeparator)
                .Include(m => m.DateFormat)
                .Include(m => m.HourFormat)
                .Include(m => m.Language)
                .Include(m => m.SystemOfMeasurement).AsNoTracking()
                .FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener el catalogo del separador de miles
        /// </summary>
        /// <returns>Enumerable de ThousandsSeparator</returns>
        public IEnumerable<ThousandsSeparator> GetThousandsSeparator() => _context.ThousandsSeparators.AsTracking();

        /// <summary>
        /// Método para obtener el catalogo del separador de decimales
        /// </summary>
        /// <returns>Enumerable de DecimalSeparator</returns>
        public IEnumerable<DecimalSeparator> GetDecimalSeparator() => _context.DecimalSeparators.AsTracking();

        /// <summary>
        /// Método para obtener el catalogo de systemas de medidas 
        /// </summary>
        /// <returns>Enumerable de SystemOfMeasurement</returns>
        public IEnumerable<SystemOfMeasurement> GetSystemOfMeasurements() => _context.SystemOfMeasurements.AsTracking();

        /// <summary>
        /// Método para obtener el catalogo de lenguajes
        /// </summary>
        /// <returns>Enumerable de Language</returns>
        public IEnumerable<Language> GetLanguages() => _context.Languages.AsTracking();


        /// <summary>
        /// Método para obtener el catalogo de formatos de fecha
        /// </summary>
        /// <returns>Enumerable de DateFormat</returns>
        public IEnumerable<DateFormat> GetDateFormats() => _context.DateFormats.AsTracking();

        /// <summary>
        /// Método para obtener el catalogo de formatos de hora
        /// </summary>
        /// <returns>Enumerable de HourFormat</returns>
        public IEnumerable<HourFormat> GetHourFormats() => _context.HourFormats.AsTracking();

        /// <summary>
        /// Método para obtener el catalogo de Countries
        /// </summary>
        /// <returns>Enumerable de Countries</returns>
        public IEnumerable<Country> GetCountries() => _context.Countries.AsTracking();

        public IEnumerable<ConfigurationSequenceHeader> GetConfigurationSequencesByWarehouse(Guid warehouseId)
        {
            return _context.ConfigurationSequenceHeaders
                .Where(csH => csH.WarehouseId == warehouseId)
                .Include(csH => csH.ConfigurationSequences).AsNoTracking();
        }

        public IEnumerable<ConfigurationSequence> GetSequenceByConfigurationSequence(Guid configurationId)
        {
            return _context.ConfigurationSequences.Where(x => x.ConfigurationSequenceHeaderId == configurationId).AsNoTracking();
        }

        public IEnumerable<Alert> GetAlerts(Guid warehouseId)
        {
            return _context.Alerts.Where(x => x.WarehouseId == warehouseId).Include(x => x.Configurations).ToList();
        }

        public IEnumerable<AlertFilter> GetAlertFilters(Guid AlertId)
        {
            return _context.AlertFilters.Where(x => x.AlertId == AlertId).ToList();
        }

        public IEnumerable<AlertConfiguration> GetAlertConfigurations(Guid AlertId)
        {
            return _context.AlertConfigurations.Where(x => x.AlertId == AlertId);
        }

        public void AddConfigurationSequence(ConfigurationSequence configurationSequence)
        {
            _context.ConfigurationSequences.Add(configurationSequence);
            _context.SaveChanges();
        }

        public void RemoveConfigurationSequence(Guid sequenceId)
        {
            var configSequence = _context.ConfigurationSequences.FirstOrDefault(x => x.Id == sequenceId);
            if (configSequence != null)
                _context.ConfigurationSequences.RemoveRange(configSequence);
            _context.SaveChanges();
        }

        public bool ValidateUser(string user, string password)
        {
            return _context.Users.Any(m => m.Code == user && m.Password == password && m.IsEnabled);
        }

        public string ReasonLoginError(string user, string password)
        {
            return _context.Users.Any(m => m.Code == user && m.Password == password) ? "Disabled user" : "Incorrect user or password";
        }

        #region Designer      
        /// <summary>
        /// Obtiene todos los objetos de tipo <see cref="Layout"/> del contexto de la base de datos.
        /// </summary>
        /// <returns>
        /// Una colección de objetos <see cref="Layout"/> que representa todos los registros almacenados.
        /// </returns>
        public IEnumerable<Layout> GetLayouts(Guid UserGuid)
        {
            if (_context.Users.Include(x => x.Warehouses).AsNoTracking().Where(x => x.Id == UserGuid).FirstOrDefault() is User userdto && userdto.Warehouses.Select(w => w.Id).ToHashSet() is HashSet<Guid> ListGuid)
                return _context.Layouts.Include(x => x.Warehouse).AsNoTracking().Where(x => ListGuid.Contains(x.WarehouseId)).ToList();
            else
                return _context.Layouts.Include(x => x.Warehouse).AsNoTracking().ToList();
        }

        /// <summary>
        /// Obtiene un objeto de tipo <see cref="Layout"/> basado en su identificador único.
        /// </summary>
        /// <param name="Id">El identificador único (<see cref="Guid"/>) del <see cref="Layout"/> que se desea obtener.</param>
        /// <returns>
        /// El objeto <see cref="Layout"/> correspondiente al identificador proporcionado, 
        /// o <c>null</c> si no se encuentra ningún registro coincidente.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanza esta excepción si no se encuentra un <see cref="Layout"/> con el <paramref name="Id"/> especificado.
        /// </exception>
        public Layout GetLayoutById(Guid Id)
        {
            return _context.Layouts.Include(x => x.Warehouse).AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Layout with the Id was not found: {Id}");
        }

        /// <summary>
        /// Obtiene objetos de tipo <see cref="Objects"/> basado en su relación única con layout.
        /// </summary>
        /// <param name="layoutId">El identificador único de layout (<see cref="Guid"/>) del <see cref="Objects"/> que se desea obtener.</param>
        /// <returns>
        /// Una colección <see cref="Objects"/> correspondiente al identificador proporcionado,
        /// o una lista vacía si no se encuentra ningún registro coincidente.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanza esta excepción si no se encuentra un <see cref="Layout"/> con el <paramref name="layoutId"/> especificado.
        /// </exception>
        public IEnumerable<Objects> GetObjectsByLayout(Guid layoutId)
        {
            if (!_context.Layouts.AsNoTracking().Any(l => l.Id == layoutId))
            {
                throw new KeyNotFoundException($"A Layout with the Id was not found: {layoutId}");
            }

            return _context.Objects.Include(x => x.Layout).AsNoTracking().Where(x => x.LayoutId == layoutId).ToList();
        }

        /// <summary>
        /// Obtiene objetos de tipo <see cref="Area"/> basado en su relación única con layout.
        /// </summary>
        /// <param name="layoutId">El identificador único de layout (<see cref="Guid"/>) del <see cref="Area"/> que se desea obtener.</param>
        /// <returns>
        /// Una colección <see cref="Area"/> correspondiente al identificador proporcionado,
        /// o una lista vacía si no se encuentra ningún registro coincidente.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanza esta excepción si no se encuentra un <see cref="Layout"/> con el <paramref name="layoutId"/> especificado.
        /// </exception>
        public IEnumerable<Area> GetAreasByLayout(Guid layoutId)
        {
            if (!_context.Layouts.AsNoTracking().Any(l => l.Id == layoutId))
            {
                throw new KeyNotFoundException($"A Layout with the Id was not found: {layoutId}");
            }

            return _context.Areas
                .Include(x => x.AlternativeArea)
                    .ThenInclude(x => x.Layout)
                .Include(x => x.AlternativeArea)
                    .ThenInclude(x => x.AlternativeArea)
                    .ThenInclude(x => x.Layout)
                .Include(x => x.Layout)
                .AsNoTracking().Where(x => x.LayoutId == layoutId).ToList();
        }

        public Area? GetAreaByAreaId(Guid areaDtoId)
        {
            return _context.Areas.AsNoTracking().FirstOrDefault(x => x.Id == areaDtoId);
        }

        public Area? GetAreaByAreaIdWithAlternativeAreaAsNoTracking(Guid areaDtoId)
        {
            return _context.Areas
                .Include(x => x.AlternativeArea) // Obtains alternative areas
                    .ThenInclude(x => x.Layout) // Gets the AlternativeArea Layout
                .Include(x => x.AlternativeArea)
                    .ThenInclude(x => x.AlternativeArea) // Gets the AlternativeArea AlternativeArea
                    .ThenInclude(x => x.Layout)
                .Include(x => x.Layout).AsNoTracking().FirstOrDefault(x => x.Id == areaDtoId);
        }

        public Route? GetRouteByIdWithAreaAsNoTracking(Guid itemId)
        {
            return _context.Routes
                .Include(x => x.DepartureArea) //Gets DepartureArea
                    .ThenInclude(x => x.AlternativeArea) //Gets AlternativeArea
                .Include(x => x.ArrivalArea) //Gets ArrivalArea
                    .ThenInclude(x => x.AlternativeArea)
                .Where(r => r.Id == itemId).AsNoTracking().FirstOrDefault();
        }

        public Route? GetRouteByIdAsNoTracking(Guid itemId)
        {
            return _context.Routes
                .Include(x => x.DepartureArea) //Gets DepartureArea
                    .ThenInclude(x => x.AlternativeArea) //Gets AlternativeArea from DepartureArea
                    .ThenInclude(x => x.Layout) //Gets Layout from AlternativeArea
                .Include(x => x.DepartureArea)
                    .ThenInclude(x => x.Layout) //Gets Layout from DepartureArea
                .Include(x => x.ArrivalArea) //Gets ArrivalArea
                    .ThenInclude(x => x.AlternativeArea) //Gets AlternativeArea from ArrivalArea
                    .ThenInclude(x => x.Layout) //Gets Layout from AlternativeArea
                .Include(x => x.ArrivalArea)
                    .ThenInclude(x => x.Layout) //Gets Layout from ArrivalArea
                .Where(r => r.Id == itemId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener la lista de rutas desde areas
        /// TODO: recibir parámetro de la ruta
        /// </summary>
        /// <returns>IEnumerable de la lista de route</returns>
        public IEnumerable<Route> GetRoutesByAreaIdNoTracking(Guid areaId)
        {
            return _context.Routes
                .Include(x => x.DepartureArea)
                    .ThenInclude(d => d.AlternativeArea) // Obtain AlternativeArea from DepartureArea
                    .ThenInclude(da => da.Layout) // Obtain Layout from AlternativeArea
                .Include(x => x.DepartureArea)
                    .ThenInclude(da => da.Layout) // Obtain Layout from DepartureArea
                .Include(x => x.ArrivalArea)
                    .ThenInclude(a => a.AlternativeArea) // Obtain AlternativeArea from AlternativeArea
                    .ThenInclude(da => da.Layout) // Obtain Layout from AlternativeArea
                .Include(x => x.ArrivalArea)
                    .ThenInclude(aa => aa.Layout) // Obtain Layout from DepartureArea
               .Where(x => (x.ArrivalAreaId == areaId || x.DepartureAreaId == areaId) &&
                    (x.DepartureArea.AlternativeAreaId != null ||
                    x.ArrivalArea.AlternativeAreaId != null))
               .AsNoTracking()
               .ToList();
        }

        /// <summary>
        /// Método para obtener la lista de rutas desde areas
        /// TODO: recibir parámetro de la ruta
        /// </summary>
        /// <returns>IEnumerable de la lista de route</returns>
        public IEnumerable<Route> GetRoutesByAreaIdAsNoTracking(Guid areaId)
        {
            return _context.Routes
                .Include(x => x.DepartureArea)
                    .ThenInclude(d => d.AlternativeArea) // Obtain AlternativeArea from DepartureArea
                    .ThenInclude(da => da.Layout) // Obtain Layout from AlternativeArea
                .Include(x => x.DepartureArea)
                    .ThenInclude(da => da.Layout) // Obtain Layout from DepartureArea
                .Include(x => x.ArrivalArea)
                    .ThenInclude(a => a.AlternativeArea) // Obtain AlternativeArea from AlternativeArea
                    .ThenInclude(da => da.Layout) // Obtain Layout from AlternativeArea
                .Include(x => x.ArrivalArea)
                    .ThenInclude(aa => aa.Layout) // Obtain Layout from DepartureArea
                .Where(x =>
                    (x.ArrivalAreaId == areaId || x.DepartureAreaId == areaId)
                ).AsNoTracking().ToList();
        }

        /// <summary>
        /// Método para obtener la lista de equipamiento desde areas
        /// TODO: recibir parámetro de la equipamiento
        /// </summary>
        /// <returns>IEnumerable de la lista de equipamiento</returns>
        public IEnumerable<EquipmentGroup> GetEquipmentsByAreaIdNoTracking(Guid areaId)
        {
            return _context.EquipmentGroups.Where(x => x.AreaId == areaId).AsNoTracking().ToList();
        }

        public IEnumerable<EquipmentGroup> GetEquipmentsByAreaIdAsNoTracking(Guid areaId)
        {
            return _context.EquipmentGroups
                .Include(a => a.TypeEquipment)
                    .ThenInclude(aa => aa.Warehouse)
                .Include(b => b.Area)
                    .ThenInclude(a => a.AlternativeArea)
                    .ThenInclude(da => da.Layout)
                .Include(c => c.Area)
                    .ThenInclude(aa => aa.Layout)
                .Where(x => x.AreaId == areaId).AsNoTracking().ToList();
        }

        /// <summary>
        /// Método para obtener la lista de equipamiento
        /// TODO: recibir parámetro de la equipamiento
        /// </summary>
        /// <returns>IEnumerable de la lista de equipamiento</returns>
        public EquipmentGroup? GetEquipmentGroupWithTypeEquipmentByIdAsNoTracking(Guid equipmentId)
        {
            return _context.EquipmentGroups
                .Include(a => a.TypeEquipment)
                    .ThenInclude(aa => aa.Warehouse)
                .Include(b => b.Area)
                    .ThenInclude(a => a.AlternativeArea)
                    .ThenInclude(da => da.Layout)
                .Include(c => c.Area)
                    .ThenInclude(aa => aa.Layout)
                .AsNoTracking()
                .Where(r => r.Id == equipmentId).FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener el tipo de equipamiento seleccionado
        /// TODO: recibir parámetro del tipo de equipamiento
        /// </summary>
        /// <returns>TypeEquipment </returns>
        public TypeEquipment? GetTypeEquipmentByIdNoTracking(Guid TypeEquipmentId)
        {
            return _context.TypeEquipment.Where(x => x.Id == TypeEquipmentId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener la lista de tipos de equipamientos 
        /// TODO: recibir parámetro del almacén
        /// </summary>
        /// <returns>IEnumerable de la lista de tipos de equipamientos</returns>
        public IEnumerable<TypeEquipment> GetEquipmentTypesByWarehouseIdNoTracking(Guid warehouseId)
        {
            return _context.TypeEquipment.Where(x => x.WarehouseId == warehouseId).AsNoTracking();
        }

        /// <summary>
        /// Método para obtener el equipamiento
        /// TODO: recibir parámetro del id del equipamiento
        /// </summary>
        /// <returns>Equipamiento</returns>
        public EquipmentGroup? GetEquipmentGroupsByIdAsNoTracking(Guid equipmentGroupId)
        {
            return _context.EquipmentGroups.Where(e => e.Id == equipmentGroupId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener lista de Aisles correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD </param>
        /// <returns>Lista de aisles que estan en un área</returns>
        public IEnumerable<Aisle> GetAislesByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Aisles.Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Aisle>();

        }

        /// <summary>
        /// Método para obtener lista de Aisles correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD </param>
        /// <returns>Lista de aisles que estan en un área</returns>
        public IEnumerable<Aisle> GetAislesByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Aisles
                    .Include(x => x.Zone)
                        .ThenInclude(x => x.Area)
                            .ThenInclude(x => x.AlternativeArea)
                                .ThenInclude(x => x.Layout)
                    .Include(x => x.Zone)
                        .ThenInclude(x => x.Area)
                        .ThenInclude(x => x.Layout)
                    .Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Aisle>();

        }

        /// <summary>
        /// Método para obtener un aisle por id de zone 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns>Aisle</returns>
        public Aisle? GetAisleWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.Aisles
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.Layout)
                .Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un aisle
        /// </summary>
        /// <param name="aisleId">Guid correspondiente el registro de BD</param>
        /// <returns>Aisle</returns>
        public Aisle? GetAisleByIdNoTracking(Guid aisleId)
        {
            return _context.Aisles.Where(e => e.Id == aisleId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un aisle
        /// </summary>
        /// <param name="aisleId">Guid correspondiente el registro de BD</param>
        /// <returns>Aisle</returns>
        public Aisle? GetAisleWithZoneByAisleIdAsNoTracking(Guid aisleId)
        {
            return _context.Aisles
                .Include(x => x.Zone)
                    .ThenInclude(x => x.Area)
                        .ThenInclude(x => x.AlternativeArea)
                            .ThenInclude(x => x.Layout)
                .Include(x => x.Zone)
                    .ThenInclude(x => x.Area)
                        .ThenInclude(x => x.Layout)
                .Where(e => e.Id == aisleId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener lista de Zones correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Zones que estan en un área</returns>
        public IEnumerable<Zone> GetZonesWithAreaByAreaIdNoTracking(Guid areaId)
        {
            return _context.Zones
                .Include(z => z.Area)
                    .ThenInclude(za => za.AlternativeArea)
                    .ThenInclude(zb => zb.Layout)
                .Include(z => z.Area)
                    .ThenInclude(zc => zc.Layout)
                .Where(zones => zones.AreaId == areaId).AsNoTracking().ToList();
        }

        /// <summary>
        /// Método para obtener lista de Docks correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Docks que estan en un área</returns>
        public IEnumerable<Dock> GetDocksByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Docks.Where(dock => stationIds.Contains(dock.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Dock>();
        }

        /// <summary>
        /// Método para obtener lista de Docks correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Docks que estan en un área</returns>
        public IEnumerable<Dock> GetDocksWithZoneByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Docks
                    .Include(a => a.Zone)
                        .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.AlternativeArea)
                        .ThenInclude(ac => ac.Layout)
                    .Include(a => a.Zone)
                        .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.Layout)
                    .Where(dock => stationIds.Contains(dock.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Dock>();
        }

        /// <summary>
        /// Método para obtener lista de Docks correspondientes a un layout
        /// </summary>
        /// <param name="layoutId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Docks que estan en un layout</returns>
        public IEnumerable<object> GetDocksWithZoneByLayoutIdAsNoTracking(Guid layoutId)
        {
            return _context.Docks
                    .Where(d => d.Zone.Area.LayoutId == layoutId)
                    .Select(d => new
                    {
                        DockId = d.Id,
                        ZoneName = d.Zone.Name
                    })
                    .AsNoTracking()
                    .ToList();
        }


        /// <summary>
        /// Método para obtener un dock por id de zone 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns>Dock</returns>
        public Dock? GetDockWithZoneByZoneId(Guid zoneId)
        {
            return _context.Docks
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.Layout)
                .Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un dock
        /// </summary>
        /// <param name="dockId"></param>
        /// <returns>Dock</returns>
        public Dock? GetDockByIdNoTracking(Guid dockId)
        {
            return _context.Docks.Where(e => e.Id == dockId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un dock
        /// </summary>
        /// <param name="dockId"></param>
        /// <returns>Dock</returns>
        public Dock? GetDockWithZoneByIdAsNoTracking(Guid dockId)
        {
            return _context.Docks
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.Layout)
                .Where(e => e.Id == dockId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener el valor mas grande de los registros de Dock para Inbound
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns>int</returns>
        public int GetMaxInboundRangeByAreaId(Guid areaId)
        {
            return _context.Docks.Where(d => d.Zone.AreaId == areaId && d.InboundRange.HasValue).Max(d => d.InboundRange) ?? 0;
        }

        /// <summary>
        /// Método para obtener el valor mas grande de los registros de Dock para Outbound
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns>int</returns>
        public int GetMaxOutboundRangeByAreaId(Guid areaId)
        {
            return _context.Docks.Where(d => d.Zone.AreaId == areaId && d.OutboundRange.HasValue).Max(d => d.OutboundRange) ?? 0;
        }

        /// <summary>
        /// Método para obtener un stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns>Stage</returns>
        public Stage? GetStageByIdNoTracking(Guid stageId)
        {
            return _context.Stages.Where(e => e.Id == stageId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener lista de Stages correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Stages que estan en un área</returns>
        public IEnumerable<Stage> GetStagesByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Stages.Where(stage => stationIds.Contains(stage.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Stage>();
        }

        /// <summary>
        /// Método para obtener lista de Stages correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Stages que estan en un área</returns>
        public IEnumerable<Stage> GetStagesWithZoneByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Stages
                    .Include(a => a.Zone)
                        .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.AlternativeArea)
                        .ThenInclude(ac => ac.Layout)
                    .Include(a => a.Zone)
                        .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.Layout)
                    .Where(stage => stationIds.Contains(stage.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Stage>();
        }

        /// <summary>
        /// Método para obtener un stage por id de zone 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns>Stage</returns>
        public Stage? GetStageWithZoneByZoneId(Guid zoneId)
        {
            return _context.Stages
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.Layout)
                .Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns>Stage</returns>
        public Stage? GetStageWithZoneByIdAsNoTracking(Guid dockId)
        {
            return _context.Stages
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.Layout)
                .Where(e => e.Id == dockId).AsNoTracking().FirstOrDefault();
        }

        public Stage? GetStageWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.Stages
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.Layout)
                .Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener lista de Buffers correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Buffer que estan en un área</returns>
        public IEnumerable<Buffer> GetBuffersByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Buffers.Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Buffer>();
        }

        /// <summary>
        /// Método para obtener lista de AvailableDocksPerStages correspondientes a un stage
        /// </summary>
        /// <param name="stageId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de AvailableDocksPerStagesDto que estan en un layout</returns>
        public IEnumerable<object> GetAvailableDocksPerStagesByStepsIdAsNoTracking(Guid stageId)
        {
            return _context.AvailableDocksPerStages
               .Where(d => d.StageId == stageId)
               .Select(d => new
               {
                   Id = d.Id,
                   StageId = d.StageId,
                   DockId = d.DockId,
                   ZoneName = d.Dock.Zone.Name
               })
               .ToList();
        }

        public IEnumerable<object> GetAvailableDocksPerStagesByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.AvailableDocksPerStages
                .AsNoTracking()
                .Where(d => d.Stage.ZoneId == zoneId)
                .Select(d => new
                {
                    Id = d.Id,
                    StageId = d.StageId,
                    DockId = d.DockId,
                    ZoneName = d.Dock.Zone.Name
                })
                .ToList();
        }

        /// <summary>
        /// Método para obtener lista de Buffers correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Buffer que estan en un área</returns>
        public IEnumerable<Buffer> GetBuffersWithZoneByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Buffers
                    .Include(a => a.Zone)
                        .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.AlternativeArea)
                        .ThenInclude(ac => ac.Layout)
                    .Include(a => a.Zone)
                        .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.Layout)
                    .Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Buffer>();
        }

        /// <summary>
        /// Método para obtener un buffer
        /// </summary>
        /// <param name="bufferId">Guid correspondiente el registro de BD</param>
        /// <returns>Buffer</returns>
        public Buffer? GetBufferByIdNoTracking(Guid bufferId)
        {
            return _context.Buffers.Where(e => e.Id == bufferId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un buffer
        /// </summary>
        /// <param name="bufferId">Guid correspondiente el registro de BD</param>
        /// <returns>Buffer</returns>
        public Buffer? GetBufferWithZoneByIdAsNoTracking(Guid bufferId)
        {
            return _context.Buffers
                .Include(a => a.Zone)

                    .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.AlternativeArea)
                            .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                        .ThenInclude(ab => ab.Layout)
                .Where(e => e.Id == bufferId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener una Zone
        /// </summary>
        /// <param name="zoneId">Guid correspondiente el registro de BD</param>
        /// <returns>Zone</returns>

        public Zone? GetZoneByIdNoTracking(Guid zoneId)
        {
            return _context.Zones.Where(e => e.Id == zoneId).AsNoTracking().FirstOrDefault();
        }

        public Zone? GetZoneWithAreaByIdAsNoTracking(Guid zoneId)
        {
            return _context.Zones
                .Include(z => z.Area)
                    .ThenInclude(za => za.AlternativeArea)
                    .ThenInclude(za => za.Layout)
                .Include(z => z.Area)
                    .ThenInclude(zb => zb.Layout)
                .Where(e => e.Id == zoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un buffer por id de zone 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns>Aisle</returns>
        public Buffer? GetBufferWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.Buffers
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(a => a.Zone)
                    .ThenInclude(aa => aa.Area)
                    .ThenInclude(ab => ab.Layout)
                .Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        public Buffer? GetBufferWithZoneByZoneId(Guid zoneId)
        {
            return _context.Buffers
             .Include(b => b.Zone)
                 .ThenInclude(z => z.Area)
                 .ThenInclude(a => a.AlternativeArea)
                 .ThenInclude(la => la.Layout)
             .Include(b => b.Zone)
                 .ThenInclude(z => z.Area)
                 .ThenInclude(a => a.Layout)
             .Where(b => b.ZoneId == zoneId)
             .AsNoTracking()
             .FirstOrDefault();

        }

        /// <summary>
        /// Método para obtener la lista de <see cref="Process"/> vía IdArea sin seguimiento (AsNoTracking).
        /// </summary>
        /// <returns>IEnumerable de <see cref="Process"/> que se encuentran en la base de datos.</returns>
        public IEnumerable<Process> GetProcessesByIdAreaNoTracking(Guid itemArea)
        {
            return _context.Processes.Where(x => x.AreaId == itemArea).Include(c => c.Flow).AsNoTracking().ToList();
        }

        /// <summary>
        /// Método para obtener la lista de <see cref="Process"/> vía IdArea sin seguimiento (AsNoTracking).
        /// </summary>
        /// <returns>IEnumerable de <see cref="Process"/> que se encuentran en la base de datos.</returns>
        public IEnumerable<Process> GetProcessesWithAreaByAreaIdAsNoTracking(Guid itemArea)
        {
            return _context.Processes
                .Include(a => a.Area)
                    .ThenInclude(aa => aa.AlternativeArea)
                    .ThenInclude(ab => ab.Layout)
                .Include(b => b.Area)
                    .ThenInclude(ba => ba.Layout)
                .Include(c => c.Flow)
                .Where(x => x.AreaId == itemArea)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToList();
        }

        /// <summary>
        /// Método para obtener la lista de <see cref="Process"/> vía warehouseId sin seguimiento (AsNoTracking).
        /// </summary>
        /// <returns>IEnumerable de <see cref="Process"/> que se encuentran en la base de datos.</returns>
        public IEnumerable<Process> GetAllProcessWithAreaByLayoutId(Guid layoutId)
        {
            return _context.Processes
            .Include(p => p.Area)
                .ThenInclude(a => a.Layout)
                .ThenInclude(l => l.Warehouse)
            .Include(b => b.Area)
                .ThenInclude(ba => ba.AlternativeArea)
                    .ThenInclude(bb => bb.Layout)
            .Include(c => c.Flow)
            .Where(p => p.Area.LayoutId == layoutId)
            .AsNoTracking().ToList();
        }

        /// <summary>
        /// Método para obtener un <see cref="Process"/> por su identificador único.
        /// </summary>
        /// <param name="processId">Guid correspondiente al registro de BD</param>
        /// <returns>Instancia de <see cref="Process"/> o <c>null</c> si no existe.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanza esta excepción si no se encuentra un <see cref="Process"/> con el <paramref name="processId"/> especificado.
        /// </exception>
        public Process GetProcessByIdNoTracking(Guid processId)
        {
            return _context.Processes.Include(c => c.Flow).AsNoTracking().FirstOrDefault(x => x.Id == processId)
                ?? throw new KeyNotFoundException($"A Process with the Id was not found: {processId}");
        }

        /// <summary>
        /// Método para obtener un <see cref="Process"/> por su identificador único.
        /// </summary>
        /// <param name="processId">Guid correspondiente al registro de BD</param>
        /// <returns>Instancia de <see cref="Process"/> o <c>null</c> si no existe.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanza esta excepción si no se encuentra un <see cref="Process"/> con el <paramref name="processId"/> especificado.
        /// </exception>
        public Process GetProcessWithAreaByProcessIdAsNoTracking(Guid processId)
        {
            return _context.Processes
                .Include(a => a.Area)
                    .ThenInclude(ab => ab.AlternativeArea)
                    .ThenInclude(ac => ac.Layout)
                .Include(b => b.Area)
                    .ThenInclude(b => b.Layout)
                .Include(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.Id == processId)
                ?? throw new KeyNotFoundException($"A Process with the Id was not found: {processId}");
        }

        /// <summary>
        /// Método para obtener lista de Racks correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Rack que estan en un área</returns>
        public IEnumerable<Rack> GetRacksByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Racks.Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Rack>();
        }

        /// <summary>
        /// Método para obtener lista de Racks correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Rack que estan en un área</returns>
        public IEnumerable<Rack> GetRacksWithZoneByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.Racks
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                    .Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<Rack>();
        }

        public Rack? GetRacksWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.Racks
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                    .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(zone => zoneId == zone.ZoneId).AsNoTracking().FirstOrDefault();
        }

        public DriveIn? GetDriveInByIdNoTracking(Guid driveInId)
        {
            return _context.DriveIns.Where(e => e.Id == driveInId).AsNoTracking().FirstOrDefault();
        }

        public DriveIn? GetDriveInWithZoneByDriveInIdAsNoTracking(Guid driveInId)
        {
            return _context.DriveIns
                .Include(a => a.Zone) // Gets Zone from DriveIns
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from DriveIns
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(e => e.Id == driveInId).AsNoTracking().FirstOrDefault();
        }

        public DriveIn? GetDriveInWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.DriveIns
                .Include(a => a.Zone) // Gets Zone from DriveIns
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from DriveIns
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        public IEnumerable<DriveIn> GetDriveInsByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.DriveIns.Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<DriveIn>();
        }

        public IEnumerable<DriveIn> GetDriveInsWithZoneByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.DriveIns
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                    .Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<DriveIn>();
        }

        public DriveIn? GetDriveInsWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.DriveIns
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                    .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                  .Where(zone => zoneId == zone.ZoneId).AsNoTracking().FirstOrDefault();
        }

        public IEnumerable<ChaoticStorage> GetChaoticStoragesWithZoneByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.ChaoticStorages
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                    .Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<ChaoticStorage>();
        }

        public ChaoticStorage? GetChaoticStoragesWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.ChaoticStorages
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                    .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(c => zoneId == c.ZoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un rack
        /// </summary>
        /// <param name="chaoticid">Guid correspondiente el registro de BD</param>
        /// <returns>Rack</returns>
        public ChaoticStorage? GetChaoticByIdNoTracking(Guid chaoticid)
        {
            return _context.ChaoticStorages.Where(e => e.Id == chaoticid).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un rack
        /// </summary>
        /// <param name="chaoticId">Guid correspondiente el registro de BD</param>
        /// <returns>Rack</returns>
        public ChaoticStorage? GetChaoticWithZoneByIdAsNoTracking(Guid chaoticId)
        {
            return _context.ChaoticStorages
                .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(e => e.Id == chaoticId).AsNoTracking().FirstOrDefault();
        }
        public ChaoticStorage? GetChaoticWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.ChaoticStorages
                .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(zone => zoneId == zone.ZoneId).AsNoTracking().FirstOrDefault();
        }

        public IEnumerable<AutomaticStorage> GetAutomaticStoragesWithZoneByAreaIdAsNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.AutomaticStorages
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                    .Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<AutomaticStorage>();
        }

        public AutomaticStorage? GetAutomaticStoragesWithZoneByZoneIdAsNoTracking(Guid zoneId)
        {
            return _context.AutomaticStorages
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                    .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                 .Where(zone => zoneId == zone.ZoneId).AsNoTracking().FirstOrDefault();
        }


        public AutomaticStorage? GetAutomaticStorageByIdNoTracking(Guid automaticStorageId)
        {
            return _context.AutomaticStorages.Where(e => e.Id == automaticStorageId).AsNoTracking().FirstOrDefault();
        }

        public AutomaticStorage? GetAutomaticStorageWithZoneByAutomaticStorageIdAsNoTracking(Guid automaticStorageId)
        {
            return _context.AutomaticStorages
                .Include(a => a.Zone) // Gets Zone from AutomaticStorages
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from AutomaticStorages
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(e => e.Id == automaticStorageId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener lista de Racks correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Rack que estan en un área</returns>
        public IEnumerable<ChaoticStorage> GetChaoticStoragesByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.ChaoticStorages.Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<ChaoticStorage>();
        }

        public ChaoticStorage? GetChaoticStorageWithZoneByChaoticStorageIdAsNoTracking(Guid chaoticStorageId)
        {
            return _context.ChaoticStorages
                .Include(a => a.Zone) // Gets Zone from ChaoticStorages
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from ChaoticStorages
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(e => e.Id == chaoticStorageId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener lista de Racks correspondientes a un área
        /// </summary>
        /// <param name="areaId">Guid correspondiente el registro de BD</param>
        /// <returns>Lista de Rack que estan en un área</returns>
        public IEnumerable<AutomaticStorage> GetAutomaticStoragesByAreaNoTracking(Guid areaId)
        {
            var stationIds = _context.Zones.Where(station => station.AreaId == areaId).Select(station => station.Id).ToList();
            if (stationIds.Count > 0)
                return _context.AutomaticStorages.Where(aisle => stationIds.Contains(aisle.ZoneId)).AsNoTracking().ToList();
            else
                return Enumerable.Empty<AutomaticStorage>();
        }

        /// <summary>
        /// Método para obtener un rack
        /// </summary>
        /// <param name="rackId">Guid correspondiente el registro de BD</param>
        /// <returns>Rack</returns>
        public Rack? GetRackByIdNoTracking(Guid rackId)
        {
            return _context.Racks.Where(e => e.Id == rackId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un rack
        /// </summary>
        /// <param name="rackId">Guid correspondiente el registro de BD</param>
        /// <returns>Rack</returns>
        public Rack? GetRackWithZoneByRackIdAsNoTracking(Guid rackId)
        {
            return _context.Racks
                .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                    .Include(a => a.Zone) // Gets Zone from Racks
                        .ThenInclude(aa => aa.Area) // Gets Area from Zone
                        .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(e => e.Id == rackId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un rack por id de zone 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns>Aisle</returns>
        public Rack? GetRackByIdZone(Guid zoneId)
        {
            return _context.Racks.Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un rack por id de zone 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns>Aisle</returns>
        public Rack? GetRackWithZoneByIdZoneAsNoTraking(Guid zoneId)
        {
            return _context.Racks
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.AlternativeArea) // Gets AlternativeArea from Area
                    .ThenInclude(ac => ac.Layout) // Gets Layput from Alternative Area
                .Include(a => a.Zone) // Gets Zone from Racks
                    .ThenInclude(aa => aa.Area) // Gets Area from Zone
                    .ThenInclude(ab => ab.Layout) // Gets Layout from Area
                .Where(e => e.ZoneId == zoneId).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// Método para obtener un <see cref="ProcessDirectionProperty"/> por su identificador único.
        /// </summary>
        /// <param name="itemId">Guid correspondiente al registro de BD</param>
        /// <returns>Instancia de <see cref="ProcessDirectionProperty"/> o <c>null</c> si no existe.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanza esta excepción si no se encuentra un <see cref="ProcessDirectionProperty"/> con el <paramref name="itemId"/> especificado.
        /// </exception>
        public ProcessDirectionProperty GetProcessDirectionPropertyByIdNoTracking(Guid itemId)
        {
            return _context.ProcessDirectionProperties.AsNoTracking().FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"A Process with the Id was not found: {itemId}");
        }

        /// <summary>
        /// Método para obtener un <see cref="ProcessDirectionProperty"/> por su identificador único.
        /// </summary>
        /// <param name="itemId">Guid correspondiente al registro de BD</param>
        /// <returns>Instancia de <see cref="ProcessDirectionProperty"/> o <c>null</c> si no existe.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanza esta excepción si no se encuentra un <see cref="ProcessDirectionProperty"/> con el <paramref name="itemId"/> especificado.
        /// </exception>
        public ProcessDirectionProperty GetProcessDirectionPropertyByIdWithProcessAsNoTracking(Guid itemId)
        {
            return _context.ProcessDirectionProperties
                .Include(p => p.InitProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ip => ip.Area) // Gets Area from Process
                        .ThenInclude(ipa => ipa.AlternativeArea) // Gets AlternativeArea from Area
                            .ThenInclude(ipaa => ipaa.Layout) // Gets Layout from AlternativeArea
                .Include(p => p.InitProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ip => ip.Area) // Gets Area from Process
                        .ThenInclude(ipab => ipab.Layout) // Gets Layout from Area
                .Include(p => p.InitProcess)
                    .ThenInclude(o => o.Flow)
                .Include(p => p.EndProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ep => ep.Area) // Gets Area from Process
                        .ThenInclude(ipb => ipb.AlternativeArea) // Gets AlternativeArea from Area
                            .ThenInclude(ipba => ipba.Layout) // Gets Layout from AlternativeArea
                .Include(p => p.EndProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ep => ep.Area) // Gets Area from Process
                        .ThenInclude(ipc => ipc.Layout) // Gets Layout from Area
                .Include(p => p.EndProcess)
                    .ThenInclude(o => o.Flow)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"A Process with the Id was not found: {itemId}");
        }

        public IEnumerable<ProcessDirectionProperty> GetAllProcessDirectionPropertyByLayoutId(Guid layoutId)
        {
            return _context.ProcessDirectionProperties
                .Include(p => p.InitProcess) // Gets Process
                    .ThenInclude(ip => ip.Area) // Gets Area from Process
                        .ThenInclude(ipa => ipa.Layout) // Gets Layout from Area
                .Include(p => p.InitProcess)
                    .ThenInclude(o => o.Flow)
                .Include(p => p.EndProcess) // Gets Process
                    .ThenInclude(ep => ep.Area) // Gets Area from Process
                        .ThenInclude(epa => epa.Layout) // Gets Layout from Area
                .Include(p => p.EndProcess)
                    .ThenInclude(o => o.Flow)
                .Where(p =>
                    p.InitProcess.Area.LayoutId == layoutId ||
                    p.EndProcess.Area.LayoutId == layoutId)
                .AsNoTracking()
                .ToList();
        }

        /// <summary>
        /// Método para obtener la lista de process direction desde processs
        /// TODO: recibir parámetro de la process Id
        /// </summary>
        /// <returns>IEnumerable de la lista de process direction</returns>
        public IEnumerable<ProcessDirectionProperty> GetProcessDirectionsByIdProcessAsNoTracking(Guid processId)
        {
            return _context.ProcessDirectionProperties
                .Include(p => p.InitProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ip => ip.Area) // Gets Area from Process
                        .ThenInclude(ipa => ipa.AlternativeArea) // Gets AlternativeArea from Area
                            .ThenInclude(ipaa => ipaa.Layout) // Gets Layout from AlternativeArea
                .Include(p => p.InitProcess)
                    .ThenInclude(o => o.Flow)
                .Include(p => p.InitProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ip => ip.Area) // Gets Area from Process
                        .ThenInclude(ipab => ipab.Layout) // Gets Layout from Area

                .Include(p => p.EndProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ep => ep.Area) // Gets Area from Process
                        .ThenInclude(ipb => ipb.AlternativeArea) // Gets AlternativeArea from Area
                            .ThenInclude(ipba => ipba.Layout) // Gets Layout from AlternativeArea
                .Include(p => p.EndProcess)
                    .ThenInclude(o => o.Flow)
                .Include(p => p.EndProcess) // Gets Process from ProcessDirectionProperties
                    .ThenInclude(ep => ep.Area) // Gets Area from Process
                        .ThenInclude(ipc => ipc.Layout) // Gets Layout from Area
                .AsNoTracking()
              .Where(x =>
                x.InitProcessId == processId ||
                x.EndProcessId == processId)
              .ToList();
        }

        public Loading GetLoadingByIdWithProcess(Guid Id)
        {
            return _context.Loadings
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Loading with the Id was not found: {Id}");
        }

        public Loading GetLoadingByProcessId(Guid processId)
        {
            return _context.Loadings
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Loading with the Id was not found: {processId}");
        }

        public IEnumerable<Loading?> GetLoadingByIdProcess(Guid processId)
        {
            return _context.Loadings
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public Picking GetPickingByIdWithProcess(Guid Id)
        {
            return _context.Pickings
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Picking with the Id was not found: {Id}");
        }

        public Picking GetPickingByProcessId(Guid processId)
        {
            return _context.Pickings
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Picking with the Id was not found: {processId}");
        }

        public IEnumerable<Picking?> GetPickingByIdProcess(Guid processId)
        {
            return _context.Pickings.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public Putaway GetPutawayByIdWithProcess(Guid Id)
        {
            return _context.Putaways
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Putaway with the Id was not found: {Id}");
        }

        public Putaway GetPutawayByProcessId(Guid processId)
        {
            return _context.Putaways
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Putaway with the Id was not found: {processId}");
        }

        public IEnumerable<Putaway?> GetPutawayByIdProcessAsNoTracking(Guid processId)
        {
            return _context.Putaways.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public Shipping GetShippingByIdWithProcess(Guid Id)
        {
            return _context.Shippings
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Shipping with the Id was not found: {Id}");
        }

        public Shipping GetShippingByProcessId(Guid processId)
        {
            return _context.Shippings
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Shipping with the Id was not found: {processId}");
        }

        public IEnumerable<Shipping?> GetShippingByIdProcess(Guid processId)
        {
            return _context.Shippings.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public Reception GetReceptionByIdWithProcessAsNoTracking(Guid Id)
        {
            return _context.Receptions
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Reception with the Id was not found: {Id}");
        }

        public Reception GetReceptionByProcessIdAsNoTracking(Guid processId)
        {
            return _context.Receptions
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Reception with the Id was not found: {processId}");
        }

        public IEnumerable<Reception?> GetReceptionByIdProcess(Guid processId)
        {
            return _context.Receptions.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public Inbound GetInboundDesignerById(Guid Id)
        {
            return _context.Inbounds
                .Include(a => a.Process)
                    .ThenInclude(aa => aa.Area)
                        .ThenInclude(aaa => aaa.AlternativeArea)
                        .ThenInclude(aab => aab.Layout)
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area)
                        .ThenInclude(bba => bba.Layout)
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Inbound with the Id was not found: {Id}");
        }

        public CustomProcess GetCustomProcessDesignerById(Guid Id)
        {
            return _context.CustomProcesses
                .Include(a => a.Process)
                    .ThenInclude(aa => aa.Area)
                        .ThenInclude(aaa => aaa.AlternativeArea)
                        .ThenInclude(aab => aab.Layout)
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area)
                        .ThenInclude(bba => bba.Layout)
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A CustomProcesses with the Id was not found: {Id}");
        }

        public CustomProcess? GetCustomProcessDesignerByProcessId(Guid processId)
        {
            return _context.CustomProcesses
                .Include(a => a.Process)
                    .ThenInclude(aa => aa.Area)
                        .ThenInclude(aaa => aaa.AlternativeArea)
                        .ThenInclude(aab => aab.Layout)
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area)
                        .ThenInclude(bba => bba.Layout)
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A CustomProcesses with the Id was not found: {processId}");
        }

        /// <summary>
        /// Método para obtener un Custome Process
        /// </summary>
        /// <param name="processId">Guid correspondiente el registro de BD</param>
        /// <returns>Buffer</returns>
        public IEnumerable<CustomProcess?> GetCustomProcessDesignerByIdProcess(Guid processId)
        {
            return _context.CustomProcesses.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public IEnumerable<Inbound?> GetInboundDesignerByIdProcess(Guid processId)
        {
            return _context.Inbounds.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public Inbound? GetInboundByProcessId(Guid processId)
        {
            return _context.Inbounds
                .Include(a => a.Process)
                    .ThenInclude(aa => aa.Area)
                        .ThenInclude(aaa => aaa.AlternativeArea)
                        .ThenInclude(aab => aab.Layout)
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area)
                        .ThenInclude(bba => bba.Layout)
                .Include(a => a.Process)
                    .ThenInclude(c => c.Flow)
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Inbounds with the Id was not found: {processId}");
        }

        //Para Flow        
        public InboundFlowGraph? GetInboundFlowGraphById(Guid id)
        {
            return _context.InboundFlowGraphs
                .Include(i => i.Warehouse)
                .Where(i => i.Id == id)
                .AsNoTracking()
                .FirstOrDefault();
        }

        public InboundFlowGraph? GetInboundFlowGraphByWarehouseid(Guid Warehouseid)
        {
            return _context.InboundFlowGraphs
                .Include(i => i.Warehouse)
                    .ThenInclude(s => s.Organization)
                .Include(i => i.DockSelectionStrategy)
                    .ThenInclude(s => s.Organization)
                .Where(i => i.WarehouseId == Warehouseid)
                .AsNoTracking()
                .FirstOrDefault();
        }

        public IEnumerable<InboundFlowGraph> GetInboundFlowGraphsByWarehouseId(Guid warehouseId)
        {
            return _context.InboundFlowGraphs
                .Include(i => i.Warehouse)
                .Where(i => i.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToList();
        }

        public OutboundFlowGraph? GetOutboundFlowGraphById(Guid id)
        {
            return _context.OutboundFlowGraphs
                .Include(o => o.Warehouse)
                .Where(o => o.Id == id)
                .AsNoTracking()
                .FirstOrDefault();
        }

        public CustomFlowGraphDto? GetCustomFlowGraphDtoById(Guid id)
        {
            return _context.CustomFlowGraphs.Include(x => x.Flow)
                .Where(o => o.Id == id)
                .Select(z => new CustomFlowGraphDto
                {
                    Id = z.Id,
                    Name = z.Name,
                    FlowId = z.FlowId,
                    WarehouseId = z.Flow.WarehouseId,
                })
                .FirstOrDefault();
        }


        public OutboundFlowGraph? GetOutboundFlowGraphByWarehouseId(Guid Warehouseid)
        {
            return _context.OutboundFlowGraphs
                .Include(o => o.Warehouse).Include(i => i.DockSelectionStrategy)
                .Where(o => o.WarehouseId == Warehouseid)
                .AsNoTracking()
                .FirstOrDefault();
        }

        public CustomFlowGraph? GetCustomFlowGraphByWarehouseId(Guid Warehouseid)
        {
            return _context.CustomFlowGraphs.Include(x => x.Flow)
                .Where(o => o.Flow.WarehouseId == Warehouseid)
                .AsNoTracking()
                .FirstOrDefault();
        }



        public IEnumerable<OutboundFlowGraph> GetOutboundFlowGraphsByWarehouseId(Guid warehouseId)
        {
            return _context.OutboundFlowGraphs
                .Include(o => o.Warehouse)
                .Where(o => o.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToList();
        }

        public DockSelectionStrategy? GetDockSelectionStrategyById(Guid id)
        {
            return _context.DockSelectionStrategies
                .AsNoTracking()
                .FirstOrDefault();
        }

        public DockSelectionStrategy? GetDockSelectionStrategyBywarehouseId(Guid warehouseId)
        {

            var org = _context.Warehouses
                .AsNoTracking()
                .Where(w => w.Id == warehouseId)
                .Select(w => w.OrganizationId)
                .FirstOrDefault();

            return _context.DockSelectionStrategies
                 .Include(o => o.Organization)
                .AsNoTracking()
                .Where(s => s.OrganizationId == org)
                .FirstOrDefault();
        }

        public IEnumerable<DockSelectionStrategy> GetDockSelectionStrategiesByWarehouseId(Guid warehouseId)
        {
            var organizationId = _context.Warehouses
                .AsNoTracking()
                .Where(w => w.Id == warehouseId)
                .Select(w => w.OrganizationId)
                .FirstOrDefault();

            if (organizationId == Guid.Empty) return Enumerable.Empty<DockSelectionStrategy>();

            return _context.DockSelectionStrategies
                 .Include(o => o.Organization)
                .AsNoTracking()
                .Where(s => s.OrganizationId == organizationId)
                .ToList();
        }

        public Step GetStepByStepIdAsNoTracking(Guid stepId)
        {
            return _context.Steps.AsNoTracking().FirstOrDefault(x => x.Id == stepId) ?? throw new KeyNotFoundException($"A Step with the Id was not found: {stepId}");
        }

        public Step GetStepsWithProcessByProcessIdNoTracking(Guid processId)
        {
            return _context.Steps.Include(x => x.Process).AsNoTracking().FirstOrDefault(x => x.Id == processId) ?? throw new KeyNotFoundException($"A Step with the Id was not found: {processId}");
        }

        public IEnumerable<Step> GetStepsListWithProcessByProcessIdNoTracking(Guid processId)
        {
            return _context.Steps.Include(x => x.Process).AsNoTracking().Where(x => x.ProcessId == processId) ?? throw new KeyNotFoundException($"A Step with the Id was not found: {processId}");
        }

        public Replenishment GetReplenishmentByIdWithProcessAsNoTracking(Guid Id)
        {
            return _context.Replenishments
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Replenishment with the Id was not found: {Id}");
        }

        public IEnumerable<PackingMode> GetPackModes()
        {
            return _context.PackingMode.AsNoTracking();
        }

        public Packing GetPackingByIdWithProcessAsNoTracking(Guid Id)
        {
            return _context.Packing
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Packing with the Id was not found: {Id}");
        }

        public IEnumerable<PackingPacksMode> GetPackModesByPackId(Guid id)
        {
            return _context.PackingPacksMode.Where(x => x.PackingId == id).Include(x => x.PackingMode).AsNoTracking();
        }

        public Replenishment GetReplenishmentByProcessIdAsNoTracking(Guid processId)
        {
            return _context.Replenishments
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Replenishment with the Id was not found: {processId}");
        }

        public Packing GetPackingByProcessIdAsNoTracking(Guid processId)
        {
            return _context.Packing
                .Include(a => a.Process) // Gets Process
                    .ThenInclude(aa => aa.Area) // Gets Area from Process
                        .ThenInclude(aaa => aaa.AlternativeArea) // Gets AlternativeArea from Area
                        .ThenInclude(aab => aab.Layout) // Gets Layout from AlternativeArea
                .Include(a => a.Process)
                    .ThenInclude(bb => bb.Area) // Gets Area from Process
                        .ThenInclude(bba => bba.Layout) // Gets Layout from Area
                .AsNoTracking().FirstOrDefault(x => x.ProcessId == processId)
           ?? throw new KeyNotFoundException($"A Replenishment with the Id was not found: {processId}");
        }

        public IEnumerable<Replenishment?> GetReplenishmentByIdProcess(Guid processId)
        {
            return _context.Replenishments.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public IEnumerable<Packing?> GetPackingByIdProcess(Guid processId)
        {
            return _context.Packing.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }


        public Flow? GetFlowById(Guid flowId)
        {
            return _context.Flow.FirstOrDefault(x => x.Id == flowId);
        }

        public InboundFlowGraph? GetInboundFlowGraphByFlowId(Guid flowId)
        {
            return _context.InboundFlowGraphs.FirstOrDefault(x => x.FlowId == flowId);
        }

        public OutboundFlowGraph? GetOutboundFlowGraphByFlowId(Guid flowId)
        {
            return _context.OutboundFlowGraphs.FirstOrDefault(x => x.FlowId == flowId);
        }

        public CustomFlowGraph? GetCustomFlowGraphByFlowId(Guid flowId)
        {
            return _context.CustomFlowGraphs.FirstOrDefault(x => x.FlowId == flowId);
        }

        public bool HasProcessDependenciesToFlow(Guid flowId)
        {
            return _context.Processes.Any(p => p.FlowId == flowId);
        }

        #endregion

        public void AddUserToWarehouse(Guid userId, Guid warehouseId)
        {
            var warehouse = _context.Warehouses.Include(w => w.Users).FirstOrDefault(w => w.Id == warehouseId);

            if (warehouse == null) return;

            var user = _context.Users.Include(w => w.Warehouses).FirstOrDefault(u => u.Id == userId);

            if (user == null) return;

            warehouse.Users.Add(user);
            _context.SaveChanges();
        }


        public void RemoveUserFromWarehouse(Guid userId, Guid warehouseId)
        {
            var warehouse = _context.Warehouses.Include(w => w.Users).FirstOrDefault(w => w.Id == warehouseId);

            if (warehouse == null) return;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null) return;
            if (!warehouse.Users.Contains(user)) return;

            warehouse.Users.Remove(user);
            _context.SaveChanges();
        }

        public void UpdateUserToWarehouse(Guid userId, List<Guid> warehouseIds)
        {
            var user = _context.Users
                       .Include(u => u.Warehouses)
                       .FirstOrDefault(u => u.Id == userId);


            if (user == null) return;

            if (user.Warehouses == null)
            {
                user.Warehouses = new List<Warehouse>();
            }

            var warehousesToRemove = user.Warehouses
                .Where(w => !warehouseIds.Contains(w.Id))
                .ToList();

            if (warehousesToRemove.Any())
            {
                foreach (var oldWarehouse in warehousesToRemove)
                {
                    user.Warehouses.Remove(oldWarehouse);
                }
            }

            var existingWarehouseIds = user.Warehouses.Select(w => w.Id).ToList();

            var warehousesToAdd = _context.Warehouses
                .Where(w => warehouseIds.Contains(w.Id) && !existingWarehouseIds.Contains(w.Id))
                .ToList();

            foreach (var warehouse in warehousesToAdd)
            {
                user.Warehouses.Add(warehouse);
            }

            _context.SaveChanges();
        }

        public void AddRolProcessSequenceSaveChanges(Rol rol, List<RolProcessSequence> rolProcessSequence)
        {
            _context.Roles.Add(rol);
            AddRolProcessSequence(rolProcessSequence);
            _context.SaveChanges();
        }

        private void AddRolProcessSequence(List<RolProcessSequence> rolProcessSequence)
        {
            foreach (var item in rolProcessSequence)
                _context.RolProcessSequences.Add(item);
        }

        public void UpdateRolProcessSequenceSaveChanges(Rol rol, List<RolProcessSequence> rolProcessSequence)
        {
            var updateRol = _context.Roles.Find(rol.Id);
            if (updateRol != null)
                updateRol.Name = rol.Name;

            EditRolSaveChanges(rol);
            DeleteProcessSequenceSaveChanges(rol);
            AddRolProcessSequence(rolProcessSequence);

            _context.SaveChanges();
        }


        public void EditRolSaveChanges(Rol rol) => _context.SaveChanges();


        public void DeleteProcessSequenceSaveChanges(Rol rol)
        {
            List<RolProcessSequence> rolProcessList = _context.RolProcessSequences.Where(x => x.RolId == rol.Id)
                 .Include(r => r.Rol)
                 .Include(p => p.Process).ToList();

            foreach (var item in rolProcessList)
                _context.RolProcessSequences.RemoveRange(item);

            _context.SaveChanges();
        }


        public void DeleteRolProcessSequenceSaveChanges(Rol rol)
        {
            DeleteProcessSequenceSaveChanges(rol);

            Rol DeleteITem = _context.Roles.FirstOrDefault(x => x.Id == rol.Id);
            _context.Roles.RemoveRange(DeleteITem);

            _context.SaveChanges();
        }

        /// <summary>
        /// Método para obtener un Custome Process
        /// </summary>
        /// <param name="processId">Guid correspondiente el registro de BD</param>
        /// <returns>Buffer</returns>
        public IEnumerable<CustomProcess?> GetCustomProcessByIdProcess(Guid processId)
        {
            return _context.CustomProcesses.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }

        public CustomProcess GetCustomeProcesById(Guid Id)
        {
            return _context.CustomProcesses.AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A CustomProcesses with the Id was not found: {Id}");
        }

        public Inbound GetInboundById(Guid Id)
        {
            return _context.Inbounds.AsNoTracking().FirstOrDefault(x => x.Id == Id)
           ?? throw new KeyNotFoundException($"A Inbound with the Id was not found: {Id}");
        }

        public IEnumerable<Inbound?> GetInboundByIdProcess(Guid processId)
        {
            return _context.Inbounds.Where(e => e.ProcessId == processId).AsNoTracking().ToList();
        }
        public void WorkerProfileInsert(ResourceWorkerScheduleDto resourceWorkerDto)
        {
            try
            {
                Worker worker = new()
                {
                    Id = resourceWorkerDto.WorkerId,
                    Name = resourceWorkerDto.Name,
                    WorkerNumber = resourceWorkerDto.WorkerNumber,
                    RolId = resourceWorkerDto.Rol.Id,
                    TeamId = resourceWorkerDto.Team.Id,
                    Rol = null,
                    Team = null
                };
                _context.Workers.Add(worker);

                AvailableWorker availableWorker = new()
                {
                    Id = Guid.NewGuid(),
                    Name = resourceWorkerDto.Name,
                    WorkerId = resourceWorkerDto.WorkerId,
                    Worker = null,
                };
                _context.AvailableWorkers.Add(availableWorker);

                var breakProfile = _context.BreakProfiles.FirstOrDefault(x => x.Id == resourceWorkerDto.Break.Id).Id;

                Schedule schedule = new()
                {
                    Id = Guid.NewGuid(),
                    Name = $"{resourceWorkerDto.Shift.Name}-{resourceWorkerDto.Break.Name}-{resourceWorkerDto.Name}",
                    Date = DateTime.Now,
                    AvailableWorkerId = availableWorker.Id,
                    ShiftId = resourceWorkerDto.Shift.Id,
                    BreakProfileId = breakProfile,
                    Available = resourceWorkerDto.Available,
                    Shift = null,
                    BreakProfile = null,
                    AvailableWorker = null


                };
                _context.Schedules.Add(schedule);

                _context.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void WorkerProfileUpdate(ResourceWorkerScheduleDto resourceWorkerDto)
        {
            try
            {

                var worker = _context.Workers.FirstOrDefault(x => x.Id == resourceWorkerDto.WorkerId);

                if (worker != null)
                {

                    worker.Name = resourceWorkerDto.Name;
                    worker.WorkerNumber = resourceWorkerDto.WorkerNumber;
                    worker.RolId = resourceWorkerDto.Rol.Id;
                    worker.TeamId = resourceWorkerDto.Team.Id;
                }

                var schedule = _context.Schedules.FirstOrDefault(x => x.Id == resourceWorkerDto.Id);

                if (schedule != null)
                {
                    schedule.Name = $"{resourceWorkerDto.Shift.Name}-{resourceWorkerDto.Break.Name}-{resourceWorkerDto.Name}";
                    schedule.BreakProfileId = resourceWorkerDto.Break.Id;
                    schedule.ShiftId = resourceWorkerDto.Shift.Id;
                    schedule.Available = resourceWorkerDto.Available;
                }

                _context.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void WorkerProfileDelete(ResourceWorkerScheduleDto resourceWorkerDto)
        {
            try
            {

                var worker = _context.Workers.FirstOrDefault(x => x.Id == resourceWorkerDto.WorkerId);
                var availableWorker = _context.AvailableWorkers.FirstOrDefault(x => x.WorkerId == resourceWorkerDto.WorkerId);
                var schedule = _context.Schedules.FirstOrDefault(x => x.Id == resourceWorkerDto.Id);


                if (schedule != null)
                {
                    _context.Schedules.Remove(schedule);
                }

                if (availableWorker != null)
                {
                    _context.AvailableWorkers.Remove(availableWorker);
                }

                if (worker != null)
                {
                    _context.Workers.Remove(worker);
                }

                _context.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region ExtraConfiguration

        public IEnumerable<Process> GetAllProcess()
        {
            return _context.Processes.Include(x => x.Area).Include(c => c.Flow).AsNoTracking();
        }

        public IEnumerable<Process> GetAllProcessByWarehouse(Guid warehouseId)
        {
            return _context.Processes
            .Include(p => p.Area)
            .ThenInclude(a => a.Layout)
            .ThenInclude(l => l.Warehouse)
            .Include(c => c.Flow)
            .Where(p => p.Area.Layout.Warehouse.Id == warehouseId)
            .AsNoTracking();
        }

        public Process GetProcessById(Guid processId)
        {
            return _context.Processes.Include(x => x.Area).Include(c => c.Flow).FirstOrDefault(x => x.Id == processId);
        }

        public IEnumerable<Area> GetAllAreas()
        {
            return _context.Areas.Include(x => x.Layout).Include(x => x.AlternativeArea).AsNoTracking().ToList();
        }

        public IEnumerable<Layout> GetAllLayouts()
        {
            return _context.Layouts.AsNoTracking().ToList();
        }

        public Area GetAreaById(Guid areaDtoId)
        {
            return _context.Areas.Include(x => x.Layout).AsNoTracking().FirstOrDefault(x => x.Id == areaDtoId);
        }

        public Area GetAreaNoTackById(Guid areaDtoId)
        {
            return _context.Areas.Where(x => x.Id == areaDtoId).AsNoTracking().FirstOrDefault();
        }

        public IEnumerable<Rol> GetAllRoles()
        {
            return _context.Roles.Include(x => x.Warehouse).AsNoTracking();
        }

        public Rol GetRolById(Guid rolDtoId)
        {
            return _context.Roles.Include(x => x.Warehouse).FirstOrDefault(x => x.Id == rolDtoId);
        }

        public Shift GetShiftById(Guid shiftDtoId)
        {
            return _context.Shifts.Include(x => x.Warehouse).FirstOrDefault(x => x.Id == shiftDtoId);
        }

        public IEnumerable<Shift> GetAllShifts()
        {
            return _context.Shifts.Include(x => x.Warehouse).AsNoTracking();
        }

        public IEnumerable<BreakProfile> GetAllBreakProfiles()
        {
            return _context.BreakProfiles.Include(x => x.Warehouse).AsNoTracking();
        }

        public BreakProfile GetBreakProfileById(Guid breakProfileId)
        {
            return _context.BreakProfiles.Include(x => x.Warehouse).FirstOrDefault(x => x.Id == breakProfileId);
        }

        public IEnumerable<Team> GetAllTeams()
        {
            return _context.Teams.Include(x => x.Warehouse).AsNoTracking();
        }

        public IEnumerable<Team> GetTeams(Guid warehouseId)
        {
            return _context.Teams.Where(x => x.WarehouseId == warehouseId).AsNoTracking();
        }

        public Team GetTeamById(Guid teamId)
        {
            return _context.Teams.Include(x => x.Warehouse).FirstOrDefault(x => x.Id == teamId);
        }

        public IEnumerable<Worker> GetAllWorkers()
        {
            return _context.Workers.Include(x => x.Rol).Include(x => x.Team).AsNoTracking();
        }

        public IEnumerable<Worker> GetAllWorkers(Guid warehouseId)
        {
            return _context.Workers
                .Include(x => x.Rol)
                .Include(x => x.Team)
                .Where(x => x.Rol != null && x.Rol.WarehouseId == warehouseId)
                .AsNoTracking();
        }

        public IEnumerable<AvailableWorker> GetAllAvailableWorkers()
        {
            return _context.AvailableWorkers.Include(x => x.Worker);
        }

        public AvailableWorker GetAvailableWorkerById(Guid availableWorkerId)
        {
            return _context.AvailableWorkers.Include(x => x.Worker).FirstOrDefault(x => x.Id == availableWorkerId)!;
        }

        public Worker GetWorkerById(Guid workerId)
        {
            return _context.Workers.Include(x => x.Rol).Include(x => x.Team).FirstOrDefault(x => x.Id == workerId)!;
        }

        public Schedule GetScheduleById(Guid scheduleId)
        {
            return _context.Schedules.Include(x => x.AvailableWorker)
                .Include(x => x.Shift).Include(x => x.BreakProfile).FirstOrDefault(x => x.Id == scheduleId)!;
        }

        public List<Schedule> GetAllSchedule(Guid? TemporalidadId)
        {
            List<Schedule> Schedules = _context.Schedules.Include(x => x.AvailableWorker).Include(x => x.Shift).Include(x => x.BreakProfile).ToList();

            if (_context.ConfigurationSequenceHeaders.Any()) //cuando se borra y no hay temporalidades anteriores
            {
                if (_context.ConfigurationSequenceHeaders.Any(m => m.Id == TemporalidadId)) //cuando se borra y hay temporalidades anteriores
                {
                    var actions = JsonConvert.DeserializeObject<Actions>(_context.ConfigurationSequenceHeaders.First(m => m.Id == TemporalidadId).Data);

                    foreach (var NewConfig in actions.New.Where(m => m.Key == "Schedule").Select(m => m.Value)) //cuando se borra una temporalidad con una actualización
                    {
                        if (NewConfig.Any())
                        {
                            var serializeValue = JsonConvert.SerializeObject(NewConfig);
                            Schedule NewSchedule = JsonConvert.DeserializeObject<List<Schedule>>(serializeValue).First();
                            NewSchedule.AvailableWorker = _context.AvailableWorkers.FirstOrDefault(m => m.Id == NewSchedule.AvailableWorkerId);
                            NewSchedule.Shift = _context.Shifts.FirstOrDefault(m => m.Id == NewSchedule.ShiftId);
                            NewSchedule.BreakProfile = _context.BreakProfiles.FirstOrDefault(m => m.Id == NewSchedule.BreakProfileId);
                            Schedules.Add(NewSchedule);
                        }
                    }
                }
            }

            return Schedules;
        }

        /// <summary>
        /// Método para obtener los almacenes a partir de una organización.
        /// </summary>
        /// /// <param name="OrganizationId">Identificador de la organización.</param>
        /// <returns>IEnumerable de la lista de almacenes que corresponden a una organización concreta.</returns>
        public Warehouse GetWarehouseById2(Guid warehouseId)
        {
            return _context.Warehouses.Where(x => x.Id == warehouseId).Include(x => x.TimeZone_).Include(x => x.Country).AsNoTracking().FirstOrDefault()!;
        }

        /// <summary>
        /// Método para obtener los almacenes a partir de una organización.
        /// </summary>
        /// /// <param name="OrganizationId">Identificador de la organización.</param>
        /// <returns>IEnumerable de la lista de almacenes que corresponden a una organización concreta.</returns>
        public Warehouse GetWarehouseById(Guid warehouseId)
        {
            return _context.Warehouses.Where(x => x.Id == warehouseId).AsNoTracking().FirstOrDefault()!;
        }

        public Rol GetRolAsNoTrackedById(Guid rolDtoId)
        {
            return _context.Roles.Where(x => x.Id == rolDtoId).AsNoTracking().FirstOrDefault();
        }

        public Team GetTeamAsNoTrackedById(Guid teamIs)
        {
            return _context.Teams.Where(x => x.Id == teamIs).AsNoTracking().FirstOrDefault();
        }

        public AvailableWorker GetAllAvailableWorkeAsNotracking(Guid id)
        {
            return _context.AvailableWorkers.Where(x => x.Id == id).AsNoTracking().FirstOrDefault()!;
        }

        public BreakProfile GetBreakProfileAsNoTracking(Guid id)
        {
            return _context.BreakProfiles.Where(x => x.Id == id).AsNoTracking().FirstOrDefault()!;
        }

        public Shift GetShiftAsNoTracking(Guid id)
        {
            return _context.Shifts.Where(x => x.Id == id).AsNoTracking().FirstOrDefault()!;
        }

        public Worker GetWorkerAsNoTrackingById(Guid workerId)
        {
            return _context.Workers.Where(x => x.Id == workerId).AsNoTracking().FirstOrDefault()!;
        }

        public List<OrderSchedule> GetAllOrderSchedules()
        {
            return _context.OrderSchedules.Include(x => x.Load).Include(x => x.Vehicle).Include(x => x.Warehouse).AsNoTracking().ToList();
        }
        public LoadProfile GetLoadFromDto(Guid workerId)
        {
            return _context.LoadProfiles.Where(x => x.Id == workerId).AsNoTracking().FirstOrDefault()!;
        }

        public VehicleProfile GetVehicleFromDto(Guid workerId)
        {
            return _context.VehicleProfiles.Where(x => x.Id == workerId).AsNoTracking().FirstOrDefault()!;
        }


        #endregion

        public IEnumerable<Route> GetRoutesConfig()
        {
            return _context.Routes.Include(m => m.DepartureArea)
                    .Include(m => m.ArrivalArea).AsNoTracking();
        }

        public IEnumerable<Aisle> GetAislesConfig() => _context.Aisles.Include(m => m.Zone).AsNoTracking();
        public IEnumerable<Buffer> GetBuffersConfig() => _context.Buffers.Include(m => m.Zone).AsNoTracking();
        public IEnumerable<Dock> GetDockersConfig() => _context.Docks.Include(m => m.Zone).AsNoTracking();
        public IEnumerable<TypeEquipment> GetAllTypeEquipments() => _context.TypeEquipment.Include(m => m.Warehouse).AsNoTracking();
        public IEnumerable<Zone> GetStationsConfig() => _context.Zones.Include(m => m.Area).AsNoTracking();
        public IEnumerable<EquipmentGroup> GetEquipmentGroupConfig() => _context.EquipmentGroups.Include(m => m.Area).Include(m => m.TypeEquipment).AsNoTracking();
        public IEnumerable<InboundFlowGraph> GetInboundFlowGraphsConfig() => _context.InboundFlowGraphs.Include(m => m.Warehouse).AsNoTracking();
        public IEnumerable<OutboundFlowGraph> GetOutboundFlowGraphsConfig() => _context.OutboundFlowGraphs.Include(m => m.Warehouse).AsNoTracking();
        public TypeEquipment GetEquipmentTById(Guid TypeEquipmentId) => _context.TypeEquipment.FirstOrDefault(x => x.Id == TypeEquipmentId);
        public IEnumerable<ProcessDirectionProperty> GetProcessDirectionProperty() => _context.ProcessDirectionProperties.Include(x => x.InitProcess).Include(x => x.EndProcess).AsNoTracking();
        public IEnumerable<Picking> GetPickingProfiles() => _context.Pickings.Include(x => x.Process).AsNoTracking();
        public IEnumerable<Rack> GetRacks() => _context.Racks.Include(x => x.Zone).AsNoTracking();
        public IEnumerable<ChaoticStorage> GetChaoticStorages() => _context.ChaoticStorages.Include(x => x.Zone).AsNoTracking();
        public IEnumerable<AutomaticStorage> GetAutomaticStorages() => _context.AutomaticStorages.Include(x => x.Zone).AsNoTracking();
        public IEnumerable<Break> GetAllBreaks() => _context.Breaks.Include(x => x.BreakProfile).AsNoTracking();
        public IEnumerable<Break> GetAllBreaks(Guid warehouseId) => _context.Breaks.Where(x => x.BreakProfile.WarehouseId == warehouseId).Include(x => x.BreakProfile).AsNoTracking();
        public IEnumerable<BreakProfile> GetAllBreakProfiles(Guid warehouseId) => _context.BreakProfiles.Where(x => x.WarehouseId == warehouseId).AsNoTracking();
        public IEnumerable<Replenishment> GetReplenishments() => _context.Replenishments.Include(x => x.Process).AsNoTracking();
        public IEnumerable<Shipping> GetShippings() => _context.Shippings.Include(x => x.Process).AsNoTracking();
        public IEnumerable<RolProcessSequence> GetRolProcessSequences() => _context.RolProcessSequences.Include(x => x.Rol).Include(x => x.Process).AsNoTracking();
        public IEnumerable<PreprocessProfile> GetPreprocessProfiles() => _context.PreprocessProfiles.Include(x => x.Warehouse).AsNoTracking();
        public IEnumerable<PostprocessProfile> GetPostprocessProfiles() => _context.PostprocessProfiles.Include(x => x.Warehouse).AsNoTracking();

        public LoadProfile GetLoadProfilesCatalogue(Guid Id) => _context.LoadProfiles.FirstOrDefault(x => x.Id == Id);
        public VehicleProfile GetVehicleProfileCatalogue(Guid Id) => _context.VehicleProfiles.FirstOrDefault(x => x.Id == Id);
        public IEnumerable<OrderLoadRatio> GetOrderLoadRatios() => _context.OrderLoadRatios.Include(x => x.Load).Include(x => x.Vehicle).AsNoTracking();
        public IEnumerable<OrderLoadRatio> GetOrderLoadRatios(Guid warehouseId) => _context.OrderLoadRatios.Include(x => x.Load).Include(x => x.Vehicle).Where(x => x.Vehicle.WarehouseId == warehouseId).AsNoTracking();

        public IEnumerable<LoadProfile> GetAllLoadProfiles() => _context.LoadProfiles.Include(x => x.Warehouse).AsNoTracking();
        public IEnumerable<VehicleProfile> GetAllVehicleProfiles() => _context.VehicleProfiles.Include(x => x.Warehouse).AsNoTracking();
        public IEnumerable<Step> GetSteps() => _context.Steps.Include(x => x.Process).AsNoTracking();
        public IEnumerable<Loading> GetAllLoadings() => _context.Loadings.Include(x => x.Process).AsNoTracking();
        public IEnumerable<Inbound> GetAllInbounds() => _context.Inbounds.Include(x => x.Process).AsNoTracking();
        public IEnumerable<Putaway> GetAllPutAways() => _context.Putaways.Include(x => x.Process).AsNoTracking();
        public IEnumerable<Reception> GetAllReceptions() => _context.Receptions.Include(x => x.Process).AsNoTracking();
        public IEnumerable<PutawayProfile> GetAllPutawayProfiles() => _context.PutawayProfiles.Include(x => x.Warehouse).AsNoTracking();
        public IEnumerable<ProcessHour> GetProcessHours() => _context.ProcessHours.Include(x => x.Process).AsNoTracking();
        public User GetUserByCode(string code) => _context.Users.AsNoTracking().Include(x => x.Warehouses).Include(l => l.Language).FirstOrDefault(x => x.Code == code);

        // TODO: Revisar mantenimiento de historicos
        public bool IsTaskStillPrepared(Guid order)
        {
            var existOrder = _context.WorkOrdersPlanning.FirstOrDefault(y => y.Id == order) != null;
            if (!existOrder) return false;
            var isOrderInInputs = _context.InputOrders.Any(x => _context.WorkOrdersPlanning.FirstOrDefault(y => y.Id == order).InputOrderId == x.Id);
            if (!isOrderInInputs) return false;

            var orderPrepared = _context.InputOrders.FirstOrDefault(x => _context.WorkOrdersPlanning.FirstOrDefault(y => y.Id == order).InputOrderId == x.Id);
            var statusIsPrepared = orderPrepared.Status == InputOrderStatus.Waiting;
            return statusIsPrepared;
        }


        public class Actions
        {
            public Dictionary<string, List<Dictionary<string, object>>> New { get; set; }
            public Dictionary<string, List<Dictionary<string, object>>> Update { get; set; }
            public Dictionary<string, List<Dictionary<string, object>>> Delete { get; set; }
        }

        /// <summary>
        /// Lista  las configuraciones de las alertas
        /// </summary>
        /// <param name="warehosueId"></param>
        /// <returns></returns>
        public IEnumerable<Alert> GetAlertManagement() => _context.Alerts.Include(w => w.Warehouse).AsNoTracking();

        /// <summary>
        /// Eliminar todos los filtros de una alerta
        /// </summary>
        /// <returns></returns>
        public void AlertFilterRemoveAll(Guid AlertId)
        {
            var alertFilters = _context.AlertFilters.Where(x => x.AlertId == AlertId);
            _context.AlertFilters.RemoveRange(alertFilters);

            _context.SaveChanges();
        }
        /// <summary>
        /// Remove all configurations from an alert
        /// </summary>
        /// <returns></returns>
        public void AlertConfigurationRemoveAll(Guid AlertId)
        {
            var aConfs = _context.AlertConfigurations.Where(x => x.AlertId == AlertId);
            _context.AlertConfigurations.RemoveRange(aConfs);

            _context.SaveChanges();
        }
        public void AddAlertConfiguration(AlertConfiguration conf)
        {
            _context.AlertConfigurations.Add(conf);

            _context.SaveChanges();
        }

        public IEnumerable<SLAConfig> GetAllSLAConfigs() => _context.SLAConfigs.AsNoTracking();

        public IEnumerable<ProcessPriorityOrder> GetAllProcessPriorityOrderByWh() => _context.ProcessPriorityOrder.AsNoTracking();

        public IEnumerable<OrderPriority> GetAllOrderPriorityByWh() => _context.OrderPriority.AsNoTracking();

        public IEnumerable<AlertResponse> GetAllAlertResponsesByPlanningId(Guid PlanningId)
        {
            return _context.AlertResponses.Where(a => a.PlanningId == PlanningId).Include(a => a.Alert).ThenInclude(a => a.Configurations).AsNoTracking();
        }

        public IEnumerable<AlertResponse> GetAllAlertResponses()
        {
            var query = from Alerts in _context.AlertResponses
                        .Include(a => a.Alert)
                        .Include(a => a.Planning)
                        .AsNoTracking()
                        where _context.Plannings
                            .Where(p => p.WarehouseId == Alerts.Planning.WarehouseId)
                            .OrderByDescending(p => p.CreationDate)
                            .Select(p => p.Id)
                            .FirstOrDefault() == Alerts.PlanningId
                        select Alerts;

            return query;
        }

        public WorkOrderPlanning GetWorkOrderPlanningById(Guid workOrderId)
        {
            return _context.WorkOrdersPlanning.Include(o => o.InputOrder).AsNoTracking().FirstOrDefault(o => o.Id == workOrderId);
        }

        /// <summary>
        /// Obtiene una colección de objetos que coinciden con los identificadores proporcionados.
        /// </summary>
        /// <param name="workerOrderIds"Lista de Guids únicos de WorkerOrderPlanning a consultar.></param>
        /// <returns>Lista de objetos  que corresponden a los IDs proporcionados.</returns>
        /// <returns>Lista de objetos  que corresponden a los IDs proporcionados.</returns>
        public IEnumerable<WorkOrderPlanning> GetWorkOrderPlanningsByIds(List<Guid> workerOrderIds)
        {
            return _context.WorkOrdersPlanning
                .Include(o => o.InputOrder)
                .Where(o => workerOrderIds.Contains(o.Id))
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<WFMLaborWorker> GetWFMLaborsByPlanningId(Guid planningId)
        {
            return _context.WFMLaborWorker
                .Include(x => x.Planning)
                .Include(x => x.Worker).ThenInclude(x => x.Rol)
                .Include(x => x.Worker).ThenInclude(x => x.Team)
                .Include(x => x.Schedule).ThenInclude(x => x.Shift)
                .Where(x => x.PlanningId == planningId)
                .AsNoTracking();
        }

        public IEnumerable<WFMLaborItemPlanning> GetWFMLaborItemPlanningByPlanningId(Guid planningId)
        {
            return _context.WFMLaborItemPlanning
                .Include(x => x.InputOrder)
                .Include(x => x.WFMLaborPerProcessType).ThenInclude(x => x.WFMLaborPerFlow)
                .Where(x => x.WFMLaborPerProcessType.PlanningId == planningId)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<WFMLaborItemPlanning> GetWFMLaborItemPlanningEquipmentByPlanningId(Guid planningId)
        {
            return _context.WFMLaborItemPlanning
                .Include(x => x.InputOrder)
                .Include(x => x.WFMLaborEquipmentPerProcessType).ThenInclude(x => x.WFMLaborEquipmentPerFlow)
                .Where(x => x.WFMLaborEquipmentPerProcessType.PlanningId == planningId)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<WFMLaborEquipment> GetWFMLaborEquipmentsByPlanningId(Guid planningId)
        {
            return _context.WFMLaborEquipment
                .Include(x => x.Planning)
                .Include(x => x.EquipmentGroup)
                .Include(x => x.TypeEquipment)
                .Where(x => x.PlanningId == planningId);
        }

        public IEnumerable<Break> GetBreakByBreakprofilesId(List<Guid> breakProfiles)
        {
            return _context.Breaks.Where(x => breakProfiles.Contains(x.BreakProfileId)).AsNoTracking();
        }

        public IEnumerable<CalendarTaskDto> GetCalendarTaskDtos(Guid warehouseId)
        {
            var rawData = _context.ConfigurationSequences
            .Include(cs => cs.ConfigurationSequenceHeader)
            .Where(x => x.ConfigurationSequenceHeader.WarehouseId == warehouseId)
            .ToList();


            var result = rawData
                .GroupBy(cs => cs.Date.Date)
                .SelectMany(g =>
                    g.Select(cs => new CalendarTaskDto
                    {
                        Id = cs.Id,
                        StartDate = cs.Date.Date,
                        EndDate = cs.Date.Date.AddDays(1),
                        Caption = cs.ConfigurationSequenceHeader.Code,
                        IsActive = true,
                        IsConflicting = g.Count() > 1
                    }))
                .ToList();

            return result;

        }

        public List<string> GetViews()
        {
            return new List<string> { "Order", "Priority", "Trailer" };
        }


        public IEnumerable<YardMetricsAppointmentsPerDock> GetYardMetricsAppointmentByPlanningId(Guid planningId)
        {
            return _context.YardMetricsAppointmentsPerDock
                .Include(x => x.Planning)
                .Include(x => x.Dock).ThenInclude(x => x.Zone)
                .Include(x => x.YardMetricsPerDock)
                .Where(x => x.PlanningId == planningId);
        }

        /// <summary>
        /// Obtener una lista de los codigo del vehiculo 
        /// </summary>
        /// <param name="licences"></param>
        /// <returns></returns>
        public Dictionary<string, string?> GetVehicleCodeByLicences(IList<string> licences)
        {
            return _context.YardAppointmentsNotifications
                .Where(x => licences.Contains(x.License))
                .GroupBy(x => x.License)
                .ToDictionary(d => d.Key!,
                    d => d.Select(x => x.VehicleCode).FirstOrDefault());
        }

        public double? GetPlanning(Guid planningId, Guid warehouseId)
        {
            return _context.Plannings.Where(x => x.Id == planningId && x.WarehouseId == warehouseId)
                .Select(x => (double?)x.SLAWorkOrdersOnTimePercentage)
                .FirstOrDefault();
        }

        public double? GetLastPlanning(Guid warehouseId)
        {
            return _context.Plannings.Where(x => x.WarehouseId == warehouseId)
                .OrderByDescending(x => x.CreationDate)
                .Select(x => (double?)x.SLAWorkOrdersOnTimePercentage)
                .FirstOrDefault();
        }

        public List<FlowDto> GetFlowsByWarehouseId(Guid warehouseId, List<DTO.Enums.Designer.ObjectsTypes.FlowType> flowOrder)
        {

            var of = _context.OutboundFlowGraphs.AsNoTracking().Where(x => x.WarehouseId == warehouseId)
                .Select(x => new FlowDto
                {
                    WarehouseId = warehouseId,
                    Id = x.Id,
                    Name = x.Name,
                    Type = DTO.Enums.Designer.ObjectsTypes.FlowType.Outbound
                });

            var ifw = _context.InboundFlowGraphs.AsNoTracking().Where(x => x.WarehouseId == warehouseId)
                .Select(x => new FlowDto
                {
                    WarehouseId = warehouseId,
                    Id = x.Id,
                    Name = x.Name,
                    Type = DTO.Enums.Designer.ObjectsTypes.FlowType.Inbound
                });

            var cf = _context.CustomFlowGraphs.AsNoTracking().Include(x => x.Flow).Where(x => x.Flow.WarehouseId == warehouseId)
                .Select(x => new FlowDto
                {
                    WarehouseId = warehouseId,
                    Id = x.Id,
                    Name = x.Name,
                    Type = DTO.Enums.Designer.ObjectsTypes.FlowType.Custom
                });

            return of.Concat(ifw).Concat(cf).OrderBy(x => flowOrder.IndexOf(x.Type ?? DTO.Enums.Designer.ObjectsTypes.FlowType.Inbound)) // Order by type
             .ThenBy(x => x.Name) // Alphabetical order within each type
             .ToList();
        }

        public List<AreaTabDto> GetAreaTabsByLayoutId(Guid layoutId, List<string> areaOrder)
        {
            return _context.Areas.AsNoTracking().Where(x => x.LayoutId == layoutId)
                .OrderBy(x => areaOrder.IndexOf(x.Type)) // Order by type
                .ThenBy(x => x.Name) // Alphabetical order within each type
                .Select(x => new AreaTabDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type
                }).ToList();
        }

        public object GetProcessAndDirectionsByFlow(Guid layoutId, List<Guid> flows)
        {
            // Diccionario FlowId -> Tipo
            var flowTypes = new Dictionary<Guid, string>();

            // Outbound
            var outboundFlows = _context.OutboundFlowGraphs
                .Where(x => flows.Contains(x.Id))
                .Select(x => new { x.Id, x.FlowId })
                .ToList();
            outboundFlows.ForEach(x => flowTypes[x.FlowId ?? Guid.NewGuid()] = "Outbound");

            // Inbound
            var inboundFlows = _context.InboundFlowGraphs
                .Where(x => flows.Contains(x.Id))
                .Select(x => new { x.Id, x.FlowId })
                .ToList();
            inboundFlows.ForEach(x => flowTypes[x.FlowId ?? Guid.NewGuid()] = "Inbound");

            // Custom
            var customFlows = _context.CustomFlowGraphs
                .Where(x => flows.Contains(x.Id))
                .Select(x => new { x.Id, x.FlowId })
                .ToList();
            customFlows.ForEach(x => flowTypes[x.FlowId ?? Guid.NewGuid()] = "Custom");

            var flowIds = flowTypes.Keys.ToList();

            if (!flowIds.Any())
            {
                return new
                {
                    ProcessDirectionIds = new List<Guid>(),
                    ProcessIds = new List<Guid>(),
                    AreaIds = new List<Guid>(),
                    FlowTypes = new Dictionary<Guid, string>()
                };
            }

            // 1. Obtener procesos que pertenezcan a esos FlowIds
            var processes = _context.Processes
                .Where(p => flowIds.Contains(p.FlowId ?? Guid.NewGuid()) && p.Area.LayoutId == layoutId)
                .Select(p => new { p.Id, p.FlowId })
                .ToList();

            var processIds = processes.Select(p => p.Id).ToList();

            // 2. Filtrar ProcessDirectionProperties SOLO por InitProcess
            var result = _context.ProcessDirectionProperties
                .Include(x => x.InitProcess).ThenInclude(x => x.Area)
                .Include(x => x.EndProcess).ThenInclude(x => x.Area)
                .Where(x => processIds.Contains(x.InitProcessId)) // 🔑 Init manda
                .Select(x => new
                {
                    DirectionId = x.Id,
                    ProcessIdI = x.InitProcess.Id,
                    ProcessIdO = x.EndProcess.Id,
                    AreaIdI = x.InitProcess.AreaId,
                    AreaIdO = x.EndProcess.AreaId,
                    FlowId = x.InitProcess.FlowId // para saber su tipo
                })
                .AsNoTracking()
                .ToList();

            // 3. Construcción del resultado
            var directions = result.Select(x => x.DirectionId).Distinct().ToList();
            var allProcessIds = result
                .SelectMany(x => new[] { x.ProcessIdI, x.ProcessIdO })
                .Distinct()
                .ToList();
            var areaIds = result
                .SelectMany(x => new[] { x.AreaIdI, x.AreaIdO })
                .Distinct()
                .ToList();

            // Mapear cada direction a su tipo de flow según InitProcess
            var directionTypes = result.ToDictionary(
                x => x.DirectionId,
                x => flowTypes.ContainsKey(x.FlowId ?? Guid.NewGuid()) ? flowTypes[x.FlowId ?? Guid.NewGuid()] : "Unknown"
            );

            return new
            {
                ProcessDirectionIds = directions,
                ProcessIds = allProcessIds,
                AreaIds = areaIds,
                DirectionTypes = directionTypes,
                FlowTypes = flowTypes
            };
        }


        public List<RouteTabDto> GetRoutesWithAreas(Guid layout)
        {
            return _context.Routes.Include(x => x.ArrivalArea).Include(x => x.DepartureArea).Where(x => x.ArrivalArea.LayoutId == layout)
                .OrderBy(x => x.Name) // Alphabetical order
                .Select(x => new RouteTabDto
                {
                    Id = x.Id,
                    NameFrom = x.ArrivalArea.Name,
                    NameTo = x.DepartureArea.Name,
                }).AsNoTracking().ToList();
        }

        public List<DTO.Enums.Designer.ObjectsTypes.ProcessType> GetProcessTypesFilter(Guid layoutId)
        {
            var usedTypes = _context.Processes.AsNoTracking().Include(p => p.Area).Where(p => p.Area.LayoutId == layoutId).Select(p => GetProcessTypeEnum(p.Type)).Distinct().ToList();
            usedTypes.Sort(); // Orden natural por enum (numérico)
            return usedTypes;
        }

        public IEnumerable<Process> GetProcessesByAreaType(Guid layoutId, AreaType areaType)
        {
            return _context.Processes.Include(p => p.Area).AsNoTracking().Where(p => p.Area.LayoutId == layoutId && p.Area.Type == areaType.ToString()).ToList();
        }

        public List<ParentFlowDto> GetParentFlows(Guid layoutId)
        {
            return _context.Flow.AsNoTracking().Where(x => x.WarehouseId == _context.Layouts.Where(y => y.Id == layoutId).FirstOrDefault().WarehouseId)
                .Select(x => new ParentFlowDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                }).ToList();
        }

        public List<WorkerStops> GetWorkersBySchedule(Guid Warehouse)
        {
            return _context.Schedules.Include(x => x.Shift).Include(x => x.AvailableWorker).ThenInclude(x => x.Worker)
                .Where(x => x.Shift.WarehouseId == Warehouse)
                .Select(x => new WorkerStops
                {
                    shiftName = x.Shift.Name,
                    workerId = x.AvailableWorker.Worker.Id,
                    workerName = x.AvailableWorker.Worker.Name,
                    workerNumber = x.AvailableWorker.Worker.WorkerNumber ?? 0,
                    init = TimeSpan.FromHours(x.Shift.InitHour),
                    end = TimeSpan.FromHours(x.Shift.EndHour)
                }).ToList();
        }

        public List<ShiftRolDto> GetWorkersByShiftAndRol(Guid warehouseId)
        {
            var query = _context.Schedules
                .Where(sc => sc.Shift.WarehouseId == warehouseId)
                .Include(sc => sc.Shift)
                .Include(sc => sc.AvailableWorker)
                    .ThenInclude(aw => aw.Worker)
                        .ThenInclude(w => w.Rol)
                .Select(sc => new
                {
                    ShiftId = sc.Shift.Id,
                    ShiftName = sc.Shift.Name,
                    ShiftInit = sc.Shift.InitHour,
                    ShiftEnd = sc.Shift.EndHour,

                    RolId = sc.AvailableWorker.Worker.Rol.Id,
                    RolName = sc.AvailableWorker.Worker.Rol.Name
                });

            var result = query
                .AsEnumerable()
                .GroupBy(x => new { x.ShiftId, x.ShiftName, x.ShiftInit, x.ShiftEnd })
                .Select(g => new ShiftRolDto
                {
                    id = g.Key.ShiftId,
                    name = g.Key.ShiftName,
                    initHour = TimeSpan.FromHours(g.Key.ShiftInit),
                    endHour = TimeSpan.FromHours(g.Key.ShiftEnd),

                    workersInRol = g
                        .GroupBy(x => new { x.RolId, x.RolName })
                        .Select(r => new WorkersInRolDto
                        {
                            id = r.Key.RolId,
                            name = r.Key.RolName,
                            Workers = r.Count()
                        })
                        .OrderBy(x => x.name)
                        .ToList()
                })
                .OrderBy(s => s.initHour)
                .ToList();

            return result;
        }




        public List<Flow> GetFlowsByWarehouse(Guid warehouseId)
        {
            return _context.Flow
                .Where(f => f.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToList();
        }

        public List<InboundFlowGraph> DeleteInboundFlowGraphsByFlow(Guid flowId, Guid warehouseId)
        {
            return _context.InboundFlowGraphs
                .Where(g => g.FlowId == flowId || g.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToList();
        }

        public List<OutboundFlowGraph> DeleteOutboundFlowGraphsByFlow(Guid flowId, Guid warehouseId)
        {
            return _context.OutboundFlowGraphs
                .Where(g => g.FlowId == flowId || g.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToList();
        }

        public List<CustomFlowGraph> DeleteCustomFlowGraphsByFlow(Guid flowId)
        {
            return _context.CustomFlowGraphs
                .Where(g => g.FlowId == flowId)
                .AsNoTracking()
                .ToList();
        }


        public bool IsPlanningInWarehouse(Guid planningId, Guid warehouseId) => _context.Plannings.Any(p => p.Id == planningId && p.WarehouseId == warehouseId);

        public List<YardDockUsagePerHour> GetDockSaturations(Guid planningId)
            => _context.YardDockUsagePerHour
            .Where(z => z.PlanningId == planningId)
            .Include(x => x.Dock).ThenInclude(x => x.Zone)
            .ToList();

        public List<YardStageUsagePerHour> GetStageSaturations(Guid planningId)
            => _context.YardStageUsagePerHour
            .Where(z => z.PlanningId == planningId)
            .Include(x => x.Stage).ThenInclude(x => x.Zone)
            .ToList();

        public List<AvailableDocksPerStage> GetAvailableDockStageRelations()
            => _context.AvailableDocksPerStages
            .Include(r => r.Dock).ThenInclude(d => d.Zone)
            .Include(r => r.Stage).ThenInclude(s => s.Zone)
            .ToList();

        public IEnumerable<Warehouse> GetWarehouseProjectsByUserId(Guid UserGuid)
        {
            var AvailableLayoutWarehouses = _context.Layouts.Select(x => x.WarehouseId).ToList();
            if (_context.Users.Include(x => x.Warehouses).Where(x => x.Id == UserGuid).FirstOrDefault() is User userdto && userdto.Warehouses.Select(w => w.Id).ToHashSet() is HashSet<Guid> ListGuid)
                return GetWarehouses().Where(x => ListGuid.Contains(x.Id) && !AvailableLayoutWarehouses.Contains(x.Id));
            else
                return new List<Warehouse>();
        }

        public IEnumerable<Warehouse> GetWarehouseEditProjectsByUserId(Guid UserGuid, Guid layoutId)
        {
            var discarGuidConfigs = _context.Layouts.Where(x => x.Id != layoutId && x.WarehouseId != Guid.Empty).Select(x => x.WarehouseId).ToHashSet();
            if (_context.Users.Include(x => x.Warehouses).Where(x => x.Id == UserGuid).FirstOrDefault() is User userdto && userdto.Warehouses.Select(w => w.Id).ToHashSet() is HashSet<Guid> ListGuid)
            {
                ListGuid = new HashSet<Guid>(ListGuid.Except(discarGuidConfigs));
                return GetWarehouses().Where(x => ListGuid.Contains(x.Id));
            }
            else
                return new List<Warehouse>();
        }


        public IEnumerable<Warehouse> GetWarehouseByUserId(Guid UserGuid)
        {
            if (_context.Users.Include(x => x.Warehouses).Where(x => x.Id == UserGuid).FirstOrDefault() is User userdto && userdto.Warehouses.Select(w => w.Id).ToHashSet() is HashSet<Guid> ListGuid)
                return GetWarehouses().Where(x => ListGuid.Contains(x.Id));
            else
                return new List<Warehouse>();
        }


        public IEnumerable<CustomFlowGraph> GetCustomFlowsByFlow(Guid flowId)
        {
            return _context.CustomFlowGraphs
                .Where(cfg => cfg.FlowId == flowId)
                .ToList();
        }

        public IEnumerable<OutboundFlowGraph> GetOutboundFlowsByFlow(Guid flowId)
        {
            return _context.OutboundFlowGraphs
                .Where(of => of.FlowId == flowId)
                .ToList();
        }

        public IEnumerable<InboundFlowGraph> GetInboundFlowsByFlow(Guid flowId)
        {
            return _context.InboundFlowGraphs
                .Where(inf => inf.FlowId == flowId)
                .ToList();
        }

        public List<Process> getTmpProcess(Guid LayoutId)
        {
            return _context.Processes
            .Include(p => p.ProcessDirectionPropertiesEntry)
            .Include(p => p.Area)
            .Where(x => x.Area.LayoutId == LayoutId)
            .AsNoTracking()
            .ToList();
        }



        public List<Area> getTmpArea(Guid LayoutId)
        {
            return _context.Areas.Where(x => x.LayoutId == LayoutId)
            .Include(a => a.Layout)
            .AsNoTracking()
            .ToList();
        }

        public Layout GetLayoutByLayout(Guid layout)
        {
            // No se edita, por tanto lectura sin tracking
            return _context.Layouts
                .AsNoTracking()
                .First(x => x.Id == layout);
        }

        // ============================================================
        // ==========        PROCESOS Y ÁREAS (TMP)         ==========
        // ============================================================

        public List<Process> GetProcesses(Guid layoutId)
        {
            // Se devuelven sin tracking para evitar carga en cascada de Layout/Warehouse/UserWarehouse
            return _context.Processes
                .Include(p => p.ProcessDirectionPropertiesEntry)
                .Include(p => p.Area)
                .Where(p => p.Area.LayoutId == layoutId)
                .AsNoTracking()
                .ToList();
        }

        public List<Area> GetAreas(Guid layoutId)
        {
            return _context.Areas
                .Include(a => a.Layout)
                .Where(a => a.LayoutId == layoutId)
                .AsNoTracking()
                .ToList();
        }

        public List<Flow> GetFlows(Guid warehouseId)
        {
            return _context.Flow
                .Where(f => f.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToList();
        }

        public List<TypeEquipment> GetTypeEquipments(Guid warehouseId)
        {
            return _context.TypeEquipment
                .Where(t => t.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToList();
        }

        // ============================================================
        // ==========        SUBELEMENTOS RELACIONADOS       ==========
        // ============================================================

        public IEnumerable<Zone> GetZones(Guid areaId)
        {
            return _context.Zones
                .Where(z => z.AreaId == areaId)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<EquipmentGroup> GetEquipmentsLayout(Guid areaId)
        {
            return _context.EquipmentGroups
                .Where(eq => eq.AreaId == areaId)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Route> GetRoutes(Guid areaId)
        {
            return _context.Routes
                .Where(r => r.DepartureAreaId == areaId)
                .AsNoTracking()
                .ToList();
        }

        // ============================================================
        // ==========              GUARDADO                ==========
        // ============================================================

        public bool ExistsProcess(Guid id) =>
            _context.Processes.AsNoTracking().Any(p => p.Id == id);

        public bool ExistsProcessDirection(Guid id) =>
            _context.ProcessDirectionProperties.AsNoTracking().Any(d => d.Id == id);

        public bool ExistsArea(Guid id) =>
            _context.Areas.AsNoTracking().Any(a => a.Id == id);

        public bool ExistsZone(Guid id) =>
            _context.Zones.AsNoTracking().Any(z => z.Id == id);
        public bool ExistsDock(Guid id) =>
            _context.Docks.AsNoTracking().Any(z => z.Id == id);
        public bool ExistsBuffer(Guid id) =>
            _context.Buffers.AsNoTracking().Any(z => z.Id == id);
        public bool ExistsStage(Guid id) =>
            _context.Stages.AsNoTracking().Any(z => z.Id == id);
        public bool ExistsRack(Guid id) =>
            _context.Racks.AsNoTracking().Any(z => z.Id == id);
        public bool ExistsDriveIn(Guid id) =>
            _context.DriveIns.AsNoTracking().Any(z => z.Id == id);
        public bool ExistsChaotic(Guid id) =>
            _context.ChaoticStorages.AsNoTracking().Any(z => z.Id == id);
        public bool ExistsAutomatic(Guid id) =>
            _context.AutomaticStorages.AsNoTracking().Any(z => z.Id == id);

        public bool ExistsEquipment(Guid id) =>
            _context.EquipmentGroups.AsNoTracking().Any(e => e.Id == id);

        public bool ExistsRoute(Guid id) =>
            _context.Routes.AsNoTracking().Any(r => r.Id == id);


        public IEnumerable<ProcessDirectionProperty> GetProcessDirectionPropertyByProcessIdsAsNoTracking(HashSet<Guid> processIds)
        {
            if (processIds == null || processIds.Count == 0)
                return Enumerable.Empty<ProcessDirectionProperty>();

            return _context.ProcessDirectionProperties
                .Include(p => p.InitProcess)
                    .ThenInclude(ip => ip.Area)
                        .ThenInclude(a => a.Layout)
                .Include(p => p.InitProcess)
                    .ThenInclude(ip => ip.Flow)
                .Include(p => p.EndProcess)
                    .ThenInclude(ep => ep.Area)
                        .ThenInclude(a => a.Layout)
                .Include(p => p.EndProcess)
                    .ThenInclude(ep => ep.Flow)
                .Where(p =>
                    processIds.Contains(p.InitProcessId) ||
                    processIds.Contains(p.EndProcessId))
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Transaction> GetTransactions(Guid userId)
        {
            var warehouseCodes = _context.Users.Include(x => x.Warehouses).AsNoTracking().FirstOrDefault(x => x.Id == userId)?.Warehouses.Select(x => x.Code);

            if (warehouseCodes == null || !warehouseCodes.Any()) return _context.TransactionLog.AsNoTracking();

            return _context.TransactionLog.Where(x => warehouseCodes.Contains(x.WarehouseCode)).AsNoTracking();
        }

        public static void UpdatePlanningMode(bool changeValue)
        {
            throw new NotImplementedException();
        }
    }
}
