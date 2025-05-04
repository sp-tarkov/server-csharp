﻿using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record BackupConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-backup";

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("maxBackups")]
    public int MaxBackups { get; set; }

    [JsonPropertyName("directory")]
    public string Directory { get; set; }

    [JsonPropertyName("backupInterval")]
    public BackupConfigInterval BackupInterval { get; set; }
}

public record BackupConfigInterval
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("intervalMinutes")]
    public int IntervalMinutes { get; set; }
}
