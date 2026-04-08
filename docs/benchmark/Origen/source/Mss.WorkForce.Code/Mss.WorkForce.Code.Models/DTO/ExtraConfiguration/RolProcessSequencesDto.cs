using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    public class RolProcessSequencesDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Name { get; set; }

        //public Guid RolId { get; set; }

        [DisplayAttributes(2, "Role", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required SelectionRolDto Rol { get; set; }

        //public Guid ProcessId { get; set; }

        [DisplayAttributes(3, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(4, "Sequence", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Sequence { get; set; }

        public static RolProcessSequencesDto NewDto()
        {
            return new RolProcessSequencesDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Rol = SelectionRolDto.NewDto(),
                Process = ProcessCatalogueDto.NewDto(),
                Sequence = 0,
            };
        }
    }
}
