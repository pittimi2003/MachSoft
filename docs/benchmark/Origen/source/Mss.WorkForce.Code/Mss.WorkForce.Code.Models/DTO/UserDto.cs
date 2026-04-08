using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;


namespace Mss.WorkForce.Code.Models.DTO
{
    [Valid]
    public class UserDto : ICustomValidation
    {

        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Code", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        [ValidationAttributes(ValidationType.NoSpecialCharacters)]
        [UniqueAttributes]
        public required string Code { get; set; }

        [DisplayAttributes(2, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Name { get; set; }

        [DisplayAttributes(3, "Last name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Lastname { get; set; }

        [DisplayAttributes(5, "Enabled", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsEnabled { get; set; }

        public bool IsActive { get; set; }

        [DisplayAttributes(7, "Password", true, ComponentType.PassWord)]
        [ValidationAttributes(ValidationType.AllExceptWhiteSpace)]
        public required string Password { get; set; }

        [DisplayAttributes(8, "Warehouses", true, ComponentType.WarehouseGrid, isVisibleDefault: true)]
        public ICollection<Warehouse> Warehouses { get; set; }

        [DisplayAttributes(6, "Format and language settings", false, ComponentType.None, "Format and language settings", GroupTypes.Accordion)]
        public RegionalSettings RegionalSettings { get; set; }

        public DateTime? LastAccessDate { get; set; }

        public DateTime CreationDate { get; set; }

        public Guid OrganizationId { get; set; }

        public OrganizationDto Organization { get; set; }

        [DisplayAttributes(12, "", false, ComponentType.IdWareHouse, "", GroupTypes.None, false, true, false)]
        public Guid? WarehouseDefaultId { get; set; }


        public string WarehouseCodes => Warehouses != null ? string.Join(",", Warehouses.Select(x => x.Name)) : "";

        public static UserDto NewDto()
        {
            return new UserDto
            {
                Id = Guid.NewGuid(),
                Code = string.Empty,
                Name = string.Empty,
                Lastname = string.Empty,
                Password = string.Empty,
                RegionalSettings = new RegionalSettings
                {
                    DecimalSeparator = new DecimalSeparator(),
                    ThousandsSeparator = new ThousandsSeparator(),
                    DateFormat = new DateFormat(),
                    HourFormat = new HourFormat(),
                    Language = new Language(),
                },
                LastAccessDate = DateTime.MinValue,
                Organization = OrganizationDto.NewDto(),
            };
        }

        public ValidationResult CustomValidation()
        {
            if (this.RegionalSettings.DecimalSeparator != null && this.RegionalSettings.ThousandsSeparator != null)
            {
                if (this.RegionalSettings.DecimalSeparator.Name == this.RegionalSettings.ThousandsSeparator.Name)
                    return new ValidationResult(
                               "Decimal separator must be different from thousands separator.",
                               new[]{$"{nameof(RegionalSettings)}.{nameof(RegionalSettings.DecimalSeparator)}",
                                   $"{nameof(RegionalSettings)}.{nameof(RegionalSettings.ThousandsSeparator)}"
                                   });

            }

            return ValidationResult.Success;

        }
    }
}