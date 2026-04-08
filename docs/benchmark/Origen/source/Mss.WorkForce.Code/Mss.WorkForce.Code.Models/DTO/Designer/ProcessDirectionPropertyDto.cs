using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class ProcessDirectionPropertyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Percentage { get; set; }
        public Guid InitProcessId { get; set; }
        public bool InitProcessIsOut { get; set; }
        public bool InitProcessIsIn { get; set; }

        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public ProcessDto? InitProcess { get; set; }
        public Guid EndProcessId { get; set; }
        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public ProcessDto? EndProcess { get; set; }
        public bool IsEnd { get; set; }
        //[JsonIgnore]
        public string ViewPort { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de ProcessDirectionPropertyDto con valores por defecto.
        /// </summary>
        /// <param name="initProcess">Proceso de inicio</param>
        /// <param name="endProcess">Proceso de fin</param>
        public static ProcessDirectionPropertyDto NewDto(ProcessDto initProcess, ProcessDto endProcess)
        {
            return new ProcessDirectionPropertyDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Percentage = 0,
                InitProcessId = (Guid)initProcess.Id,
                InitProcessIsIn = (bool)initProcess.IsIn,
                InitProcessIsOut = (bool)initProcess.IsOut,
                InitProcess = initProcess,
                EndProcessId = (Guid)endProcess.Id,
                EndProcess = endProcess,
                IsEnd = false,
                ViewPort = string.Empty
            };
        }
    }

}
