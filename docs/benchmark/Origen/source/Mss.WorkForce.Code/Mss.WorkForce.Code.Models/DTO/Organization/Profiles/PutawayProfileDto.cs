using System.ComponentModel.DataAnnotations;
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class PutawayProfileDto : PenelEditorOperations
    {
        [DisplayAttributes(2, "Storage time", true, Web.Model.Enums.ComponentType.TextBox, "", Web.Model.Enums.GroupTypes.None, false, true, true)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string Type { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not LoadProfileDto putawayProfile)
                return false;

            return Id == putawayProfile.Id &&
            Name == putawayProfile.Name &&
            Type == putawayProfile.Type;
        }
    }
}
