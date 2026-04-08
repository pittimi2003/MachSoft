using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface ISimulationLogicService
    {
        #region Methods
        Task UpdateProcessPriorityOrder(List<ProcessPriorityOrderDto> lstProcessPriorityOrder);
        Task UpdateOrderPriority(List<OrderPriorityDto> lstProcessPriorityOrder);
        Task UpdateSLAConfig(SLAConfigDto slaConfig);
        IEnumerable<SLAConfigDto> GetSLAConfigs();
        double? GetPlanning(Guid planningId, Guid warehouseId);
        double? GetLastPlanning(Guid warehouseId);
        #endregion
    }
}
