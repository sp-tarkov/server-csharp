using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class GetMailDialogListRequestData
{
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }
    
    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}