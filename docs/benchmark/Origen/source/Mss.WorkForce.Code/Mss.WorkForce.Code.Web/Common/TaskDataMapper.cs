namespace Mss.WorkForce.Code.Web.Common
{
    using System.Text.Json;
    using System.Reflection;
    public class TaskDataMapper
    {
        private readonly Dictionary<string, string> _fieldMappings;

        public TaskDataMapper(string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);

            var model = JsonSerializer.Deserialize<TaskDataDescriptionModel>(json);

            _fieldMappings = model?.TaskData ?? new Dictionary<string, string>();
        }

        public string GetAlias(string originalName)
        {
            return _fieldMappings.TryGetValue(originalName, out var alias) ? alias : originalName;
        }

        public Dictionary<string, string> ApplyAliases(Dictionary<string, string> input)
        {
            return input.ToDictionary(
                kvp => GetAlias(kvp.Key),
                kvp => kvp.Value
            );
        }
    }

    public class TaskDataDescriptionModel
    {
        public Dictionary<string, string> TaskData { get; set; }
    }
}
