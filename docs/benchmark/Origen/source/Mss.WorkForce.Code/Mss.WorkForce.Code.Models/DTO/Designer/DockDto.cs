using Mss.WorkForce.Code.Models.DTO.Designer;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class DockDto
    {
        public Guid Id { get; set; }

        public bool OperatesFromBuffer { get; set; }

        public double? OverloadHandling { get; set; }

        public int InboundRange { get; set; }

        public int OutboundRange { get; set; }

        public int MaxStockCrossdocking { get; set; }

        public bool AllowInbound { get; set; }

        public bool AllowOutbound { get; set; }

        public Guid ZoneId { get; set; }

        [JsonIgnore]
        public Designer.ZoneDto? Zone { get; set; }

        [JsonIgnore]
        public string? ViewPort { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de DockDto con valores por defecto, incluyendo una Zone preinicializada.
        /// </summary>
        /// <param name="area">Instancia de AreaDto necesaria para crear la Zone asociada.</param>
        public static DockDto NewDto(string Name, AreaDto area, ObjectTypes objectsTypes)
        {
            return new DockDto
            {
                Id = Guid.NewGuid(),
                OperatesFromBuffer = false,
                OverloadHandling = 0,
                InboundRange = 0,
                OutboundRange = 0,
                MaxStockCrossdocking = 0,
                AllowInbound = false,
                AllowOutbound = false,
                ZoneId = Guid.Empty,
                ViewPort = string.Empty,
                Zone = Designer.ZoneDto.NewDto(area, objectsTypes, Name)
            };
        }

        public static List<DockDto> NewDtoList(string Name, int Quantity, OrientationType? orientation, bool allowInbound, bool allowOutbound, ObjectTypes objectTypes, AreaDto areaDto, int nextInbound, int nextOutbound)
        {
            List<DockDto> dockDtos = new();
            int inboundCounter = nextInbound;
            int outboundCounter = nextOutbound;

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = Designer.ZoneDto.NewDto(areaDto, objectTypes, Name, orientation);
                var aisle = new DockDto
                {
                    Id = Guid.NewGuid(),
                    OperatesFromBuffer = false,
                    OverloadHandling = 0,
                    InboundRange = allowInbound ? inboundCounter++ : 0,
                    OutboundRange = allowOutbound ? outboundCounter++ : 0,
                    MaxStockCrossdocking = 0,
                    AllowInbound = allowInbound,
                    AllowOutbound = allowOutbound,
                    ZoneId = zone.Id ?? Guid.Empty,
                    ViewPort = string.Empty,
                    Zone = zone
                };

                dockDtos.Add(aisle);
            }

            return dockDtos;
        }
    }
}
