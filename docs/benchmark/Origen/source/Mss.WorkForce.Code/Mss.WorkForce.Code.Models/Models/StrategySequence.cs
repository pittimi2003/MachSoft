using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class StrategySequence : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string StrategyCode { get; set; }
        public bool IsActive { get; set; }
        public int Sequence { get; set; }
        public required string Comparation { get; set; }
        public Guid StrategyId { get; set; }
        public required Strategy Strategy { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Strategy = context.Strategies.FirstOrDefault(x => x.Id == StrategyId)!;
        }
        public object Clone()
        {
            StrategySequence clonedStrategySequence = (StrategySequence)this.MemberwiseClone();
            clonedStrategySequence.Id = Guid.NewGuid();
            return clonedStrategySequence;
        }
    }
}
