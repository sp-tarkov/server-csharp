using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ColliderParams
{
    private string? _parent;

    [JsonPropertyName("_parent")]
    public string? Parent
    {
        get
        {
            return _parent;
        }
        set
        {
            _parent = string.Intern(value);
        }
    }

    [JsonPropertyName("_props")]
    public ColliderProps? Props
    {
        get;
        set;
    }
}
