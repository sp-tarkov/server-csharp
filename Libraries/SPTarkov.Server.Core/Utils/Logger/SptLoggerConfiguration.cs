using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils.Logger;

public class SptLoggerConfiguration
{
    [JsonPropertyName("loggers")]
    public List<BaseSptLoggerReference> Loggers { get; set; }

    [JsonPropertyName("poolingTimeMs")]
    public uint PoolingTimeMs { get; set; } = 500;
}

public abstract class BaseSptLoggerReference
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoggerType Type { get; set; }

    [JsonPropertyName("filters")]
    public List<SptLoggerFilter> Filters { get; set; }

    [JsonPropertyName("logLevel")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LogLevel LogLevel { get; set; }

    [JsonPropertyName("format")]
    public required string Format { get; set; }

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

public class SptLoggerFilter
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SptLoggerFilterType Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("matchingType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MatchingType MatchingType { get; set; }

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

        return Equals((SptLoggerFilter)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Name, (int)MatchingType);
    }
}

public class FileSptLoggerReference : BaseSptLoggerReference
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; }

    [JsonPropertyName("filePattern")]
    public string FilePattern { get; set; }

    private readonly int _maxFileSizeMb;

    [JsonPropertyName("maxFileSizeMB")]
    public int MaxFileSizeMb
    {
        get { return _maxFileSizeMb; }
        init
        {
            if (value < 0)
            {
                throw new Exception("Invalid value for MaxFileSizeMb, must be >= 0");
            }
            _maxFileSizeMb = value;
        }
    }

    private readonly int _maxRollingFiles;

    [JsonPropertyName("maxRollingFiles")]
    public int MaxRollingFiles
    {
        get { return _maxRollingFiles; }
        init
        {
            if (value < 0)
            {
                throw new Exception("Invalid value for MaxRollingFiles, must be >= 0");
            }
            _maxRollingFiles = value;
        }
    }
}

public class ConsoleSptLoggerReference : BaseSptLoggerReference { }

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
    private static readonly ConcurrentDictionary<SptLoggerFilter, Regex> _cachedRegexes = new();

    public static bool Match(this SptLoggerFilter filter, SptLogMessage message)
    {
        switch (filter.MatchingType)
        {
            case MatchingType.Literal:
                if (filter.Name != message.Logger)
                {
                    return false;
                }
                break;
            case MatchingType.Regex:
                if (!_cachedRegexes.TryGetValue(filter, out var regex))
                {
                    regex = new Regex(filter.Name);
                    while (!_cachedRegexes.TryAdd(filter, regex))
                        ;
                }

                if (!regex.IsMatch(message.Logger))
                {
                    return false;
                }
                break;
        }

        return true;
    }

    public static bool CanLog(this LogLevel logLevel, LogLevel messageLevel)
    {
        return logLevel >= messageLevel;
    }
}
