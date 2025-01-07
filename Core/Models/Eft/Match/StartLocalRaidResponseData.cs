using System.Text.Json.Serialization;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Models.Eft.Match;

public class StartLocalRaidResponseData
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; }
    
    [JsonPropertyName("serverSettings")]
    public LocationServices ServerSettings { get; set; }
    
    [JsonPropertyName("profile")]
    public ProfileInsuredItems Profile { get; set; }
    
    [JsonPropertyName("locationLoot")]
    public LocationBase LocationLoot { get; set; }
    
    [JsonPropertyName("transitionType")]
    public TransitionType TransitionType { get; set; }
    
    [JsonPropertyName("transition")]
    public Transition Transition { get; set; }
}

public class ProfileInsuredItems
{
    [JsonPropertyName("insuredItems")]
    public List<InsuredItem> InsuredItems { get; set; }
}

public class Transition
{
    [JsonPropertyName("transitionType")]
    public TransitionType TransitionType { get; set; }
    
    [JsonPropertyName("transitionRaidId")]
    public string TransitionRaidId { get; set; }
    
    [JsonPropertyName("transitionCount")]
    public int TransitionCount { get; set; }
    
    [JsonPropertyName("visitedLocations")]
    public List<string> VisitedLocations { get; set; }
}