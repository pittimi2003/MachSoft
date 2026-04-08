using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class WorkerDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(index: 1, caption: "Name", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.TextBox)]
        public string Name { get; set; }
        [DisplayAttributes(index: 2, caption: "Worker number", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.NumericSpinInt)]
        public int WorkerNumber { get; set; }
        [DisplayAttributes(index: 3, caption: "Team", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionTeamDto? SelectionTeamDto { get; set; }
        [DisplayAttributes(index: 4, caption: "Role", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionRolDto? SelectionRolDto { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static WorkerDto? NewDto()
        {
            return new WorkerDto
            {
                SelectionTeamDto = new SelectionTeamDto(),
                Id = Guid.NewGuid(),
                Name = string.Empty,
                SelectionRolDto = new SelectionRolDto(),
                WorkerNumber = 0,
            };
        }

    }
}