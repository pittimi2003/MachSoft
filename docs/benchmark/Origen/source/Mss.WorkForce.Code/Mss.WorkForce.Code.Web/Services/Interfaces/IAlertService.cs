using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;

namespace Mss.WorkForce.Code.Web.Services.Interfaces
{
    public interface IAlertService
    {
        Task AddAlert(AlertDto alert);

        IEnumerable<AlertDto> GetAlertManagement();

        Task UpdateAlert(List<AlertDto> alert);

        Task DeleteAlert(List<AlertDto> alerts);

        /// <summary>
        /// Función para obtener las alertas asociadas a un planning
        /// </summary>
        /// <param name="IdPlanning">Guid del planning</param>
        /// <returns>Lista de los mensajes de alertas</returns>
        Task<IEnumerable<AlertMessageDto>> GetAlertNotificationsByPlanningAsync(Guid IdPlanning);

        Task<IEnumerable<AlertMessageDto>> GetBellAlertsForAllWarehousesAsync();

        Task CloneAlert(AlertDto alert, SiteModel newWarehouse);
    }
}
