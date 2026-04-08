namespace Mss.WorkForce.Code.Models
{
    public class TypeEquipmentCatalogueNoFilteredDto
    {

        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static TypeEquipmentCatalogueNoFilteredDto NewDto()
        {
            return new TypeEquipmentCatalogueNoFilteredDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
