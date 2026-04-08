using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models
{
    public class InboundsDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Quantity", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Quantity { get; set; }

        [DisplayAttributes(2, "Unit of measure", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string? UnitOfMeasure { get; set; }

        [DisplayAttributes(3, "Vehicle Per Hour", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double VehiclePerHour { get; set; }

        [DisplayAttributes(4, "Truck per day", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double TruckPerDay { get; set; }

        [DisplayAttributes(5, "Min time in buffer", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double MinTimeInBuffer { get; set; }

        [DisplayAttributes(6, "Load time", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double LoadTime { get; set; }

        [DisplayAttributes(7, "Additional time in buffer", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double AdditionalTimeInBuffer { get; set; }

        [DisplayAttributes(8, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(9, "View port", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string? Viewport { get; set; }

        public static InboundsDto NewDto()
        {
            return new InboundsDto
            {
                Id = Guid.NewGuid(),
                Quantity = 0,
                UnitOfMeasure = string.Empty,
                VehiclePerHour = 0,
                TruckPerDay = 0,
                MinTimeInBuffer = 0,
                LoadTime = 0,
                AdditionalTimeInBuffer = 0,
                Process = ProcessCatalogueDto.NewDto(),
                Viewport = string.Empty,
            };
        }
    }
}
