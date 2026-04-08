using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class RoutesDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        [ValidationAttributes(ValidationType.AllExceptWhiteSpace)]
        public required string Name { get; set; }

        [DisplayAttributes(2, "Departure area", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required AreasCatalogueDto DepartureArea { get; set; }


        [DisplayAttributes(3, "Arrival area", true, ComponentType.DropDown, isVisibleDefault: true)]
        public  required AreasCatalogueDto ArrivalArea { get; set; }

        [DisplayAttributes(4, "Bidirectional", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool Bidirectional { get; set; }

        [DisplayAttributes(4, "Time min", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int TimeMin { get; set; } = 0;


        public static RoutesDto NewDto()
        {
            return new RoutesDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                ArrivalArea = AreasCatalogueDto.NewDto(),
                DepartureArea = AreasCatalogueDto.NewDto(),
            };
        }

    }
}
