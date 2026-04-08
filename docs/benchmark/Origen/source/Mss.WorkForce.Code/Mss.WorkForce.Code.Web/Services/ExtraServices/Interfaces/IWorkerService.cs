using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
namespace Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces
{
    public interface IWorkerService
    {
        #region Methods

        Task DeleteWorkerDto(List<WorkerDto> lstWorkerDto);
        IEnumerable<WorkerDto> GetAvailableWorkerDto();
        WorkerDto GetWorkerDtoById(Guid WorkerDtoId);
        Task UpdateWorkerDto(List<WorkerDto> lstWorkerDto);
        Task AddWorkerDto(WorkerDto WorkerDto);

        #endregion
    }
}
