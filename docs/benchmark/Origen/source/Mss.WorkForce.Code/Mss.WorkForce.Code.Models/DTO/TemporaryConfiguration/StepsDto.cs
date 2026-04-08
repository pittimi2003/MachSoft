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
    public class StepsDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Name { get; set; }

        [DisplayAttributes(2, "Time min", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double TimeMin { get; set; }

        [DisplayAttributes(3, "Order", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Order { get; set; }

        [DisplayAttributes(4, "Init process", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool InitProcess { get; set; }

        [DisplayAttributes(5, "End process", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool EndProcess { get; set; }

        [DisplayAttributes(6, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(7, "View port", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string? ViewPort { get; set; }

        public static StepsDto NewDto()
        {
            return new StepsDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                TimeMin = 0,
                Order = 0,
                InitProcess = false,
                EndProcess = false,
                Process = ProcessCatalogueDto.NewDto(),
                ViewPort = string.Empty,
            };
        }
    }
}
