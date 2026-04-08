using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class PostprocessProfileDto : PenelEditorOperations,IDataOperation
    {
        [DisplayAttributes(required: false, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, Web.Model.Enums.ComponentType.TextBox, "", Web.Model.Enums.GroupTypes.None, false, true, true)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string Name { get; set; }

        [DisplayAttributes(2, "Postprocess type", true, Web.Model.Enums.ComponentType.TextBox, "", Web.Model.Enums.GroupTypes.None, false, true, true)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string Type { get; set; }
        public Guid WarehouseId { get; set; }
        public OperationType DataOperationType { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not PostprocessProfileDto postprocessProfile)
                return false;

            return Id == postprocessProfile.Id &&
            Name == postprocessProfile.Name &&
            Type == postprocessProfile.Type;
        }

        public object GetProperty(string propertyName)
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            return propertyInfo?.GetValue(this);
        }
    }
}
