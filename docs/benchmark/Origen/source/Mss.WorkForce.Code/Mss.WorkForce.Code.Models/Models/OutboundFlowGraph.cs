using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class OutboundFlowGraph : IFillable, ICloneable
    {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public int AverageItemsPerOrder { get; set; }
		public int AverageLinesPerOrder { get; set; }
		public bool? Group { get; set; }
		public bool? PartialClosed { get; set; }
		public Guid WarehouseId { get; set; }
		public required Warehouse Warehouse { get; set; }
        public double? SecurityLoadTime { get; set; }
        public string? ViewPort { get; set; }
        public double? MaxVehicleTime { get; set; }
        public DockSelectionStrategy? DockSelectionStrategy { get; set; }
        public Guid? DockSelectionStrategyId { get; set; }

        public Guid? FlowId { get; set; }

        public Flow? Flow { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
            this.DockSelectionStrategy = context.DockSelectionStrategies.FirstOrDefault(x => x.Id == DockSelectionStrategyId);
            this.Flow = context.Flow.FirstOrDefault(x => x.Id == FlowId);
        }
        public object Clone()
        {
            OutboundFlowGraph clonedOutboundFlowGraph = (OutboundFlowGraph)this.MemberwiseClone();
            clonedOutboundFlowGraph.Id = Guid.NewGuid();
            return clonedOutboundFlowGraph;
        }
    }
}
