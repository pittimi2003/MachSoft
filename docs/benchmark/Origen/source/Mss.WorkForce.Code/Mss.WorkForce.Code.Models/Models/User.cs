using Mss.WorkForce.Code.Models.DBContext;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mss.WorkForce.Code.Models.Models
{
	public class User : IFillable
	{
		public Guid Id { get; set; }
		public required string Code { get; set; }
		public required string Name { get; set; }
		public required string Lastname { get; set; }
		public required string Password { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsActive { get; set; }
        public Guid? DecimalSeparatorId { get; set; }
		public DecimalSeparator? DecimalSeparator { get; set; }
        public Guid? ThousandsSeparatorId { get; set; }
        public ThousandsSeparator? ThousandsSeparator { get; set; }
        public Guid? DateFormatId { get; set; }
        public DateFormat? DateFormat { get; set; }
        public Guid? HourFormatId { get; set; }
        public HourFormat? HourFormat { get; set; }
        public Guid? LanguageId { get; set; }
        public Language? Language { get; set; }
        private DateTime? lastAccessDate;
        public DateTime? LastAccessDate
        {
            get => lastAccessDate;
            set => lastAccessDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        private DateTime creationDate;
		public DateTime CreationDate
        {
            get => creationDate;
            set => creationDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public Guid OrganizationId { get; set; }
		public Organization Organization { get; set; }
        public Guid? WarehouseDefaultId { get; set; }
        public Warehouse? WarehouseDefault { get; set; }
        public ICollection<Warehouse> Warehouses { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Organization = context.Organizations.FirstOrDefault(x => x.Id == OrganizationId)!;
            DecimalSeparator = context.DecimalSeparators.FirstOrDefault(x => x.Id == DecimalSeparatorId);
            ThousandsSeparator = context.ThousandsSeparators.FirstOrDefault(x => x.Id == ThousandsSeparatorId);
            DateFormat = context.DateFormats.FirstOrDefault(x => x.Id == DateFormatId);
            HourFormat = context.HourFormats.FirstOrDefault(x => x.Id == HourFormatId);
            Language = context.Languages.FirstOrDefault(x => x.Id == LanguageId);
            WarehouseDefault = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseDefaultId);
        }
    }
}
