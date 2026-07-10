using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SPTarkov.Common.Json.Converters;

namespace SPTarkov.Common.Models.Logging;

public sealed class SptLoggerConfiguration
{
    [JsonPropertyName("loggers")]
    public List<BaseSptLoggerReference> Loggers { get; init; } = [];
}

[JsonConverter(typeof(BaseSptLoggerReferenceConverter))]
public abstract class BaseSptLoggerReference
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required LoggerType Type { get; init; }

    [JsonPropertyName("filters")]
    public List<SptLoggerFilter> Filters { get; init; } = [];

    [JsonPropertyName("logLevel")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required LogLevel LogLevel { get; init; }

    [JsonPropertyName("format")]
    public required string Format { get; init; }

    private string? _cachedFormat;
    private CompositeFormat? _compiledFormat;

    public virtual CompositeFormat GetCompiledFormat()
    {
        if (_cachedFormat != Format)
        {
            var convertedFormat = Format
                .Replace("%date%", "{0}")
                .Replace("%time%", "{1}")
                .Replace("%message%", "{2}")
                .Replace("%loggerShort%", "{3}")
                .Replace("%logger%", "{4}")
                .Replace("%tid%", "{5}")
                .Replace("%tname%", "{6}")
                .Replace("%level%", "{7}");

            _compiledFormat = CompositeFormat.Parse(convertedFormat);
            _cachedFormat = Format;
        }

        return _compiledFormat!;
    }
}

public sealed class SptLoggerFilter
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required SptLoggerFilterType Type { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("matchingType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required MatchingType MatchingType { get; init; }

    private bool Equals(SptLoggerFilter other)
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

        return Equals((SptLoggerFilter)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Name, (int)MatchingType);
    }
}

public sealed class FileSptLoggerReference : BaseSptLoggerReference
{
    [JsonPropertyName("filePath")]
    public required string FilePath { get; init; }

    [JsonPropertyName("filePattern")]
    public required string FilePattern { get; init; }

    [JsonPropertyName("maxFileSizeMB")]
    public int MaxFileSizeMb
    {
        get;
        init
        {
            if (value < 0)
            {
                throw new Exception("Invalid value for MaxFileSizeMb, must be >= 0");
            }

            field = value;
        }
    }

    [JsonPropertyName("maxRollingFiles")]
    public int MaxRollingFiles
    {
        get;
        init
        {
            if (value < 0)
            {
                throw new Exception("Invalid value for MaxRollingFiles, must be >= 0");
            }

            field = value;
        }
    }
}

public sealed class ConsoleSptLoggerReference : BaseSptLoggerReference { }

public enum LoggerType
{
    File,
    Console,
}

public enum MatchingType
{
    Literal,
    Regex,
}

public enum SptLoggerFilterType
{
    Exclude,
    Include,
}

public static class SptLoggerFilterExtensions
{
    /// <summary>
    /// The cached regex's, keyed to the filter's name with the value being the regex in question
    /// </summary>
    private static readonly ConcurrentDictionary<string, Regex> _cachedRegexes = [];

    public static bool Match(this SptLoggerFilter filter, SptLogMessage message)
    {
        if (string.IsNullOrEmpty(filter.Name))
        {
            return false;
        }

        if (string.IsNullOrEmpty(message.Logger))
        {
            return false;
        }

        switch (filter.MatchingType)
        {
            case MatchingType.Literal:
            {
                return string.Equals(filter.Name, message.Logger, StringComparison.Ordinal);
            }

            case MatchingType.Regex:
            {
                var regex = _cachedRegexes.GetOrAdd(filter.Name, static pattern => new Regex(pattern));

                return regex.IsMatch(message.Logger);
            }

            default:
            {
                return false;
            }
        }
    }

    public static bool CanLog(this LogLevel logLevel, LogLevel messageLevel)
    {
        return messageLevel >= logLevel;
    }
}
