using Mss.WorkForce.Code.Models.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class CustomProcessDto : BaseDesignerDto
    {
        public Guid Id { get; set; }
        public double? InitHour { get; set; }
        public double? EndHour { get; set; }
        public double? Percentage { get; set; }
        public double? NumPossibleTimes { get; set; }
        public Guid ProcessId { get; set; }
        public ProcessDto? Process { get; set; }
        public string? ViewPort { get; set; }

        public static CustomProcessDto? NewDto(ProcessDto Process)
        {
            return new CustomProcessDto
            {
                Id = Guid.NewGuid(),
                InitHour = 0,
                EndHour = 0,
                Percentage = 0,
                NumPossibleTimes = 0,
                ProcessId = (Guid)Process.Id,
                Process = Process,
                ViewPort = string.Empty,
                StatusObject = "StoredInBase"
            };
        }
    }
}
