using Mss.WorkForce.Code.Models.DBContext;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mss.WorkForce.Code.Models.Models
{
    public class InputOrder : IFillable
    {
        public Guid Id { get; set; }
        public required string OrderCode { get; set; }
        public bool IsStarted { get; set; }
        public string Status { get; set; }
        public string? Priority { get; set; }
        public bool IsOutbound { get; set; }
        public bool AllowPartialClosed { get; set; }
        public bool AllowGroup { get; set; }
        private DateTime appointmentDate;
        public DateTime AppointmentDate
        {
            get => appointmentDate;
            set => appointmentDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime? realArrivalTime;
        public DateTime? RealArrivalTime
        {
            get => realArrivalTime;
            set => realArrivalTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        private DateTime? updateDate;
        public DateTime? UpdateDate
        {
            get => updateDate;
            set => updateDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }

        private DateTime? updateDateWMS;

        public DateTime? UpdateDateWMS
        {
            get => updateDateWMS;
            set => updateDateWMS = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        public string? Carrier { get; set; }
        public string? Account { get; set; }
        public string? Supplier { get; set; }
        public string? Trailer { get; set; }
        public bool IsEstimated { get; set; }
        public Guid? AssignedDockId { get; set; }
        public Dock? AssignedDock { get; set; }
        public Guid? PreferredDockId { get; set; }
        public Dock? PreferredDock { get; set; }
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public bool? IsBlocked { get; set; }
        private DateTime? blockDate;
        public DateTime? BlockDate
        {
            get => blockDate;
            set => blockDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        public double? Progress { get; set; }
        private DateTime? endBlockDate;
        public DateTime? EndBlockDate
        {
            get => endBlockDate;
            set => endBlockDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        public DateTime CreationDate { get; set; } = DateTime.SpecifyKind(new DateTime(2000,1,1), DateTimeKind.Utc);
        public DateTime? ReleaseDate { get; set; }
        public string? VehicleCode { get; set; }
        public Guid? DeliveryId { get; set; }
        public Deliveries? Delivery { get; set; }
        [NotMapped]
        public OrderSchedule? OrderSchedule { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
            this.AssignedDock = context.Docks.FirstOrDefault(x => x.Id == AssignedDockId);
            this.PreferredDock = context.Docks.FirstOrDefault(x => x.Id == PreferredDockId);
            this.Delivery = context.Deliveries.FirstOrDefault(x => x.Id == DeliveryId);
        }


    }
}


