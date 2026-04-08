using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;

namespace Mss.WorkForce.Code.Models
{
    public class ReceptionsDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Breakage factor", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double BreakageFactor { get; set; }

        [DisplayAttributes(2, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(3, "View port", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string? ViewPort { get; set; }

        public static ReceptionsDto NewDto()
        {
            return new ReceptionsDto
            {
                Id = Guid.NewGuid(),
                BreakageFactor = 0,
                Process = ProcessCatalogueDto.NewDto(),
                ViewPort = string.Empty,
            };
        }
    }
}
