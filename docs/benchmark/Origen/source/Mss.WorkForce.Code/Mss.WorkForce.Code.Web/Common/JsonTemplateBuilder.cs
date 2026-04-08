using System.Text;
using System.Text.Json;

namespace Mss.WorkForce.Code.Web.Common
{
    public static class JsonTemplateBuilder
    {
        public static string BuildContentText(string contentJson)
        {            
            if (string.IsNullOrWhiteSpace(contentJson))
                return string.Empty;

            try
            {
                using var doc = JsonDocument.Parse(contentJson);
                var root = NormalizeRoot(doc.RootElement);

                var sb = new StringBuilder();

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in root.EnumerateArray())
                    {
                        BuildObjectText(item, 0, sb);
                        sb.AppendLine();
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                    BuildObjectText(root, 0, sb);

                var result = sb.ToString().TrimEnd();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TransactionService::BuildContentText => error in building the transaction content");
                return string.Empty;
            }
        }

        private static void BuildObjectText(JsonElement element, int level, StringBuilder sb)
        {
            var indent = level != 0 ? new string(' ', level * 4) : "";
            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                {
                    sb.AppendLine($"{indent}{prop.Name}:");
                    BuildValueText(prop.Value, level + 1, sb);
                }
                else
                    sb.AppendLine($"{indent}{prop.Name}: {GetValue(prop.Value)}");

            }
        }

        private static void BuildValueText(JsonElement value, int level, StringBuilder sb)
        {
            var indent = new string(' ', level * 4);

            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    BuildObjectText(value, level, sb);
                    break;

                case JsonValueKind.Array:
                    foreach (var item in value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                            BuildValueText(item, level + 1, sb);
                        else
                            sb.AppendLine($"{indent}- {GetValue(item)}");
                    }
                    break;
            }
        }

        private static string GetValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Null => "null",
                JsonValueKind.String => value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => value.ToString()
            };
        }

        private static JsonElement NormalizeRoot(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                var list = root.EnumerateArray().Select(NormalizeRoot).ToList();
                return JsonDocument.Parse(JsonSerializer.Serialize(list)).RootElement.Clone();
            }

            if (root.ValueKind != JsonValueKind.Object)
                return root;

            var dict = new Dictionary<string, object>();
            foreach (var prop in root.EnumerateObject())
            {
                var normalizedValue = NormalizeRoot(prop.Value);
                AddProperty(dict, prop.Name.Split('.'), normalizedValue);
            }

            var json = JsonSerializer.Serialize(dict);
            return JsonDocument.Parse(json).RootElement.Clone();
        }

        private static void AddProperty(Dictionary<string, object> current, string[] path, JsonElement value, int index = 0)
        {
            var key = path[index];

            if (index == path.Length - 1)
            {
                if (value.ValueKind == JsonValueKind.Object || value.ValueKind == JsonValueKind.Array)
                    current[key] = value;
                else
                    current[key] = GetValue(value);

                return;
            }

            if (!current.TryGetValue(key, out var next) || next is not Dictionary<string, object>)
            {
                next = new Dictionary<string, object>();
                current[key] = next;
            }

            AddProperty((Dictionary<string, object>)next, path, value, index + 1);
        }
    }
}
