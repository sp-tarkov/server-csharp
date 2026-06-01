using System.Text.Json;
using SPTarkov.Server.Web.Models.Database;

namespace SPTarkov.Server.Web.Utils;

public static class JsonPropertyFlattener
{
    public static IReadOnlyList<DatabaseProperty> BuildProperties(string propertiesJson)
    {
        if (string.IsNullOrWhiteSpace(propertiesJson) || propertiesJson == "{}")
        {
            return [];
        }

        using var document = JsonDocument.Parse(propertiesJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        var properties = new List<DatabaseProperty>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            AddPropertyRows(properties, property.Name, property.Value);
        }

        return properties;
    }

    private static void AddPropertyRows(List<DatabaseProperty> properties, string path, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (!element.EnumerateObject().Any())
                {
                    properties.Add(new DatabaseProperty(path, "{}", "Object"));
                    return;
                }

                foreach (var property in element.EnumerateObject())
                {
                    AddPropertyRows(properties, $"{path}.{property.Name}", property.Value);
                }

                break;
            case JsonValueKind.Array:
                if (!element.EnumerateArray().Any())
                {
                    properties.Add(new DatabaseProperty(path, "[]", "Array"));
                    return;
                }

                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    AddPropertyRows(properties, $"{path}[{index}]", item);
                    index++;
                }

                break;
            case JsonValueKind.Null:
                properties.Add(new DatabaseProperty(path, "null", "Null"));
                break;
            default:
                properties.Add(new DatabaseProperty(path, GetJsonValueLabel(element), GetJsonValueKind(element)));
                break;
        }
    }

    private static string GetJsonValueLabel(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.GetRawText(),
        };
    }

    private static string GetJsonValueKind(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => "String",
            JsonValueKind.Number => "Number",
            JsonValueKind.True or JsonValueKind.False => "Boolean",
            _ => "Value",
        };
    }
}
