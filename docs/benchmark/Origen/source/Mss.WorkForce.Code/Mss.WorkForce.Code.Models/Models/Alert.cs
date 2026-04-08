using Mss.WorkForce.Code.Models.DBContext;
using System.ComponentModel;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Alert : IFillable
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public string EntityCode { get; set; }
        public string EntityField { get; set; }
        public AlertOperator Operator { get; set; }
        public string? Reference { get; set; }
        public string? FixedValue { get; set; }
        public bool IsFixed { get; set; }
        public IEnumerable<AlertConfiguration> Configurations { get; set; } = new List<AlertConfiguration>();
        public bool IsMail { get; set; }
        public string? Message { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
    }
    public enum AlertOperator
    {
        [Description(">")]
        GreaterThan,
        [Description("<")]
        LessThan,
        [Description("=")]
        Equal,
        [Description("!=")]
        NotEqual,
        [Description(">=")]
        GreaterOrEqual,
        [Description("<=")]
        LessOrEqual
    }
    public enum AlertType
    {
        [Description("Bell")]
        Bell,

        [Description("Email")]
        Email,

        [Description("General Indicator")]
        GeneralIndicator,

        [Description("Specific Indicator")]
        SpecificIndicator
    }
    public enum AlertSeverity
    {
        [Description("Error")]
        Error,
        [Description("Warning")]
        Warning,
        [Description("Information")]
        Information
    }
}
