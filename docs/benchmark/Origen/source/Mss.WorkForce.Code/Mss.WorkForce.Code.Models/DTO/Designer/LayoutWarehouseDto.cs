using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{   
       public class LayoutWarehouseDto : IComparable
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static LayoutWarehouseDto NewDto()
        {
            return new LayoutWarehouseDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }

  

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            // Comparar por la propiedad Name
            return string.Compare(this.Name,((LayoutWarehouseDto)obj).Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
