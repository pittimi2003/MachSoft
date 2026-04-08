using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
namespace Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces
{
    public interface IShiftService
    {
        #region Methods

        Task DeleteShiftDto(List<ShiftDto> lstShiftDto);
        IEnumerable<ShiftDto> GetShiftDto();
        ShiftDto GetShiftById(Guid shiftDtoId);
        Task UpdateShift(List<ShiftDto> lstShiftDto);
        Task AddShift(ShiftDto shiftDto);

        #endregion
    }
}
