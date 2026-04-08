
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IRoutesService
    {

        #region Methods

        Task DeleteRoute(List<RoutesDto> lstRoute);
        IEnumerable<RoutesDto> GetRoutes();
        Task UpdateRoute(List<RoutesDto> lstRoute);
        Task AddRoute(RoutesDto Route);

        #endregion

    }
}
