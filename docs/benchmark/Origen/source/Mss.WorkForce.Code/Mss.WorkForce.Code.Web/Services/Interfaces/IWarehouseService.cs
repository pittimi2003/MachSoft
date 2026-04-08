using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IWarehouseService
    {

        #region Methods

        Task DeleteWarehouse(List<WarehouseDto> lstWarehouse);
        IEnumerable<WarehouseDto> GetWarehouses();
        WarehouseDto GetWarehousesById(Guid warehouseId);
        Task UpdateWarehouse(List<WarehouseDto> lstWarehouse);
        Task AddWarehouse(WarehouseDto warehouse);

        #endregion

    }
}
