using Mss.WorkForce.Code.Models.Resources;

namespace Mss.WorkForce.Code.ConfigurationCheckLogs
{
    public class ConfigCheck
    {
        public List<string> Values = new List<string>();

        public Dictionary<string, List<ResourceMessage>> KeyValuePairs { get; set; } = new Dictionary<string, List<ResourceMessage>>
        {
            {"Areas", new List<ResourceMessage>()},
            {"Routes", new List<ResourceMessage>()},
            {"Zones", new List<ResourceMessage>()},
            {"Docks", new List<ResourceMessage>()},
            {"Resources", new List<ResourceMessage>()},
            {"Flows", new List<ResourceMessage>()},
            {"Processes", new List<ResourceMessage>()},
        };
    }
}
