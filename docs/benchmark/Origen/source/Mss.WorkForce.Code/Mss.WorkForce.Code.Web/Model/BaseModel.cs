using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DTO.Enums;

namespace Mss.WorkForce.Code.Web.Model
{
    public class BaseModel : DisplayAttributes
    {
        public BaseModel(DisplayAttributes displayAttributes) : base(displayAttributes)
        {
            // Copiar las propiedades específicas de DisplayAttributes
            MinValue = displayAttributes.MinValue;
            MaxValue = displayAttributes.MaxValue;
        }

        public string FieldName { get; set; }
        public List<object> InitialValue { get; set; }
        public IEnumerable<SelectItemComboBox>? Options { get; set; }
        public List<SelectItemEnum>? EnumItems { get; set; }
        public bool IsMultiEditable {  get; set; }
        public bool IsDropDownCascade { get; set; } = false;
        public DataSourceDropdownType DataSourceTypeCascade { get; set; }
        public string PropertyNameCascade { get; set; }     
        public ValidationType ValidationType {  get; set; }
        public bool IsUnique { get; set; }    
        public bool AlignRight { get; set; }

	}
}
