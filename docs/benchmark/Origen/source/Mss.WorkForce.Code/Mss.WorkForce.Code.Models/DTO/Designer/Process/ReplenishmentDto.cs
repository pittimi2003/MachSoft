namespace Mss.WorkForce.Code.Models.DTO.Designer.Process
{
    public class ReplenishmentDto : BaseDesignerDto
    {
        public int Percentage { get; set; }

        public Guid ProcessId { get; set; }

        public ProcessDto? Process { get; set; }

        public string? ViewPort { get; set; }

        public static ReplenishmentDto NewDto(ProcessDto process)
        {
            return new ReplenishmentDto
            {
                Id = Guid.NewGuid(),
                Percentage = 0,
                ProcessId = process.Id ?? Guid.Empty,
                Process = process,
                ViewPort = string.Empty
            };
        }
    }
}
