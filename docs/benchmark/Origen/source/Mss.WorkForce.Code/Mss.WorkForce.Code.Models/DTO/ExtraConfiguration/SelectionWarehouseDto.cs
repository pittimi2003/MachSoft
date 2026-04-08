namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionWarehouseDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static SelectionWarehouseDto NewDto()
        {
            return new SelectionWarehouseDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
