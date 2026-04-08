using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.WMSSimulator
{
    public class DataBaseResponse
    {
        public List<Warehouse>? Warehouses { get; set; }
        public List<Dock>? Docks { get; set; }
        public List<Process>? Processses { get; set; }
        public List<InputOrder>? InputOrders { get; set; }
        public List<OrderPriority>? OrderPriority { get; set; }
        public List<OrderSchedule>? OrderSchedules { get; set; }
        public List<Worker>? Workers { get; set; }
        public List<EquipmentGroup>? EquipmentGroups { get; set; }
        public List<Step>? Steps { get; set; }
        public List<Planning>? Plannings { get; set; }
        public List<WorkOrderPlanning>? WorkOrderPlannings { get; set; }
        public List<ItemPlanning>? ItemPlannings { get; set; }
        public List<Picking>? Pickings { get; set; }
        public List<Zone>? Zones { get; set; }
    }
}
