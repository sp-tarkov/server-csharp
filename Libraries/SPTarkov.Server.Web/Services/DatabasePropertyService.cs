using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Web.Models.Database;

namespace SPTarkov.Server.Web.Services;

[Injectable]
public class DatabasePropertyService(ISptLogger<DatabasePropertyService> logger)
{
    public IReadOnlyList<DatabaseProperty> BuildProperties(string propertiesJson)
    {
        if (string.IsNullOrWhiteSpace(propertiesJson) || propertiesJson == "{}")
        {
            return [];
        }

        var properties = new List<DatabaseProperty>();
        var node = JsonNode.Parse(propertiesJson);

        if (node is JsonObject obj)
        {
            foreach (var property in obj)
            {
                AddPropertyRows(properties, property.Key, property.Value);
            }
        }

        return properties;
    }

    private static void AddPropertyRows(List<DatabaseProperty> properties, string path, JsonNode? node)
    {
        switch (node)
        {
            case null:
                properties.Add(new DatabaseProperty(path, "null", "Null"));
                break;
            case JsonObject obj:
                if (obj.Count == 0)
                {
                    properties.Add(new DatabaseProperty(path, "{}", "Object"));
                    return;
                }

                foreach (var property in obj)
                {
                    AddPropertyRows(properties, $"{path}.{property.Key}", property.Value);
                }

                break;
            case JsonArray array:
                if (array.Count == 0)
                {
                    properties.Add(new DatabaseProperty(path, "[]", "Array"));
                    return;
                }

                for (var index = 0; index < array.Count; index++)
                {
                    AddPropertyRows(properties, $"{path}[{index}]", array[index]);
                }

                break;
            case JsonValue value:
                properties.Add(new DatabaseProperty(path, GetJsonValueLabel(value), GetJsonValueKind(value)));
                break;
        }
    }

    private static string GetJsonValueLabel(JsonValue value)
    {
        var element = value.GetValue<JsonElement>();

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => element.GetRawText(),
            _ => element.GetRawText(),
        };
    }

    private static string GetJsonValueKind(JsonValue value)
    {
        return value.GetValue<JsonElement>().ValueKind switch
        {
            JsonValueKind.String => "String",
            JsonValueKind.Number => "Number",
            JsonValueKind.True or JsonValueKind.False => "Boolean",
            _ => "Value",
        };
    }
}
