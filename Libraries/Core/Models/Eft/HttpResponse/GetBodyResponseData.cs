using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.HttpResponse;

public record GetBodyResponseData<T>
{
    [JsonPropertyName("err")]
    public BackendErrorCodes? Err
    {
        get;
        set;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("errmsg")]
    public string? ErrMsg
    {
        get;
        set;
    }

    [JsonPropertyName("data")]
    public T? Data
    {
        get;
        set;
    }
}
