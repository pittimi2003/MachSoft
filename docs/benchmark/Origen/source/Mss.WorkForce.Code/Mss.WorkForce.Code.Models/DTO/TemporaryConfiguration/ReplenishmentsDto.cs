using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    public class ReplenishmentsDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Percentage", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Percentage { get; set; }

        [DisplayAttributes(2, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(3, "View port", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string ViewPort { get; set; }

        public static ReplenishmentsDto NewDto()
        {
            return new ReplenishmentsDto
            {
                Id = Guid.NewGuid(),
                Process= ProcessCatalogueDto.NewDto(),
            };
        }
    }
}
