using Mss.WorkForce.Code.Models.DBContext;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Process : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public int MinTime { get; set; }
        public int? PreprocessTime { get; set; }
        public int? PostprocessTime { get; set; }
        public bool IsWarehouseProcess { get; set; }
        public bool IsOut { get; set; }
        public bool IsIn { get; set; }
        public bool IsInitProcess { get; set; }
        public double? PercentageInitProcess { get; set; }
        public Guid AreaId { get; set; }
        public required Area Area { get; set; }
        public string? ViewPort { get; set; }
        public bool IsEffective { get; set; } = true;
        public Guid? FlowId { get; set; }
        public Flow? Flow { get; set; }

        [JsonIgnore]
        public List<ProcessDirectionProperty> ProcessDirectionPropertiesEntry { get; set; }

        [JsonIgnore]
        public List<ProcessDirectionProperty> ProcessDirectionPropertiesExit { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Area = context.Areas.FirstOrDefault(x => x.Id == AreaId)!;
            this.Flow = context.Flow.FirstOrDefault(x => x.Id == FlowId);
        }
        public object Clone()
        {
            Process clonedProcess = (Process)this.MemberwiseClone();
            clonedProcess.Id = Guid.NewGuid();
            return clonedProcess;
        }
    }
}
