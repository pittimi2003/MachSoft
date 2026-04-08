using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.WMSSimulator.Generator
{
    public static class DoGenerate
    {
        private static readonly Random random = new Random();

        #region Methods
        /// <summary>
        /// Generates the outbound orders and lines and inserts them into the WFM and WMS database. 
        /// </summary>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="wfmData">WFM necessary data</param>
        /// <param name="workForceTask">Registry containing the necessary data to generate the orders</param>
        public static void GenerateOutboundOrders(List<WMSModel.Parameter> parameters, WMSModel.WorkForceTask workForceTask, DataBaseResponse wfmData)
        {
            var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

            List<Models.Models.Deliveries> WFMDeliveries = new List<Models.Models.Deliveries>();
            List<Models.Models.InputOrder> WFMOrders = new List<Models.Models.InputOrder>();
            List<WMSModel.InputOrder> WMSOrders = new List<WMSModel.InputOrder>();
            List<Models.Models.InputOrderLine> WFMOrderLines = new List<Models.Models.InputOrderLine>();
            List<Models.Models.YardAppointmentsNotifications> yardAppointments = new List<Models.Models.YardAppointmentsNotifications>();

            using (var context = new WMSDbContext(optionsBuilderWMS.Options))
            {
                var numberOfOrders = context.InputOrders.Count(x => x.IsOut == workForceTask.IsOut);
                var numberOfVehicle = context.InputOrders.Where(x => x.IsOut == workForceTask.IsOut).Select(x => x.AppointmentDate).Distinct().Count();
                var creationDate = DateTime.UtcNow;

                (WFMOrders, WFMDeliveries) = WFMOrderGenerator.GenerateOrders(workForceTask, wfmData, parameters, numberOfOrders, creationDate, numberOfVehicle);

                int j = 0;
                foreach (var item in WFMOrders.Where(m => m.IsOutbound == true))
                {              
                    item.OrderCode = "O" + "_" + (numberOfOrders + j + 1).ToString("D7");
                    j = j + 1;
                }

                WMSOrders = WMSOrderGenerator.GenerateOrders(workForceTask, WFMOrders, wfmData, parameters, creationDate);
                WFMOrderLines = WFMOrderGenerator.GenerateLines(WFMOrders, WMSOrders);

                for (int i = 0; i < WFMOrders.Count; i++)
                {
                    var warehouseOrderPriorirties = wfmData.OrderPriority.Where(x => x.WarehouseId == WFMOrders[i].WarehouseId).ToList();

                    if (warehouseOrderPriorirties.Any())
                    {
                        WFMOrders[i].Priority = warehouseOrderPriorirties[i % warehouseOrderPriorirties.Count()].Code;
                    }
                }

                Console.WriteLine("Outbound orders and lines generated.");

                context.InputOrders.AddRange(WMSOrders);
                context.WorkForceTasks.FirstOrDefault(x => x.Id == workForceTask.Id).NumOrdersCompleted += WMSOrders.Count();

                context.SaveChanges();

                Console.WriteLine("Outbound orders for WMS saved successfully.");
            }

            var optionsBuilderWFM = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilderWFM.UseNpgsql(DataBaseActions.WFMConnectionString);

            using (var context = new ApplicationDbContext(optionsBuilderWFM.Options))
            {
                var appointmentsNumber = context.YardAppointmentsNotifications.Count(x => x.AppointmentCode.Contains("APO"));
                yardAppointments = AppointmentsGenerator.Generate(WFMOrders, wfmData, true, random, appointmentsNumber + 1);

                context.Deliveries.AddRange(WFMDeliveries);
                context.InputOrders.AddRange(WFMOrders);
                context.InputOrderLines.AddRange(WFMOrderLines);
                context.YardAppointmentsNotifications.AddRange(yardAppointments);

                context.SaveChanges();

                Console.WriteLine("Outbound orders for WFM saved successfully.");
            }

            WFMDeliveries = null;
            WFMOrders = null;
            WMSOrders = null;
            WFMOrderLines = null;
            yardAppointments = null;
            optionsBuilderWFM = null;
            optionsBuilderWMS = null;

            GC.Collect();
        }

        /// <summary>
        /// Generates the inbound orders and lines and inserts them into the WFM and WMS database. 
        /// </summary>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="wfmData">WFM necessary data</param>
        /// <param name="workForceTask">Registry containing the necessary data to generate the orders</param>
        public static void GenerateInboundOrders(List<WMSModel.Parameter> parameters, WMSModel.WorkForceTask workForceTask, DataBaseResponse wfmData)
        {
            var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
            optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

            List<Models.Models.InputOrder> WFMOrders = new List<Models.Models.InputOrder>();
            List<WMSModel.InputOrder> WMSOrders = new List<WMSModel.InputOrder>();
            List<Models.Models.InputOrderLine> WFMOrderLines = new List<Models.Models.InputOrderLine>();
            List<Models.Models.YardAppointmentsNotifications> yardAppointments = new List<Models.Models.YardAppointmentsNotifications>();

            using (var context = new WMSDbContext(optionsBuilderWMS.Options))
            {
                var numberOfOrders = context.InputOrders.Count(x => x.IsOut == workForceTask.IsOut);
                var numberOfVehicle = context.InputOrders.Where(x => x.IsOut == workForceTask.IsOut).Select(x => x.AppointmentDate).Distinct().Count();
                var creationDate = DateTime.UtcNow;

                WFMOrders = WFMOrderGenerator.GenerateOrders(workForceTask, wfmData, parameters, numberOfOrders, creationDate, numberOfVehicle).WFMOrders;

                int h = 0;
                foreach (var item in WFMOrders.Where(m => m.IsOutbound == false))
                {
                    item.OrderCode = "I" + "_" + (numberOfOrders + h + 1).ToString("D7");
                    h = h + 1;
                }

                WMSOrders = WMSOrderGenerator.GenerateOrders(workForceTask, WFMOrders, wfmData, parameters, creationDate);
                WFMOrderLines = WFMOrderGenerator.GenerateLines(WFMOrders, WMSOrders);

                for (int i = 0; i < WFMOrders.Count; i++)
                {
                    var warehouseOrderPriorirties = wfmData.OrderPriority.Where(x => x.WarehouseId == WFMOrders[i].WarehouseId).ToList();

                    if (warehouseOrderPriorirties.Any())
                    {
                        WFMOrders[i].Priority = warehouseOrderPriorirties[i % warehouseOrderPriorirties.Count()].Code;
                    }
                }

                Console.WriteLine("Inbound orders and lines generated.");

                context.InputOrders.AddRange(WMSOrders);
                context.WorkForceTasks.FirstOrDefault(x => x.Id == workForceTask.Id).NumOrdersCompleted += WMSOrders.Count();

                context.SaveChanges();

                Console.WriteLine("Inbound orders for WMS saved successfully.");
            }

            var optionsBuilderWFM = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilderWFM.UseNpgsql(DataBaseActions.WFMConnectionString);

            using (var context = new ApplicationDbContext(optionsBuilderWFM.Options))
            {
                var appointmentsNumber = context.YardAppointmentsNotifications.Count(x => x.AppointmentCode.Contains("API"));
                yardAppointments = AppointmentsGenerator.Generate(WFMOrders, wfmData, false, random, appointmentsNumber+1);

                context.InputOrders.AddRange(WFMOrders);
                context.InputOrderLines.AddRange(WFMOrderLines);
                context.YardAppointmentsNotifications.AddRange(yardAppointments);

                context.SaveChanges();

                Console.WriteLine("Inbound orders for WFM saved successfully.");
            }

            WFMOrders = null;
            WMSOrders = null;
            WFMOrderLines = null;
            yardAppointments = null;
            optionsBuilderWMS = null;
            optionsBuilderWFM = null;

            GC.Collect();
        }

        #endregion

    }
}
