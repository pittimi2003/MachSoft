using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class AlertDto : ICloneable
    {
        #region Properties

        [Key]
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        public Guid Id { get; set; }
        [DisplayAttributes(index: 1, caption: "Warehouse", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown, IsVisibleDefault = true)]
        [MultiEditAttributes]
        public LayoutWarehouseDto? Warehouse { get; set; }

        [DisplayAttributes(index: 2, caption: "Element", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDownEnum, IsVisibleDefault = true)]
        public EntityTypeEnum EntityCode { get; set; }

        [DisplayAttributes(index: 3, caption: "Notice", required: true, fieldType: ComponentType.AlertConfiguration, isVisible: true, isVisibleDefault: false, Group = "Notice")]
        public ICollection<AlertConfigurationDto> Configurations { get; set; } = new List<AlertConfigurationDto>();

        [DisplayAttributes(4, "Condition", false, ComponentType.AlertCondition, "Condition", GroupTypes.Accordion)]
        [DropDownCascade(DataSourceDropdownType.Unknown, "EntityCode")]
        public AlertFilterDto Condition { get; set; } = new();

        [DisplayAttributes(index: 12, caption: "Filters", required: false, fieldType: ComponentType.AlertFilter, isVisible: true, isVisibleDefault: true, Group = "Filters")]
        [DropDownCascade(DataSourceDropdownType.Unknown, "EntityCode")]
        public ICollection<AlertFilterDto> AlertFilters { get; set; } = new List<AlertFilterDto>();

        [DisplayAttributes(index: 13, caption: "Alert information", isVisible: true, required: false, isVisibleDefault: true, fieldType: ComponentType.CommetText, IsVisibleDefault = true, Group = "Alert")]
        public string Message { get; set; } = "Order [WMSCode] generated an alert with the conditions [Field] [Operator] [Reference]";

        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }


        #endregion

        #region Methods

        public static AlertDto NewDto()
        {
            return new AlertDto
            {
                Id = Guid.NewGuid(),
                Warehouse = LayoutWarehouseDto.NewDto()
            };
        }

        public object Clone()
        {
            var clone = new AlertDto
            {
                Id = Guid.NewGuid(),
                Warehouse = null, // lo asignas externamente
                EntityCode = this.EntityCode,
                Message = this.Message,
                Condition = new AlertFilterDto
                {
                    Id = Guid.NewGuid(),
                    FilterField = this.Condition.FilterField,
                    Operator = this.Condition.Operator,
                    FilterReference = this.Condition.FilterReference,
                    FilterFixedValue = this.Condition.FilterFixedValue,
                    IsFixed = this.Condition.IsFixed
                },
                AlertFilters = this.AlertFilters.Select(f => new AlertFilterDto
                {
                    Id = Guid.NewGuid(),
                    FilterField = f.FilterField,
                    Operator = f.Operator,
                    FilterReference = f.FilterReference,
                    FilterFixedValue = f.FilterFixedValue,
                    IsFixed = f.IsFixed
                }).ToList(),
                Configurations = this.Configurations.Select(c => new AlertConfigurationDto
                {
                    Id = Guid.NewGuid(),
                    Severity = c.Severity,
                    Type = c.Type
                }).ToList()
            };

            return clone;
        }

        #endregion
    }
}
