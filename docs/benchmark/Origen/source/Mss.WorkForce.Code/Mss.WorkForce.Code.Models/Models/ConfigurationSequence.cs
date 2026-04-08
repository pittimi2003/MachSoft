using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class ConfigurationSequence : IFillable
    {
        public Guid Id { get; set; }
        private DateTime date;
		public DateTime Date
        {
            get => date;
            set => date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public int Sequence { get; set; }
        public Guid ConfigurationSequenceHeaderId { get; set; }
        public required ConfigurationSequenceHeader ConfigurationSequenceHeader { get; set; }


        public void Fill(ApplicationDbContext context)
        {
            this.ConfigurationSequenceHeader = context.ConfigurationSequenceHeaders.FirstOrDefault(x => x.Id == ConfigurationSequenceHeaderId)!;
        }
        public object Clone()
        {
            ConfigurationSequenceHeader clonedConfigurationSequenceHeader = (ConfigurationSequenceHeader)this.MemberwiseClone();
            clonedConfigurationSequenceHeader.Id = Guid.NewGuid();
            return clonedConfigurationSequenceHeader;
        }
    }
}
