using System.Text.Json.Serialization;

namespace Core.Models.Eft.HttpResponse;

public class GetBodyResponseData<T>
{
    [JsonPropertyName("err")]
    public int? Err { get; set; }

    [JsonPropertyName("errmsg")]
    public string? ErrMsg { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}