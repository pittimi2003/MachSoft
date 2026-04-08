namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class InboundDto : BaseDesignerDto
    {
        public int Quantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public int VehiclePerHour { get; set; }
        public int TruckPerDay { get; set; }
        public int? MinTimeInBuffer { get; set; }
        public int? LoadTime { get; set; }
        public int? AdditionalTimeInBuffer { get; set; }
        public Guid ProcessId { get; set; }
        public ProcessDto? Process { get; set; }
        public string? Viewport { get; set; }

        public static InboundDto? NewDto(ProcessDto Process) 
        {
            return new InboundDto
            {
                Id = Guid.NewGuid(),
                Quantity = 0,
                UnitOfMeasure = string.Empty,
                VehiclePerHour = 0,
                TruckPerDay = 0,
                MinTimeInBuffer = 0,
                LoadTime = 0,
                AdditionalTimeInBuffer = 0,
                ProcessId = (Guid)Process.Id,
                Process = Process,
                Viewport = string.Empty
            };
        }
    }
}
