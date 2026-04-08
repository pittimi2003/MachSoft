using Mss.WorkForce.Code.Simulator.Core.Orders;
using Mss.WorkForce.Code.Simulator.Helper.Constants;
using Process = Mss.WorkForce.Code.Simulator.Core.Layout.Process;
using Warehouse = Mss.WorkForce.Code.Simulator.Core.Layout.Warehouse;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess
{
    public class DoGrouping
    {
        #region Variables
        private Simulation simulation;
        #endregion

        #region Constructor
        public DoGrouping(Simulation simulation)
        {
            this.simulation = simulation;
        }
        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Makes the grouping of the different processes of a line into an order
        /// </summary>
        /// <returns>Yields a SimSharp Event</returns>
        public Warehouse GetGroupings()
        {
            if (!this.simulation.Data.OrderPriority.Any()) { Console.WriteLine("Warning. No order priority configuration found. Please, check the configuration."); }

            foreach (var order in this.simulation.Warehouse.Orders)
            {
                List<Grouping> groups = new List<Grouping>();
                var lines = this.simulation.Warehouse.OrderLines.Where(x => x.OrderId == order.Id);

                var i = 0;
                foreach (var line in lines)
                {
                    // 1. La primera linea la asignamos íntegra con las posiciones, iniciando a 0 Loading o Inbound según corresponda
                    // Viene de la selección de procesos y se van añadiendo a la lista, por lo que están ordenados según lo que haya en BBDD

                    if (i == 0)
                    {
                        groups = FirstProcessesAssignation(groups, line, order);
                        i++;
                    }

                    // Planteamos las distintas posibilidades de proceso/posicion
                    else
                    {
                        // Creamos la agrupación necesaria con la posición que corresponda y miramos cada una de las reglas para ver donde encaja
                        if (line.IsOutbound)
                        {
                            int j = line.Processes.Count - 1;
                            foreach (var process in line.Processes)
                            {
                                groups = GroupingRules(groups, process, order, j).OrderByDescending(x => x.Position).ToList();
                                j--;
                            }
                        }
                        else
                        {
                            int j = 0;
                            foreach (var process in line.Processes)
                            {
                                groups = GroupingRules(groups, process, order, j).OrderBy(x => x.Position).ToList();
                                j++;
                            }
                        }
                    }
                }

                groups = OrderOutboundPositionsForTimeCalculation(groups, lines);
                groups = PauseNoInitProcesses(groups);

                this.simulation.Warehouse.Orders.FirstOrDefault(x => x.Id == order.Id).Grouping = groups;
            }

            return this.simulation.Warehouse;
        }

        #endregion

        #region Private

        /// <summary>
        /// After using a diferent position sequence to have a determined limit, this process orders the sequence with the position 0 to the init processes.
        /// </summary>
        /// <param name="groups">Complete list of grouping to order</param>
        /// <param name="lines">IEnumerable of the OrderLines that belongs to the order of the grouping</param>
        /// <returns></returns>
        private List<Grouping> OrderOutboundPositionsForTimeCalculation(List<Grouping> groups, IEnumerable<OrderLine> lines) 
        {
            if (lines.Any(x => x.IsOutbound))
            {
                var maxPosition = groups.Max(x => x.Position);

                foreach (var group in groups) group.Position = maxPosition - group.Position;
            }

            return groups;
        }

        // TODO -> importante -> Estos métodos en un futuro deberán diferenciar si trabajamos con stock o contenedores.
        // En caso de ser contenedores, cada línea será un solo contenedor, para stock mirariamos quantity

        /// <summary>
        /// Assigns the order of positions and the counter of each process.
        /// </summary>
        /// <param name="groups">Reference grouping list</param>
        /// <param name="process">Process to add into the grouping list</param>
        /// <param name="order">Order to which que process belongs</param>
        /// <param name="j">Position of the process in the original line list of processes</param>
        /// <returns>List of grouping with the new process added</returns>
        private List<Grouping> GroupingRules(List<Grouping> groups, Process process, Order order, int j)
        {
            if (groups.Select(x => x.Process.Id).Contains(process.Id))
            {
                // Si existe el proceso y coincide su posición con la lista actual su posición 
                if (groups.FirstOrDefault(x => x.Process.Id == process.Id).Position == j)
                {
                    groups.FirstOrDefault(x => x.Process.Id == process.Id && x.Position == j).Count++;
                    groups.FirstOrDefault(x => x.Process.Id == process.Id && x.Position == j).Containers += process.Containers;
                }
                // Si tenemos proceso pero no coincide la posición
                else
                {
                    var currentPosition = groups.FirstOrDefault(x => x.Process.Id == process.Id).Position;

                    if (currentPosition > j)
                    {
                        groups.FirstOrDefault(x => x.Process.Id == process.Id).Count++;
                        groups.FirstOrDefault(x => x.Process.Id == process.Id && x.Position == j).Containers += process.Containers;
                    }
                    else
                    {
                        var upperPositionProcesses = groups.Where(x => x.Position >= j);

                        foreach (var item in upperPositionProcesses) item.Position++;

                        groups.FirstOrDefault(x => x.Process.Id == process.Id).Position = j;
                    }
                }
            }
            else
            {
                var inputOrder = this.simulation.Data.InputOrder.FirstOrDefault(x => x.Id == order.Id);
                var appointment = this.simulation.Warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == inputOrder.AppointmentDate && x.VehicleCode == inputOrder.VehicleCode);
                var referenceDate = appointment.StartDate;
                var estimatedPriority = inputOrder.IsEstimated ? Priority.EstimatedOrder : Priority.WMSOrder;
                var orderPriority = this.simulation.Data.OrderPriority.FirstOrDefault(x => x.Code == inputOrder.Priority)?.Priority;
                var delivery = this.simulation.Warehouse.Deliveries.FirstOrDefault(x => x.Id == inputOrder.DeliveryId);

                // Si no existe el proceso pero ya tenemos esa posición ocupada
                if (groups.Select(x => x.Position).Contains(j))
                    groups.Add(new Grouping(order.Id, process, 1, j, 1, referenceDate, estimatedPriority, orderPriority, inputOrder.CreationDate, inputOrder.ReleaseDate, appointment, delivery));
                // Si no tenemos proceso ni esa posición, añadimos proceso y posición
                else
                    groups.Add(new Grouping(order.Id, process, 1, j, 1, referenceDate, estimatedPriority, orderPriority, inputOrder.CreationDate, inputOrder.ReleaseDate, appointment, delivery));
            }

            return groups;
        }

        /// <summary>
        /// Creates the first grouping list with the processes of the first line.
        /// </summary>
        /// <param name="groups">List of grouping to fill with</param>
        /// <param name="line">First line found for the order whose processes we are going to insert in the list</param>
        /// <param name="order">Order which contains the line</param>
        /// <returns>List of grouping with the first set of processes added</returns>
        private List<Grouping> FirstProcessesAssignation(List<Grouping> groups, OrderLine line, Order order)
        {
            var inputOrder = this.simulation.Data.InputOrder.FirstOrDefault(x => x.Id == order.Id);
            var appointment = this.simulation.Warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == inputOrder.AppointmentDate && x.VehicleCode == inputOrder.VehicleCode);
            var referenceDate = appointment.StartDate;
            var estimatedPriority = inputOrder.IsEstimated ? Priority.EstimatedOrder : Priority.WMSOrder;
            var orderPriority = this.simulation.Data.OrderPriority.FirstOrDefault(x => x.Code == inputOrder.Priority)?.Priority;
            var delivery = this.simulation.Warehouse.Deliveries.FirstOrDefault(x => x.Id == inputOrder.DeliveryId);

            if (line.IsOutbound)
            {
                int j = line.Processes.Count - 1;
                foreach (var process in line.Processes)
                {
                    groups.Add(new Grouping(order.Id, process, 1, j, 1, referenceDate, estimatedPriority, orderPriority, inputOrder.CreationDate, inputOrder.ReleaseDate, appointment, delivery));
                    j--;
                }
            }
            else
            {
                int j = 0;
                foreach (var process in line.Processes)
                {
                    groups.Add(new Grouping(order.Id, process, 1, j, 1, referenceDate, estimatedPriority, orderPriority, inputOrder.CreationDate, inputOrder.ReleaseDate, appointment, delivery));
                    j++;
                }
            }

            return groups;
        }

        /// <summary>
        /// Set the flag IsPaused to true to all the processes whose position is not the initial
        /// </summary>
        /// <param name="groups">List of grouping</param>
        /// <returns>List of grouping with the flag set</returns>
        private List<Grouping> PauseNoInitProcesses(List<Grouping> groups)
        {
            foreach (var process in groups)
                if (process.Position != 0)
                    process.IsPaused = true;

            return groups;
        }

        #endregion

        #endregion
    }
}
