using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    public class ProcessDirectionPropertysDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        [ValidationAttributes(ValidationType.AllExceptWhiteSpace)]
        public required string Name { get; set; }

        [DisplayAttributes(2, "Percentage", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Percentage { get; set; }

        [DisplayAttributes(3, "Init process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto InitProcess { get; set; }

        [DisplayAttributes(4, "End process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto EndProcess { get; set; }

        [DisplayAttributes(5, "Is limit stock", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsEnd { get; set; }

        public static ProcessDirectionPropertysDto NewDto()
        {
            return new ProcessDirectionPropertysDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                InitProcess=  ProcessCatalogueDto.NewDto(),
                EndProcess = ProcessCatalogueDto.NewDto()
            };
        }
    }
}
