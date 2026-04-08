using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class OrderScheduleDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }
        [DisplayAttributes(4, "Init hour", true, ComponentType.Time, "", GroupTypes.None, true, isVisibleDefault: true, IsVisible = true)]
        public TimeSpan InitHour { get; set; }
        [DisplayAttributes(5, "End hour", true, ComponentType.Time, "", GroupTypes.None, true, isVisibleDefault: true, IsVisible = true)]
        public TimeSpan EndHour { get; set; }
        [DisplayAttributes(2, "Vehicle", true, ComponentType.DropDown, "", GroupTypes.None, true, isVisibleDefault: true)]

        public SelectionVehicleDto SelectionVehicleDto { get; set; }
        [DisplayAttributes(3, "Number of vehicles", true, ComponentType.NumericSpin, "", GroupTypes.None, true, isVisibleDefault: true)]
        public double NumberVehicles { get; set; }
        [DisplayAttributes(1, "Load", true, ComponentType.DropDown, "", GroupTypes.None, true, isVisibleDefault: true)]
        public SelectionLoadDto SelectionLoadDto { get; set; }
        [DisplayAttributes(6, "Order type", true, ComponentType.CheckBox, "", GroupTypes.None, true, isVisibleDefault: true)]
        public bool IsOut { get; set; }
        
        public required SelectionWarehouseDto SelectionWarehouseDto { get; set; }

        public static OrderScheduleDto? NewDto()
        {
            return new OrderScheduleDto
            {
                SelectionWarehouseDto = new SelectionWarehouseDto(),
                SelectionLoadDto = new SelectionLoadDto(),
                SelectionVehicleDto = new SelectionVehicleDto(),
                Id = Guid.NewGuid(),
                EndHour = new TimeSpan(0, 0, 0),
                InitHour = new TimeSpan(0, 0, 0),
                IsOut = false,
                NumberVehicles = 0,

            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not OrderScheduleDto item)
                return false;

            return Id == item.Id;
        }

        public object GetProperty(string propertyName)
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            return propertyInfo?.GetValue(this);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}