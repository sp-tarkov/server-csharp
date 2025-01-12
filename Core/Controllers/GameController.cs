using Core.Annotations;
using Core.Context;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Game;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class GameController
{
    private readonly ILogger _logger;
    private readonly ConfigServer _configServer;
    private readonly DatabaseService _databaseService;

    private readonly TimeUtil _timeUtil;

    // private readonly PreSptModLoader _preSptModLoader;
    private readonly HttpServerHelper _httpServerHelper;
    private readonly InventoryHelper _inventoryHelper;
    private readonly RandomUtil _randomUtil;
    private readonly HideoutHelper _hideoutHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly ProfileFixerService _profileFixerService;
    private readonly LocalisationService _localisationService;
    private readonly PostDbLoadService _postDbLoadService;
    private readonly CustomLocationWaveService _customLocationWaveService;
    private readonly OpenZoneService _openZoneService;
    private readonly SeasonalEventService _seasonalEventService;
    private readonly ItemBaseClassService _itemBaseClassService;
    private readonly GiftService _giftService;
    private readonly RaidTimeAdjustmentService _raidTimeAdjustmentService;
    private readonly ProfileActivityService _profileActivityService;
    private readonly ApplicationContext _applicationContext;
    private readonly ICloner _cloner;

    private readonly CoreConfig _coreConfig;
    private readonly HttpConfig _httpConfig;
    private readonly RagfairConfig _ragfairConfig;
    private readonly HideoutConfig _hideoutConfig;
    private readonly BotConfig _botConfig;

    public GameController(
        ILogger logger,
        ConfigServer configServer,
        DatabaseService databaseService,
        TimeUtil timeUtil,
        HttpServerHelper httpServerHelper,
        InventoryHelper inventoryHelper,
        RandomUtil randomUtil,
        HideoutHelper hideoutHelper,
        ProfileHelper profileHelper,
        ProfileFixerService profileFixerService,
        LocalisationService localisationService,
        PostDbLoadService postDbLoadService,
        CustomLocationWaveService customLocationWaveService,
        OpenZoneService openZoneService,
        SeasonalEventService seasonalEventService,
        ItemBaseClassService itemBaseClassService,
        GiftService giftService,
        RaidTimeAdjustmentService raidTimeAdjustmentService,
        ProfileActivityService profileActivityService,
        ApplicationContext applicationContext,
        ICloner cloner
    )
    {
        _logger = logger;
        _configServer = configServer;
        _databaseService = databaseService;
        _timeUtil = timeUtil;
        _httpServerHelper = httpServerHelper;
        _inventoryHelper = inventoryHelper;
        _randomUtil = randomUtil;
        _hideoutHelper = hideoutHelper;
        _profileHelper = profileHelper;
        _profileFixerService = profileFixerService;
        _localisationService = localisationService;
        _postDbLoadService = postDbLoadService;
        _customLocationWaveService = customLocationWaveService;
        _openZoneService = openZoneService;
        _seasonalEventService = seasonalEventService;
        _itemBaseClassService = itemBaseClassService;
        _giftService = giftService;
        _raidTimeAdjustmentService = raidTimeAdjustmentService;
        _profileActivityService = profileActivityService;
        _applicationContext = applicationContext;
        _cloner = cloner;

        _coreConfig = configServer.GetConfig<CoreConfig>(ConfigTypes.CORE);
        _httpConfig = configServer.GetConfig<HttpConfig>(ConfigTypes.HTTP);
        _ragfairConfig = configServer.GetConfig<RagfairConfig>(ConfigTypes.RAGFAIR);
        _hideoutConfig = configServer.GetConfig<HideoutConfig>(ConfigTypes.HIDEOUT);
        _botConfig = configServer.GetConfig<BotConfig>(ConfigTypes.BOT);
    }

    /// <summary>
    /// Handle client/game/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionId"></param>
    /// <param name="startTimeStampMs"></param>
    public void GameStart(string url, EmptyRequestData info, string sessionId, long startTimeStampMs)
    {
        // Store client start time in app context
        _applicationContext.AddValue(ContextVariableType.CLIENT_START_TIMESTAMP, $"{sessionId}_{startTimeStampMs}");

        _profileActivityService.SetActivityTimestamp(sessionId);

        // repeatableQuests are stored by in profile.Quests due to the responses of the client (e.g. Quests in
        // offraidData). Since we don't want to clutter the Quests list, we need to remove all completed (failed or
        // successful) repeatable quests. We also have to remove the Counters from the repeatableQuests
        if (sessionId != null)
        {
            var fullProfile = _profileHelper.GetFullProfile(sessionId);
            if (fullProfile.ProfileInfo.IsWiped.Value)
                return;

            if (fullProfile.SptData.Migrations == null)
                fullProfile.SptData.Migrations = new();

            if (fullProfile.FriendProfileIds == null)
                fullProfile.FriendProfileIds = new();

            if (fullProfile.SptData.Version.Contains("3.9.") && !fullProfile.SptData.Migrations.Any(m => m.Key == "39x"))
            {
                _inventoryHelper.ValidateInventoryUsesMongoIds(fullProfile.CharacterData.PmcData.Inventory.Items);
                Migrate39xProfile(fullProfile);

                // flag as migrated
                fullProfile.SptData.Migrations.Add("39x", _timeUtil.GetTimeStamp());
                _logger.Success($"Migration of 3.9.x profile: {fullProfile.ProfileInfo.Username} completed successfully");
            }

            // with our method of converting type from array for this prop, we *might* not need this?
            // if (Array.isArray(fullProfile.characters.pmc.WishList)) {
            //     fullProfile.characters.pmc.WishList = {};
            // }
            //
            // if (Array.isArray(fullProfile.characters.scav.WishList)) {
            //     fullProfile.characters.scav.WishList = {};
            // }

            if (fullProfile.DialogueRecords != null)
                _profileFixerService.CheckForAndFixDialogueAttachments(fullProfile);

            _logger.Debug($"Started game with session {sessionId} {fullProfile.ProfileInfo.Username}");

            var pmcProfile = fullProfile.CharacterData.PmcData;

            if (_coreConfig.Fixes.FixProfileBreakingInventoryItemIssues)
                _profileFixerService.FixProfileBreakingInventoryItemIssues(pmcProfile);

            if (pmcProfile.Health != null)
                UpdateProfileHealthValues(pmcProfile);

            if (pmcProfile.Inventory != null)
            {
                SendPraporGiftsToNewProfiles(pmcProfile);
                _profileFixerService.CheckForOrphanedModdedItems(sessionId, fullProfile);
            }

            _profileFixerService.CheckForAndRemoveInvalidTraders(fullProfile);
            _profileFixerService.CheckForAndFixPmcProfileIssues(pmcProfile);

            if (pmcProfile.Hideout != null)
            {
                _profileFixerService.AddMissingHideoutBonusesToProfile(pmcProfile);
                _hideoutHelper.SetHideoutImprovementsToCompleted(pmcProfile);
                _hideoutHelper.UnlockHideoutWallInProfile(pmcProfile);
            }

            LogProfileDetails(fullProfile);
            SaveActiveModsToProfile(fullProfile);

            if (pmcProfile.Info != null)
            {
                AddPlayerToPmcNames(pmcProfile);
                CheckForAndRemoveUndefinedDialogues(fullProfile);
            }

            if (pmcProfile.Skills.Common != null)
                WarnOnActiveBotReloadSkill(pmcProfile);

            _seasonalEventService.GivePlayerSeasonalGifts(sessionId);
        }
    }

    private void Migrate39xProfile(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles migrating profiles from older SPT versions
    /// </summary>
    /// <param name="fullProfile"></param>
    /// <remarks>Formerly migrate39xProfile in node server</remarks>
    private void MigrateProfile(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/config
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GameConfigResponse GetGameConfig(string sessionId)
    {
        var profile = _profileHelper.GetPmcProfile(sessionId);
        var gameTime = profile?.Stats?.Eft?.OverallCounters?.Items.FirstOrDefault(c => 
            c.Key.Contains("LifeTime") && 
            c.Key.Contains("Pmc")).Value ?? 0D;

        var config = new GameConfigResponse
        {
            Languages = _databaseService.GetLocales().Languages,
            IsNdaFree = false,
            IsReportAvailable = false,
            IsTwitchEventMember = false,
            Language = "en",
            Aid = profile.Aid,
            Taxonomy = 6,
            ActiveProfileId = sessionId,
            Backend = new()
            {
                Lobby = _httpServerHelper.GetBackendUrl(),
                Trading = _httpServerHelper.GetBackendUrl(),
                Messaging = _httpServerHelper.GetBackendUrl(),
                Main = _httpServerHelper.GetBackendUrl(),
                RagFair = _httpServerHelper.GetBackendUrl()
            },
            UseProtobuf = false,
            UtcTime = _timeUtil.GetTimeStamp(),
            TotalInGame = gameTime,
            SessionMode = "pve",
            PurchasedGames = new()
            {
                IsEftPurchased = true,
                IsArenaPurchased = false
            }
        };
        
        return config;
    }

    /// <summary>
    /// Handle client/game/mode
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="requestData"></param>
    /// <returns></returns>
    public GameModeResponse GetGameMode(
        string sessionId,
        GameModeRequestData requestData)
    {
        return new()
        {
            GameMode = "pve",
            BackendUrl = "127.0.0.1:6969"
        };
    }

    /// <summary>
    /// Handle client/server/list
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public List<ServerDetails> GetServer(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/match/group/current
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public CurrentGroupResponse GetCurrentGroup(string sessionId)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Handle client/checkVersion
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public CheckVersionResponse GetValidGameVersion(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/keepalive
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GameKeepAliveResponse GetKeepAlive(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetRaidTimeResponse GetRaidTime(
        string sessionId,
        GetRaidTimeRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public SurveyResponseData GetSurvey(string sessionId)
    {
        return this._coreConfig.Survey;
    }

    /// <summary>
    /// Players set botReload to a high value and don't expect the crazy fast reload speeds, give them a warn about it
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    private void WarnOnActiveBotReloadSkill(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// When player logs in, iterate over all active effects and reduce timer
    /// </summary>
    /// <param name="pmcProfile">Profile to adjust values for</param>
    private void UpdateProfileHealthValues(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send starting gifts to profile after x days
    /// </summary>
    /// <param name="pmcProfile">Profile to add gifts to</param>
    private void SendPraporGiftsToNewProfiles(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a list of installed mods and save their details to the profile being used
    /// </summary>
    /// <param name="fullProfile">Profile to add mod details to</param>
    private void SaveActiveModsToProfile(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add the logged in players name to PMC name pool
    /// </summary>
    /// <param name="pmcProfile">Profile of player to get name from</param>
    private void AddPlayerToPmcNames(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check for a dialog with the key 'undefined', and remove it
    /// </summary>
    /// <param name="fullProfile">Profile to check for dialog in</param>
    private void CheckForAndRemoveUndefinedDialogues(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fullProfile"></param>
    private void LogProfileDetails(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    public void Load()
    {
        _postDbLoadService.PerformPostDbLoadActions();
    }
}
