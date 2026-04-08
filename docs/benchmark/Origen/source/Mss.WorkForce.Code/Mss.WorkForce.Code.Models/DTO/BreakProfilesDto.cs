using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Models.Attributtes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class BreakProfilesDto : PenelEditorOperations
    {
        public Guid Id { get; set; }

        [DisplayAttributes(index: 1, caption: "Name", isVisible: true, required: true, fieldType: ComponentType.TextBox)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string Name { get; set; }

        [DisplayAttributes(index: 2, caption: "Type", isVisible: true, required: true, fieldType: ComponentType.TextBox)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string Type { get; set; }

        //[DisplayAttributes(index: 3, caption: "Allow Inbound Flow", isVisible: true, required: true, fieldType: ComponentType.DropDownYesNo)]
        //public bool? AllowInInboundFlow { get; set; }

        //[DisplayAttributes(index: 4, caption: "Allow Outbound Flow", isVisible: true, required: true, fieldType: ComponentType.DropDownYesNo)]
        //public bool? AllowInOutboundFlow { get; set; }

        public Guid WarehouseId { get; set; }

        public OperationType DataOperationType { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public object GetProperty(string propertyName)
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            return propertyInfo?.GetValue(this);
        }
    }
}
