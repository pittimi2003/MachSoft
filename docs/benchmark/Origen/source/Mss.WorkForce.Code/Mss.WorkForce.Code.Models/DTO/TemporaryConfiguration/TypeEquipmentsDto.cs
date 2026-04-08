using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class TypeEquipmentsDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Name { get; set; }

        [DisplayAttributes(2, "Description", false, ComponentType.CommetText, isVisibleDefault: true)]
        public string Description { get; set; }

        [DisplayAttributes(3, "Capacity", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Capacity { get; set; }

        [DisplayAttributes(4, "Quantity", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Quantity { get; set; }

        [DisplayAttributes(5, "Loading wait time", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double LoadingWaitTime { get; set; }

        //public Guid WarehouseId { get; set; }

        [DisplayAttributes(6, "Warehouse", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required SelectionWarehouseDto Warehouse { get; set; }

        public static TypeEquipmentsDto NewDto()
        {
            return new TypeEquipmentsDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                //WarehouseId = Guid.NewGuid(),
                Warehouse = SelectionWarehouseDto.NewDto(),
            };
        }
    }
}
