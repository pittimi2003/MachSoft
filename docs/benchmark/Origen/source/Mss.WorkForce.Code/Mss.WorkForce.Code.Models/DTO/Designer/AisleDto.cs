using Mss.WorkForce.Code.Models.DTO.Designer;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO
{    
    public class AisleDto
    {
        public Guid Id { get; set; }

        public int? AdditionalTimePerUnitEntry { get; set; }

        public int? AdditionalTimePerUnitExit { get; set; }

        public int? MaxTasks { get; set; }

        public int? AisleChangeTime { get; set; }

        public bool NarrowAisle { get; set; }

        public int? MaxPickers { get; set; }

        public bool EndPicking { get; set; }

        public bool Bidirectional { get; set; }

        public int? WidthPerDirection { get; set; }

        public int? MaxMHEPickOrPutaway { get; set; }

        public bool ReplenishmentControl { get; set; }

        public int? MaxMovement { get; set; }

        public Guid ZoneId { get; set; }

        public Designer.ZoneDto? Zone { get; set; }

        public string? ViewPort { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de AisleDto con valores por defecto, incluyendo una Zone preinicializada.
        /// </summary>
        /// <param name="area">Instancia de AreaDto necesaria para crear la Zone asociada.</param>
        public static AisleDto NewDto(string Name, AreaDto area, ObjectTypes objectTypes)
        {
            return new AisleDto
            {
                Id = Guid.NewGuid(),
                AdditionalTimePerUnitEntry = 0,
                AdditionalTimePerUnitExit = 0,
                MaxTasks = 0,
                AisleChangeTime = 0,
                NarrowAisle = false,
                MaxPickers = 0,
                EndPicking = false,
                Bidirectional = false,
                WidthPerDirection = 0,
                MaxMHEPickOrPutaway = 0,
                ReplenishmentControl = false,
                MaxMovement = 0,
                ZoneId = Guid.Empty, // Se espera que se sincronice con Zone.Id si es necesario.
                ViewPort = string.Empty,
                Zone = Designer.ZoneDto.NewDto(area, objectTypes, Name)
            };
        }

        public static List<AisleDto> NewDtoList(string Name, int Quantity, ObjectTypes objectTypes, AreaDto areaDto)
        {
            List<AisleDto> aisleDtos = new();

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = Designer.ZoneDto.NewDto(areaDto, objectTypes, Name);
                var aisle = new AisleDto
                {
                    Id = Guid.NewGuid(),
                    AdditionalTimePerUnitEntry = 0,
                    AdditionalTimePerUnitExit = 0,
                    MaxTasks = 0,
                    AisleChangeTime = 0,
                    NarrowAisle = false,
                    MaxPickers = 0,
                    EndPicking = false,
                    Bidirectional = false,
                    WidthPerDirection = 0,
                    MaxMHEPickOrPutaway = 0,
                    ReplenishmentControl = false,
                    MaxMovement = 0,
                    ZoneId = zone.Id ?? Guid.Empty, // Se espera que se sincronice con Zone.Id si es necesario.
                    ViewPort = string.Empty,
                    Zone = zone
                };

                aisleDtos.Add(aisle);
            }

            return aisleDtos;
        }
    }

}
