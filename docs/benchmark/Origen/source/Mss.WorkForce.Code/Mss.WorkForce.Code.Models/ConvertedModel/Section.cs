using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models.ConvertedModel
{
    public class Section
    {
        public List<ConfigurationSequence> ConfigurationSequences { get; set; }
        public List<Warehouse> Warehouses { get; set; }
        public List<Organization> Organizations { get; set; }
        public List<User> Users { get; set; }
    }
}
