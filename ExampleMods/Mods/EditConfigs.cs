using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;

namespace ExampleMods.Mods
{
    [Injectable]
    public class EditConfigs
    {
        private readonly ConfigServer _configServer;
        private readonly BotConfig _botConfig;
        private readonly HideoutConfig _hideoutConfig;
        private readonly WeatherConfig _weatherConfig;
        private readonly AirdropConfig _airdropConfig;
        private readonly PmcChatResponse _pmcChatResponseConfig;
        private readonly QuestConfig _questConfig;
        private readonly PmcConfig _pmcConfig;

        // We access configs via ConfigServer
        public EditConfigs(
            ConfigServer configServer)
        {
            _configServer = configServer;

            // We get the bot config by calling GetConfig and passing the configs 'type' within the diamond brackets
            _botConfig = _configServer.GetConfig<BotConfig>();
            _hideoutConfig = _configServer.GetConfig<HideoutConfig>();
            _weatherConfig = _configServer.GetConfig<WeatherConfig>();
            _airdropConfig = _configServer.GetConfig<AirdropConfig>();
            _pmcChatResponseConfig = _configServer.GetConfig<PmcChatResponse>();
            _questConfig = _configServer.GetConfig<QuestConfig>();
            _pmcConfig = _configServer.GetConfig<PmcConfig>();

            Run();
        }

        public void Run()
        {
            // Let's edit the weather config to make the season winter
            _weatherConfig.OverrideSeason = Season.WINTER;

            // Let's edit the hideout config to Make all crafts take 60 seconds
            _hideoutConfig.OverrideCraftTimeSeconds = 60;

            // Let's edit the hideout config to Make all upgrades take 60 seconds
            _hideoutConfig.OverrideBuildTimeSeconds = 60;

            // Let's edit the airdrop config to Make weapon/armor drops really common
            _airdropConfig.AirdropTypeWeightings[SptAirdropTypeEnum.weaponArmor] = 999;

            // Let's edit the airdrop config to Make weapon/armor drops always have 3 sealed weapon crates
            var weaponCrateMinMax = _airdropConfig.Loot["weaponArmor"].WeaponCrateCount;
            weaponCrateMinMax.Min = 3;
            weaponCrateMinMax.Max = 3;

            // Let's make PMCs always mail you when they kill you
            _pmcChatResponseConfig.Killer.ResponseChancePercent = 100;

            // Let's make quest rewards sent to you via mail last for over a week if you have an unheard profile
            _questConfig.MailRedeemTimeHours["unheard_edition"] = 168;

            // Let's make the interchange bot cap huge
            _botConfig.MaxBotCap["interchange"] = 50;

            // Let's disable loot on scavs
            _botConfig.DisableLootOnBotTypes.Add("assault");

            // Let's make the conversion rate of scavs to pmcs 100% on factory day
            var factory4DayConversionSettings = _pmcConfig.ConvertIntoPmcChance["factory4_day"];
            var assaultConversionSettings = factory4DayConversionSettings["assault"];
            assaultConversionSettings.Min = 100;
            assaultConversionSettings.Max = 100;
        }
    }
}
