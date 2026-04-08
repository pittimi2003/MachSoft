using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class DockSelectionStrategy : IFillable
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public void Fill(ApplicationDbContext context)
        {
            this.Organization = context.Organizations.FirstOrDefault(x => x.Id == OrganizationId)!;
        }
    }
}
