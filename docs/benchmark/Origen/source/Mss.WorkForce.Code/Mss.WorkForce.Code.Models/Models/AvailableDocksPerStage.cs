using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class AvailableDocksPerStage : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public Guid StageId { get; set; }
        public Stage Stage { get; set; }
        public Guid DockId { get; set; }
        public Dock Dock { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Stage = context.Stages.FirstOrDefault(x => x.Id == StageId)!;
            this.Dock = context.Docks.FirstOrDefault(x => x.Id == DockId)!;
        }
        public object Clone()
        {
            AvailableDocksPerStage clonedAvailableDocksPerStage = (AvailableDocksPerStage)this.MemberwiseClone();
            clonedAvailableDocksPerStage.Id = Guid.NewGuid();
            return clonedAvailableDocksPerStage;
        }
    }
}
