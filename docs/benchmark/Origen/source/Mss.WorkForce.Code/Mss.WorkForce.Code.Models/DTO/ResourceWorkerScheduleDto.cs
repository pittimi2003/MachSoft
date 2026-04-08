using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class ResourceWorkerScheduleDto : PenelEditorOperations
    {
        public Guid WorkerId { get; set; }
        public Guid BreakProfileId { get; set; }

        [DisplayAttributes(index: 2, caption: "Worker number", isVisible: true, required: true, fieldType: ComponentType.NumericSpinInt, textAlignment: true)]
        [UniqueAttributes(true)]
        public int WorkerNumber { get; set; }

        [DisplayAttributes(index: 3, caption: "Role", isVisible: true, required: true, fieldType: ComponentType.DropDown)]
        public CatalogEntity Rol { get; set; } = new CatalogEntity(EntityNamesConst.Rol);

        [DisplayAttributes(index: 4, caption: "Team", isVisible: true, required: true, fieldType: ComponentType.DropDown)]
        public CatalogEntity Team { get; set; } = new CatalogEntity(EntityNamesConst.Team);

        [DisplayAttributes(index: 5, caption: "Shift", isVisible: true, required: true, fieldType: ComponentType.DropDown)]
        public CatalogEntity Shift { get; set; } = new CatalogEntity(EntityNamesConst.Shift);

        [DisplayAttributes(index: 6, caption: "Break", isVisible: true, required: true, fieldType: ComponentType.DropDown)]
        public CatalogEntity Break { get; set; } = new CatalogEntity(EntityNamesConst.BreakProfile);

        [DisplayAttributes(index: 7, caption: "Available", isVisible: true, required: false, fieldType: ComponentType.CheckBox)]
        public bool Available { get; set; } = true;

        [DisplayAttributes(index: 8, caption: "Init", isVisible: false, required: false, fieldType: ComponentType.Time)]
        public DateTime? InitHour { get; set; }

        [DisplayAttributes(index: 9, caption: "End", isVisible: false, required: false, fieldType: ComponentType.Time)]
        public DateTime? EndHour { get; set; }

        [DisplayAttributes(index: 10, caption: "Lapse", isVisible: false, required: false, fieldType: ComponentType.TextBox)]
        public string WorkHours =>
            $"{(EndHour - InitHour)?.Hours:D2}h" +
            $"{((EndHour - InitHour)?.Minutes > 0 ? $" {((EndHour - InitHour)?.Minutes ?? 0):D2}m" : "")}";

        public ResourceWorkerScheduleDto DeepClone()
        {
            return new ResourceWorkerScheduleDto
            {
                Id = Id,
                WorkerId = WorkerId,
                BreakProfileId = BreakProfileId,
                Name = Name,
                WorkerNumber = WorkerNumber,
                Rol = Rol,
                Team = Team,
                Shift = Shift,
                Break = Break,
                InitHour = InitHour,
                EndHour = EndHour,
                DataOperationType = DataOperationType,
                Available= Available,
            };
        }
    }
}
