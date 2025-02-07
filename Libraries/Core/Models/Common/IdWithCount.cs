using System.Text.Json.Serialization;

namespace Core.Models.Common;

public record IdWithCount
{
    /**
     * Id of stack to take money from
     */
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    /**
     * Amount of money to take off player for treatment
     */
    [JsonPropertyName("count")]
    public double? Count
    {
        get;
        set;
    }
}
