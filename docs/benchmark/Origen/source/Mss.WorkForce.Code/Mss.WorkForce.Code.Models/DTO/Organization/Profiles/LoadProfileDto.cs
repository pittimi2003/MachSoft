using System.ComponentModel.DataAnnotations;
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class LoadProfileDto : PenelEditorOperations
    {
        
        [Key]
        public Guid Id { get; set; }


        [DisplayAttributes(2, "Type", true, Web.Model.Enums.ComponentType.TextBox, "", Web.Model.Enums.GroupTypes.None, false, true, true)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string Type { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not LoadProfileDto loadProfile)
                return false;

            return Id == loadProfile.Id &&
            Name == loadProfile.Name &&
            Type == loadProfile.Type;
        }
    }
}
