using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Profile;

public class GetOtherProfileResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("aid")]
    public int Aid { get; set; }
    
    [JsonPropertyName("info")]
    public OtherProfileInfo Info { get; set; }
    
    [JsonPropertyName("customization")]
    public OtherProfileCustomization Customization { get; set; }
    
    [JsonPropertyName("skills")]
    public Skills Skills { get; set; }
    
    [JsonPropertyName("equipment")]
    public OtherProfileEquipment Equipment { get; set; }
    
    [JsonPropertyName("achievements")]
    public Dictionary<string, int> Achievements { get; set; }
    
    [JsonPropertyName("favoriteItems")]
    public List<Item> FavoriteItems { get; set; }
    
    [JsonPropertyName("pmcStats")]
    public OtherProfileStats PmcStats { get; set; }
    
    [JsonPropertyName("scavStats")]
    public OtherProfileStats ScavStats { get; set; }
}

public class OtherProfileInfo
{
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }
    
    [JsonPropertyName("side")]
    public string Side { get; set; }
    
    [JsonPropertyName("experience")]
    public int Experience { get; set; }
    
    [JsonPropertyName("memberCategory")]
    public int MemberCategory { get; set; }
    
    [JsonPropertyName("bannedState")]
    public bool BannedState { get; set; }
    
    [JsonPropertyName("bannedUntil")]
    public long BannedUntil { get; set; }
    
    [JsonPropertyName("registrationDate")]
    public long RegistrationDate { get; set; }
}

public class OtherProfileCustomization
{
    [JsonPropertyName("head")]
    public string Head { get; set; }
    
    [JsonPropertyName("body")]
    public string Body { get; set; }
    
    [JsonPropertyName("feet")]
    public string Feet { get; set; }
    
    [JsonPropertyName("hands")]
    public string Hands { get; set; }
    
    [JsonPropertyName("dogtag")]
    public string Dogtag { get; set; }
}

public class OtherProfileEquipment
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }
    
    [JsonPropertyName("Items")]
    public List<Item> Items { get; set; }
}

public class OtherProfileStats
{
    [JsonPropertyName("eft")]
    public OtherProfileSubStats Eft { get; set; }
}

public class OtherProfileSubStats
{
    [JsonPropertyName("totalInGameTime")]
    public int TotalInGameTime { get; set; }
    
    [JsonPropertyName("overAllCounters")]
    public OverallCounters OverAllCounters { get; set; }
}