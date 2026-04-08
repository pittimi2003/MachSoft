using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.WMSSimulator.WMSModel;

namespace Mss.WorkForce.Code.WMSSimulator
{
    public class DataBaseActions
    {
        #region Variables
        public static string WFMConnectionString { get; set; }
        public static string WMSConnectionString { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Gets the necessary data from WFM
        /// </summary>
        /// <returns>The extracted data</returns>
        public static DataBaseResponse GetData()
        {
            DataBaseResponse response = new DataBaseResponse();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(WFMConnectionString);

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                response.Warehouses = context.Warehouses.ToList();
                response.Docks = context.Docks.Include(x => x.Zone).ThenInclude(x => x.Area).ThenInclude(x => x.Layout).ToList();
                response.Processses = context.Processes.Include(x => x.Area.Layout.Warehouse).ToList();
                response.InputOrders = context.InputOrders.ToList();
                response.OrderPriority = context.OrderPriority.Include(x => x.Warehouse).ToList();
                response.OrderSchedules = context.OrderSchedules.Include(x => x.Warehouse).ToList();
                response.Workers = context.Workers.ToList();
                response.EquipmentGroups = context.EquipmentGroups.ToList();
                response.Steps = context.Steps.ToList();
                response.Plannings = context.Plannings.ToList();
                response.WorkOrderPlannings = context.WorkOrdersPlanning.ToList();
                response.ItemPlannings = context.ItemsPlanning.ToList();
                response.Pickings = context.Pickings.ToList();
                response.Zones = context.Zones.ToList();
            }

            return response;
        }

        /// <summary>
        /// Gets the parameter from WMS.Parameters
        /// </summary>
        /// <returns>The parameters for the WMS simulation</returns>
        public static List<Parameter> GetParameters()
        {
            var optionsBuilder = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilder.UseNpgsql(WMSConnectionString);

            using (var context = new WMSDbContext(optionsBuilder.Options))
            {
                List<Parameter> parameters = new List<Parameter>();

                parameters.AddRange(context.Parameters);

                return parameters;
            }

        }

        public static List<Planning> GetTodayPlannings()
        {
            var optionsBuilder = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilder.UseNpgsql(WMSConnectionString);

            using (var context = new WMSDbContext(optionsBuilder.Options))
            {
                List<Planning> plannings = new List<Planning>();

                plannings.AddRange(context.Plannings.Where(x => x.CreationDate.Date == DateTime.UtcNow.Date));

                return plannings;
            }
        }

        /// <summary>
        /// Gets the work force tasks from WMS.WorkForceTasks
        /// </summary>
        /// <returns>The work force tasks for the WMS simulation</returns>
        public static List<WorkForceTask> GetWorkForceTasks(List<Models.Models.Warehouse> warehouses) 
        {
            var optionsBuilder = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilder.UseNpgsql(WMSConnectionString);

            using (var context = new WMSDbContext(optionsBuilder.Options))
            {
                List<WorkForceTask> workForceTasks = new List<WorkForceTask>();

                workForceTasks.AddRange(context.WorkForceTasks.ToList().Join(warehouses, wft => wft.WarehouseId, w => w.Id, (wft, w) => new WorkForceTask
                {
                    Id = wft.Id,
                    InitHour = wft.InitHour,
                    EndHour = wft.EndHour,
                    NumOrders = wft.NumOrders,
                    NumOrdersCompleted = wft.NumOrdersCompleted,
                    LinesPerOrder = wft.LinesPerOrder,
                    IsOut = wft.IsOut,
                    Date = wft.Date,
                    WarehouseId = wft.WarehouseId,
                    WarehouseCode = wft.WarehouseCode
                }));

                return workForceTasks;
            }
        }

        internal static void SetLastInfo(List<Models.Models.Planning> newPlannings, DataBaseResponse wfmData)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilder.UseNpgsql(WMSConnectionString);

            using (var context = new WMSDbContext(optionsBuilder.Options))
            {
                // Esto no va a valer porque son cosas diferentes, uno es un WMSModel.Planning y otro es un Model.Planning, e igual pasa con el resto 

                foreach (var p in newPlannings)
                    context.Plannings.Add(new WMSModel.Planning()
                    {
                        Id = p.Id,
                        Date = p.Date,
                        CreationDate = p.CreationDate,
                        WarehouseId = p.WarehouseId
                    });

                foreach (var planning in newPlannings)
                {
                    var wop = wfmData.WorkOrderPlannings.Where(x => x.PlanningId == planning.Id && !x.IsEstimated);

                    var wopids = wop.Select(x => x.Id).ToList();

                    //Añado los wop
                    foreach (var w in wop)
                        context.WorkOrderPlannings.Add(new WMSModel.WorkOrderPlanning()
                        {
                            Id = w.Id,
                            InitDate = w.InitDate,
                            EndDate = w.EndDate,
                            PlanningId = w.PlanningId,
                            InputOrderId = w.InputOrderId,
                            IsOutbound = w.IsOutbound
                        });

                    //Añado los ip
                    foreach (var ip in wfmData.ItemPlannings.Where(x => wopids.Contains(x.WorkOrderPlanningId)))
                        context.ItemPlannings.Add(new WMSModel.ItemPlanning()
                        {
                            Id = ip.Id,
                            ProcessId = ip.ProcessId,
                            InitDate = ip.InitDate,
                            EndDate = ip.EndDate,
                            WorkOrderPlanningId = ip.WorkOrderPlanningId,
                            WorkerId = ip.WorkerId,
                            EquipmentGroupId = ip.EquipmentGroupId,
                            IsDrawn = false,
                            ZoneId = wfmData.Zones!.First(x => x.AreaId == wfmData.Processses!.First(x => x.Id == ip.ProcessId).AreaId).Id,
                        });
                }

                context.SaveChanges();
            }
        }

        #endregion
    }
}
