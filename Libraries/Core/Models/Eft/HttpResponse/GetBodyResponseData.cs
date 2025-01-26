using System.Text.Json.Serialization;

namespace Core.Models.Eft.HttpResponse;

public record GetBodyResponseData<T>
{
    [JsonPropertyName("err")]
    public int? Err { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("errmsg")]
    public string? ErrMsg { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
