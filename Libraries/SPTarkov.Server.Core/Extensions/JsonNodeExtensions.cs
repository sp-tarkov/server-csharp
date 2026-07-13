using System.Text.Json.Nodes;

namespace SPTarkov.Server.Core.Extensions;

public static class JsonNodeExtensions
{
    public static bool TryGetObject(this JsonNode? node, out JsonObject value, params string[] path)
    {
        value = null!;

        if (!TryGetNode(node, out var foundNode, path) || foundNode is not JsonObject foundObject)
        {
            return false;
        }

        value = foundObject;
        return true;
    }

    public static bool TryGetArray(this JsonNode? node, out JsonArray value, params string[] path)
    {
        value = null!;

        if (!TryGetNode(node, out var foundNode, path) || foundNode is not JsonArray foundArray)
        {
            return false;
        }

        value = foundArray;
        return true;
    }

    public static bool TryGetValue<T>(this JsonNode? node, out T value, params string[] path)
    {
        value = default!;

        if (!TryGetNode(node, out var foundNode, path) || foundNode is not JsonValue foundValue)
        {
            return false;
        }

        return foundValue.TryGetValue(out value!);
    }

    public static bool TryGetNode(this JsonNode? node, out JsonNode? value, params string[] path)
    {
        value = node;

        foreach (var pathPart in path)
        {
            if (value is not JsonObject currentObject || !currentObject.TryGetPropertyValue(pathPart, out value))
            {
                value = null;
                return false;
            }
        }

        return value is not null;
    }
}
