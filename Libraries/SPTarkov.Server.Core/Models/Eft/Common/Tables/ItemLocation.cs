using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ItemLocation
{
    [JsonPropertyName("x")]
    public int? X
    {
        get;
        set;
    }

    [JsonPropertyName("y")]
    public int? Y
    {
        get;
        set;
    }

    [JsonPropertyName("r")]
    public object? R
    {
        get;
        set;
    } // TODO: Can be string or number

    [JsonPropertyName("isSearched")]
    public bool? IsSearched
    {
        get;
        set;
    }

    /// <summary>
    ///     SPT property?
    /// </summary>
    [JsonPropertyName("rotation")]
    public object? Rotation
    {
        get;
        set;
    } // TODO: Can be string or boolean
}
