namespace Mss.WorkForce.Code.Models.DTO.Designer.Process
{
    public class ReceptionDto : BaseDesignerDto
    {
        public int? BreakageFactor { get; set; }
        public Guid ProcessId { get; set; }
        public ProcessDto? Process { get; set; }
        public string? Viewport { get; set; }

        public static ReceptionDto NewDto(ProcessDto process)
        {
            return new ReceptionDto
            {
                Id = Guid.NewGuid(),
                BreakageFactor = 0,
                ProcessId = process.Id ?? Guid.Empty,
                Process = process,
                Viewport = string.Empty
            };
        }
    }

}
