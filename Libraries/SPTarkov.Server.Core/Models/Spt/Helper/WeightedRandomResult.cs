using System.Text.Json.Serialization;
namespace SPTarkov.Server.Core.Models.Spt.Helper;

public record WeightedRandomResult<T>
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public required T Item
    {
        get;
        set;
    }

    public required int Index
    {
        get;
        set;
    }
}
