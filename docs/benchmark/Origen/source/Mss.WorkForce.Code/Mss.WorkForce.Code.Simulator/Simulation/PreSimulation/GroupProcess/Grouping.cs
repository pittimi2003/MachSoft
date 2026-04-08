using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Simulator.Core.Layout;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Appointments;
using Mss.WorkForce.Code.Simulator.Simulation.Simulator.Deliveries;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess
{
    /// <summary>
    /// Defines a group of processes
    /// </summary>
    public class Grouping
    {
        #region Variables
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public List<Grouping>? AssociatedProcesses { get; set; }
        public Process Process { get; set; }
        public int Count { get; set; }
        public int Duration { get; set; }
        public int? EstimatedPriority { get; set; }
        public int? OnTimePriority { get; set; }
        public int? OrderPriority { get; set; }
        public int? MarginPriority { get; set; }
        public DateTime StartWorkingDate { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPaused { get; set; }
        public Resource AssignedWorker { get; set; }
        public Resource AssignedEquipment { get; set; }
        public int Position { get; set; }
        public Guid? AssignedPickingProcess { get; set; }
        public int? ProcessesLeftToUnlock { get; set; }
        public Models.Models.Zone? SelectedStation { get; set; }
        public ItemPlanningReturn? ItemPlanningReturn { get; set; }
        public WarehouseProcessPlanningReturn? WarehouseProcessPlanningReturn { get; set; }
        public double Containers { get; set; }
        public DateTime? WaitingForResource { get; set; }
        public DateTime? MaxOnTimeDate { get; set; }
        public DateTime? MaxMarginDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Appointment? Appointment { get; set; }
        public Deliveries? Deliveries { get; set; }
        #endregion

        #region Constructor
        public Grouping(Guid? orderId, Process process, int count, int position, double containers, DateTime? appointmentDate, int? estimatedPriority, int? orderPriority,
            DateTime? creationDate, DateTime? releaseDate, Appointment? appointment, Deliveries? deliveries, Guid? assignedPickingProcess = null, int? processesLeftToUnlock = null)
        {
            this.Id = Guid.NewGuid();
            this.OrderId = orderId;
            this.Process = process;
            this.Count = count;
            this.IsActive = true;
            this.Position = position;
            this.IsPaused = false;
            this.AssignedPickingProcess = assignedPickingProcess;
            this.ProcessesLeftToUnlock = processesLeftToUnlock;
            this.Containers = containers;
            this.AppointmentDate = appointmentDate;
            this.EstimatedPriority = estimatedPriority;
            this.OrderPriority = orderPriority;
            this.CreationDate = creationDate;
            this.ReleaseDate = releaseDate;
            this.Appointment = appointment;
            this.Deliveries = deliveries;
            this.OnTimePriority = 0;
            this.AssociatedProcesses = new();
        }

        #endregion
    }
}
