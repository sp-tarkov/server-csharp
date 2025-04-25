using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record GroupPosition
{
    private string? _name;

    [JsonPropertyName("Name")]
    public string? Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value == null ? null : string.Intern(value);
        }
    }

    [JsonPropertyName("Weight")]
    public double? Weight
    {
        get;
        set;
    }

    [JsonPropertyName("Position")]
    public XYZ? Position
    {
        get;
        set;
    }

    [JsonPropertyName("Rotation")]
    public XYZ? Rotation
    {
        get;
        set;
    }
}
