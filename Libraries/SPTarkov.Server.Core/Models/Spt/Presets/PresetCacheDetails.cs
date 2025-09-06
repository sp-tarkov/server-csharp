using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Presets;

public record PresetCacheDetails
{
    // Preset Ids related to the tpl
    public HashSet<MongoId> PresetIds { get; set; }

    // Id of the default preset for this tpl
    public MongoId? DefaultId { get; set; }
}
