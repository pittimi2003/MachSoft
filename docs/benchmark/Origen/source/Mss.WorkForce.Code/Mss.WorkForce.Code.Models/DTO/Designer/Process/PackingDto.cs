using System;
using System.Collections.Generic;

namespace Mss.WorkForce.Code.Models.DTO.Designer.Process
{
    public class PackingDto : BaseDesignerDto
    {
        public List<PackModeDto> PackModes { get; set; } = new();

        public Guid ProcessId { get; set; }

        public ProcessDto? Process { get; set; }

        public string? ViewPort { get; set; }

        public static PackingDto NewDto(ProcessDto process)
        {
            return new PackingDto
            {
                Id = Guid.NewGuid(),
                ProcessId = process.Id ?? Guid.Empty,
                Process = process,
                ViewPort = string.Empty,
                PackModes = PackModeDto.NewDto()
            };
        }
    }

    public class PackModeDto
    {
        public Guid Id { get; set; }
        public PackModes PackName { get; set; }
        public int PackQty { get; set; }

        public static List<PackModeDto> NewDto()
        {
            return new List<PackModeDto>()
            {
                new PackModeDto {Id = Guid.NewGuid(), PackName = PackModes.OnePackage, PackQty = 1 },
                new PackModeDto {Id = Guid.NewGuid(), PackName = PackModes.ChooseNumPackages, PackQty = 1 },
                new PackModeDto {Id = Guid.NewGuid(), PackName = PackModes.CaptureStock, PackQty = 1 }
            };
        }
    }

    public enum PackModes
    {
        OnePackage = 1,
        ChooseNumPackages = 2,
        CaptureStock = 3
    }
}
