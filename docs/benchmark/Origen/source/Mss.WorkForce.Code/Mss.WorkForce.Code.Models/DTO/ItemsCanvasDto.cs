using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.DTO.Designer.LocationZones;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.DTO
{
    /// <summary>
    /// Items used to list all viewports to objects
    /// </summary>
    public class ItemsCanvasDto
    {
        [JsonPropertyName("Areas")]
        public List<AreaDto>? areas { get; set; }

        [JsonPropertyName("Objects")]
        public List<ObjectsDto>? objects { get; set; }

        [JsonPropertyName("Routes")]
        public HashSet<RouteDto>? routes { get; set; }

        [JsonPropertyName("Equipments")]
        public List<EquipmentDesignerDto>? equipments { get; set; }

        [JsonPropertyName("Stations")]
        public List<Designer.ZoneDto>? zones { get; set; }

        [JsonPropertyName("Process")]
        public List<Designer.ProcessDto>? process { get; set; }

        [JsonPropertyName("Shelf")]
        public List<ShelfDto>? shelf { get; set; }

        [JsonPropertyName("ProcessDirections")]
        public List<ProcessDirectionPropertyDto>? processDirections { get; set; }

        [JsonPropertyName("Steps")]
        public List<StepDto>? steps { get; set; }

        [JsonPropertyName("Flows")]
        public List<FlowDto>? flows { get; set; }
    }
}
