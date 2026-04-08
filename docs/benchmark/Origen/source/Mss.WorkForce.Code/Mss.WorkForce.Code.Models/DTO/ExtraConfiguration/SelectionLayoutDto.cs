namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionLayoutDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;

        public override string ToString()
        {
            return Name;
        }

        public SelectionLayoutDto NewDto()
        {
            return new SelectionLayoutDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
