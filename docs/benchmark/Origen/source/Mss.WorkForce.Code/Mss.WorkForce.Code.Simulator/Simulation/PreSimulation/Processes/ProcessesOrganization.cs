using Mss.WorkForce.Code.Simulator.Core.Orders;
using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.TimeAssignment;
using static Mss.WorkForce.Code.Simulator.Simulation.Simulation;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.Processes
{
    public class ProcessesOrganization
    {
        #region Variables
        private Simulation simulation;
        #endregion

        #region Constructor
        public ProcessesOrganization(Simulation simulation)
        {
            this.simulation = simulation;
        }
        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Take the already closed processes of an order from its simulation
        /// </summary>
        public void AdjustPendingProcesses()
        {
            foreach (var order in this.simulation.Warehouse.Orders.Where(x => x.Status == OrderStatus.Released || x.Status == OrderStatus.Paused))
            {
                bool positionRemoved = false;
                int? positionValue = null;
                // Vemos si tenemos algo de estas ordenes en la tabla de procesos cerrados
                // Si es asi, restamos los procesos que tengamos a los simulados
                // Si no, continuamos con todo lo simulado

                if (this.simulation.Data.InputOrderProcessClosing.Any(x => x.InputOrder == order.Code))
                {
                    var closedProcesses = this.simulation.Data.InputOrderProcessClosing.Where(x => x.InputOrder == order.Code)
                        .GroupBy(x => x.ProcessType).Select(x => new CustomClosed
                        {
                            Type = x.Key,
                            Total = x.Sum(m => m.NumProcesses).GetValueOrDefault(0)
                        }).ToList();

                    // Vamos a ver cuantos de cada tipo hay cerrados y cuantos tenemos simulados
                    // Si tenemos más cerrados que simulados, directamente los dejamos a 0 y desbloqueamos los procesos siguientes y hacemos la resta que corresponda
                    // Si no tenemos más, solo hacemos la resta y continuamos.
                    // Más adelante, al añadir al planningData las cosas cerradas, se añadirán estos procesos ya cerrados
                    List<Grouping> originalGropuing = new List<Grouping>();
                    foreach (var process in order.Grouping)
                        originalGropuing.Add(process);

                    foreach (var process in originalGropuing)
                    {
                        if (closedProcesses.Select(x => x.Type).Contains(process.Process.ProcessType))
                        {
                            var first = order.Grouping.FirstOrDefault(x => x.Process.ProcessType == process.Process.ProcessType);
                            var singleTime = first.Duration / first.Count;

                            var number = closedProcesses.FirstOrDefault(x => x.Type == first.Process.ProcessType).Total;

                            if (number >= order.Grouping.FirstOrDefault(x => x.Id == first.Id).Count || process.Process.ProcessType == "Inbound")
                            {
                                closedProcesses.FirstOrDefault(x => x.Type == first.Process.ProcessType).Total -= order.Grouping.FirstOrDefault(x => x.Id == first.Id).Count;
                                positionRemoved = true;
                                positionValue = first.Position;
                                order.Grouping.FirstOrDefault(x => x.Id == first.Id).Count = 0;
                                order.Grouping = order.Grouping.Where(x => x.Count > 0).ToList();

                                AdjustPositionAndDates(positionRemoved, order, positionValue.GetValueOrDefault());
                            }
                            else
                            {
                                order.Grouping.FirstOrDefault(x => x.Id == first.Id).Count -= number;
                                order.Grouping.FirstOrDefault(x => x.Id == first.Id).Duration -= singleTime;
                                closedProcesses.FirstOrDefault(x => x.Type == first.Process.ProcessType).Total = 0;
                                order.Grouping = order.Grouping.Where(x => x.Count > 0).ToList();

                                AdjustPositionAndDates(positionRemoved, order, positionValue.GetValueOrDefault());
                            }

                            foreach (var p in order.Grouping) p.AssociatedProcesses = order.Grouping;

                            closedProcesses = closedProcesses.Where(x => x.Total > 0).ToList();
                            positionRemoved = false;
                            positionValue = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the order processes to the list of processes
        /// <param name="doPlanning">Object of the class DoPlanning to generate the processes.</param>
        /// <param name="rand">Instance of Random</param>
        /// </summary>
        public void AddOrderProcesses(DoPlanning doPlanning, Random rand)
        {
            foreach (var groupings in this.simulation.Warehouse.Orders.Select(x => x.Grouping))
                foreach (var grouping in groupings)
                    this.simulation.Processes.Add(grouping);

            AddWarehouseProcesses(doPlanning, rand);
        }
        #endregion

        #region Private
        /// <summary>
        /// Adjusts the positions and the times of the order groupings
        /// </summary>
        /// <param name="positionRemoved">True if a position is removed, false if not</param>
        /// <param name="order">Order the processes belong to</param>
        /// <param name="positionValue">Deleted position</param>
        private void AdjustPositionAndDates(bool positionRemoved, Order order, int? positionValue)
        {
            if (positionRemoved && positionValue != null)
            {
                order.Grouping = order.Grouping.OrderBy(x => x.Position).ToList();

                if (this.simulation.Data.InputOrder.FirstOrDefault(x => x.Id == order.Id).IsOutbound)
                {
                    ForPositions(order, positionValue.Value);
                    ForDates(order, this.simulation.StartSimulationDate);
                }
                else
                {
                    ForPositions(order, positionValue.Value);

                    var inputOrder = this.simulation.Data.InputOrder.FirstOrDefault(x => x.Id == order.Id);
                    var appointment = this.simulation.Warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == inputOrder.AppointmentDate && x.VehicleCode == inputOrder.VehicleCode);
                    var date = appointment.StartDate;

                    if (date > this.simulation.StartSimulationDate) ForDates(order, date);
                    else ForDates(order, this.simulation.StartSimulationDate); 
                }
            }
            else
            {
                order.Grouping = order.Grouping.OrderBy(x => x.Position).ToList();

                if (this.simulation.Data.InputOrder.FirstOrDefault(x => x.Id == order.Id).IsOutbound) ForDates(order, this.simulation.StartSimulationDate);
                else
                {
                    var inputOrder = this.simulation.Data.InputOrder.FirstOrDefault(x => x.Id == order.Id);
                    var appointment = this.simulation.Warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == inputOrder.AppointmentDate && x.VehicleCode == inputOrder.VehicleCode);
                    var date = appointment.StartDate;

                    if (date > this.simulation.StartSimulationDate) ForDates(order, date);
                    else ForDates(order, this.simulation.StartSimulationDate);
                }
            }
        }

        /// <summary>
        /// Changes the postitions in order to adjust the new processes to the simulation.
        /// </summary>
        /// <param name="order">Order the processes belong to</param>
        /// <param name="positionValue">Deleted position</param>
        private void ForPositions(Order order, int positionValue)
        {
            // A partir de la posición que hayamos quitado, vamos a restar una al resto
            // Si la posición que hemos quitado es la 0, tenemos que poner tambien las flags correctas

            if (!order.Grouping.Any(x => x.Position == positionValue))
            {
                for (int i = 0; i < order.Grouping.Count(); i++)
                {
                    if (order.Grouping[i].Position > positionValue)
                        order.Grouping[i].Position--;

                    if (positionValue == 0 && order.Grouping[i].Position == 0)
                    {
                        order.Grouping[i].IsActive = true;
                        order.Grouping[i].IsPaused = false;
                    }
                }
            }
        }

        /// <summary>
        /// Changes the dates in order to adjust the new processes to the simulation.
        /// </summary>
        /// <param name="order">Order the processes belong to</param>
        /// <param name="date">Initial start working date</param>
        private void ForDates(Order order, DateTime date)
        {
            for (int i = 0; i < order.Grouping.Count(); i++)
            {
                if (order.Grouping[i].Process.ProcessType != ProcessType.Loading)
                {
                    if (order.Grouping[i].Position == 0)
                        order.Grouping[i].StartWorkingDate = date;
                    else
                        order.Grouping[i].StartWorkingDate = order.Grouping[i - 1].StartWorkingDate.AddSeconds(order.Grouping[i - 1].Duration);
                }
            }
        }

        /// <summary>
        /// Generates the warehouse processes
        /// <param name="doPlanning">Object of the class DoPlanning to generate the processes.</param>
        /// <param name="rand">Instance of Random</param>
        /// </summary>
        private void AddWarehouseProcesses(DoPlanning doPlanning, Random rand)
        {
            this.simulation.Processes.UnionWith(CreateReplenishments(doPlanning));
            this.simulation.Processes.UnionWith(CreateCustomWarehouseProcesses(doPlanning, rand));
        }

        /// <summary>
        /// Creates the necessary replenishment processes based on the picking processes found.
        /// </summary>
        /// <param name="doPlanning">Object of the class DoPlanning to generate the processes.</param>
        /// <returns>Returns the list of replenishment processes</returns>
        private HashSet<Grouping> CreateReplenishments(DoPlanning doPlanning)
        {
            HashSet<Grouping> processes = new HashSet<Grouping>();

            var replenishmentProcesses = this.simulation.Processes.Where(x => x.Process.ProcessType == ProcessType.Picking)
                .Join(this.simulation.Data.Process.Where(x => x.Type == ProcessType.Replenishment), p => p.Process.Area.Id, r => r.AreaId, (p, r) => new
                {
                    PickingId = p.Process.Id,
                    ReplenishmentId = r.Id
                })
                .GroupBy(x => (x.PickingId, x.ReplenishmentId)).Select(x => new
                {
                    PickingId = x.Key.PickingId,
                    Count = this.simulation.Processes.Where(m => m.Process.Id == x.Key.PickingId).Sum(x => x.Count),
                    ReplenishmentId = x.Key.ReplenishmentId,
                })
                .Join(this.simulation.Data.Replenishment, p => p.ReplenishmentId, r => r.ProcessId, (p, r) => new
                {
                    PickingId = p.PickingId,
                    Count = p.Count,
                    ReplenishmentId = p.ReplenishmentId,
                    Percentage = Convert.ToDouble(r.Percentage) / 100
                });

            foreach (var replenishmentProcess in replenishmentProcesses)
            {
                var countLeftToUnlock = (int)(replenishmentProcess.Count * replenishmentProcess.Percentage);
                var process = this.simulation.Data.Process.FirstOrDefault(x => x.Id == replenishmentProcess.ReplenishmentId);

                for (int i = 0; i < countLeftToUnlock; i++)
                {
                    var replenish = new Grouping
                    (
                        null,
                        new Core.Layout.Process(process.Id, process.Name, process.Area, process.Type, process.IsInitProcess, process.IsWarehouseProcess, 0),
                        1,
                        0,
                        0,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        replenishmentProcess.PickingId,
                        (int)(replenishmentProcess.Count / countLeftToUnlock)
                    );

                    replenish.IsPaused = true;

                    var steps = this.simulation.Data.Step.Where(x => x.ProcessId == replenish.Process.Id);
                    replenish.Duration = doPlanning.CalculateProcessDuration(steps, replenish);

                    processes.Add(replenish);
                }
            }

            return processes;
        }

        /// <summary>
        /// Creates the custom warehouse processes taking into account the probability of happening.
        /// </summary>
        /// <param name="doPlanning">Object of the class DoPlanning to generate the processes.</param>
        /// <param name="rand">Instance of Random</param>
        /// <returns>Returns the list of custom warehouse processes</returns>
        private HashSet<Grouping> CreateCustomWarehouseProcesses(DoPlanning doPlanning, Random rand)
        {
            HashSet<Grouping> result = new HashSet<Grouping>();

            // Sacamos los procesos custom tipo warehouse que no sean replenishment
            var warehouseCustomProcesses = this.simulation.Data.CustomProcess
                .Where(x => x.Process.IsWarehouseProcess && x.Process.Type != ProcessType.Replenishment && x.NumPossibleTimes > 0);

            foreach (var warehouseCustomProcess in warehouseCustomProcesses)
            {
                var uniformDistribution = UniformDistribution.Distribute(warehouseCustomProcess.InitHour.GetValueOrDefault(0), warehouseCustomProcess.EndHour.GetValueOrDefault(0), (int)warehouseCustomProcess.NumPossibleTimes.GetValueOrDefault(1));

                for (int i = 0; i < warehouseCustomProcess.NumPossibleTimes; i++)
                {
                    // Lanzamos la moneda y vemos si entra o no en la lista
                    if (rand.NextDouble() <= warehouseCustomProcess.Percentage / 100)
                    {
                        var group = new Grouping
                        (
                            null,
                            new Core.Layout.Process(warehouseCustomProcess.Process.Id, warehouseCustomProcess.Process.Name, warehouseCustomProcess.Process.Area,
                                                            warehouseCustomProcess.Process.Type, warehouseCustomProcess.Process.IsInitProcess, warehouseCustomProcess.Process.IsWarehouseProcess, 0),
                            1,
                            0,
                            0,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null
                        );

                        group.StartWorkingDate = DateTime.UtcNow.Date.AddHours(uniformDistribution[i]);
                        group.Duration = doPlanning.CalculateProcessDuration(this.simulation.Data.Step.Where(x => x.ProcessId == warehouseCustomProcess.ProcessId), group);

                        result.Add(group);
                    }
                }
            }

            return result;
        }
        #endregion

        #endregion
    }
}
