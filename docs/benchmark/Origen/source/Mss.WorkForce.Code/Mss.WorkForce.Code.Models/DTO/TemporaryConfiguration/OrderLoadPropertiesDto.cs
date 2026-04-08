using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    public class OrderLoadPropertiesDto : PenelEditorOperations
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(index: 0, caption: "Name", isVisible: false, required: false, fieldType: ComponentType.TextBox)]
        public override string Name { get => base.Name; set => base.Name = value; }

        [DisplayAttributes(1, "Vehicle", true, ComponentType.DropDown, isVisibleDefault: true)]
        public CatalogEntity Vehicle { get; set; } = new CatalogEntity(EntityNamesConst.VehicleProfile);

        [DisplayAttributes(2, "Packaging", true, ComponentType.DropDown, isVisibleDefault: true)]
        public CatalogEntity Load { get; set; } = new CatalogEntity(EntityNamesConst.LoadProfile);

        [DisplayAttributes(3, "Load by vehicle", true, ComponentType.NumericSpinInt, isVisibleDefault: true, textAlignment:true)]
        public int LoadInVehicle { get; set; }

        [DisplayAttributes(4, "Order by load", true, ComponentType.NumericSpin, isVisibleDefault: true, textAlignment:true)]
        public double OrderInLoad { get; set; }
    }
}
