using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
namespace Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces
{
    public interface IAvailableWorkerService
    {
        #region Methods

        Task DeleteAvailableWorker(List<AvailableWorkerDto> lstAvailableWorker);
        IEnumerable<AvailableWorkerDto> GetAvailableWorker();
        AvailableWorkerDto GetAvailableWorkerById(Guid AvailableWorkerId);
        Task UpdateAvailableWorker(List<AvailableWorkerDto> lstAvailableWorker);
        Task AddAvailableWorker(AvailableWorkerDto AvailableWorker);

        #endregion
    }
}
