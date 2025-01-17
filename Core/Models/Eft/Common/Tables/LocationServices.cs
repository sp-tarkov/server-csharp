using System.Text.Json.Serialization;
using Core.Utils.Json.Converters;

namespace Core.Models.Eft.Common.Tables;

public record LocationServices
{
    [JsonPropertyName("TraderServerSettings")]
    public TraderServerSettings? TraderServerSettings { get; set; }

    [JsonPropertyName("BTRServerSettings")]
    public BtrServerSettings? BtrServerSettings { get; set; }
}

public record TraderServerSettings
{
    [JsonPropertyName("TraderServices")]
    public TraderServices? TraderServices { get; set; }
}

public record TraderServices
{
    [JsonPropertyName("ExUsecLoyalty")]
    public TraderService? ExUsecLoyalty { get; set; }

    [JsonPropertyName("ZryachiyAid")]
    public TraderService? ZryachiyAid { get; set; }

    [JsonPropertyName("CultistsAid")]
    public TraderService? CultistsAid { get; set; }

    [JsonPropertyName("PlayerTaxi")]
    public TraderService? PlayerTaxi { get; set; }

    [JsonPropertyName("BtrItemsDelivery")]
    public TraderService? BtrItemsDelivery { get; set; }

    [JsonPropertyName("BtrBotCover")]
    public TraderService? BtrBotCover { get; set; }

    [JsonPropertyName("TransitItemsDelivery")]
    public TraderService? TransitItemsDelivery { get; set; }
}

public record TraderService
{
    [JsonPropertyName("TraderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("TraderServiceType")]
    public string? TraderServiceType { get; set; }

    [JsonPropertyName("Requirements")]
    public ServiceRequirements? Requirements { get; set; }

    [JsonPropertyName("ServiceItemCost")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, ServiceItemCostDetails>? ServiceItemCost { get; set; }

    [JsonPropertyName("UniqueItems")]
    public List<string>? UniqueItems { get; set; }
}

public record ServiceRequirements
{
    [JsonPropertyName("CompletedQuests")]
    public List<CompletedQuest>? CompletedQuests { get; set; }

    [JsonPropertyName("Standings")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, StandingRequirement>? Standings { get; set; }
}

public record CompletedQuest
{
    [JsonPropertyName("QuestId")]
    public string? QuestId { get; set; }
}

public record StandingRequirement
{
    [JsonPropertyName("Value")]
    public double? Value { get; set; }
}

public record ServiceItemCostDetails
{
    [JsonPropertyName("Count")]
    public int? Count { get; set; }
}

public record BtrServerSettings
{
    [JsonPropertyName("ChanceSpawn")]
    public double? ChanceSpawn { get; set; }

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
    public double? DecreaseSpeedOnTurnLimit { get; set; }

    [JsonPropertyName("EndSplineDecelerationDistance")]
    public double? EndSplineDecelerationDistance { get; set; }

    [JsonPropertyName("AccelerationSpeed")]
    public double? AccelerationSpeed { get; set; }

    [JsonPropertyName("DecelerationSpeed")]
    public double? DecelerationSpeed { get; set; }

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

public record ServerMapBtrsettings
{
    [JsonPropertyName("MapID")]
    public string? MapID { get; set; }

    [JsonPropertyName("ChanceSpawn")]
    public double? ChanceSpawn { get; set; }

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
