using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Exit
{
    /// <summary>
    ///     % Chance out of 100 exit will appear in raid
    /// </summary>
    [JsonPropertyName("Chance")]
    public double? Chance
    {
        get;
        set;
    }

    [JsonPropertyName("ChancePVE")]
    public double? ChancePVE
    {
        get;
        set;
    }

    [JsonPropertyName("Count")]
    public int? Count
    {
        get;
        set;
    }

    [JsonPropertyName("CountPVE")]
    public int? CountPVE
    {
        get;
        set;
    }

    // Had to add this property as BSG sometimes names the properties with full PVE capitals
    // This property will just point the value to CountPve
    [JsonPropertyName("CountPve")]
    public int CountPve
    {
        set
        {
            CountPVE = value;
        }
    }

    [JsonPropertyName("EntryPoints")]
    public string? EntryPoints
    {
        get;
        set;
    }

    [JsonPropertyName("EventAvailable")]
    public bool? EventAvailable
    {
        get;
        set;
    }

    [JsonPropertyName("EligibleForPMC")]
    public bool? EligibleForPMC
    {
        get;
        set;
    }

    [JsonPropertyName("EligibleForScav")]
    public bool? EligibleForScav
    {
        get;
        set;
    }

    [JsonPropertyName("ExfiltrationTime")]
    public double? ExfiltrationTime
    {
        get;
        set;
    }

    [JsonPropertyName("ExfiltrationTimePVE")]
    public double? ExfiltrationTimePVE
    {
        get;
        set;
    }

    [JsonPropertyName("ExfiltrationType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExfiltrationType? ExfiltrationType
    {
        get;
        set;
    }

    [JsonPropertyName("RequiredSlot")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EquipmentSlots? RequiredSlot
    {
        get;
        set;
    }

    [JsonPropertyName("Id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("MaxTime")]
    public double? MaxTime
    {
        get;
        set;
    }

    [JsonPropertyName("MaxTimePVE")]
    public double? MaxTimePVE
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("MinTime")]
    public double? MinTime
    {
        get;
        set;
    }

    [JsonPropertyName("MinTimePVE")]
    public double? MinTimePVE
    {
        get;
        set;
    }

    [JsonPropertyName("Name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("_Name")]
    public string? _Name
    {
        get;
        set;
    }

    [JsonPropertyName("_name")]
    public string? _NameLower
    {
        get;
        set;
    }

    [JsonPropertyName("PassageRequirement")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RequirementState? PassageRequirement
    {
        get;
        set;
    }

    [JsonPropertyName("PlayersCount")]
    public int? PlayersCount
    {
        get;
        set;
    }

    [JsonPropertyName("PlayersCountPVE")]
    public int? PlayersCountPVE
    {
        get;
        set;
    }

    [JsonPropertyName("RequirementTip")]
    public string? RequirementTip
    {
        get;
        set;
    }

    [JsonPropertyName("Side")]
    public string? Side
    {
        get;
        set;
    }
}
