using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionBreakProfileDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public override string ToString()
        {
            return Name;
        }
        public static SelectionBreakProfileDto NewDto()
        {
            return new SelectionBreakProfileDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
