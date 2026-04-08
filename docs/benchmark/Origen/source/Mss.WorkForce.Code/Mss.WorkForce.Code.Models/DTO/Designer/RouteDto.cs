using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class RouteDto
    {
        public Guid? Id { get; set; }

        public string? CanvasObjectType { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string? Name { get; set; }

        public bool Bidirectional { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public Guid? OutboundAreaId { get; set; }

        [JsonIgnore]
        public AreaDto? OutboundArea { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public Guid? InboundAreaId { get; set; }

        [JsonIgnore]
        public AreaDto? InboundArea { get; set; }

        //[Range(1, 999999, ErrorMessageResourceName = "THETIMEMUSTBEBETWEEN0AND999999", ErrorMessageResourceType = typeof(ValidationResources))]
        public int? TimeMin { get; set; }

        [JsonIgnore]
        public Guid? LayoutId { get; set; }

        public string ViewPort { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            RouteDto other = (RouteDto)obj;
            return Id == other.Id; // Compares the relevant properties
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode(); // Uses the relevant properties to calculate the hash code
        }
    }
}
