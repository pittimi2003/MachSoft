using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Organization : IFillable
	{
		public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
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
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? ContactComment { get; set; }
        public Guid DecimalSeparatorId { get; set; }
        public required DecimalSeparator DecimalSeparator { get; set; }
        public Guid ThousandsSeparatorId { get; set; }
        public required ThousandsSeparator ThousandsSeparator { get; set; }
        public Guid DateFormatId { get; set; }
        public required DateFormat DateFormat { get; set; }
        public Guid? HourFormatId { get; set; }
        public HourFormat? HourFormat { get; set; }
        public Guid LanguageId { get; set; }
        public required Language Language { get; set; }
        public Guid SystemOfMeasurementId { get; set; }
        public required SystemOfMeasurement SystemOfMeasurement { get; set; }
        public string? Logo { get; set; }
        public ICollection<Warehouse> Warehouses { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Country = context.Countries.FirstOrDefault(x => x.Id == CountryId);
            DecimalSeparator = context.DecimalSeparators.FirstOrDefault(x => x.Id == DecimalSeparatorId)!;
            ThousandsSeparator = context.ThousandsSeparators.FirstOrDefault(x => x.Id == ThousandsSeparatorId)!;
            DateFormat = context.DateFormats.FirstOrDefault(x => x.Id == DateFormatId)!;
            HourFormat = context.HourFormats.FirstOrDefault(x => x.Id == HourFormatId)!;
            Language = context.Languages.FirstOrDefault(x => x.Id == LanguageId)!;
            SystemOfMeasurement = context.SystemOfMeasurements.FirstOrDefault(x => x.Id == SystemOfMeasurementId)!;
        }
    }
}
