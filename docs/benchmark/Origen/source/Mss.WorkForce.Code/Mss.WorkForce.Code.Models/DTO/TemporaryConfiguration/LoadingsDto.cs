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
    public class LoadingsDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Dock", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string? Dock { get; set; }

        [DisplayAttributes(2, "Automatic Loading Time", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double AutomaticLoadingTime { get; set; }

        [DisplayAttributes(3, "Additional Time In Buffer", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double AdditionalTimeInBuffer { get; set; }

        [DisplayAttributes(4, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required ProcessCatalogueDto Process { get; set; }

        [DisplayAttributes(1, "Viewport", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string? Viewport { get; set; }

        public static LoadingsDto NewDto()
        {
            return new LoadingsDto
            {
                Id = Guid.NewGuid(),
                Dock = string.Empty,
                AutomaticLoadingTime = 0,
                AdditionalTimeInBuffer = 0,
                Process = ProcessCatalogueDto.NewDto(),
                Viewport = string.Empty,
            };
        }
    }
}
