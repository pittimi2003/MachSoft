namespace Mss.WorkForce.Code.Models.DTO.Designer.Process
{
    public class PickingDto : BaseDesignerDto
    {        
        public int? PickingRoadTime { get; set; }
        public Guid ProcessId { get; set; }
        public ProcessDto? Process { get; set; }
        public string? Viewport { get; set; }

        public static PickingDto NewDto(ProcessDto process)
        {
            return new PickingDto
            {
                Id = Guid.NewGuid(),
                PickingRoadTime = 0,
                ProcessId = process.Id ?? Guid.Empty,
                Process = process,
                Viewport = string.Empty
            };
        }
    }

}
