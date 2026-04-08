using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System.ComponentModel.DataAnnotations;
using Mss.WorkForce.Code.Models.Attributtes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class OrderProfilesDto : PenelEditorOperations, ICustomValidation
    {

        [DisplayAttributes(index: 0, caption: "Name", isVisible: false, required: false, fieldType: ComponentType.TextBox)]
        [ValidationAttributes(Enums.ValidationType.None), UniqueAttributes(true)]
        public override string Name { get => base.Name; set => base.Name = value; }

        [DisplayAttributes(index: 5, caption: "Load", isVisible: true, required: true, fieldType: ComponentType.DropDown)]
        public CatalogEntity Load { get; set; } = new CatalogEntity(EntityNamesConst.LoadProfile);

        [DisplayAttributes(index: 6, caption: "Vehicle", isVisible: true, required: true, fieldType: ComponentType.DropDown)]
        public CatalogEntity Vehicle { get; set; } = new CatalogEntity(EntityNamesConst.VehicleProfile);

        [DisplayAttributes(index: 4, caption: "Number of vehicles", isVisible: true, required: true, fieldType: ComponentType.NumericSpinInt, textAlignment: true)]
        public double NumberVehicles { get; set; }

        
        [DisplayAttributes(index: 2, caption: "Init hour", isVisible: true, required: false, fieldType: ComponentType.Time)]
        public TimeSpan InitHour { get; set; }

        [DisplayAttributes(index: 3, caption: "End hour", isVisible: true, required: false, fieldType: ComponentType.Time)]
        public TimeSpan EndHour { get; set; }

        [DisplayAttributes(index: 1, caption: "Order type", isVisible: true, required: false, fieldType: ComponentType.TypeBound)]
        public bool IsOut { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not OrderScheduleDto item)
                return false;

            return Id == item.Id;
        }

        public ValidationResult CustomValidation()
        {
            if (InitHour != default && EndHour != default)
            {
                if (InitHour > EndHour)
                {
                    return new ValidationResult(
                        "Init hour cannot be greater than End hour.",
                        new[] { nameof(InitHour), nameof(EndHour) }
                    );
                }
            }

            return ValidationResult.Success;
        }
    }
}
