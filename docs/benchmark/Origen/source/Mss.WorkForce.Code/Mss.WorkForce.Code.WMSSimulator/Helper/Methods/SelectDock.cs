using Mss.WorkForce.Code.WMSSimulator.WMSModel;

namespace Mss.WorkForce.Code.WMSSimulator.Helper.Methods
{
    public static class SelectDock
    {
        #region Methods

        /// <summary>
        /// Selects an assigned dock for the order
        /// </summary>
        /// <param name="parameters">Parameters to generate the order</param>
        /// <param name="isOut">Boolean to check if the order is inbound or outbound</param>
        /// <param name="probability">Parameter that defines the wanted probability</param>
        /// <param name="wfmData">WFM necessary data</param>
        /// <param name="workForceTask">Registry of the work force task to generate the order</param>
        /// <returns>Assigned dock</returns>
        public static Models.Models.Dock? GetDock(List<Parameter> parameters, DataBaseResponse wfmData, string probability, bool isOut, WorkForceTask workForceTask)
        {
            double dockProbability = parameters.FirstOrDefault(x => x.Code == probability).Value/100;

            if (Random.Shared.NextDouble() <= dockProbability)
            {
                return ChooseDock(parameters, wfmData, isOut, workForceTask);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Selects a random dock between all the posibilities
        /// </summary>
        /// <param name="parameters">Parameters to generate the order</param>
        /// <param name="isOut">Boolean to check if the order is inbound or outbound</param>
        /// <param name="wfmData">WFM necessary data</param>
        /// <param name="workForceTask">Registry of the work force task to generate the order</param>
        /// <returns>Selected dock</returns>
        private static Models.Models.Dock? ChooseDock(List<Parameter> parameters, DataBaseResponse wfmData, bool isOut, WorkForceTask workForceTask)
        {
            List<Models.Models.Dock> docks = wfmData.Docks
                .Where(x => x.Zone.Area.Layout.WarehouseId == workForceTask.WarehouseId)
                .Where(x => x.AllowOutbound == isOut || x.AllowInbound != isOut).ToList();

            var a = Random.Shared.Next(docks.Count);
            return docks[a];
        }

        #endregion
    }
}
