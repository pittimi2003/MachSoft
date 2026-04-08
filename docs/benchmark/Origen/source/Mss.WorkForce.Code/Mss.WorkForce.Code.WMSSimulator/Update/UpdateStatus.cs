using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.WMSSimulator.Helper;

namespace Mss.WorkForce.Code.WMSSimulator.Update
{
    public static class UpdateStatus
    {
        #region Methods

        /// <summary>
        /// Updates the WMS orders with status "Prepared"
        /// </summary>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="status">Order status</param>
        /// <param name="wfmData">WFM data</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="releaseDate">Release date of the order when opens</param>
        /// <returns>List of updated WMS orders</returns>
        public static List<WMSModel.InputOrder> Prepared(List<WMSModel.InputOrder> wmsOrders, string status, DataBaseResponse wfmData, List<WMSModel.Parameter> parameters,
            DateTime releaseDate)
        {
            var statusOrders = wmsOrders.Where(x => x.Status == status);

            foreach (var o in statusOrders)
            {
                if (wfmData.InputOrders.Select(x => x.OrderCode).Contains(o.OrderCode))
                {
                    wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Released;
                    wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = 0;
                    wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).ReleaseDate = releaseDate;
                }
            }
            return wmsOrders;
        }

        /// <summary>
        /// Updates the WFM orders with status "Prepared"
        /// </summary>
        /// <param name="wfmOrders">Orders in the WFM database</param>
        /// <param name="status">Order status</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="releaseDate">Release date of the order when opens</param>
        /// <returns>List of updated WFM orders</returns>
        public static List<Models.Models.InputOrder> Prepared(List<Models.Models.InputOrder> wfmOrders, string status, List<WMSModel.Parameter> parameters, 
            List<WMSModel.InputOrder> wmsOrders, DateTime releaseDate)
        {
            var statusOrders = wfmOrders.Where(x => x.Status == status);

            var date = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            foreach (var o in statusOrders)
            {
                if (wmsOrders.Select(x => x.OrderCode).Contains(o.OrderCode))
                {
                    wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = wmsOrders.FirstOrDefault(m => m.OrderCode == o.OrderCode).Progress;

                    if (wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).IsOutbound)
                    {
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Released;
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).IsStarted = true;
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).UpdateDate = date;
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).ReleaseDate = releaseDate;
                    }

                    else
                    {
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Released;
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).IsStarted = true;
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).UpdateDate = date;
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).RealArrivalTime = date;
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).ReleaseDate = releaseDate;
                    }
                }
            }

            return wfmOrders;
        }

        /// <summary>
        /// Updates the WMS orders with status "Open"
        /// </summary>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="status">Order status</param>
        /// <param name="wfmData">WFM data</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <returns>List of updated WMS orders</returns>
        public static List<WMSModel.InputOrder> Open(List<WMSModel.InputOrder> wmsOrders, string status, DataBaseResponse wfmData, List<WMSModel.Parameter> parameters)
        {
            var statusOrders = wmsOrders.Where(x => x.Status == status);

            foreach (var o in statusOrders)
            {
                if (wfmData.InputOrders.Select(x => x.OrderCode).Contains(o.OrderCode))
                {
                    if (o.Progress == 0)
                    {
                        wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = 25;
                    }
                    else if (o.Progress == 25)
                    {
                        wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = 50;
                    }
                    else if (o.Progress == 50)
                    {
                        wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = 75;
                    }
                    else
                    {

                        if(wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).IsOut)
                        {
                            if(wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).AppointmentDate <= DateTime.UtcNow)
                            {
                                wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Closed;
                                wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = 100;
                            }
                        }

                        else
                        {
                            wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Closed;
                            wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = 100;
                        }
                    }
                }
            }

            return wmsOrders;
        }

        /// <summary>
        /// Updates the WFM orders with status "Open"
        /// </summary>
        /// <param name="wfmOrders">Orders in the WFM database</param>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="status">Order status</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <returns>List of updated WFM orders</returns>
        public static List<Models.Models.InputOrder> Open(List<Models.Models.InputOrder> wfmOrders, List<WMSModel.InputOrder> wmsOrders, string status, List<WMSModel.Parameter> parameters)
        {
            var statusOrders = wfmOrders.Where(x => x.Status == status);

            foreach (var o in statusOrders)
            {
                if (wmsOrders.Select(x => x.OrderCode).Contains(o.OrderCode))
                {
                    wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress = wmsOrders.FirstOrDefault(m => m.OrderCode == o.OrderCode).Progress;

                    if (wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress != 100)
                    {
                        wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).UpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                    }
                    else
                    {
                        if (wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).IsOutbound)
                        {
                            
                            if(wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).AppointmentDate <= DateTime.UtcNow)
                            {
                                wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Closed;
                                wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).UpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                                wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).RealArrivalTime = o.AppointmentDate.Add(wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).MarginTime);
                            }   
                        }
                        else
                        {
                            wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Closed;
                            wfmOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).UpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                        }
                    }
                }
            }

            return wfmOrders;
        }

        /// <summary>
        /// Updates de order lines
        /// </summary>
        /// <param name="wmsOrders">Orders in the WMS database</param>
        /// <param name="wfmOrders">Orders in the WFM database</param>
        /// <param name="wfmOrderLines">Order lines in the WFM database</param>
        /// <returns>List of updated WFM order lines</returns>
        public static List<Models.Models.InputOrderLine> Lines(List<WMSModel.InputOrder> wmsOrders, List<Models.Models.InputOrder> wfmOrders, List<Models.Models.InputOrderLine> wfmOrderLines)
        {
            foreach (var o in wfmOrders)
            {
                if (wmsOrders.Select(x => x.OrderCode).Contains(o.OrderCode))
                {
                    var lines = wfmOrderLines.Where(x => x.InputOutboundOrder.OrderCode == o.OrderCode).ToList();

                    var progress = wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Progress;
                    var linesToClose = (int)(lines.Count() * progress);

                    for (int i = 0; i < lines.Count(); i++)
                    {
                        if (linesToClose >= 1)
                        {
                            if (!lines[i].IsClosed.GetValueOrDefault(false))
                            {
                                wfmOrderLines.FirstOrDefault(x => x.Id == lines[i].Id).IsClosed = true;
                                linesToClose--;
                            }

                            if (linesToClose == 0) { break; }
                        }
                    }
                }
            }

            return wfmOrderLines;
        }

        #endregion
    }
}
