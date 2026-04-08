using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class StageDto
    {
        public Guid Id { get; set; }

        public int EntryCapacity { get; set; }

        public int ExitCapacity { get; set; }

        public bool MixCarriers { get; set; }

        public Guid ZoneId { get; set; }

        public Designer.ZoneDto? Zone { get; set; }
        [JsonIgnore]
        public string? ViewPort { get; set; }
        public bool IsIn { get; set; }
        public bool IsOut { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de StageDto con valores por defecto, incluyendo una Zone preinicializada.
        /// </summary>
        /// <param name="area">Instancia de AreaDto necesaria para crear la Zone asociada.</param>
        public static StageDto NewDto(string Name, AreaDto area, ObjectTypes objectTypes, DockDto dock)
        {
            return new StageDto
            {
                Id = Guid.NewGuid(),
                EntryCapacity = 0,
                ExitCapacity = 0,
                MixCarriers = false,
                ZoneId = Guid.Empty,
                ViewPort = string.Empty,
                Zone = Designer.ZoneDto.NewDto(area, objectTypes, Name),
                IsIn = false,
                IsOut =false
            };
        }

        public static List<StageDto> NewDtoList(string Name, int Quantity, ObjectTypes objectTypes, AreaDto areaDto, bool IsIn, bool IsOut)
        {
            List<StageDto> stageDtos = new();

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = Designer.ZoneDto.NewDto(areaDto, objectTypes, Name);
                var stage = new StageDto
                {
                    Id = Guid.NewGuid(),
                    EntryCapacity = 0,
                    ExitCapacity = 0,
                    MixCarriers = false,
                    ZoneId = zone.Id ?? Guid.Empty,
                    ViewPort = string.Empty,
                    Zone = zone,
                    IsIn = IsIn,
                    IsOut = IsOut
                };

                stageDtos.Add(stage);
            }

            return stageDtos;
        }

    }
}
