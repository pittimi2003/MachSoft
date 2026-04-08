using Mss.WorkForce.Code.Models.Models;
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
    public class RacksDto
    {
        [Key]
        public Guid Id { get; set; }
        
        [DisplayAttributes(2, "View port", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string ViewPort { get; set; }

        [DisplayAttributes(1, "Zone", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required StationCatalogueDto Station { get; set; }


        public static RacksDto NewDto()
        {
            return new RacksDto
            {
                Id = Guid.NewGuid(),
                Station = StationCatalogueDto.NewDto(),
                ViewPort = string.Empty
            };
        }
    }
}
