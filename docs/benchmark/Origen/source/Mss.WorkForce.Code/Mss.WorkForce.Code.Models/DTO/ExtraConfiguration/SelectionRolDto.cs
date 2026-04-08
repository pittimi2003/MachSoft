namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionRolDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static SelectionRolDto NewDto()
        {
            return new SelectionRolDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
