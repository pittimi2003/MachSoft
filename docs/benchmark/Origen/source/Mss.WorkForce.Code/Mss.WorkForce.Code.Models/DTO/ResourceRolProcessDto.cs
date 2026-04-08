using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class ResourceRolProcessDto : PenelEditorOperations

    {
        public Guid RolId { get; set; }

        public string RolName { get; set; }

        [DisplayAttributes(2, "Process", true, ComponentType.Multiselec, true, EntityNamesConst.Process)]
        public Multiselect Process { get; set; } = new Multiselect();

    }
}
