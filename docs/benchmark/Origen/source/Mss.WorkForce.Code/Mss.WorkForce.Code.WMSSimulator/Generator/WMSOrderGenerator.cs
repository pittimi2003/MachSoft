using Mss.WorkForce.Code.WMSSimulator.Helper;
using Mss.WorkForce.Code.WMSSimulator.WMSModel;

namespace Mss.WorkForce.Code.WMSSimulator.Generator
{
    public static class WMSOrderGenerator
    {
        #region Methods
        /// <summary>
        /// Generates the WMS orders. 
        /// </summary>
        /// <param name="workForceTask">Registry containing the necessary data to generate the orders</param>
        /// <param name="wfmOrders">WFM orders previously created</param>
        /// <param name="wfmData">WFM necessary data</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <param name="creationDate">Creation of the InputOrder DateTime</param>
        public static List<InputOrder> GenerateOrders(WorkForceTask workForceTask, List<Models.Models.InputOrder> wfmOrders, DataBaseResponse wfmData, 
            List<Parameter> parameters, DateTime creationDate)
        {
            List<InputOrder> wmsOrders = new List<InputOrder>();

            if (wfmOrders.Any())
            {
                var orderOptions = workForceTask.IsOut ? "outbound" : "inbound";
                var marginOption = workForceTask.IsOut ? ParameterValue.OutboundMarginTime : ParameterValue.InboundMarginTime;
                Console.WriteLine($"Generating WMS {orderOptions} orders");

                foreach (var o in wfmOrders)
                {
                    int marginTime = (int)parameters.FirstOrDefault(x => x.Code == marginOption).Value;
                    TimeSpan margin = new TimeSpan(0, 0, RandomSelector.Select(-marginTime, marginTime));

                    wmsOrders.Add(new InputOrder
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        IsOut = o.IsOutbound,
                        PreferedDockCode = o.PreferredDockId == null ? null : wfmData.Docks.FirstOrDefault(x => x.Id == o.PreferredDockId).Zone.Name,
                        Status = o.Status,
                        Progress = o.Progress == null ? 0 : o.Progress.Value,
                        NumLines = workForceTask.LinesPerOrder,
                        UpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                        MarginTime = margin,
                        WarehouseId = o.WarehouseId,
                        WarehouseCode = wfmData.Warehouses.FirstOrDefault(x => x.Id == o.WarehouseId).Name,
                        WorkForceTaskId = workForceTask.Id,
                        AppointmentDate = DateTime.SpecifyKind(o.AppointmentDate, DateTimeKind.Utc),
                        CreationDate = DateTime.SpecifyKind(creationDate, DateTimeKind.Utc)
                    });
                }
            }

            return wmsOrders;
        }

        #endregion
    }
}
