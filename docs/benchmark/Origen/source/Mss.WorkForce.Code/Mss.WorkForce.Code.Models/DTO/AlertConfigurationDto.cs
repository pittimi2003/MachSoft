using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class AlertConfigurationDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Severity", true, ComponentType.ItemIcon, isVisibleDefault: true, showIconEnum: true, Group = "Notice")]
        public AlertSeverity Severity { get; set; }

        [DisplayAttributes(2, "Type", true, ComponentType.DropDownEnum, isVisibleDefault: true, Group = "Notice")]
        public AlertType? Type { get; set; }


        public override string ToString()
        {
            return $"{Severity.ToString()} {Type.ToString()} ";
        }

    }
}