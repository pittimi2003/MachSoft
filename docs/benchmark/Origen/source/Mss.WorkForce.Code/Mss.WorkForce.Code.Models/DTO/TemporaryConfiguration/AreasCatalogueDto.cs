
namespace Mss.WorkForce.Code.Models
{
    public class AreasCatalogueDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static AreasCatalogueDto NewDto()
        {
            return new AreasCatalogueDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
