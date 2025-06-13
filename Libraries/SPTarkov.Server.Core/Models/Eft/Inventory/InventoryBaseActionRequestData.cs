using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Request;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public abstract record InventoryBaseActionRequestData : BaseInteractionRequestData
{
}

public record To
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("container")]
    public string? Container
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public object? Location
    {
        get;
        set;
    } // TODO: types given IItemLocation or number

    [JsonPropertyName("isSearched")]
    public bool? IsSearched
    {
        get;
        set;
    }
}

public record Container
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("container")]
    public string? ContainerName
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public object? Location
    {
        get;
        set;
    } // TODO: types given: ILocation or number - BSG data object shows as location only.
}

public record Location
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("x")]
    public double? X
    {
        get;
        set;
    }

    [JsonPropertyName("y")]
    public double? Y
    {
        get;
        set;
    }

    [JsonPropertyName("r")]
    public string? R
    {
        get;
        set;
    }

    [JsonPropertyName("rotation")]
    public string? Rotation
    {
        get;
        set;
    }

    [JsonPropertyName("isSearched")]
    public bool? IsSearched
    {
        get;
        set;
    }
}
