using System.Text.Json.Serialization;
using Types.Models.Spt.Repeatable;

namespace Types.Models.Spt.Hideout;

public class Hideout
{
    [JsonPropertyName("areas")]
    public List<HideoutArea> Areas { get; set; }
    
    [JsonPropertyName("customisation")]
    public HideoutCustomisation Customisation { get; set; }
    
    [JsonPropertyName("production")]
    public HideoutProductionData Production { get; set; }
    
    [JsonPropertyName("settings")]
    public HideoutSettingsBase Settings { get; set; }
    
    [JsonPropertyName("qte")]
    public List<qteData> Qte { get; set; }
}