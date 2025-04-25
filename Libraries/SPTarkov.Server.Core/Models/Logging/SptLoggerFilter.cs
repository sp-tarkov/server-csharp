using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.Logger;

namespace SPTarkov.Server.Core.Models.Logging;

public class SptLoggerFilter
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SptLoggerFilterType Type
    {
        get;
        set;
    }

    [JsonPropertyName("name")]
    public string Name
    {
        get;
        set;
    }

    [JsonPropertyName("matchingType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MatchingType MatchingType
    {
        get;
        set;
    }

    protected bool Equals(SptLoggerFilter other)
    {
        return Type == other.Type && Name == other.Name && MatchingType == other.MatchingType;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((SptLoggerFilter) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) Type, Name, (int) MatchingType);
    }
}
