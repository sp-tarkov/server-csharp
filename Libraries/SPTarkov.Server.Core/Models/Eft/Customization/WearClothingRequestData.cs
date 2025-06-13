using System.Text.Json.Serialization;
namespace SPTarkov.Server.Core.Models.Eft.Customization;

public record WearClothingRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

}
