 
namespace Mss.WorkForce.Code.Models
{
    public class StationCatalogueDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static StationCatalogueDto NewDto()
        {
            return new StationCatalogueDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }

    }
}
