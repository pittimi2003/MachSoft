using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Packing : IFillable
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public Process Process { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Process = context.Processes.FirstOrDefault(x => x.Id == ProcessId)!;
        }
    }
}
