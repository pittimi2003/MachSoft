using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionAvailableWorkerDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static SelectionAvailableWorkerDto NewDto()
        {
            return new SelectionAvailableWorkerDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
