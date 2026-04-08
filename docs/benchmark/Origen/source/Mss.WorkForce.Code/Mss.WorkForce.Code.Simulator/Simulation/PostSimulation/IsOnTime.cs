using Mss.WorkForce.Code.Simulator.Helper;

namespace Mss.WorkForce.Code.Simulator.Simulation.PostSimulation
{
    public class IsOnTime
    {
        #region Variables
        private Simulation simulation;
        #endregion

        #region Constructor
        public IsOnTime(Simulation simulation)
        {
            this.simulation = simulation;
        }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Check if the WorkOrders are on time
        /// </summary>
        public void Check()
        {
            DoCheck(true);
            //DoCheck(false);
        }

        #endregion

        #region Private
        /// <summary>
        /// Check if the WorkOrders are on time
        /// </summary>
        /// <param name="isVehicle">True if checks for IsOnTime for a vehicle, False if for appointment date</param>
        private void DoCheck(bool isVehicle)
        {
            foreach (var workOrder in this.simulation.PlanningReturn.WorkOrderPlanning)
            {
                var inputOrder = workOrder.InputOrder;
                var appointment = this.simulation.Warehouse.Appointments.FirstOrDefault(x => x.VehicleCode == inputOrder.VehicleCode && x.AppointmentDate == inputOrder.AppointmentDate);
                workOrder.AppointmentEndDate = appointment.EndDate;

                //Si no tiene ItemPlanning marcar como IsOnTime
                if (!workOrder.ItemPlanning.Any())
                {
                    SetWorkOrderOnTime(workOrder.Id, isVehicle);
                }

                else
                {
                    DateTime referenceDate;
                    if (!isVehicle) referenceDate = workOrder.InputOrder.AppointmentDate;
                    else
                    {
                        var warehouseId = workOrder.InputOrder.WarehouseId;
                        referenceDate = appointment.EndDate;
                    }

                    if (workOrder.InputOrder.IsOutbound)
                    {

                        var loadingItem = workOrder.ItemPlanning
                            .OrderBy(x => x.EndDate)
                            .LastOrDefault();

                        // Si es Outbound y no hay Loading o si la InitDate del proceso Loading cumple la condición, marcar como IsOnTime
                        if (loadingItem == null || loadingItem.EndDate <= referenceDate)
                        {
                            SetWorkOrderOnTime(workOrder.Id, isVehicle);
                        }
                    }

                    //Si es inbound y cumple la condición, marcar como IsOnTime
                    else
                    {
                        var inboundItem = workOrder.ItemPlanning
                            .FirstOrDefault(x => x.Process?.Type == ProcessType.Inbound);

                        if (inboundItem == null || inboundItem.EndDate <= referenceDate)
                        {
                            SetWorkOrderOnTime(workOrder.Id, isVehicle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the IsOnTime boolean
        /// </summary>
        /// <param name="workOrderId">Id of the workorder to set the boolean</param>
        /// <param name="isVehicle">True if for IsOnTime for a vehicle, False if for appointment date</param>
        private void SetWorkOrderOnTime(Guid workOrderId, bool isVehicle)
        {
            var workOrderPlanning = this.simulation.PlanningReturn.WorkOrderPlanning
                .FirstOrDefault(x => x.Id == workOrderId);

            if (workOrderPlanning != null)
            {
                if (isVehicle)
                {
                    workOrderPlanning.IsInVehicleTime = true;
                    workOrderPlanning.IsOnTime = true;
                }
                else workOrderPlanning.IsOnTime = true;
            }
        }
        #endregion

        #endregion
    }
}
