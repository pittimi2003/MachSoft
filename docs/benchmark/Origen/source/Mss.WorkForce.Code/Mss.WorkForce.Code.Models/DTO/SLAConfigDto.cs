using Mss.WorkForce.Code.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class SLAConfigDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public double Value { get; set; }
        public Guid WarehouseId { get; set; }
        public static SLAConfigDto NewDto()
        {
            return new SLAConfigDto
            {
                Id = Guid.NewGuid(),
                Code = string.Empty,
                Value = 0,
            };
        }
    }
}
