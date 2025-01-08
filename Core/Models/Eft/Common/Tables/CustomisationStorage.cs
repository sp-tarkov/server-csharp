using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class CustomisationStorage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}