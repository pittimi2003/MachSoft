
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IBuffersService
    {

        #region Methods

        Task DeleteBuffer(List<BuffersDto> lstItems);
        IEnumerable<BuffersDto> GetBuffers();
        Task UpdateBuffer(List<BuffersDto> lstItems);
        Task AddBuffer(BuffersDto item);

        #endregion

    }
}
