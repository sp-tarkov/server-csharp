using System.Text.Json.Serialization;
using Core.Models.Spt.Repeatable;

namespace Core.Models.Spt.Hideout;

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