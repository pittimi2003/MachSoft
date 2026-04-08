using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class ScheduleDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(index: 1, caption: "Name", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.TextBox)]
        public string Name { get; set; }
        private DateTime date;
        [DisplayAttributes(index: 2, caption: "Date", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DateTime)]
        public DateTime Date
        {
            get => date;
            set => date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        [DisplayAttributes(index: 3, caption: "Worker", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionAvailableWorkerDto? SelectionAvailableWorkerDto { get; set; }
        [DisplayAttributes(index: 4, caption: "Shift", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionShiftDto? SelectionShiftDto { get; set; }
        [DisplayAttributes(index: 5, caption: "Break profile", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionBreakProfileDto? SelectionBreakProfileDto { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static ScheduleDto? NewDto()
        {
            return new ScheduleDto
            {
                SelectionBreakProfileDto = new SelectionBreakProfileDto(),
                SelectionShiftDto = new SelectionShiftDto(),
                SelectionAvailableWorkerDto = new SelectionAvailableWorkerDto(),
                Date = new DateTime(),
                date = new DateTime(),
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
