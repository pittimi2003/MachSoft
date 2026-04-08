namespace Mss.WorkForce.Code.Models.Resources
{
    public class ResourceMessage
    {
        public string Key { get; set; }
        public string DefaultValue { get; set; }
        public List<string> Arguments { get; set; } = new();
    
        public ResourceMessage() { }

        public ResourceMessage(string key, string defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
        }

        public ResourceMessage(string key, string defaultValue, params string[] arguments) 
        {
            Key = key;
            DefaultValue = defaultValue;
            Arguments = arguments?.ToList() ?? new List<string>();
        }
    }
}
