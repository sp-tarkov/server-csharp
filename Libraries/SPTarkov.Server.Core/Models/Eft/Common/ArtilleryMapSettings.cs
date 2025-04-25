using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArtilleryMapSettings
{
    [JsonPropertyName("PlanedShellingOn")]
    public bool? PlanedShellingOn
    {
        get;
        set;
    }

    [JsonPropertyName("InitShellingTimer")]
    public double? InitShellingTimer
    {
        get;
        set;
    }

    [JsonPropertyName("BeforeShellingSignalTime")]
    public double? BeforeShellingSignalTime
    {
        get;
        set;
    }

    [JsonPropertyName("ShellingCount")]
    public double? ShellingCount
    {
        get;
        set;
    }

    [JsonPropertyName("ZonesInShelling")]
    public double? ZonesInShelling
    {
        get;
        set;
    }

    [JsonPropertyName("NewZonesForEachShelling")]
    public bool? NewZonesForEachShelling
    {
        get;
        set;
    }

    [JsonPropertyName("InitCalledShellingTime")]
    public double? InitCalledShellingTime
    {
        get;
        set;
    }

    [JsonPropertyName("ShellingZones")]
    public List<ShellingZone>? ShellingZones
    {
        get;
        set;
    }

    [JsonPropertyName("Brigades")]
    public List<Brigade>? Brigades
    {
        get;
        set;
    }

    [JsonPropertyName("ArtilleryShellingAirDropSettings")]
    public ArtilleryShellingAirDropSettings? ArtilleryShellingAirDropSettings
    {
        get;
        set;
    }

    [JsonPropertyName("PauseBetweenShellings")]
    public XYZ? PauseBetweenShellings
    {
        get;
        set;
    }
}
