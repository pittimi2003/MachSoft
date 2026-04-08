namespace Mss.WorkForce.Code.Models
{
    public class TypeEquipmentCatalogueDto
    {

        public Guid? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static TypeEquipmentCatalogueDto NewDto()
        {
            return new TypeEquipmentCatalogueDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
