

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionLoadDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static SelectionLoadDto NewDto()
        {
            return new SelectionLoadDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
