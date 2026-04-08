using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class ProcessDto : BaseDesignerDto
    {
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public ProcessType? Type { get; set; }
        [JsonIgnore]
        public string TypeName { get; set; }
        [JsonIgnore]
        public int MinTime { get; set; }
        [JsonIgnore]
        public int? PreprocessTime { get; set; }
        [JsonIgnore]
        public int? PostprocessTime { get; set; }
        [JsonIgnore]
        public bool IsWarehouseProcess { get; set; }        
        public bool IsOut { get; set; }        
        public bool IsIn { get; set; }
        [JsonIgnore]
        public bool IsInitProcess { get; set; }        
        public double? PercentageInitProcess { get; set; }
        public Guid AreaId { get; set; }
        [JsonIgnore]
        public AreaDto? AreaDto { get; set; }
        public Guid? ProcessTypeId { get; set; }

        public Guid? FlowId { get; set; }

        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public ParentFlowDto? Flow { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Ignore in Serialize and Not ignore in Deserialize
        public Guid? IdOriginal { get; set; }

        public static ProcessDto? NewDto(AreaDto itemArea, ParentFlowDto flow)
        {
            return new ProcessDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Type = null,
                TypeName = string.Empty,
                MinTime = 0,
                PostprocessTime = 0,
                PreprocessTime = 0,
                IsWarehouseProcess = false,
                IsOut = false,
                IsIn = false,
                IsInitProcess = false,
                PercentageInitProcess = 0,
                AreaId = (Guid)itemArea.Id,
                AreaDto = itemArea,
                ProcessTypeId = Guid.Empty,
                FlowId = flow.Id,
                Flow = flow,
            };
        }
    }
}
