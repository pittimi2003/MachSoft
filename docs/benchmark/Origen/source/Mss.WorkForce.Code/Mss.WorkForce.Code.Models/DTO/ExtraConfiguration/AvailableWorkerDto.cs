using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class AvailableWorkerDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(index: 1, caption: "Name", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.TextBox)]
        public required string Name { get; set; }
        [DisplayAttributes(index: 2, caption: "Worker", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionWorkerDto? SelectionWorkerDto { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static AvailableWorkerDto? NewDto()
        {
            return new AvailableWorkerDto
            {
                SelectionWorkerDto = new SelectionWorkerDto(),
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
