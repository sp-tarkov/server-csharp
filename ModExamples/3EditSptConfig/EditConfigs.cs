using Core.Models.Enums;
using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using SptCommon.Annotations;

namespace _3EditSptConfig;

[Injectable]
public class EditConfigs : IPostDBLoadMod
{
    private readonly AirdropConfig _airdropConfig;
    private readonly BotConfig _botConfig;
    private readonly ConfigServer _configServer;
    private readonly HideoutConfig _hideoutConfig;

    private readonly ISptLogger<EditConfigs> _logger;
    private readonly PmcChatResponse _pmcChatResponseConfig;
    private readonly PmcConfig _pmcConfig;
    private readonly QuestConfig _questConfig;
    private readonly WeatherConfig _weatherConfig;

    // We access configs via ConfigServer
    public EditConfigs(
        ConfigServer configServer,
        ISptLogger<EditConfigs> logger
    )
    {
        _configServer = configServer;
        _logger = logger;

        // We get the bot config by calling GetConfig and passing the configs 'type' within the diamond brackets
        _botConfig = _configServer.GetConfig<BotConfig>();
        _hideoutConfig = _configServer.GetConfig<HideoutConfig>();
        _weatherConfig = _configServer.GetConfig<WeatherConfig>();
        _airdropConfig = _configServer.GetConfig<AirdropConfig>();
        _pmcChatResponseConfig = _configServer.GetConfig<PmcChatResponse>();
        _questConfig = _configServer.GetConfig<QuestConfig>();
        _pmcConfig = _configServer.GetConfig<PmcConfig>();
    }

    public void PostDBLoad()
    {
        // Let's edit the weather config to force the season to winter
        _weatherConfig.OverrideSeason = Season.WINTER;

        // Let's edit the hideout config to Make all crafts take 60 seconds
        _hideoutConfig.OverrideCraftTimeSeconds = 60;

        // Let's edit the hideout config to Make all upgrades take 60 seconds
        _hideoutConfig.OverrideBuildTimeSeconds = 60;

        // Let's edit the airdrop config to Make weapon/armor drops REALLY common
        _airdropConfig.AirdropTypeWeightings[SptAirdropTypeEnum.weaponArmor] = 999;

        // Let's edit the airdrop config to Make weapon/armor drops always have 3 sealed weapon crates
        var weaponCrateMinMax = _airdropConfig.Loot["weaponArmor"].WeaponCrateCount;
        weaponCrateMinMax.Min = 3;
        weaponCrateMinMax.Max = 3;

        // Let's make PMCs always mail you when they kill you
        _pmcChatResponseConfig.Killer.ResponseChancePercent = 100;

        // Let's make quest rewards sent to you via mail last for over a week for unheard profiles
        _questConfig.MailRedeemTimeHours["unheard_edition"] = 168;

        // Let's make the interchange bot cap huge
        _botConfig.MaxBotCap["interchange"] = 50;

        // Let's disable loot on scavs
        _botConfig.DisableLootOnBotTypes.Add("assault");

        _logger.Success("Finished Editing Configs");
    }
}
