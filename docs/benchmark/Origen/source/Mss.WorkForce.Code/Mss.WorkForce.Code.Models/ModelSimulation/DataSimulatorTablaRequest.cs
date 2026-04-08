using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models.ModelSimulation
{
    public class DataSimulatorTablaRequest
    {
        public List<Area>? Area { get; set; }
        public Warehouse? Warehouse { get; set; }
        public List<Zone>? Zone { get; set; }
        public List<Models.Route>? Route { get; set; }
        public List<InputOrder>? InputOrder { get; set; }
        public List<InputOrderLine>? InputOrderLine { get; set; }
        public List<Process>? Process { get; set; }
        public List<ProcessDirectionProperty>? ProcessDirectionProperty { get; set; }
        public List<AvailableWorker>? AvailableWorker { get; set; }
        public List<Worker>? Worker { get; set; }
        public List<Rol>? Rol { get; set; }
        public List<RolProcessSequence>? RolProcessSequence { get; set; }
        public List<Layout>? Layout { get; set; }
        public List<Step>? Step { get; set; }
        public List<Picking>? Picking { get; set; }
        public List<Strategy>? Strategy { get; set; }
        public List<Schedule>? Schedule { get; set; }
        public List<Break>? Break { get; set; }
        public List<Shift>? Shift { get; set; }
        public List<EquipmentGroup>? EquipmentGroup { get; set; }
        public List<TypeEquipment>? TypeEquipment { get; set; }
        public List<Replenishment>? Replenishment { get; set; }
        public List<CustomProcess>? CustomProcess { get; set; }
        public List<OrderSchedule>? OrderSchedule { get; set; }
        public List<Aisle>? Aisle { get; set; }
        public List<Models.Buffer>? Buffer { get; set; }
        public List<Dock>? Dock { get; set; }
        public List<Rack>? Rack { get; set; }
        public List<DriveIn>? DriveIn { get; set; }
        public List<ChaoticStorage>? ChaoticStorage { get; set; }
        public List<AutomaticStorage>? AutomaticStorage { get; set; }
        public List<DockSelectionStrategy>? DockSelectionStrategy { get; set; }
        public List<InputOrderProcessClosing>? InputOrderProcessClosing { get; set; }
        public List<OrderLoadRatio>? OrderLoadRatio { get; set; }
        public List<OutboundFlowGraph>? OutboundFlowGraph { get; set; }
        public List<InboundFlowGraph>? InboundFlowGraph { get; set; }
        public List<LoadProfile>? LoadProfile { get; set; }
        public List<VehicleProfile>? VehicleProfile { get; set; }
        public List<ProcessPriorityOrder>? ProcessPriorityOrder { get; set; }
        public List<OrderPriority>? OrderPriority { get; set; }
        public List<YardAppointmentsNotifications>? YardAppointmentsNotifications { get; set; }
        public List<Stage>? Stage { get; set; }
        public List<AvailableDocksPerStage>? AvailableDocksPerStage { get; set; }
        public List<Deliveries>? Deliveries { get; set; }
        public List<Packing>? Packings { get; set; }
        public List<PackingPacksMode>? PackingPacksModes { get; set; }
        public DateTime? Date { get; set; }
    }
}
