using System.Text.Json.Serialization;

namespace Core.Models.Eft.HttpResponse;

public class NullResponseData
{
    [JsonPropertyName("err")]
    public int? Err { get; set; }

    [JsonPropertyName("errmsg")]
    public object? ErrMsg { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}