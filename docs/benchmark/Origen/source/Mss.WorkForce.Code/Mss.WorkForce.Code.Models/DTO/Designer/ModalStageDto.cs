using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class ModalStageDto
    {
        public Guid Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int Quantity { get; set; }

        public int QuantityWidth { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string Name { get; set; }

        public int QuantityHeigth { get; set; }

        public Guid? AreaId { get; set; }

        public AreaDto? Area { get; set; }
        [JsonIgnore]
        public StageDto Stage { get; set; }

        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public IsoutType? StageIsin
        {
            get
            {
                if (Stage.IsIn && Stage.IsOut)
                    return IsoutType.Both;
                if (Stage.IsIn)
                    return IsoutType.IsIn;
                if (Stage.IsOut)
                    return IsoutType.IsOut;
                return null;
            }
            set
            {
                _isoutType = value;

                //Assign true as appropriate
                switch (value)
                {
                    case IsoutType.IsIn:
                        Stage.IsIn = true;
                        Stage.IsOut = false;
                        break;
                    case IsoutType.IsOut:
                        Stage.IsIn = false;
                        Stage.IsOut = true;
                        break;
                    case IsoutType.Both:
                        Stage.IsIn = true;
                        Stage.IsOut = true;
                        break;
                    default:
                        Stage.IsIn = false;
                        Stage.IsOut = false;
                        break;
                }


            }
        }

        private IsoutType? _isoutType;

    }
}
