using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// See BotGlobalsChangeSettings in the client, this record should match that
/// </summary>
public record BotGlobalsChangeSettings
{
    [JsonPropertyName("SMOKE_VISION_DIST")]
    public float SmokeVisionDist { get; set; }

    [JsonPropertyName("SMOKE_GAIN_SIGHT")]
    public float SmokeGainSight { get; set; }

    [JsonPropertyName("SMOKE_SCATTERING")]
    public float SmokeScattering { get; set; }

    [JsonPropertyName("SMOKE_PRECICING")]
    public float SmokePrecicing { get; set; }

    [JsonPropertyName("SMOKE_HEARING")]
    public float SmokeHearing { get; set; }

    [JsonPropertyName("SMOKE_ACCURATY")]
    public float SmokeAccuraty { get; set; }

    [JsonPropertyName("SMOKE_LAY_CHANCE")]
    public float SmokeLayChance { get; set; }

    [JsonPropertyName("FLASH_VISION_DIST")]
    public float FlashVisionDist { get; set; }

    [JsonPropertyName("FLASH_GAIN_SIGHT")]
    public float FlashGainSight { get; set; }

    [JsonPropertyName("FLASH_SCATTERING")]
    public float FlashScattering { get; set; }

    [JsonPropertyName("FLASH_PRECICING")]
    public float FlashPrecicing { get; set; }

    [JsonPropertyName("FLASH_HEARING")]
    public float FlashHearing { get; set; }

    [JsonPropertyName("FLASH_ACCURATY")]
    public float FlashAccuraty { get; set; }

    [JsonPropertyName("FLASH_LAY_CHANCE")]
    public float FlashLayChance { get; set; }

    [JsonPropertyName("STUN_HEARING")]
    public float StunHearing { get; set; }

    [JsonPropertyName("INVISIBLE_ON_CLIENT")]
    public bool InvisibleOnClient { get; set; }
}
