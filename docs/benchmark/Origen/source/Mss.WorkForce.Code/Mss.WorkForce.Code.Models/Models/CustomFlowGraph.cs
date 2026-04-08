using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class CustomFlowGraph: IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid? FlowId { get; set; }

        public Flow? Flow { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Flow = context.Flow.FirstOrDefault(x => x.Id == FlowId);
        }
        public object Clone()
        {
            CustomFlowGraph clonedCustomFlowGraph = (CustomFlowGraph)this.MemberwiseClone();
            clonedCustomFlowGraph.Id = Guid.NewGuid();
            return clonedCustomFlowGraph;
        }
    }
}
