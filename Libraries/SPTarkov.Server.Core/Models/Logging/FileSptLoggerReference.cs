using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Logging;

public class FileSptLoggerReference : BaseSptLoggerReference
{
    [JsonPropertyName("filePath")]
    public string FilePath
    {
        get;
        set;
    }
}
