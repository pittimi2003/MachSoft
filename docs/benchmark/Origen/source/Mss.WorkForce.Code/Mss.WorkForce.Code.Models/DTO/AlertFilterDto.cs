
using System.ComponentModel.DataAnnotations;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web;

namespace Mss.WorkForce.Code.Models.DTO
{
    [Valid]
    public class AlertFilterDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        public Guid Id { get; set; }

        [DisplayAttributes(8, "ConditionOperator", true, isVisibleDefault: true, FieldType = Web.Model.Enums.ComponentType.CustomProperty, ShowIconEnum = true)]
        public AlertOperator Operator { get; set; }

        [DisplayAttributes(7, "Field", true, isVisibleDefault: true, FieldType = Web.Model.Enums.ComponentType.CustomProperty)]
        public string FilterField { get; set; }

        [DisplayAttributes(9, "Reference", true, isVisibleDefault: true, FieldType = Web.Model.Enums.ComponentType.CustomProperty)]
        public string FilterReference { get; set; }

        [DisplayAttributes(10, "Reference fixed value", true, isVisibleDefault: true, FieldType = Web.Model.Enums.ComponentType.CustomProperty)]
        public string FilterFixedValue { get; set; }

        [DisplayAttributes(11, "Fixed", false, isVisibleDefault: true, FieldType = Web.Model.Enums.ComponentType.CustomProperty)]
        public bool IsFixed { get; set; }

        public override string ToString()
        {
            return $"{FilterField} {Operator.ToString()} {FilterReference} ";
        }


        
    }
}
