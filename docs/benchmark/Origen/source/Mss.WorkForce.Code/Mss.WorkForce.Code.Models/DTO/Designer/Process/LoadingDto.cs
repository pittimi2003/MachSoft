namespace Mss.WorkForce.Code.Models.DTO.Designer.Process
{
    public class LoadingDto : BaseDesignerDto
    {
        public string? Dock { get; set; }
        public int? AutomaticLoadingTime { get; set; }
        public int? AdditionalTimeInBuffer { get; set; }
        public Guid ProcessId { get; set; }
        public ProcessDto? Process { get; set; }
        public string? Viewport { get; set; }

        public static LoadingDto NewDto(ProcessDto process)
        {
            return new LoadingDto
            {
                Id = Guid.NewGuid(),
                Dock = string.Empty,
                AutomaticLoadingTime = 0,
                AdditionalTimeInBuffer = 0,
                ProcessId = process.Id ?? Guid.Empty,
                Process = process,
                Viewport = string.Empty
            };
        }
    }
}

