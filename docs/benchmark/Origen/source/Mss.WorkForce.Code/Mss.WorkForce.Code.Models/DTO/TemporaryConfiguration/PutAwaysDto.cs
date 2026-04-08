using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System.ComponentModel.DataAnnotations;


namespace Mss.WorkForce.Code.Models
{
    public class PutAwaysDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Addition time to putaway", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double AdditionTmeToPutaway { get; set; }

        [DisplayAttributes(2, "Min hour", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double MinHour { get; set; }

        [DisplayAttributes(3, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(4, "View port", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string? ViewPort { get; set; }

        public static PutAwaysDto NewDto()
        {
            return new PutAwaysDto
            {
                Id = Guid.NewGuid(),
                AdditionTmeToPutaway = 0,
                MinHour = 0,
                Process = ProcessCatalogueDto.NewDto(),
                ViewPort = string.Empty,
            };
        }
    }
}
