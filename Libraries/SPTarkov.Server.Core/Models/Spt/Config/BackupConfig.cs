using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record BackupConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-backup";

    [JsonPropertyName("enabled")]
    public required bool Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("maxBackups")]
    public int MaxBackups
    {
        get;
        set;
    }

    [JsonPropertyName("directory")]
    public string Directory
    {
        get;
        set;
    } = string.Empty;

    [JsonPropertyName("backupInterval")]
    public required BackupConfigInterval BackupInterval
    {
        get;
        set;
    }
}

public record BackupConfigInterval
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("intervalMinutes")]
    public int IntervalMinutes
    {
        get;
        set;
    }
}
