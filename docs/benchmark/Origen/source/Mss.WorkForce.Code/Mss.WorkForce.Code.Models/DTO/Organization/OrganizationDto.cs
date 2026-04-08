using System.ComponentModel.DataAnnotations;
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.DTO
{
    [Valid]
    public class OrganizationDto : ICustomValidation
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Code", true, ComponentType.TextBox, isVisible: false)]
        [ValidationAttributes(ValidationType.NoSpecialCharacters)]
        public string Code { get; set; }

        [DisplayAttributes(2, "Name", true)]
        public string Name { get; set; }

        [DisplayAttributes(3, "Description", false, ComponentType.CommetText)]
        public string? Description { get; set; }

        [DisplayAttributes(4, "Logo", false, ComponentType.Image)]
        public string? Logo { get; set; }

        [DisplayAttributes(7, "Format and language settings", false, ComponentType.None, "Format and language settings", GroupTypes.Accordion)]
        public RegionalSettingsOrganization RegionalSettings { get; set; }

        [DisplayAttributes(6, "Address", false, ComponentType.None, "Address", GroupTypes.Accordion)]
        public Adress Adress { get; set; }

        [DisplayAttributes(7, "Contact", false, ComponentType.None, "Contact", GroupTypes.Accordion)]
        public Contact Contact { get; set; }

        [DisplayAttributes(13, "Warehouses", true, ComponentType.Link, "Warehouses", GroupTypes.Accordion)]
        public List<WarehouseDto> Warehouses { get; set; }



        public static OrganizationDto NewDto()
        {
            return new OrganizationDto
            {
                Id = Guid.NewGuid(),
                Code = string.Empty,
                Name = string.Empty,
                Description = string.Empty,
                Logo = string.Empty,
            };
        }

        public ValidationResult CustomValidation()
        {
            if (this.RegionalSettings.DecimalSeparator.Name == this.RegionalSettings.ThousandsSeparator.Name)
                return new ValidationResult(
                           "Decimal separator must be different from thousands separator.",
                           new[]  {$"{nameof(RegionalSettings)}.{nameof(RegionalSettings.DecimalSeparator)}",
                                   $"{nameof(RegionalSettings)}.{nameof(RegionalSettings.ThousandsSeparator)}"
                                   });

            return ValidationResult.Success;

        }
    }
}
