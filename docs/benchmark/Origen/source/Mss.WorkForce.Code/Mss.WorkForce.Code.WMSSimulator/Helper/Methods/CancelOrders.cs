namespace Mss.WorkForce.Code.WMSSimulator.Helper.Methods
{
    public static class CancelOrders
    {
        #region Methods

        #region Public

        /// <summary>
        /// Try to cancel the WMS orders
        /// </summary>
        /// <param name="isOut">True if the orders are outbound, false if not</param>
        /// <param name="wmsOrders">WMS orders, previously filtered</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <returns>List of WMS orders updated</returns>
        public static List<WMSModel.InputOrder> TryCancelling(bool isOut, List<WMSModel.InputOrder> wmsOrders, List<WMSModel.Parameter> parameters)
        {
            if (isOut)
            {
                return TryCancellingOutboundOrders(wmsOrders, parameters);
            }
            else
            {
                return TryCancellingInboundOrders(wmsOrders, parameters);
            }
        }

        /// <summary>
        /// Try to cancel the WFM orders
        /// </summary>
        /// <param name="wmsOrders">WMS orders, previously filtered</param>
        /// <param name="wfmOrders">WFM orders, previously filtered</param>
        /// <returns>List of WFM orders updated</returns>
        public static List<Models.Models.InputOrder> TryCancelling(List<WMSModel.InputOrder> wmsOrders, List<Models.Models.InputOrder> wfmOrders)
        {
            var cancelledOrders = wmsOrders.Where(x => x.Status == OrderStatus.Cancelled);

            foreach (var o in cancelledOrders)
            {
                if (wfmOrders.Select(x => x.OrderCode).Contains(o.OrderCode))
                {
                    wfmOrders.Where(x => x.OrderCode == o.OrderCode).FirstOrDefault().Status = OrderStatus.Cancelled;
                }
            }

            return wfmOrders;
        }

        #endregion

        #region Private

        /// <summary>
        /// Try to cancel the WMS inbound orders
        /// </summary>
        /// <param name="wmsOrders">WMS orders, previously filtered</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <returns>List of WMS orders updated</returns>
        private static List<WMSModel.InputOrder> TryCancellingInboundOrders(List<WMSModel.InputOrder> wmsOrders, List<WMSModel.Parameter> parameters)
        {
            var inboundOrders = wmsOrders.Where(x => x.Status == OrderStatus.Waiting);
            double cancelProbability = parameters.FirstOrDefault(x => x.Code == ParameterValue.CancelInboundProbability).Value / 100;

            foreach (var o in inboundOrders)
            {
                Random rnd = new Random();
                if (rnd.NextDouble() < cancelProbability)
                {
                    wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Cancelled;
                }
            }

            return wmsOrders;
        }

        /// <summary>
        /// Try to cancel the WMS outbound orders
        /// </summary>
        /// <param name="wmsOrders">WMS orders, previously filtered</param>
        /// <param name="parameters">Parameters to generate the orders</param>
        /// <returns>List of WMS orders updated</returns>
        private static List<WMSModel.InputOrder> TryCancellingOutboundOrders(List<WMSModel.InputOrder> wmsOrders, List<WMSModel.Parameter> parameters)
        {
            var outboundOrders = wmsOrders.Where(x => x.Status == OrderStatus.Waiting || x.Status == OrderStatus.Released);
            double cancelProbability = parameters.FirstOrDefault(x => x.Code == ParameterValue.CancelOutboundProbability).Value / 100;

            foreach (var o in outboundOrders)
            {
                Random rnd = new Random();
                if (Random.Shared.NextDouble() < cancelProbability)
                {
                    wmsOrders.FirstOrDefault(x => x.OrderCode == o.OrderCode).Status = OrderStatus.Cancelled;
                }
            }

            return wmsOrders;
        }

        #endregion

        #endregion
    }
}
