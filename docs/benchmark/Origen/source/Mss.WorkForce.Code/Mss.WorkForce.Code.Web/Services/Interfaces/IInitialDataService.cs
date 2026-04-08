using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.SignalR;
using System.Security.Claims;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IInitialDataService
    {
        #region Properties

        OrganizationDto Organization { get; }

        #endregion

        #region Methods

        void flushData();

        Datauser GetDatauser();

        Task GetDataUserLocal();

        ClaimsPrincipal SetClaims();

        Language GetLanguage();

        UserFormatOptions GetUserFormat();

        bool PullData(string user, string pass);

        void RefreshData(string user, string pass);

        bool UpdateOrganizationData();

        /// <summary>
        /// Guarda la configuración del filtro de tareas del Gantt para una vista específica.
        /// </summary>
        /// <param name="planningFilterDto">Los parámetros y la configuración del filtro a guardar.</param>
        /// <param name="viewName">El nombre de la vista del Gantt a la que se aplica el filtro.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task SaveGanttFilterForViewAsync(GanttFilterSettingsDto planningFilterDto, GanttView viewName);

        /// <summary>
        /// Carga la configuración del filtro de tareas del Gantt para una vista específica.
        /// </summary>
        /// <param name="viewName">El nombre de la vista del Gantt para la que se aplica el filtro.</param>
        /// <returns>La configuración del filtro de la vista del Gantt, o null si no se encuentra.</returns>
        Task<GanttFilterSettingsDto> GetGanttFilterForViewAsync(GanttView viewName);

        /// <summary>
        /// Elimina la configuración del filtro de tareas del Gantt para una vista específica.
        /// </summary>
        /// <param name="viewName">El nombre de la vista del Gantt de la que se desea eliminar la configuración del filtro.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task ClearGanttFilterAsync(GanttView viewName);

        Task ReloadDataUserLocal();

        #endregion
    }
}

