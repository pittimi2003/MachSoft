using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class ChaoticStorageDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(0, "Capacity", true, ComponentType.NumericSpin, "", GroupTypes.None, false, isVisibleDefault: true)]
        public int Capacity { get; set; }

        [DisplayAttributes(1, "Zone", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required StationCatalogueDto Station { get; set; }

        [DisplayAttributes(2, "View port", false, ComponentType.None, "", GroupTypes.None, false, isVisibleDefault: false)]
        public string ViewPort { get; set; }

        
        public static ChaoticStorageDto NewDto()
        {
            return new ChaoticStorageDto
            {
                Id = Guid.NewGuid(),
                Capacity = 0,
                ViewPort = string.Empty,
                Station = StationCatalogueDto.NewDto()
            };
        }
    }
}
