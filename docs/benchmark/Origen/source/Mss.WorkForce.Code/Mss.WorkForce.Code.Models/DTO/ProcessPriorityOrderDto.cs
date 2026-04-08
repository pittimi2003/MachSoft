using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class ProcessPriorityOrderDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public int Priority { get; set; }

        public Guid WarehouseId { get; set; }

        public bool HasItems => Items?.Any() == false;

        public List<OrderPriorityDto> Items { get; set; } = new();

        public bool IsActive { get; set; } = false;

        //public bool IsActive { get; set; } = false;
        //public bool IsOrderPriority { get; set; } = false;

        public static ProcessPriorityOrderDto NewDto()
        {
            return new ProcessPriorityOrderDto
            {
                Id = Guid.NewGuid(),
                Code = string.Empty,
                Priority = 0,
            };
        }
    }
}
