using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public interface IFillable
    {
        void Fill(ApplicationDbContext context);
    }
}