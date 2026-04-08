using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class BufferDto
    {
        public Guid Id { get; set; }

        public int EntryCapacity { get; set; }

        public int ExitCapacity { get; set; }

        public bool CapacityByMaterial { get; set; }

        public bool Excess { get; set; }

        public Guid ZoneId { get; set; }

        public Designer.ZoneDto? Zone { get; set; }

        public BufferType BufferType { get; set; }

        //[Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int NumShelves { get; set; }

        //[Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int NumCrossAisles { get; set; }

        public OrientationType? Orientation { get; set; } = OrientationType.None;

        public bool IsVertical
        {
            get => Orientation == OrientationType.Vertical;
            set => Orientation = value ? OrientationType.Vertical : OrientationType.Horizontal;
        }

        public string? ViewPort { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de BufferDto con valores por defecto, incluyendo una Zone preinicializada.
        /// </summary>
        /// <param name="area">Instancia de AreaDto necesaria para crear la Zone asociada.</param>
        public static BufferDto NewDto(string Name, AreaDto area, ObjectTypes objectTypes, BufferType bufferType, ModalBufferDto modalBufferDto)
        {
            return new BufferDto
            {
                Id = Guid.NewGuid(),
                EntryCapacity = 0,
                ExitCapacity = 0,
                CapacityByMaterial = false,
                Excess = false,
                ZoneId = Guid.Empty,
                ViewPort = string.Empty,
                Zone = Designer.ZoneDto.NewDto(area, objectTypes, Name),
                BufferType = bufferType,
                NumShelves = modalBufferDto.NumShelves,
                NumCrossAisles = modalBufferDto.NumCrossAisles,
                Orientation = modalBufferDto.Orientation,
                IsVertical = modalBufferDto.IsVertical
            };
        }

        public static List<BufferDto> NewDtoList(string Name, int Quantity, ObjectTypes objectTypes, AreaDto areaDto, ModalBufferDto modalBufferDto)
        {
            List<BufferDto> bufferDtos = new();

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = Designer.ZoneDto.NewDto(areaDto, objectTypes, Name, modalBufferDto.Orientation ?? OrientationType.Horizontal);
                var buffer = new BufferDto
                {
                    Id = Guid.NewGuid(),
                    EntryCapacity = 0,
                    ExitCapacity = 0,
                    CapacityByMaterial = false,
                    Excess = false,
                    ZoneId = zone.Id ?? Guid.Empty,
                    ViewPort = string.Empty,
                    Zone = zone,
                    BufferType = modalBufferDto.BufferType ?? BufferType.None,
                    NumShelves = modalBufferDto.NumShelves,
                    NumCrossAisles = modalBufferDto.NumCrossAisles,
                    Orientation = modalBufferDto.Orientation ?? OrientationType.Horizontal,
                    IsVertical = modalBufferDto.IsVertical
                };

                bufferDtos.Add(buffer);
            }

            return bufferDtos;
        }
    }
}
