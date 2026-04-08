using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
namespace Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces
{
    public interface IAreaService
    {
        #region Methods

        Task DeleteAreaDto(List<AreaDto> lstAreaDto);
        IEnumerable<AreaDto> GetAreaDto();
        AreaDto GetAreaById(Guid areaDtoId);
        Task UpdateArea(List<AreaDto> lstAreaDto);
        Task AddArea(AreaDto areaDto);

        #endregion
    }
}
