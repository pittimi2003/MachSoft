using Mss.WorkForce.Code.Models.ModelSimulation;

namespace Mss.WorkForce.Code.Simulator.Simulation.Simulator.Deliveries
{
    public class Deliveries
    {
        #region Attributes
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PackingMode { get; set; }
        public int? NumPackages { get; set; }
        public Guid? PackingZone { get; set; }
        public Guid? SelectedDockId { get; set; }
        #endregion

        #region Constructor
        public Deliveries(Guid id, string name, string packingMode)
        {
            this.Id = id;
            this.Name = name;
            this.PackingMode = packingMode;
        }
        #endregion

        #region Methods
        #region Public

        /// <summary>
        /// Creates the new deliveries entities to work with during the simulation
        /// </summary>
        /// <param name="data">Simulation data</param>
        /// <returns>List of the deliveries for the given input orders</returns>
        public static List<Deliveries> CreateGroupingDelivery(DataSimulatorTablaRequest data)
        {
            List<Deliveries> generatedDeliveries = new List<Deliveries>();
            var deliveries = data.InputOrder.Where(x => x.Delivery != null).Select(x => x.Delivery).DistinctBy(x => x.Id);

            foreach (var d in deliveries)
                generatedDeliveries.Add(new Deliveries(d!.Id, d!.Code, d!.PackagingType));

            return generatedDeliveries;
        }

        #endregion
        #endregion
    }
}
