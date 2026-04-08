using Mss.WorkForce.Code.WMSSimulator.Helper;
using Mss.WorkForce.Code.WMSSimulator.Helper.Methods;
using Mss.WorkForce.Code.WMSSimulator.WMSModel;

namespace Mss.WorkForce.Code.WMSSimulator.Generator
{
    public static class WFMOrderGenerator
    {
        #region Methods

        #region Public

        /// <summary>
        /// Generates the WFM lines. 
        /// </summary>
        /// <param name="wmsOrders">WMS orders previously created</param>
        /// <param name="wfmOrders">WFM orders previously created</param>
        public static List<Models.Models.InputOrderLine> GenerateLines(List<Models.Models.InputOrder> wfmOrders, List<InputOrder> wmsOrders)
        {
            Console.WriteLine($"Generating WFM lines orders");
            List<Models.Models.InputOrderLine> lines = new List<Models.Models.InputOrderLine>();

            foreach (var o in wfmOrders) 
            {
                for (int i = 0; i < wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).NumLines; i++)
                {
                    lines.Add(new Models.Models.InputOrderLine()
                    {
                        Id = Guid.NewGuid(),
                        Product = null,
                        Quantity = null,
                        UnitOfMeasure = null,
                        IsClosed = false,
                        InputOutboundOrderId = o.Id,
                        InputOutboundOrder = o
                    });
                }
            }

            return lines;
        }

        /// <summary>
        /// Generates the WFM orders. 
        /// </summary>
        /// <param name="workForceTask">Registry containing the necessary data to generate the orders</param>
        /// <param name="wfmData">WFM necessary data</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="numberOfOrders">Number of orders of a specific type already generated</param>
        /// <param name="creationDate">Creation of the InputOrder DateTime</param>
        public static (List<Models.Models.InputOrder> WFMOrders, List<Models.Models.Deliveries> WFMDeliveries) 
            GenerateOrders(WorkForceTask workForceTask, DataBaseResponse wfmData, List<Parameter> parameters, 
            int numberOfOrders, DateTime creationDate, int numberOfVehicle)
        {
            List<Models.Models.InputOrder> inputOrders = new List<Models.Models.InputOrder>();
            List<Models.Models.Deliveries> deliveriesList = new List<Models.Models.Deliveries>();

            if (workForceTask.NumOrdersCompleted == 0)
            {
                var vehiclesNumber = Convert.ToInt32(wfmData.OrderSchedules.FirstOrDefault(x => x.WarehouseId == workForceTask.WarehouseId
                    && x.InitHour == workForceTask.InitHour && x.EndHour == workForceTask.EndHour).NumberVehicles);
                var inputOrdersNumber = (int)Math.Round((workForceTask.NumOrders - workForceTask.NumOrdersCompleted) / vehiclesNumber, MidpointRounding.AwayFromZero);

                if (DateTime.UtcNow.TimeOfDay < workForceTask.EndHour)
                {
                    for (int i = 0; i < vehiclesNumber; i++)
                    {
                        var orderOption = workForceTask.IsOut ? "outbound" : "inbound";
                        Console.WriteLine($"Generating WFM {orderOption} orders");

                        var preferedDock = SelectDock.GetDock(parameters, wfmData, ParameterValue.PreferedDockProbability, workForceTask.IsOut, workForceTask);
                        var appointmentDate = DateTime.SpecifyKind(
                                    DateTime.UtcNow.Date.Add(
                                        RandomSelector.SelectRandomTime(
                                            TimeSpan.Compare(workForceTask.InitHour, DateTime.UtcNow.TimeOfDay) > 0
                                                ? workForceTask.InitHour
                                                : DateTime.UtcNow.TimeOfDay,
                                            workForceTask.EndHour
                                        )
                                    ),
                                    DateTimeKind.Utc
                                    );

                        var vehicleCode = "VEHICLE_" + char.ToUpper(orderOption[0]) + (numberOfVehicle + 1).ToString() + $"_{i}";
                        
                        List<Models.Models.Deliveries> tempDeliveriesList = new List<Models.Models.Deliveries>();
                        if (wfmData.Processses.Any(x => x.Type == ProcessType.Packing) && workForceTask.IsOut)
                        {
                            tempDeliveriesList.Add(new Models.Models.Deliveries()
                            {
                                Id = Guid.NewGuid(),
                                Code = $"DELIVERY_{workForceTask.WarehouseCode}_{numberOfOrders}{i}1",
                                NumPackages = null,
                                PackagingType = PackagingType.CaptureStock
                            });
                            tempDeliveriesList.Add(new Models.Models.Deliveries()
                            {
                                Id = Guid.NewGuid(),
                                Code = $"DELIVERY_{workForceTask.WarehouseCode}_{numberOfOrders}{i}2",
                                NumPackages = null,
                                PackagingType = PackagingType.CaptureStock
                            });

                            deliveriesList.AddRange(tempDeliveriesList);
                            tempDeliveriesList.Add(new Models.Models.Deliveries());
                        }

                        for (int j = 0; j < inputOrdersNumber; j++)
                        {
                            int randomNum = new Random().Next(0,3); 

                            inputOrders.Add(new Models.Models.InputOrder()
                            {
                                Id = Guid.NewGuid(),
                                OrderCode = string.Empty,
                                IsStarted = false,
                                Status = OrderStatus.Waiting,
                                IsOutbound = workForceTask.IsOut,
                                AllowPartialClosed = false,
                                AllowGroup = false,
                                UpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                                Carrier = null,
                                Account = null,
                                Supplier = null,
                                Trailer = null,
                                IsEstimated = false,
                                AssignedDock = null,
                                AssignedDockId = null,
                                PreferredDockId = preferedDock == null ? null : preferedDock.Id,
                                PreferredDock = null,
                                Warehouse = null,
                                WarehouseId = workForceTask.WarehouseId,
                                AppointmentDate = appointmentDate,
                                Progress = 0,
                                CreationDate = DateTime.SpecifyKind(creationDate, DateTimeKind.Utc),
                                VehicleCode = vehicleCode,
                                DeliveryId = (tempDeliveriesList.Count() != 0 && tempDeliveriesList[randomNum].Id != Guid.Empty) ? tempDeliveriesList[randomNum].Id : null
                            });
                        }
                    }
                }
            }

            return (inputOrders, deliveriesList);
        }


    }

    #endregion
    #endregion
}
