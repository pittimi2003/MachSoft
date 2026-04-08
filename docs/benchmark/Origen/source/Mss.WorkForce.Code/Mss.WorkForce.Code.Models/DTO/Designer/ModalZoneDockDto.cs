using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class ModalZoneDockDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string? Name { get; set; }

        [JsonIgnore]
        public DockDto Dock { get; set; }

        [JsonIgnore]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int Quantity { get; set; }
        
        public OrientationType? Orientation { get; set; } = OrientationType.None;
        
        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public DockType? FlowInOut
        {
            get
            {
                if (Dock.AllowInbound && Dock.AllowOutbound)
                    return DockType.Both;
                if (Dock.AllowInbound)
                    return DockType.Inbound;
                if (Dock.AllowOutbound)
                    return DockType.Outbound;
                return null;
            }
            set
            {
                _dockType = value;

                //Assign true as appropriate
                switch (value)
                {
                    case DockType.Inbound:
                        Dock.AllowInbound = true;
                        Dock.AllowOutbound = false;
                        break;
                    case DockType.Outbound:
                        Dock.AllowInbound = false;
                        Dock.AllowOutbound = true;
                        break;
                    case DockType.Both:
                        Dock.AllowInbound = true;
                        Dock.AllowOutbound = true;
                        break;
                    default:
                        Dock.AllowInbound = false;
                        Dock.AllowOutbound = false;
                        break;
                }
            }
        }

        private DockType? _dockType;
    }
}
