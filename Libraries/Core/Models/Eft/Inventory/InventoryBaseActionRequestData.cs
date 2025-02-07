using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Inventory;

public abstract record InventoryBaseActionRequestData : BaseInteractionRequestData
{
}

public record To
{
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
    } // TODO: types given: ILocation or number
}

public record Location
{
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
