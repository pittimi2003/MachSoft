using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.WMSSimulator.WMSModel;
using System.Data;

namespace Mss.WorkForce.Code.WMSSimulator
{
    public class DBWorkForceTask
    {
        public static string WFMConnectionString { get; set; }
        public static string WMSConnectionString { get; set; }

        public class QueryResult
        {
            public TimeSpan InitHour { get; set; }
            public TimeSpan EndHour { get; set; }
            public double NumOrders { get; set; }
            public bool IsOut { get; set; }
            public int AverageLinesPerOrder { get; set; }
            public Guid WarehouseId { get; set; }
            public string WarehouseCode { get; set; }
        }

        public static void DeleteOldData(int days)
        {

            var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilderWMS.UseNpgsql(WMSConnectionString);

            using (var context = new WMSDbContext(optionsBuilderWMS.Options))
            {
                DateTime limitDate = DateTime.UtcNow.AddDays(-days);

                var rowsToDelete = context.WorkForceTasks
                    .Where(m => m.Date < limitDate)
                    .ToList();

                context.WorkForceTasks.RemoveRange(rowsToDelete);
                context.SaveChanges();
            }

        }

        public static void GatherWFData()
        {

            var optionsBuilderWFM = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilderWFM.UseNpgsql(WFMConnectionString);

            var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilderWMS.UseNpgsql(WMSConnectionString);

            List<QueryResult> query;

            using (var contextWMS = new WMSDbContext(optionsBuilderWMS.Options))
            {
                if (contextWMS.WorkForceTasks.Any() &&
                    contextWMS.WorkForceTasks.OrderByDescending(m => m.Date).FirstOrDefault()?.Date >= DateTime.UtcNow.Date)
                {
                    return;
                }
            }

            using (var contextWFM = new ApplicationDbContext(optionsBuilderWFM.Options))
            {

                query = contextWFM.OrderSchedules.Where(m => m.IsOut == true)
                    .Join(
                        contextWFM.OrderLoadRatios.DefaultIfEmpty(),
                        os => new { os.VehicleId, os.LoadId },
                        olr => new { olr.VehicleId, olr.LoadId },
                        (os, olr) => new
                        {
                            os.InitHour,
                            os.EndHour,
                            NumOrders = Math.Max(1, olr.LoadInVehicle) * (olr.OrderInLoad == 0.0 ? 1 : olr.OrderInLoad) * os.NumberVehicles,
                            os.IsOut,
                            os.WarehouseId
                        }
                    )
                    .Join(
                        contextWFM.OutboundFlowGraphs,
                        os => os.WarehouseId,
                        ifg => ifg.WarehouseId,
                        (os, ifg) => new QueryResult
                        {
                            InitHour = os.InitHour,
                            EndHour = os.EndHour,
                            NumOrders = os.NumOrders,
                            IsOut = os.IsOut,
                            AverageLinesPerOrder = ifg.AverageLinesPerOrder,
                            WarehouseId = os.WarehouseId
                        }
                    ).Join(
                        contextWFM.Warehouses,
                        os => os.WarehouseId,
                        w => w.Id,
                        (os, w) => new QueryResult
                        {
                            InitHour = os.InitHour,
                            EndHour = os.EndHour,
                            NumOrders = os.NumOrders,
                            IsOut = os.IsOut,
                            AverageLinesPerOrder = os.AverageLinesPerOrder,
                            WarehouseId = os.WarehouseId,
                            WarehouseCode = w.Code
                        }
                    )
                    .ToList();

                if (query?.Count == 0) return;


                using (var contextWMS = new WMSDbContext(optionsBuilderWMS.Options))
                {
                    query.ForEach(t =>
                    {
                        var wft = new WorkForceTask
                        {
                            InitHour = t.InitHour,
                            EndHour = t.EndHour,
                            NumOrders = (double)Math.Round(t.NumOrders, MidpointRounding.AwayFromZero),
                            NumOrdersCompleted = 0,
                            Date = DateTime.UtcNow.Date,
                            WarehouseId = t.WarehouseId,
                            IsOut = t.IsOut,
                            LinesPerOrder = t.AverageLinesPerOrder,
                            WarehouseCode = t.WarehouseCode
                        };
                        contextWMS.WorkForceTasks.Add(wft);
                    });

                    contextWMS.SaveChanges();
                }

                Console.WriteLine("Scraped Outbound data from WF");

                query = contextWFM.OrderSchedules.Where(m => m.IsOut == false)
                       .Join(
                           contextWFM.OrderLoadRatios.DefaultIfEmpty(),
                           os => new { os.VehicleId, os.LoadId },
                           olr => new { olr.VehicleId, olr.LoadId },
                           (os, olr) => new
                           {
                               os.InitHour,
                               os.EndHour,
                               NumOrders = Math.Max(1, olr.LoadInVehicle) * (olr.OrderInLoad == 0.0 ? 1 : olr.OrderInLoad) * os.NumberVehicles,
                               os.IsOut,
                               os.WarehouseId
                           }
                       )
                       .Join(
                           contextWFM.InboundFlowGraphs,
                           os => os.WarehouseId,
                           ifg => ifg.WarehouseId,
                           (os, ifg) => new QueryResult
                           {
                               InitHour = os.InitHour,
                               EndHour = os.EndHour,
                               NumOrders = os.NumOrders,
                               IsOut = os.IsOut,
                               AverageLinesPerOrder = ifg.AverageLinesPerOrder,
                               WarehouseId = os.WarehouseId
                           }
                       )
                       .Join(
                            contextWFM.Warehouses,
                            os => os.WarehouseId,
                            w => w.Id,
                            (os, w) => new QueryResult
                            {
                                InitHour = os.InitHour,
                                EndHour = os.EndHour,
                                NumOrders = os.NumOrders,
                                IsOut = os.IsOut,
                                AverageLinesPerOrder = os.AverageLinesPerOrder,
                                WarehouseId = os.WarehouseId,
                                WarehouseCode = w.Code
                            }
                        )
                        .ToList();

                if (query?.Count == 0) return;


                using (var contextWMS = new WMSDbContext(optionsBuilderWMS.Options))
                {
                    query.ForEach(t =>
                    {
                        var wft = new WorkForceTask
                        {
                            InitHour = t.InitHour,
                            EndHour = t.EndHour,
                            NumOrders = (double)Math.Round(t.NumOrders, MidpointRounding.AwayFromZero),
                            NumOrdersCompleted = 0,
                            Date = DateTime.UtcNow.Date,
                            WarehouseId = t.WarehouseId,
                            IsOut = t.IsOut,
                            LinesPerOrder = t.AverageLinesPerOrder,
                            WarehouseCode = t.WarehouseCode
                        };
                        contextWMS.WorkForceTasks.Add(wft);
                    });

                    contextWMS.SaveChanges();
                    query = null;
                    GC.Collect();

                    Console.WriteLine("Scraped Inbound data from WF");

                }

            }

        }
        public static List<SimulationResults> GatherSimulationResults(Guid warehouseId)
        {
            var optionsBuilderWFM = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilderWFM.UseNpgsql(WFMConnectionString);

            var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilderWMS.UseNpgsql(WMSConnectionString);

            List<SimulationResults> SimulationResults = new List<SimulationResults>();

            int? tables;
            using (var contextWMS = new WMSDbContext(optionsBuilderWMS.Options))
            {
                using (var contextWFM = new ApplicationDbContext(optionsBuilderWFM.Options))
                {
                    using (var connection = contextWFM.Database.GetDbConnection())
                    {
                        connection.Open();
                        tables = connection.GetSchema("Tables").Rows.Count;

                        if (tables != 0)
                        {
                            Console.WriteLine("There are tables in database");
                            var WorkOrderWithInputOrder = contextWFM.Plannings.Where(m => m.WarehouseId == warehouseId).Join(contextWFM.WorkOrdersPlanning,
                            p => p.Id, w => w.PlanningId, (p, w) => new
                            {
                                CreationDate = p.CreationDate,
                                InputOrderId = w.InputOrderId,
                                PlanningId = w.PlanningId
                            }).Where(m => m.InputOrderId != null);

                            if (WorkOrderWithInputOrder.Count() != 0)
                            {
                                Console.WriteLine("There are WorkOrderPlanning with InputOrders");
                                DateTime MaxDatePlanning = WorkOrderWithInputOrder.Max(m => m.CreationDate);

                                if (!contextWMS.SimulationResults.Select(m => m.PlanningId).Contains(WorkOrderWithInputOrder.First(m => m.CreationDate == MaxDatePlanning).PlanningId))
                                {
                                    Console.WriteLine("Planning hasn't insert in database yet");
                                    Guid LastPlanningId = contextWFM.Plannings.First(m => m.WarehouseId == warehouseId && m.CreationDate == MaxDatePlanning).Id;
                                    var WorkOrdersPlanning = contextWFM.WorkOrdersPlanning.Where(m => m.PlanningId == LastPlanningId && m.InputOrderId != null)
                                        .Include(m => m.InputOrder)
                                        .Include(m => m.AssignedDock)
                                        .ThenInclude(m => m.Zone).ToList();

                                    foreach (var workOrderPlanning in WorkOrdersPlanning)
                                    {
                                        Guid SimulationResultsId = Guid.NewGuid();
                                        var simulationResults = new SimulationResults
                                        {
                                            Id = SimulationResultsId,
                                            Code = workOrderPlanning.InputOrder.OrderCode,
                                            ClientCode = workOrderPlanning.InputOrder.Account,
                                            ProviderCode = workOrderPlanning.InputOrder.Supplier,
                                            TypeCode = workOrderPlanning.InputOrder.IsOutbound == true ? "Outbound" : "Inbound",
                                            AppointmentDate = workOrderPlanning.InputOrder.AppointmentDate,
                                            ExpeditionEstimatedDate = workOrderPlanning.InputOrder.IsOutbound == true ? workOrderPlanning.EndDate : null,
                                            ArrivalEstimatedDate = workOrderPlanning.InputOrder.IsOutbound == false ? workOrderPlanning.InitDate : null,
                                            LiberationDate = workOrderPlanning.InitDate,
                                            DownloadDate = contextWFM.ItemsPlanning.Where(m => m.WorkOrderPlanningId == workOrderPlanning.Id).Any() ? workOrderPlanning.InputOrder.IsOutbound == false ? contextWFM.ItemsPlanning.First(m => m.WorkOrderPlanningId == workOrderPlanning.Id && m.Process.Type == "Inbound").InitDate : null : null,
                                            StartDate = workOrderPlanning.InitDate,
                                            EndDate = workOrderPlanning.EndDate,
                                            RealTruckArrivalDate = workOrderPlanning.InputOrder.RealArrivalTime,
                                            AssignedDockCode = workOrderPlanning.AssignedDock == null ? null : workOrderPlanning.AssignedDock.Zone.Name,
                                            ActionCode = workOrderPlanning.InputOrder.Status,
                                            PlanningId = workOrderPlanning.PlanningId,
                                            Processes = new List<Processes>()
                                        };
                                        SimulationResults.Add(simulationResults);
                                        contextWMS.SimulationResults.Add(simulationResults);
                                        Console.WriteLine("SimulationResults added to list");

                                        var itemplannings = contextWFM.ItemsPlanning.Where(x => x.WorkOrderPlanningId == workOrderPlanning.Id).Include(m => m.Worker).Include(q => q.EquipmentGroup).Include(m => m.Process).ToList();

                                        foreach (var itemplanning in itemplannings)
                                        {
                                            simulationResults.Processes.Add(new Processes
                                            {
                                                Id = Guid.NewGuid(),
                                                SimulationResultsId = simulationResults.Id,
                                                UserCode = itemplanning.Worker.Name,
                                                EquipmentTypeCode = itemplanning.EquipmentGroup.Name,
                                                StartDate = itemplanning.InitDate,
                                                EndDate = itemplanning.EndDate,
                                                ProcessType = itemplanning.Process.Type
                                            });
                                        }
                                        Console.WriteLine("Processes added to list");
                                    }

                                    contextWMS.SaveChanges();
                                }
                                else
                                {
                                    Console.WriteLine("Planning has insert in database");
                                    Guid PlanningId = WorkOrderWithInputOrder.First(m => m.CreationDate == MaxDatePlanning).PlanningId;
                                    SimulationResults.AddRange(contextWMS.SimulationResults.Where(m => m.PlanningId == PlanningId));
                                    Console.WriteLine("SimulationResults added to list");

                                    foreach (var simulation in contextWMS.SimulationResults.Where(m => m.PlanningId == PlanningId).ToList())
                                    {
                                        SimulationResults.First(x => x.Id == simulation.Id).Processes = contextWMS.Processes.Where(m => m.SimulationResultsId == simulation.Id).ToList();
                                    }
                                    Console.WriteLine("Processes added to list");
                                }
                            }

                        }
                    }
                }
            }

            GC.Collect();

            return SimulationResults;
        }
    }
}
