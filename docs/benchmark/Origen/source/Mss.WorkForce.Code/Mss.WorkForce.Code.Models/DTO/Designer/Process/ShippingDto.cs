namespace Mss.WorkForce.Code.Models.DTO.Designer.Process
{
    public class ShippingDto : BaseDesignerDto
    {
        public Guid Id { get; set; }
        public int? Quantity { get; set; }
        public Guid ProcessId { get; set; }
        public ProcessDto? Process { get; set; }
        public string? Viewport { get; set; }

        public static ShippingDto NewDto(ProcessDto process)
        {
            return new ShippingDto
            {
                Id = Guid.NewGuid(),
                Quantity = 0,
                ProcessId = process.Id ?? Guid.Empty,
                Process = process,
                Viewport = string.Empty
            };
        }
    }

}
