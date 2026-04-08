using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services.Interfaces
{
    public interface IPivotGridService
    {
        IEnumerable<ItemPlanning> GetPlanningData(Guid planningID);
        Task<IEnumerable<ItemPlanning>> GetPlanningForWarehouse(Guid warehouseID);
    }
}
