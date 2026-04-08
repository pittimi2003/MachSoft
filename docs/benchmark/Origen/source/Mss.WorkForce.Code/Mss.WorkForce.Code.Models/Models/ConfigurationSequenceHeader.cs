using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class ConfigurationSequenceHeader : IFillable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Data { get; set; }
        public Guid WarehouseId { get; set; }
        public required Warehouse Warehouse { get; set; }
        public ICollection<ConfigurationSequence> ConfigurationSequences { get; set; } = new List<ConfigurationSequence>();

        public void Fill(ApplicationDbContext context)
        {
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
        public object Clone()
        {
            ConfigurationSequenceHeader clonedConfigurationSequenceHeader = (ConfigurationSequenceHeader)this.MemberwiseClone();
            clonedConfigurationSequenceHeader.Id = Guid.NewGuid();
            return clonedConfigurationSequenceHeader;
        }
    }
}
