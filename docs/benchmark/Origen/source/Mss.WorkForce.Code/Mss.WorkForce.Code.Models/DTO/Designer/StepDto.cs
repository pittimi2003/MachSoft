using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class StepDto
    {
        private StepInitEndType? _stepType;

        public Guid Id { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string? Name { get; set; }

        [Range(1, double.MaxValue, ErrorMessageResourceName = "ACTIVITYTIMEMUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int TimeMin { get; set; }

        [Range(1, int.MaxValue, ErrorMessageResourceName = "ORDERMUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int Order { get; set; }

        public bool InitProcess { get; set; }

        public bool EndProcess { get; set; }

        public Guid ProcessId { get; set; }

        public string ViewPort { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public StepInitEndType? StepInitEnd
        {
            get
            {
                if (InitProcess)
                    return StepInitEndType.Before;
                if (EndProcess)
                    return StepInitEndType.After;
                return null;
            }
            set
            {
                _stepType = value;

                // Sincroniza InitProcess y EndProcess
                InitProcess = value == StepInitEndType.Before;
                EndProcess = value == StepInitEndType.After;
            }
        }

        [NotMapped]
        public IEnumerable<StepDto>? Siblings { get; set; }

        public static StepDto NewDto()
        {
            return new StepDto()
            {
                Id = Guid.Empty,
                Name = string.Empty,
                TimeMin = 0,
                Order = 0,
                InitProcess = false,
                EndProcess = false,
                ProcessId = Guid.Empty,
                ViewPort = string.Empty
            };
        }

        // Used to clone the DxGri information to have a backup copy in memory
        public StepDto Clone()
        {
            return new StepDto
            {
                Id = this.Id,
                Name = this.Name,
                TimeMin = this.TimeMin,
                Order = this.Order,
                StepInitEnd = this.StepInitEnd,
                Siblings = null,// no se copia esto porque se asigna dinámicamente
                InitProcess = this.InitProcess,
                EndProcess = this.EndProcess,
                ProcessId = this.ProcessId,
                ViewPort = this.ViewPort
            };
        }
    }
}