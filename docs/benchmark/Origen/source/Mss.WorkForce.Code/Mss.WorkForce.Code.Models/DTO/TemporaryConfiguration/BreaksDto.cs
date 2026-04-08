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
    public class BreaksDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Name { get; set; }

        [DisplayAttributes(2, "Init break", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double InitBreak { get; set; }

        [DisplayAttributes(4, "End break", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double EndBreak { get; set; }

        [DisplayAttributes(5, "Is paid", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsPaid { get; set; }

        [DisplayAttributes(6, "Is required", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsRequiered { get; set; }

        //public Guid BreakProfileId { get; set; }

        [DisplayAttributes(7, "Break profile", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required SelectionBreakProfileDto BreakProfile { get; set; }

        public static BreaksDto NewDto()
        {
            return new BreaksDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                InitBreak = 0,
                EndBreak = 0,
                IsPaid = false,
                IsRequiered = false,
                BreakProfile = SelectionBreakProfileDto.NewDto(),
            };
        }
    }
}
