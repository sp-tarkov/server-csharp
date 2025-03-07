using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Game;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record CoreConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-core";

    [JsonPropertyName("sptVersion")]
    public string SptVersion
    {
        get;
        set;
    }

    [JsonPropertyName("projectName")]
    public string ProjectName
    {
        get;
        set;
    }

    [JsonPropertyName("compatibleTarkovVersion")]
    public string CompatibleTarkovVersion
    {
        get;
        set;
    }

    [JsonPropertyName("serverName")]
    public string ServerName
    {
        get;
        set;
    }

    [JsonPropertyName("profileSaveIntervalSeconds")]
    public int ProfileSaveIntervalInSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("sptFriendNickname")]
    public string SptFriendNickname
    {
        get;
        set;
    }

    [JsonPropertyName("allowProfileWipe")]
    public bool AllowProfileWipe
    {
        get;
        set;
    }

    [JsonPropertyName("bsgLogging")]
    public BsgLogging BsgLogging
    {
        get;
        set;
    }

    [JsonPropertyName("release")]
    public Release Release
    {
        get;
        set;
    }

    [JsonPropertyName("fixes")]
    public GameFixes Fixes
    {
        get;
        set;
    }

    [JsonPropertyName("survey")]
    public SurveyResponseData Survey
    {
        get;
        set;
    }

    [JsonPropertyName("features")]
    public ServerFeatures Features
    {
        get;
        set;
    }

    /// <summary>
    /// Commit hash build server was created from
    /// </summary>
    [JsonPropertyName("commit")]
    public string? Commit
    {
        get;
        set;
    }

    /// <summary>
    /// Timestamp of server build
    /// </summary>
    [JsonPropertyName("buildTime")]
    public string? BuildTime
    {
        get;
        set;
    }

    /// <summary>
    /// Server locale keys that will be added to the bottom of the startup watermark
    /// </summary>
    [JsonPropertyName("customWatermarkLocaleKeys")]
    public List<string>? CustomWatermarkLocaleKeys
    {
        get;
        set;
    }
}

public record BsgLogging
{
    /// <summary>
    /// verbosity of what to log, yes I know this is backwards, but its how nlog deals with ordinals. <br/>
    /// complain to them about it! In all cases, better exceptions will be logged.<br/>
    /// WARNING: trace-info logging will quickly create log files in the megabytes.<br/>
    /// 0 - trace<br/>
    /// 1 - debug<br/>
    /// 2 - info<br/>
    /// 3 - warn<br/>
    /// 4 - error<br/>
    /// 5 - fatal<br/>
    /// 6 - off
    /// </summary>
    [JsonPropertyName("verbosity")]
    public int Verbosity
    {
        get;
        set;
    }

    /// <summary>
    /// Should we send the logging to the server
    /// </summary>
    [JsonPropertyName("sendToServer")]
    public bool SendToServer
    {
        get;
        set;
    }
}

public record Release
{
    /// <summary>
    /// Disclaimer outlining the intended usage of bleeding edge
    /// </summary>
    [JsonPropertyName("betaDisclaimerText")]
    public string? BetaDisclaimerText
    {
        get;
        set;
    }

    /// <summary>
    /// Text logged when users agreed to terms
    /// </summary>
    [JsonPropertyName("betaDisclaimerAcceptText")]
    public string BetaDisclaimerAcceptText
    {
        get;
        set;
    }

    /// <summary>
    /// Server mods loaded message
    /// </summary>
    [JsonPropertyName("serverModsLoadedText")]
    public string ServerModsLoadedText
    {
        get;
        set;
    }

    /// <summary>
    /// Server mods loaded debug message text
    /// </summary>
    [JsonPropertyName("serverModsLoadedDebugText")]
    public string ServerModsLoadedDebugText
    {
        get;
        set;
    }

    /// <summary>
    /// Client mods loaded message
    /// </summary>
    [JsonPropertyName("clientModsLoadedText")]
    public string ClientModsLoadedText
    {
        get;
        set;
    }

    /// <summary>
    /// Client mods loaded debug message text
    /// </summary>
    [JsonPropertyName("clientModsLoadedDebugText")]
    public string ClientModsLoadedDebugText
    {
        get;
        set;
    }
    /// <summary>
    /// Illegal plugins log message
    /// </summary>
    [JsonPropertyName("illegalPluginsLoadedText")]
    public string IllegalPluginsLoadedText
    {
        get;
        set;
    }

