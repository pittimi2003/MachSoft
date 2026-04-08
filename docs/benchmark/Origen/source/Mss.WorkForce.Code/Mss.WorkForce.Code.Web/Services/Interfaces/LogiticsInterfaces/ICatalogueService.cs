using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface ICatalogueService<T>
    {

        #region Methods

        Task DeleteItems(List<T> lstItems);
        IEnumerable<T> GetItems();
        Task UpdateItems(List<T> lstItems);
        Task AddItem(T item);

        #endregion

    }
}
