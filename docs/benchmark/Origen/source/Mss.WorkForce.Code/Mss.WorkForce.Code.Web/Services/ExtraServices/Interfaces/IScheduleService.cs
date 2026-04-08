using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
namespace Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces
{
    public interface IScheduleService
    {
        #region Methods

        Task DeleteScheduleDto(List<ScheduleDto> lstScheduleDto);
        IEnumerable<ScheduleDto> GetAvailableScheduleDto();
        ScheduleDto GetScheduleDtoById(Guid ScheduleDtoId);
        Task UpdateScheduleDto(List<ScheduleDto> lstScheduleDto);
        Task AddScheduleDto(ScheduleDto ScheduleDto);

        #endregion
    }
}
