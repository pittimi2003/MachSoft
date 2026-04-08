using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;
using TimeZone = Mss.WorkForce.Code.Models.Models.TimeZone;


namespace Mss.WorkForce.Code.Models.DTO
{
    public class WarehouseDto
    {
        [DisplayAttributes(required: true,isVisible: false, isVisibleDefault:false)]
        [Key]
        public Guid Id { get; set; }
        
        [DisplayAttributes(1, "Code", true,ComponentType.TextBox,"",GroupTypes.None, false, isVisibleDefault: false)]
        [UniqueAttributes]
        //[ValidationAttributes(ValidationType.NoSpecialCharacters)]
        public string Code { get; set; }

        [DisplayAttributes(2, "Name", true,ComponentType.TextBox, isVisibleDefault:true)]
        public string Name { get; set; }

        [DisplayAttributes(3, "Time zone", true, ComponentType.DropDown, isVisibleDefault: true)]
        [MultiEditAttributes]
        public TimeZone TimeZone { get; set; }

        [DisplayAttributes(4, "Description", false, ComponentType.CommetText, isVisibleDefault: true)]
        public string Description { get; set; }

        [DisplayAttributes(5, "Address", false, ComponentType.None, "Address", GroupTypes.Accordion)]
        public Adress Adress { get; set; }

        [DisplayAttributes(6, "Contact", false, ComponentType.None, "Contact", GroupTypes.Accordion)]
        public Contact Contact { get; set; }

        public string Mode { get; set; }

        public static WarehouseDto NewDto()
        {
            return new WarehouseDto
            {
                Id = Guid.NewGuid(),
                Code = string.Empty,
                Name = string.Empty,
                TimeZone = new TimeZone(),
                Description = string.Empty,
                Adress = Adress.NewDto(),
                Contact = Contact.NewDto(),
                Mode = "Manual",
            };
        }
    }
}


