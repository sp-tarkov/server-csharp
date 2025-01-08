using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class LocationServices
{
    [JsonPropertyName("TraderServerSettings")]
    public TraderServerSettings? TraderServerSettings { get; set; }

    [JsonPropertyName("BTRServerSettings")]
    public BtrServerSettings? BtrServerSettings { get; set; }
}

public class TraderServerSettings
{
    [JsonPropertyName("TraderServices")]
    public TraderServices? TraderServices { get; set; }
}

public class TraderServices
{
    [JsonPropertyName("ExUsecLoyalty")]
    public TraderServices? ExUsecLoyalty { get; set; }

    [JsonPropertyName("ZryachiyAid")]
    public TraderServices? ZryachiyAid { get; set; }

    [JsonPropertyName("CultistsAid")]
    public TraderServices? CultistsAid { get; set; }

    [JsonPropertyName("PlayerTaxi")]
    public TraderServices? PlayerTaxi { get; set; }

    [JsonPropertyName("BtrItemsDelivery")]
    public TraderServices? BtrItemsDelivery { get; set; }

    [JsonPropertyName("BtrBotCover")]
    public TraderServices? BtrBotCover { get; set; }

    [JsonPropertyName("TransitItemsDelivery")]
    public TraderServices? TransitItemsDelivery { get; set; }
}

public class TraderService
{
    [JsonPropertyName("TraderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("TraderServiceType")]
    public string? TraderServiceType { get; set; }

    [JsonPropertyName("Requirements")]
    public ServiceRequirements? Requirements { get; set; }

    [JsonPropertyName("ServiceItemCost")]
    public Dictionary<string, ServiceItemCostDetails>? ServiceItemCost { get; set; }

    [JsonPropertyName("UniqueItems")]
    public List<string>? UniqueItems { get; set; }
}

public class ServiceRequirements
{
    [JsonPropertyName("CompletedQuests")]
    public List<CompletedQuest>? CompletedQuests { get; set; }

    [JsonPropertyName("Standings")]
    public Dictionary<string, StandingRequirement>? Standings { get; set; }
}

public class CompletedQuest
{
    [JsonPropertyName("QuestId")]
    public string? QuestId { get; set; }
}

public class StandingRequirement
{
    [JsonPropertyName("Value")]
    public int? Value { get; set; }
}

public class ServiceItemCostDetails
{
    [JsonPropertyName("Count")]
    public int? Count { get; set; }
}

public class BtrServerSettings
{
    [JsonPropertyName("ChanceSpawn")]
    public int? ChanceSpawn { get; set; }

    [JsonPropertyName("SpawnPeriod")]
    public XYZ? SpawnPeriod { get; set; }

    [JsonPropertyName("MoveSpeed")]
    public float? MoveSpeed { get; set; }

    [JsonPropertyName("ReadyToDepartureTime")]
    public float? ReadyToDepartureTime { get; set; }

    [JsonPropertyName("CheckTurnDistanceTime")]
    public float? CheckTurnDistanceTime { get; set; }

    [JsonPropertyName("TurnCheckSensitivity")]
    public float? TurnCheckSensitivity { get; set; }

    [JsonPropertyName("DecreaseSpeedOnTurnLimit")]
    public float? DecreaseSpeedOnTurnLimit { get; set; }

    [JsonPropertyName("EndSplineDecelerationDistance")]
    public float? EndSplineDecelerationDistance { get; set; }

    [JsonPropertyName("AccelerationSpeed")]
    public float? AccelerationSpeed { get; set; }

    [JsonPropertyName("DecelerationSpeed")]
    public float? DecelerationSpeed { get; set; }

    [JsonPropertyName("PauseDurationRange")]
    public XYZ? PauseDurationRange { get; set; }

    [JsonPropertyName("BodySwingReturnSpeed")]
    public float? BodySwingReturnSpeed { get; set; }

    [JsonPropertyName("BodySwingDamping")]
    public float? BodySwingDamping { get; set; }

    [JsonPropertyName("BodySwingIntensity")]
    public float? BodySwingIntensity { get; set; }

    [JsonPropertyName("ServerMapBTRSettings")]
    public Dictionary<string, ServerMapBtrsettings>? ServerMapBTRSettings { get; set; }
}

public class ServerMapBtrsettings
{
    [JsonPropertyName("MapID")]
    public string? MapID { get; set; }

    [JsonPropertyName("ChanceSpawn")]
    public int? ChanceSpawn { get; set; }

    [JsonPropertyName("SpawnPeriod")]
    public XYZ? SpawnPeriod { get; set; }

    [JsonPropertyName("MoveSpeed")]
    public float? MoveSpeed { get; set; }

    [JsonPropertyName("ReadyToDepartureTime")]
    public float? ReadyToDepartureTime { get; set; }

    [JsonPropertyName("CheckTurnDistanceTime")]
    public float? CheckTurnDistanceTime { get; set; }

    [JsonPropertyName("TurnCheckSensitivity")]
    public float? TurnCheckSensitivity { get; set; }

    [JsonPropertyName("DecreaseSpeedOnTurnLimit")]
    public float? DecreaseSpeedOnTurnLimit { get; set; }

    [JsonPropertyName("EndSplineDecelerationDistance")]
    public float? EndSplineDecelerationDistance { get; set; }

    [JsonPropertyName("AccelerationSpeed")]
    public float? AccelerationSpeed { get; set; }

    [JsonPropertyName("DecelerationSpeed")]
    public float? DecelerationSpeed { get; set; }

    [JsonPropertyName("PauseDurationRange")]
    public XYZ? PauseDurationRange { get; set; }

    [JsonPropertyName("BodySwingReturnSpeed")]
    public float? BodySwingReturnSpeed { get; set; }

    [JsonPropertyName("BodySwingDamping")]
    public float? BodySwingDamping { get; set; }

    [JsonPropertyName("BodySwingIntensity")]
    public float? BodySwingIntensity { get; set; }
}