using System.Text.Json.Serialization;
namespace SPTarkov.Server.Core.Models.Enums;

public record AccountTypes
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public const string SPT_DEVELOPER = "spt developer";
}
