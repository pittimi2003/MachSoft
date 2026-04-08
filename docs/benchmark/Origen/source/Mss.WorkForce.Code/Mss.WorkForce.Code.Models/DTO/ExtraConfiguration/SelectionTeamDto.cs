namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionTeamDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static SelectionTeamDto NewDto()
        {
            return new SelectionTeamDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
