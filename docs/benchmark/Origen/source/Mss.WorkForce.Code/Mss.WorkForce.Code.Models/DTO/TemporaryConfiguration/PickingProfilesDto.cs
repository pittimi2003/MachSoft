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
    public class PickingProfilesDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Picking road time", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double PickingRoadTime { get; set; }

        //public Guid ProcessId { get; set; }

        [DisplayAttributes(2, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        public static PickingProfilesDto NewDto()
        {
            return new PickingProfilesDto
            {
                Id = Guid.NewGuid(),
                PickingRoadTime = 0,
                Process = ProcessCatalogueDto.NewDto()
            };
        }
    }
}
