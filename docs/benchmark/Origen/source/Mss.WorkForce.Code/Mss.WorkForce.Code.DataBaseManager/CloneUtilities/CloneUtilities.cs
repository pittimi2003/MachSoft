using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mss.WorkForce.Code.DataBaseManager.CloneUtilities
{
    public class Cloner
    {
        public string GetNewViewPort(string viewPort, Guid newValue)
        {
            if(viewPort == null || viewPort == string.Empty) return string.Empty;

            return JsonIdReplacer.ReplaceAllIds(viewPort, newValue);
        }

        public string GetNewViewPort(string viewPort)
        {
            if (viewPort == null || viewPort == string.Empty) return string.Empty;

            return JsonIdReplacer.ReplaceAllIds(viewPort);
        }
    }

    public static class JsonIdReplacer
    {
        /// <summary>
        /// Reemplaza todas las propiedades "id" (case-insensitive) por newGuid.
        /// </summary>
        public static string ReplaceAllIds(string json, Guid newGuid)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            var root = JsonNode.Parse(json);
            if (root is null)
                return json;

            // Lógica genérica: mismo GUID para todas las 'id'
            ReplaceIdsRecursive(root, newGuid);

            return root.ToJsonString(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        /// <summary>
        /// Reemplaza los "id" de un array:
        /// - primer elemento -> Guid.NewGuid()
        /// - último elemento -> Guid.NewGuid()
        /// - resto -> Guid.NewGuid()
        /// Si encuentra objetos anidados con 'id' fuera de arrays, los deja intactos.
        /// </summary>
        public static string ReplaceAllIds(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            var root = JsonNode.Parse(json);
            if (root is null)
                return json;

            // Lógica por rango: solo aplicable si es JsonArray en la raíz o anidado
            ReplaceIdsRecursive(root);

            return root.ToJsonString(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        // ------------------------------------------------
        // Reemplazo único para todas las 'id'
        // ------------------------------------------------
        private static void ReplaceIdsRecursive(JsonNode node, Guid newGuid)
        {
            if (node is JsonObject obj)
            {
                // Iteramos sobre una copia de las propiedades
                foreach (var kvp in obj.ToList())
                {
                    if (string.Equals(kvp.Key, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        obj[kvp.Key] = newGuid.ToString();
                    }
                    else if (kvp.Value is not null)
                    {
                        ReplaceIdsRecursive(kvp.Value, newGuid);
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var child in arr)
                    if (child is not null)
                        ReplaceIdsRecursive(child, newGuid);
            }
        }

        // ------------------------------------------------
        // Rango para arrays
        // ------------------------------------------------
        private static void ReplaceIdsRecursive(JsonNode node)
        {
            if (node is JsonArray arr && arr.Count > 0)
            {
                // Solo aplicamos el rango a este nivel de array
                for (int i = 0; i < arr.Count; i++)
                {
                    if (arr[i] is JsonObject itemObj)
                    {
                        // Si tiene 'id', lo sustituimos según posición
                        if (itemObj.ToList()
                                   .Any(p => string.Equals(p.Key, "id", StringComparison.OrdinalIgnoreCase)))
                        {
                            //TODO: RSCM lo dejo de esta manera por si llegara a ser necesario en un futuro identificar inicio y termino 
                            var replacement = i == 0
                                ? Guid.NewGuid() // el primer elemento recibe el GUID de inicio
                                : i == arr.Count - 1
                                    ? Guid.NewGuid() // el último elemento recibe el GUID de fin
                                    : Guid.NewGuid();// los del medio reciben uno nuevo dinámico

                            // Aseguramos que la clave exista exactamente igual
                            var key = itemObj.ToList()
                                             .First(p => string.Equals(p.Key, "id", StringComparison.OrdinalIgnoreCase))
                                             .Key;

                            itemObj[key] = replacement.ToString();
                        }
                    }

                    // Recurse en cada elemento (por si hay arrays/objetos anidados)
                    ReplaceIdsRecursive(arr[i]);
                }
            }
            else if (node is JsonObject obj)
            {               
                foreach (var kvp in obj.ToList())
                {
                    if (kvp.Value is not null)
                        ReplaceIdsRecursive(kvp.Value);
                }
            }
        }
    }   

    public static class ViewPortIdRemapper
    {
        public static string Remap(string? json, Guid selfNewId, IReadOnlyDictionary<Guid, Guid> theMap)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;

            JsonNode? root;
            try { root = JsonNode.Parse(json); }
            catch { return json; } // si no es JSON válido, lo devolvemos tal cual

            if (root is null) return json;

            RemapNode(root, selfNewId, theMap);

            return root.ToJsonString(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        private static void RemapNode(JsonNode node, Guid selfNewId, IReadOnlyDictionary<Guid, Guid> theMap)
        {
            if (node is JsonObject obj)
            {
                // Recorremos una snapshot para poder editar
                foreach (var kvp in obj.ToList())
                {
                    var propName = kvp.Key;
                    var valueNode = kvp.Value;

                    if (valueNode is null) continue;

                    // 1) Forzar "Id" propio del objeto a selfNewId
                    if (propName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        obj[propName] = selfNewId.ToString();
                        continue;
                    }

                    // 2) Para cualquier "*Id" remapear si está en mapa
                    if (propName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && valueNode is JsonValue jv)
                    {
                        if (TryGetGuid(jv, out var oldId) && theMap.TryGetValue(oldId, out var newId))
                        {
                            obj[propName] = newId.ToString();
                            continue;
                        }
                    }

                    // 3) Recursión
                    RemapNode(valueNode, selfNewId, theMap);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var child in arr)
                    if (child is not null)
                        RemapNode(child, selfNewId, theMap);
            }
        }

        private static bool TryGetGuid(JsonValue val, out Guid guid)
        {
            guid = Guid.Empty;
            try
            {
                if (val.TryGetValue<string>(out var s) && Guid.TryParse(s, out guid)) return true;
                if (val.TryGetValue<Guid>(out var g)) { guid = g; return true; }
            }
            catch { /* ignorar */ }
            return false;
        }
    }

}
