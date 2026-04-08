using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;

namespace Mss.WorkForce.Code.Simulator.Core.Orders
{

    /// <summary>
    /// Defines an order.
    /// </summary>
    public class Order
    {
        #region Variables
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public Guid WarehouseId { get; set; }
        public List<Grouping> Grouping { get; set; }
        #endregion

        #region Constructor
        public Order(Guid id, Guid warehouseId, string status, string code)
        {
            this.Id = id;
            this.Status = status;
            this.WarehouseId = warehouseId;
            this.Grouping = new List<Grouping>();
            this.Code = code;
        }
        #endregion
    }
}
