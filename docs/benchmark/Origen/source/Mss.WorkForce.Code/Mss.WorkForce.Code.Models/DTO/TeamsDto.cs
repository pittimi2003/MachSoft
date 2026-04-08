using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Models.Attributtes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class TeamsDto: PenelEditorOperations
    {
     
        [DisplayAttributes(index: 2, caption: "Description", isVisible: true, required: false, fieldType: ComponentType.TextBox)]
        [ValidationAttributes(Enums.ValidationType.None)]
        public string? Description { get; set; }
        public Guid WarehouseId { get; set; }
 
    }
}
