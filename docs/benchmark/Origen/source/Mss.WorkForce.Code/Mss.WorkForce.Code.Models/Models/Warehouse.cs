using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
	public class Warehouse : IFillable, ICloneable
    {
		public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public Guid? TimeZoneId { get; set; }
        public TimeZone? TimeZone_ { get; set; }
        public string? Description { get; set; }
		public string? Address { get; set; }
        public string? AddressLine { get; set; }
        public string? ZIPCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public Guid? CountryId { get; set; }
        public Country? Country { get; set; }
        public string? AddressComment { get; set; }
        public string? ContactName { get; set; }
        public string? Telephone { get; set; }
        public string? Extension { get; set; }
        public string? Telephone2 { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? ContactComment { get; set; }
        public required string MeasureSystem { get; set; }
		public Guid OrganizationId { get; set; }
		public Organization Organization { get; set; }
        public ICollection<User> Users { get; set; }
        public bool? IsConfigValid { get; set; }
        public SimulationMode Mode { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Organization = context.Organizations.FirstOrDefault(x => x.Id == OrganizationId)!;
            TimeZone_ = context.TimeZones.FirstOrDefault(x => x.Id == TimeZoneId);
            Country = context.Countries.FirstOrDefault(x => x.Id == CountryId);
        }
        public object Clone()
        {
            Warehouse clonedWarehouse = (Warehouse)this.MemberwiseClone();
            clonedWarehouse.Id = Guid.NewGuid();
            clonedWarehouse.Name = $"{clonedWarehouse.Name}_Cloned";
            return clonedWarehouse;
        }
    }

    public enum SimulationMode
    {
        Manual,
        Automatic,
    }
}
