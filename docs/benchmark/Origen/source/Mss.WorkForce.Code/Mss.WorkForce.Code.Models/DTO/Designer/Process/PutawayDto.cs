using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO.Designer.Process
{
    public class PutawayDto : BaseDesignerDto
    {        
        public int? AdditionTmeToPutaway { get; set; }
        public int? MinHour { get; set; }
        public Guid ProcessId { get; set; }
        public ProcessDto? Process { get; set; }
        public string? Viewport { get; set; }

        public static PutawayDto NewDto(ProcessDto process)
        {
            return new PutawayDto
            {
                Id = Guid.NewGuid(),
                AdditionTmeToPutaway = 0,
                MinHour = 0,
                ProcessId = process.Id ?? Guid.Empty,
                Process = process,
                Viewport = string.Empty
            };
        }
    }

}
