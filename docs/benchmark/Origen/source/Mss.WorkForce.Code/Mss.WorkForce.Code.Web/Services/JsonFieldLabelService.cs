using Mecalux.ITSW.ApplicationDictionary.Utilities;
using System.Text.Json;

namespace Mss.WorkForce.Code.Web.Services
{
    public class JsonFieldLabelService : IFieldLabelService
    {
        private Dictionary<string, Dictionary<string, string>> _labels = new();
        private List<string> configJS = new List<string> { "WorkOrderPlanning", "TaskDataDescription" };
        public void Load()
        {

            string json = string.Empty;
            foreach (string label in configJS)
            {
                switch (label)
                {
                    case "WorkOrderPlanning":
                        json = File.ReadAllText("Data/FieldLabels.json");
                        _labels = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json)
                                  ?? new Dictionary<string, Dictionary<string, string>>();
                        break;

                    case "TaskDataDescription":
                        json = File.ReadAllText("Data/TaskDataDescription.json");
                        _labels.AddRange(JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json)
                                  ?? new Dictionary<string, Dictionary<string, string>>());
                        break;
                }
            }
        }

        public string GetLabel(string entityName, string fieldName)
        {
            if (_labels.TryGetValue(entityName, out var fields) &&
                fields.TryGetValue(fieldName, out var label))
            {
                return label;
            }

            return fieldName;
        }


    }
}
