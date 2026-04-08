using Mss.WorkForce.Code.Simulator.Core.Orders;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Appointments;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Deliveries;

namespace Mss.WorkForce.Code.Simulator.Core.Layout
{
    /// <summary>
    /// Defines a warehouse.
    /// </summary>
    public class Warehouse
    {
        #region Variables
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
        public List<OrderLine> OrderLines { get; set; }
        public List<Route> Routes { get; set; }
        public List<Appointment> Appointments { get; set; }
        public List<Deliveries> Deliveries { get; set; }
        #endregion

        #region Constructor
        public Warehouse(Guid id, string name, List<Order> orders, List<OrderLine> orderLines, List<Route> routes)
        {
            this.Id = id;
            this.Name = name;
            this.Orders = orders;
            this.OrderLines = orderLines;
            this.Routes = routes;
            this.Appointments = new List<Appointment>();
            this.Deliveries = new List<Deliveries>();
        }
        #endregion
    }
}
