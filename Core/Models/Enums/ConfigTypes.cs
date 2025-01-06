using Core.Models.Spt.Config;

namespace Core.Models.Enums;

public static class ConfigTypesExtension
{
    public static string GetValue(this ConfigTypes type)
    {
        return type switch
        {
            ConfigTypes.AIRDROP => "spt-airdrop",
            ConfigTypes.BACKUP => "spt-backup",
            ConfigTypes.BOT => "spt-bot",
            ConfigTypes.PMC => "spt-pmc",
            ConfigTypes.CORE => "spt-core",
            ConfigTypes.HEALTH => "spt-health",
            ConfigTypes.HIDEOUT => "spt-hideout",
            ConfigTypes.HTTP => "spt-http",
            ConfigTypes.IN_RAID => "spt-inraid",
            ConfigTypes.INSURANCE => "spt-insurance",
            ConfigTypes.INVENTORY => "spt-inventory",
            ConfigTypes.ITEM => "spt-item",
            ConfigTypes.LOCALE => "spt-locale",
            ConfigTypes.LOCATION => "spt-location",
            ConfigTypes.LOOT => "spt-loot",
            ConfigTypes.MATCH => "spt-match",
            ConfigTypes.PLAYERSCAV => "spt-playerscav",
            ConfigTypes.PMC_CHAT_RESPONSE => "spt-pmcchatresponse",
            ConfigTypes.QUEST => "spt-quest",
            ConfigTypes.RAGFAIR => "spt-ragfair",
            ConfigTypes.REPAIR => "spt-repair",
            ConfigTypes.SCAVCASE => "spt-scavcase",
            ConfigTypes.TRADER => "spt-trader",
            ConfigTypes.WEATHER => "spt-weather",
            ConfigTypes.SEASONAL_EVENT => "spt-seasonalevents",
            ConfigTypes.LOST_ON_DEATH => "spt-lostondeath",
            ConfigTypes.GIFTS => "spt-gifts",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public static Type GetConfigType(this ConfigTypes type)
    {
        return type switch
        {
            ConfigTypes.AIRDROP => typeof(AirdropConfig),
            ConfigTypes.BACKUP => typeof(BackupConfig),
            ConfigTypes.BOT => typeof(BotConfig),
            ConfigTypes.PMC => typeof(PmcConfig),
            ConfigTypes.CORE => typeof(CoreConfig),
            ConfigTypes.HEALTH => typeof(HealthConfig),
            ConfigTypes.HIDEOUT => typeof(HideoutConfig),
            ConfigTypes.HTTP => typeof(HttpConfig),
            ConfigTypes.IN_RAID => typeof(InRaidConfig),
            ConfigTypes.INSURANCE => typeof(InsuranceConfig),
            ConfigTypes.INVENTORY => typeof(InventoryConfig),
            ConfigTypes.ITEM => typeof(ItemConfig),
            ConfigTypes.LOCALE => typeof(LocaleConfig),
            ConfigTypes.LOCATION => typeof(LocationConfig),
            ConfigTypes.LOOT => typeof(LootConfig),
            ConfigTypes.MATCH => typeof(MatchConfig),
            ConfigTypes.PLAYERSCAV => typeof(PlayerScavConfig),
            ConfigTypes.PMC_CHAT_RESPONSE => typeof(PmcChatResponse),
            ConfigTypes.QUEST => typeof(QuestConfig),
            ConfigTypes.RAGFAIR => typeof(RagfairConfig),
            ConfigTypes.REPAIR => typeof(RepairConfig),
            ConfigTypes.SCAVCASE => typeof(ScavCaseConfig),
            ConfigTypes.TRADER => typeof(TraderConfig),
            ConfigTypes.WEATHER => typeof(WeatherConfig),
            ConfigTypes.SEASONAL_EVENT => typeof(SeasonalEventConfig),
            ConfigTypes.LOST_ON_DEATH => typeof(LostOnDeathConfig),
            ConfigTypes.GIFTS => typeof(GiftsConfig),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public enum ConfigTypes
{
    AIRDROP,
    BACKUP,
    BOT,
    PMC,
    CORE,
    HEALTH,
    HIDEOUT,
    HTTP,
    IN_RAID,
    INSURANCE,
    INVENTORY,
    ITEM,
    LOCALE,
    LOCATION,
    LOOT,
    MATCH,
    PLAYERSCAV,
    PMC_CHAT_RESPONSE,
    QUEST,
    RAGFAIR,
    REPAIR,
    SCAVCASE,
    TRADER,
    WEATHER,
    SEASONAL_EVENT,
    LOST_ON_DEATH,
    GIFTS
}