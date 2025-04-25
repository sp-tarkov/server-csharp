using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ComposedKey
{
    private string? _key;

    [JsonPropertyName("key")]
    public string? Key
    {
        get
        {
            return _key;
        }
        set
        {
            _key = string.Intern(value);
        }
    }
}
