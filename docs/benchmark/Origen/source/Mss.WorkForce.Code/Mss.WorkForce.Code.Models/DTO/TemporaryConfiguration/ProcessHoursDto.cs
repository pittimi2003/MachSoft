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
    public class ProcessHoursDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string Name { get; set; }

        [DisplayAttributes(2, "Process", true, ComponentType.DropDown, isVisibleDefault: true)]
        public  ProcessCatalogueDto Process  { get; set; }
        
        [DisplayAttributes(2, "Hour", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double Hour { get; set; }

        public static ProcessHoursDto NewDto()
        {
            return new ProcessHoursDto
            {
                Id = Guid.NewGuid(),
                Process = ProcessCatalogueDto.NewDto(),
                
            };
        }
    }
}
