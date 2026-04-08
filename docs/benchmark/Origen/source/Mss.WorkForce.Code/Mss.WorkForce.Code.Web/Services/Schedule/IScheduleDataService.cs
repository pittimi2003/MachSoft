using Mss.WorkForce.Code.Models;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IScheduleDataService
    {
        IEnumerable<YardResourceBase> GetSaturations(Guid planningId, YardResourceType resourceGroupType, double Offset);
    }
}
