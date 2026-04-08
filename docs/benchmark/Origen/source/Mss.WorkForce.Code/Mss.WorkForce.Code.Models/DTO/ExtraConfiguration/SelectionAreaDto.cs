namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class SelectionAreaDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;

        public override string ToString()
        {
            return Name;
        }

        public SelectionAreaDto NewDto()
        {
            return new SelectionAreaDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
