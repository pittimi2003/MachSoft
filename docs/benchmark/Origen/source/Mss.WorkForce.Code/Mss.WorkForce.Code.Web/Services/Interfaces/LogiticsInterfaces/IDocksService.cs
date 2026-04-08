using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IDocksService
    {

        #region Methods

        Task DeleteItems(List<DocksDto> lstItems);
        IEnumerable<DocksDto> GetItems();
        Task UpdateItems(List<DocksDto> lstItems);
        Task AddItem(DocksDto item);

        #endregion

    }
}
