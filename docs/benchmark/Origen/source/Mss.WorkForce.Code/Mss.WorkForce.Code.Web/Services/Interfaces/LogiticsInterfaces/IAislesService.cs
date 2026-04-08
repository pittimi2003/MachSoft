
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IAislesService
    {

        #region Methods

        Task DeleteAisle(List<AislesDto> lstRoute);
        IEnumerable<AislesDto> GetAisles();
        Task UpdateAisle(List<AislesDto> lstRoute);
        Task AddAisle(AislesDto Route);

        #endregion

    }
}