    /// <summary>
    /// Illegal plugins exception
    /// </summary>
    [JsonPropertyName("illegalPluginsExceptionText")]
    public string IllegalPluginsExceptionText
    {
        get;
        set;
    }

    /// <summary>
    /// Summary of release changes
    /// </summary>
    [JsonPropertyName("releaseSummaryText")]
    public string? ReleaseSummaryText
    {
        get;
        set;
    }

    /// <summary>
    /// Enables the cool watermark in-game
    /// </summary>
    [JsonPropertyName("isBeta")]
    public bool? IsBeta
    {
        get;
        set;
    }

    /// <summary>
    /// Whether mods are enabled
    /// </summary>
    [JsonPropertyName("isModdable")]
    public bool? IsModdable
    {
        get;
        set;
    }

    /// <summary>
    /// Are mods loaded on the server?
    /// </summary>
    [JsonPropertyName("isModded")]
    public bool IsModded
    {
        get;
        set;
    }

    /// <summary>
    /// How long before the messagebox times out and closes the game
    /// </summary>
    [JsonPropertyName("betaDisclaimerTimeoutDelay")]
    public int BetaDisclaimerTimeoutDelay
    {
        get;
        set;
    }
}

public record GameFixes
{
    /// <summary>
    /// Shotguns use a different value than normal guns causing huge pellet dispersion
    /// </summary>
    [JsonPropertyName("fixShotgunDispersion")]
    public bool FixShotgunDispersion
    {
        get;
        set;
    }

    /// <summary>
    /// Remove items added by mods when the mod no longer exists - can fix dead profiles stuck at game load
    /// </summary>
    [JsonPropertyName("removeModItemsFromProfile")]
    public bool RemoveModItemsFromProfile
    {
        get;
        set;
    }

    /// <summary>
    /// Remove invalid traders from profile - trader data can be leftover when player removes trader mod
    /// </summary>
    [JsonPropertyName("removeInvalidTradersFromProfile")]
    public bool RemoveInvalidTradersFromProfile
    {
        get;
        set;
    }

    /// <summary>
    /// Fix issues that cause the game to not start due to inventory item issues
    /// </summary>
    [JsonPropertyName("fixProfileBreakingInventoryItemIssues")]
    public bool FixProfileBreakingInventoryItemIssues
    {
        get;
        set;
    }
}

public record ServerFeatures
{
    /// <summary>
    /// Controls whether the server attempts to download mod dependencies not included in the server's executable
    /// </summary>
    [JsonPropertyName("autoInstallModDependencies")]
    public bool AutoInstallModDependencies
    {
        get;
        set;
    }

    [JsonPropertyName("compressProfile")]
    public bool CompressProfile
    {
        get;
        set;
    }

    [JsonPropertyName("chatbotFeatures")]
    public ChatbotFeatures ChatbotFeatures
    {
        get;
        set;
    }

    /// <summary>
    /// Keyed to profile type e.g. "Standard" or "SPT Developer"
    /// </summary>
    [JsonPropertyName("createNewProfileTypesBlacklist")]
    public HashSet<string> CreateNewProfileTypesBlacklist
    {
        get;
        set;
    }

    /// <summary>
    /// Profile ids to ignore when calculating achievement stats
    /// </summary>
    [JsonPropertyName("achievementProfileIdBlacklist")]
    public HashSet<string>? AchievementProfileIdBlacklist
    {
        get;
        set;
    }
}

public record ChatbotFeatures
{
    [JsonPropertyName("sptFriendGiftsEnabled")]
    public bool SptFriendGiftsEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("commandoFeatures")]
    public CommandoFeatures CommandoFeatures
    {
        get;
        set;
    }

    [JsonPropertyName("commandUseLimits")]
    public Dictionary<string, int?> CommandUseLimits
    {
        get;
        set;
    }

    /// <summary>
    /// Human readable id to guid for each bot
    /// </summary>
    [JsonPropertyName("ids")]
    public Dictionary<string, string> Ids
    {
        get;
        set;
    }

    /// <summary>
    /// Bot Ids player is allowed to interact with
    /// </summary>
    [JsonPropertyName("enabledBots")]
    public Dictionary<string, bool> EnabledBots
    {
        get;
        set;
    }
}

public record CommandoFeatures
{
    [JsonPropertyName("giveCommandEnabled")]
    public bool GiveCommandEnabled
    {
        get;
        set;
    }
}
