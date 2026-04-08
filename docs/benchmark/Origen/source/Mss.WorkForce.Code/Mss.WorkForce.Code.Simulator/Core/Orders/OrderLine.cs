using Mss.WorkForce.Code.Simulator.Core.Layout;

namespace Mss.WorkForce.Code.Simulator.Core.Orders
{
    /// <summary>
    /// Defines a line of an order.
    /// </summary>
    public class OrderLine
    {
        #region Variables
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public bool IsOutbound { get; set; }
        public int Containers { get; set; }
        public List<Process> Processes { get; set; }
        #endregion

        #region Constructor
        public OrderLine(Guid id, Guid orderId, bool isOutbound)
        {
            this.Id = id;
            this.OrderId = orderId;
            this.IsOutbound = isOutbound;
            this.Processes = new List<Process>();
            this.Containers = 1;
        }
        #endregion
    }
}
