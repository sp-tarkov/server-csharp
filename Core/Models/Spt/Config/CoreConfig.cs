using System.Text.Json.Serialization;
using Core.Models.Eft.Game;

namespace Core.Models.Spt.Config;

public class CoreConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-core";

    [JsonPropertyName("sptVersion")]
    public string SptVersion { get; set; }

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }

    [JsonPropertyName("compatibleTarkovVersion")]
    public string CompatibleTarkovVersion { get; set; }

    [JsonPropertyName("serverName")]
    public string ServerName { get; set; }

    [JsonPropertyName("profileSaveIntervalSeconds")]
    public int ProfileSaveIntervalInSeconds { get; set; }

    [JsonPropertyName("sptFriendNickname")]
    public string SptFriendNickname { get; set; }

    [JsonPropertyName("allowProfileWipe")]
    public bool AllowProfileWipe { get; set; }

    [JsonPropertyName("bsgLogging")]
    public BsgLogging BsgLogging { get; set; }

    [JsonPropertyName("release")]
    public Release Release { get; set; }

    [JsonPropertyName("fixes")]
    public GameFixes Fixes { get; set; }

    [JsonPropertyName("survey")]
    public SurveyResponseData Survey { get; set; }

    [JsonPropertyName("features")]
    public ServerFeatures Features { get; set; }

    /** Commit hash build server was created from */
    [JsonPropertyName("commit")]
    public string? Commit { get; set; }

    /** Timestamp of server build */
    [JsonPropertyName("buildTime")]
    public string? BuildTime { get; set; }

    /** Server locale keys that will be added to the bottom of the startup watermark */
    [JsonPropertyName("customWatermarkLocaleKeys")]
    public List<string>? CustomWatermarkLocaleKeys { get; set; }
}

public class BsgLogging
{
    /**
     * verbosity of what to log, yes I know this is backwards, but its how nlog deals with ordinals.
     * complain to them about it! In all cases, better exceptions will be logged.
     * WARNING: trace-info logging will quickly create log files in the megabytes.
     * 0 - trace
     * 1 - debug
     * 2 - info
     * 3 - warn
     * 4 - error
     * 5 - fatal
     * 6 - off
     */
    [JsonPropertyName("verbosity")]
    public int Verbosity { get; set; }

    // Should we send the logging to the server
    [JsonPropertyName("sendToServer")]
    public bool SendToServer { get; set; }
}

public class Release
{
    // Disclaimer outlining the intended usage of bleeding edge
    [JsonPropertyName("betaDisclaimerText")]
    public string? BetaDisclaimerText { get; set; }

    // Text logged when users agreed to terms
    [JsonPropertyName("betaDisclaimerAcceptText")]
    public string BetaDisclaimerAcceptText { get; set; }

    // Server mods loaded message
    [JsonPropertyName("serverModsLoadedText")]
    public string ServerModsLoadedText { get; set; }

    // Server mods loaded debug message text
    [JsonPropertyName("serverModsLoadedDebugText")]
    public string ServerModsLoadedDebugText { get; set; }

    // Client mods loaded message
    [JsonPropertyName("clientModsLoadedText")]
    public string ClientModsLoadedText { get; set; }

    // Client mods loaded debug message text
    [JsonPropertyName("clientModsLoadedDebugText")]
    public string ClientModsLoadedDebugText { get; set; }

    // Illegal plugins log message
    [JsonPropertyName("illegalPluginsLoadedText")]
    public string IllegalPluginsLoadedText { get; set; }

    // Illegal plugins exception
    [JsonPropertyName("illegalPluginsExceptionText")]
    public string IllegalPluginsExceptionText { get; set; }

    // Summary of release changes
    [JsonPropertyName("releaseSummaryText")]
    public string? ReleaseSummaryText { get; set; }

    // Enables the cool watermark in-game
    [JsonPropertyName("isBeta")]
    public bool? IsBeta { get; set; }

    // Whether mods are enabled
    [JsonPropertyName("isModdable")]
    public bool? IsModdable { get; set; }

    // Are mods loaded on the server?
    [JsonPropertyName("isModded")]
    public bool IsModded { get; set; }

    // How long before the messagebox times out and closes the game
    [JsonPropertyName("betaDisclaimerTimeoutDelay")]
    public int BetaDisclaimerTimeoutDelay { get; set; }
}

public class GameFixes
{
    /** Shotguns use a different value than normal guns causing huge pellet dispersion  */
    [JsonPropertyName("fixShotgunDispersion")]
    public bool FixShotgunDispersion { get; set; }

    /** Remove items added by mods when the mod no longer exists - can fix dead profiles stuck at game load */
    [JsonPropertyName("removeModItemsFromProfile")]
    public bool RemoveModItemsFromProfile { get; set; }

    /** Remove invalid traders from profile - trader data can be leftover when player removes trader mod */
    [JsonPropertyName("removeInvalidTradersFromProfile")]
    public bool RemoveInvalidTradersFromProfile { get; set; }

    /** Fix issues that cause the game to not start due to inventory item issues */
    [JsonPropertyName("fixProfileBreakingInventoryItemIssues")]
    public bool FixProfileBreakingInventoryItemIssues { get; set; }
}

public class ServerFeatures
{
    /* Controls whether or not the server attempts to download mod dependencies not included in the server's executable */
    [JsonPropertyName("autoInstallModDependencies")]
    public bool AutoInstallModDependencies { get; set; }

    [JsonPropertyName("compressProfile")]
    public bool CompressProfile { get; set; }

    [JsonPropertyName("chatbotFeatures")]
    public ChatbotFeatures ChatbotFeatures { get; set; }

    /** Keyed to profile type e.g. "Standard" or "SPT Developer" */
    [JsonPropertyName("createNewProfileTypesBlacklist")]
    public List<string> CreateNewProfileTypesBlacklist { get; set; }
}

public class ChatbotFeatures
{
    [JsonPropertyName("sptFriendEnabled")]
    public bool SptFriendEnabled { get; set; }

    [JsonPropertyName("sptFriendGiftsEnabled")]
    public bool SptFriendGiftsEnabled { get; set; }

    [JsonPropertyName("commandoEnabled")]
    public bool CommandoEnabled { get; set; }

    [JsonPropertyName("commandoFeatures")]
    public CommandoFeatures CommandoFeatures { get; set; }

    [JsonPropertyName("commandUseLimits")]
    public Dictionary<string, int> CommandUseLimits { get; set; }

    [JsonPropertyName("ids")]
    public Dictionary<string, string> Ids { get; set; }
}

public class CommandoFeatures
{
    [JsonPropertyName("giveCommandEnabled")]
    public bool GiveCommandEnabled { get; set; }
}