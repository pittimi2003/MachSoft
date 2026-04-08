using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    public class ShippingsDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Quantity", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Quantity { get; set; }

        [DisplayAttributes(2, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(3, "View port", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string ViewPort { get; set; }


        public static ShippingsDto NewDto()
        {
            return new ShippingsDto
            {
                Id = Guid.NewGuid(),
                Process= ProcessCatalogueDto.NewDto(),
            };
        }
    }
}
