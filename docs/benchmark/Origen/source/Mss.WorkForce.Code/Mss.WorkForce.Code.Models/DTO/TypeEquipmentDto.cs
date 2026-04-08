using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Models.Attributtes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class TypeEquipmentDto : PenelEditorOperations, ICloneable
    {
        
        [DisplayAttributes(1, "Equipment type ", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        [ValidationAttributes(Enums.ValidationType.None), UniqueAttributes(true)]
        public override string Name { get => base.Name; set => base.Name = value; }

        
        [DisplayAttributes(2, "Description", false, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string? Description { get; set; }
       
        [DisplayAttributes(3, "Capacity", true, ComponentType.NumericSpinInt, "", GroupTypes.None, false, isVisibleDefault: true, textAlignment: true)]
        [ValidationAttributes(Enums.ValidationType.OnlyNumbers)]
        public int Capacity { get; set; }
        
        [DisplayAttributes(4, "Number of equipment", true, ComponentType.NumericSpinInt, "", GroupTypes.None, false, isVisibleDefault: true, textAlignment: true)]
        public int Quantity { get; set; }

        [DisplayAttributes(5, "Loading wait time", false, ComponentType.NumericSpinInt, "", GroupTypes.None, false, isVisibleDefault: false, textAlignment: true, isVisible:false)]
        public int LoadingWaitTime { get; set; }
    }
}
