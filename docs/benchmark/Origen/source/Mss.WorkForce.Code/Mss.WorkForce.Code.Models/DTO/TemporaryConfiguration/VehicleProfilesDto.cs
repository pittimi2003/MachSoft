using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;

namespace Mss.WorkForce.Code.Models
{
    public class VehicleProfilesDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Name { get; set; }

        [DisplayAttributes(2, "Type", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Type { get; set; }

        [DisplayAttributes(3, "Allow inbound flow", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool AllowInInboundFlow { get; set; }

        [DisplayAttributes(4, "Allow outbound flow", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool AllowInOutboundFlow { get; set; }

        [DisplayAttributes(5, "Warehouse", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required SelectionWarehouseDto Warehouse { get; set; }

        public static VehicleProfilesDto NewDto()
        {
            return new VehicleProfilesDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Type = string.Empty,
                AllowInInboundFlow = false,
                AllowInOutboundFlow = false,
                Warehouse = SelectionWarehouseDto.NewDto(),
            };
        }
    }
}
