namespace Core.Models.Eft.Common.Tables;

using System.Text.Json.Serialization;

public record CustomizationItem
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_name")]
    public string? Name { get; set; }

    [JsonPropertyName("_parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("_type")]
    public string? Type { get; set; }

    [JsonPropertyName("_props")]
    public Props? Properties { get; set; }

    [JsonPropertyName("_proto")]
    public string? Proto { get; set; }
}
