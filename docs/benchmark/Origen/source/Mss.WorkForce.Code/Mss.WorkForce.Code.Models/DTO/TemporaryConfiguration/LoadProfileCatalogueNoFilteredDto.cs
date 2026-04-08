namespace Mss.WorkForce.Code.Models
{
    public class LoadProfileCatalogueNoFilteredDto
    { 
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static ProcessCatalogueDto NewDto()
        {
            return new ProcessCatalogueDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
