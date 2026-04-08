using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System.ComponentModel.DataAnnotations;


namespace Mss.WorkForce.Code.Models
{
    public class PlanningAlertsDto
    {

        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }
        [DisplayAttributes(0, "Alert Id", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public Guid AlertId { get; set; }

        [DisplayAttributes(0, "Planning Id", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public Guid PlanningId { get; set; }

        [DisplayAttributes(0, "Entity Id", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public Guid EntityId { get; set; }

        [DisplayAttributes(0, "Trigger date", true, ComponentType.DateTime, "", GroupTypes.None, false, isVisibleDefault: true)]
        public DateTime TriggerDate { get; set; }

        [DisplayAttributes(0, "InputOrder Id", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public Guid InputOrderId { get; set; }

        [DisplayAttributes(0, "Item Planning Contains", true, ComponentType.CheckBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public bool ItemPlanningContains { get; set; }

        [DisplayAttributes(0, "Work Orders Planning Contains", true, ComponentType.CheckBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public bool WorckOrderPlanningContains { get; set; }




        public static PlanningAlertsDto? NewDto()
        {
            return new PlanningAlertsDto
            {
                Id = Guid.NewGuid(),
                AlertId = Guid.NewGuid(),
                PlanningId = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                InputOrderId = Guid.NewGuid()
            };
        }
    }
}
