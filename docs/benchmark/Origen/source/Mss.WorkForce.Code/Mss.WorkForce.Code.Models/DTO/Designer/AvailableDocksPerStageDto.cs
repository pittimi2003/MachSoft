using Mss.WorkForce.Code.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class AvailableDocksPerStageDto
    {
        public Guid Id { get; set; }
        public Guid StageId { get; set; }
        public Guid DockId { get; set; }
        public string Name { get; set; }

        public static AvailableDocksPerStageDto NewDto()
        {
            return new AvailableDocksPerStageDto
            {
                Id = Guid.Empty,
                StageId = Guid.Empty,
                DockId = Guid.Empty,
                Name = string.Empty,
            };
        }
    }
}
