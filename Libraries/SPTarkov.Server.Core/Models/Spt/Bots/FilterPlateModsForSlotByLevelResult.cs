using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record FilterPlateModsForSlotByLevelResult
{
    [JsonPropertyName("result")]
    public Result? Result { get; set; }

    [JsonPropertyName("plateModTpls")]
    public HashSet<string>? PlateModTemplates { get; set; }
}

public enum Result
{
    UNKNOWN_FAILURE = -1,
    SUCCESS = 1,
    NO_DEFAULT_FILTER = 2,
    NOT_PLATE_HOLDING_SLOT = 3,
    LACKS_PLATE_WEIGHTS = 4,
}
