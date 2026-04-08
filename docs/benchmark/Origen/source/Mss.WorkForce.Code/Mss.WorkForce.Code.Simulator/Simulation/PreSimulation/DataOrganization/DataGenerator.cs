using Mss.WorkForce.Code.Simulator.Core.Layout;
using Route = Mss.WorkForce.Code.Simulator.Core.Layout.Route;
using Mss.WorkForce.Code.Simulator.Core.Orders;
using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Models.ModelSimulation;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.DataOrganization
{
    public class DataGenerator
    {
        #region Variables
        private DataSimulatorTablaRequest data;
        private DateTime simulationTime;
        #endregion

        #region Constructor
        public DataGenerator(DataSimulatorTablaRequest data, DateTime simulationTime)
        {
            this.data = data;
            this.simulationTime = simulationTime;
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Reorganice the database data into the warehouses for the simulation.
        /// </summary>
        /// <param name="rand">Instance of Random</param>
        /// <returns>List of the warehouses with their properties.</returns>
        public Warehouse WarehousesGenerator(Random rand)
        {
            // Rutas entre áreas del correspondiente almacén
            var routes = RouteGenerator(this.data.Warehouse);

            // Ordenes asociadas a ese almacen
            var orders = OrdersGenerator(this.data.Warehouse);

            // Lineas de ordenes asociadas a ese almacen, con el cálculo de procesos necesarios incluidos
            var orderLines = OrderLinesGenerator(orders, rand);

            return new Warehouse(this.data.Warehouse.Id, this.data.Warehouse.Name, orders, orderLines, routes);
        }
        #endregion

        #region Private

        /// <summary>
        /// Assigns the routes to a warehouse.
        /// </summary>
        /// <param name="warehouse">Warehouse where the areas belong</param>
        /// <returns>List of the routes of the warehouse.</returns>
        private List<Route> RouteGenerator(Models.Models.Warehouse warehouse)
        {
            var dijkstraRoutes = new Dijkstra(this.data);

            return dijkstraRoutes.CreateRoutes(warehouse.Id);
        }

        /// <summary>
        /// Assigns the orders that belong to a specific warehouse.
        /// </summary>
        /// <param name="warehouse">Warehouse where the areas belong</param>
        /// <returns>List of the orders assigned to the warehouse.</returns>
        private List<Order> OrdersGenerator(Models.Models.Warehouse warehouse)
        {
            List<Order> ordersList = new List<Order>();

            // Ordenes asociadas a ese almacen
            var orders = this.data.InputOrder.Where(x => x.WarehouseId == warehouse.Id) 
            .Where(x =>
                x.Status == OrderStatus.Released ||
                (x.Status == OrderStatus.Waiting && (!x.IsBlocked.GetValueOrDefault() || x.BlockDate >= this.simulationTime))
            );

            foreach (var order in orders)
                ordersList.Add(new Order(order.Id, order.WarehouseId, order.Status, order.OrderCode));

            return ordersList;
        }

        /// <summary>
        /// Assigns the order lines that belong to a specific warehouse.
        /// </summary>
        /// <param name="ordersInWarehouse">Orders assigned to the warehouse.</param>
        /// <param name="rand">Instance of Random</param>
        /// <returns>List of the order lines assigned to the warehouse.</returns>
        private List<OrderLine> OrderLinesGenerator(List<Order> ordersInWarehouse, Random rand)
        {
            List<OrderLine> orderLinesList = new List<OrderLine>();

            // Lineas de ordenes asociadas al almacén
            var orderLines = this.data.InputOrderLine.Where(x => ordersInWarehouse.Select(x => x.Id).Contains(x.InputOutboundOrderId));

            foreach (var orderLine in orderLines)
            {
                bool isOutbound = this.data.InputOrder.FirstOrDefault(x => x.Id == orderLine.InputOutboundOrderId).IsOutbound;

                orderLinesList.Add(new OrderLine(orderLine.Id, orderLine.InputOutboundOrderId, isOutbound));
            }

            return orderLinesList;
        }
        #endregion

        #endregion
    }
}
