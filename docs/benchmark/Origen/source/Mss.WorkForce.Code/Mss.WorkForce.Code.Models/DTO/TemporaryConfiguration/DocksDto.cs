using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class DocksDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Max. equipment", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double MaxEquipments { get; set; }

        [DisplayAttributes(2, "Operates from buffer", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool OperatesFromBuffer { get; set; }

        [DisplayAttributes(3, "Overload handling", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double OverloadHandling { get; set; }

        [DisplayAttributes(4, "Inbound range", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double InboundRange { get; set; }

        [DisplayAttributes(5, "Outbound range", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double OutboundRange { get; set; }

        [DisplayAttributes(6, "Entry capacity", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double MaxStockCrossdocking { get; set; }

        [DisplayAttributes(7, "Allow inbound", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool AllowInbound { get; set; }

        [DisplayAttributes(8, "Allow outbound", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool AllowOutbound { get; set; }
        public Guid StationId { get; set; }

        [DisplayAttributes(9, "Zone", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required StationCatalogueDto Station { get; set; }

        public static DocksDto NewDto()
        {
            return new DocksDto
            {
                Id = Guid.NewGuid(),
                Station = StationCatalogueDto.NewDto(),
            };
        }
    }
}
