using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionShiftDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static SelectionShiftDto NewDto()
        {
            return new SelectionShiftDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}