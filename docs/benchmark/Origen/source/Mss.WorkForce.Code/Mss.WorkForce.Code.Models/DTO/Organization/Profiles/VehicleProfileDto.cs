
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class VehicleProfileDto : PenelEditorOperations
    {
        [DisplayAttributes(required: false, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }


        [DisplayAttributes(2, "Type", true, Web.Model.Enums.ComponentType.TextBox, "", Web.Model.Enums.GroupTypes.None, false, true, true)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string Type { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not VehicleProfileDto vehicleProfile)
                return false;

            return Id == vehicleProfile.Id &&
            Name == vehicleProfile.Name &&
            Type == vehicleProfile.Type;
        }
    }
}
