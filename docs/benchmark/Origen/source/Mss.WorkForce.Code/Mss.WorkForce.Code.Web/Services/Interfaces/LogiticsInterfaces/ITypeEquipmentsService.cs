using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface ITypeEquipmentsService
    {
        #region Methods

        Task DeleteItems(List<TypeEquipmentsDto> lstItems);
        IEnumerable<TypeEquipmentsDto> GetItems();
        Task UpdateItems(List<TypeEquipmentsDto> lstItems);
        Task AddItem(TypeEquipmentsDto item);

        #endregion
    }
}
