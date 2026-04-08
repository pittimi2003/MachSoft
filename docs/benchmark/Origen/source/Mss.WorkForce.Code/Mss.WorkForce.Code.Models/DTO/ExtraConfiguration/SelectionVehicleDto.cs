
namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionVehicleDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static SelectionVehicleDto NewDto()
        {
            return new SelectionVehicleDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
