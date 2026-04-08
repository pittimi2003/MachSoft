using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class PackingPacksMode : IFillable
    {
        public Guid Id { get; set; }
        public Guid PackingId { get; set; }
        public Packing Packing { get; set; }
        public Guid PackingModeId { get; set; }
        public PackingMode PackingMode { get; set; }
        public int? NumPackages { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Packing = context.Packing.FirstOrDefault(x => x.Id == PackingId)!;
            this.PackingMode = context.PackingMode.FirstOrDefault(x => x.Id == PackingModeId)!;
        }
    }
}
