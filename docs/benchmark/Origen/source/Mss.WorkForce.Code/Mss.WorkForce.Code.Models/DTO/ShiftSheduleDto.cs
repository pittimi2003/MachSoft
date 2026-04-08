using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mss.WorkForce.Code.Models.Models;
using System.Xml.Linq;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class ShiftSheduleDto
    {
        public Guid id { get; set; }

        public Guid workerId { get; set; }
        public Guid breakProfileId { get; set; }

        public string name { get; set; }
        [DisplayAttributes(index: 9, caption: "Init", isVisible: false, required: false, fieldType: ComponentType.Time)]
        public DateTime? initHour { get; set; }
        [DisplayAttributes(index: 9, caption: "End", isVisible: false, required: false, fieldType: ComponentType.Time)]
        public DateTime? endHour { get; set; }

        public CatalogEntity shift { get; set; } = new CatalogEntity(EntityNamesConst.Shift);

        public string WorkHours =>
           $"{(endHour - initHour)?.Hours:D2}h" +
           $"{((endHour - initHour)?.Minutes > 0 ? $" {((endHour - initHour)?.Minutes ?? 0):D2}m" : "")}";


        public ShiftSheduleDto DeepClone()
        {
            return new ShiftSheduleDto
            {
                id = id,
                initHour = initHour,
                endHour = endHour,
                workerId = workerId,
                shift = shift,
                name = name,
                breakProfileId = breakProfileId,
            };
        }
    }
}
