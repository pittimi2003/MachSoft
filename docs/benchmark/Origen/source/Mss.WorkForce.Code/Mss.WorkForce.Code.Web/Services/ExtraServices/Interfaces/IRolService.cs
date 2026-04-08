using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
namespace Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces
{
    public interface IRolService
    {
        #region Methods

        Task DeleteRolDto(List<RolDto> lstRolDto);
        IEnumerable<RolDto> GetRol();
        RolDto GetRolById(Guid rolDtoId);
        Task UpdateRol(List<RolDto> lstRolDto);
        Task AddRol(RolDto areaDto);

        #endregion
    }
}
