using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Common;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;


namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class GameController(
    ISptLogger<GameController> _logger,
    ConfigServer _configServer,
    DatabaseService _databaseService,
    TimeUtil _timeUtil,
    HttpServerHelper _httpServerHelper,
    InventoryHelper _inventoryHelper,
    RandomUtil _randomUtil,
    HideoutHelper _hideoutHelper,
    ProfileHelper _profileHelper,
    ProfileFixerService _profileFixerService,
    LocalisationService _localisationService,
    PostDbLoadService _postDbLoadService,
    CustomLocationWaveService _customLocationWaveService,
    OpenZoneService _openZoneService,
    SeasonalEventService _seasonalEventService,
    ItemBaseClassService _itemBaseClassService,
    GiftService _giftService,
    RaidTimeAdjustmentService _raidTimeAdjustmentService,
    ProfileActivityService _profileActivityService,
    CreateProfileService _createProfileService,
    ApplicationContext _applicationContext,
    ICloner _cloner
)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    protected double _deviation = 0.0001;
    protected HideoutConfig _hideoutConfig = _configServer.GetConfig<HideoutConfig>();
    protected HttpConfig _httpConfig = _configServer.GetConfig<HttpConfig>();
    protected RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();

    /// <summary>
    ///     Handle client/game/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="startTimeStampMs"></param>
    public void GameStart(string url, string? sessionId, long startTimeStampMs)
    {
        // Store client start time in app context
        _applicationContext.AddValue(ContextVariableType.CLIENT_START_TIMESTAMP, $"{sessionId}_{startTimeStampMs}");

        if (sessionId is null)
        {
            _logger.Error($"{nameof(sessionId)} is null on GameController.GameStart");
            return;
        }

        // repeatableQuests are stored by in profile.Quests due to the responses of the client (e.g. Quests in
        // offraidData). Since we don't want to clutter the Quests list, we need to remove all completed (failed or
        // successful) repeatable quests. We also have to remove the Counters from the repeatableQuests

        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        if (fullProfile is null)
        {
            _logger.Error($"{nameof(fullProfile)} is null on GameController.GameStart");
            return;
        }

        fullProfile.SptData ??= new Spt
        {
            //TODO: complete
            Version = "Replace_me"
        };
        fullProfile.SptData.Migrations ??= new Dictionary<string, long>();
        fullProfile.FriendProfileIds ??= [];

        if (fullProfile.ProfileInfo?.IsWiped is not null && fullProfile.ProfileInfo.IsWiped.Value)
        {
            return;
        }

        fullProfile.CharacterData!.PmcData!.WishList ??= new DictionaryOrList<MongoId, int>(new Dictionary<MongoId, int>(), []);
        fullProfile.CharacterData.ScavData!.WishList ??= new DictionaryOrList<MongoId, int>(new Dictionary<MongoId, int>(), []);

        if (fullProfile.DialogueRecords is not null)
        {
            _profileFixerService.CheckForAndFixDialogueAttachments(fullProfile);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Started game with session {sessionId} {fullProfile.ProfileInfo?.Username}");
        }

        var pmcProfile = fullProfile.CharacterData.PmcData;

        if (_coreConfig.Fixes.FixProfileBreakingInventoryItemIssues)
        {
            _profileFixerService.FixProfileBreakingInventoryItemIssues(pmcProfile);
        }

        if (pmcProfile.Health is not null)
        {
            UpdateProfileHealthValues(pmcProfile);
        }

        if (pmcProfile.Inventory is not null)
        {
            SendPraporGiftsToNewProfiles(pmcProfile);
            SendMechanicGiftsToNewProfile(pmcProfile);
            _profileFixerService.CheckForOrphanedModdedItems(sessionId, fullProfile);
        }

        _profileFixerService.CheckForAndRemoveInvalidTraders(fullProfile);
        _profileFixerService.CheckForAndFixPmcProfileIssues(pmcProfile);

        if (pmcProfile.Hideout is not null)
        {
            _profileFixerService.AddMissingHideoutBonusesToProfile(pmcProfile);
            _hideoutHelper.SetHideoutImprovementsToCompleted(pmcProfile);
            _hideoutHelper.UnlockHideoutWallInProfile(pmcProfile);

            // Handle if player has been inactive for a long time, catch up on hideout update before the user goes to his hideout
            if (!_profileActivityService.ActiveWithinLastMinutes(sessionId, _hideoutConfig.UpdateProfileHideoutWhenActiveWithinMinutes))
            {
                _hideoutHelper.UpdatePlayerHideout(sessionId);
            }
        }

        LogProfileDetails(fullProfile);
        SaveActiveModsToProfile(fullProfile);

        if (pmcProfile.Info is not null)
        {
            AddPlayerToPmcNames(pmcProfile);
            CheckForAndRemoveUndefinedDialogues(fullProfile);
        }

        if (pmcProfile.Skills?.Common is not null)
        {
            WarnOnActiveBotReloadSkill(pmcProfile);
        }

        _seasonalEventService.GivePlayerSeasonalGifts(sessionId);

        // Set activity timestamp at the end of the method, so that code that checks for an older timestamp (Updating hideout) can still run
        _profileActivityService.SetActivityTimestamp(sessionId);
    }

    /// <summary>
    ///     Handle client/game/config
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>GameConfigResponse</returns>
    public GameConfigResponse GetGameConfig(string sessionId)
    {
        var profile = _profileHelper.GetPmcProfile(sessionId);
        var gameTime = profile?.Stats?.Eft?.OverallCounters?.Items?
                           .FirstOrDefault(c => c.Key!.Contains("LifeTime") && c.Key.Contains("Pmc"))
                           ?.Value ??
                       0D;

        var config = new GameConfigResponse
        {
            Languages = _databaseService.GetLocales().Languages,
            IsNdaFree = false,
            IsReportAvailable = false,
            IsTwitchEventMember = false,
            Language = "en",
            Aid = profile?.Aid,
            Taxonomy = 6,
            ActiveProfileId = sessionId,
            Backend = new Backend
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
            PurchasedGames = new PurchasedGames
            {
                IsEftPurchased = true,
                IsArenaPurchased = false
            },
            IsGameSynced = true
        };

        return config;
    }

    /// <summary>
    ///     Handle client/game/mode
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="requestData"></param>
    /// <returns></returns>
    public GameModeResponse GetGameMode(
        string sessionId,
        GameModeRequestData requestData)
    {
        return new GameModeResponse
        {
            GameMode = "pve",
            BackendUrl = _httpServerHelper.GetBackendUrl()
        };
    }

    /// <summary>
    ///     Handle client/server/list
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public List<ServerDetails> GetServer(string sessionId)
    {
        return
        [
            new ServerDetails
            {
                Ip = _httpConfig.BackendIp,
                Port = _httpConfig.BackendPort
            }
        ];
    }

    /// <summary>
    ///     Handle client/match/group/current
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public CurrentGroupResponse GetCurrentGroup(string sessionId)
    {
        return new CurrentGroupResponse
        {
            Squad = []
        };
    }


    /// <summary>
    ///     Handle client/checkVersion
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public CheckVersionResponse GetValidGameVersion(string sessionId)
    {
        return new CheckVersionResponse
        {
            IsValid = true,
            LatestVersion = _coreConfig.CompatibleTarkovVersion
        };
    }

    /// <summary>
    ///     Handle client/game/keepalive
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public GameKeepAliveResponse GetKeepAlive(string sessionId)
    {
        _profileActivityService.SetActivityTimestamp(sessionId);
        return new GameKeepAliveResponse
        {
            Message = "OK",
            UtcTime = _timeUtil.GetTimeStamp()
        };
    }

    /// <summary>
    ///     Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetRaidTimeResponse GetRaidTime(string sessionId, GetRaidTimeRequest request)
    {
        return _raidTimeAdjustmentService.GetRaidAdjustments(sessionId, request);
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public SurveyResponseData GetSurvey(string sessionId)
    {
        return _coreConfig.Survey;
    }

    /// <summary>
    ///     Players set botReload to a high value and don't expect the crazy fast reload speeds, give them a warn about it
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    protected void WarnOnActiveBotReloadSkill(PmcData pmcProfile)
    {
        var botReloadSkill = _profileHelper.GetSkillFromProfile(pmcProfile, SkillTypes.BotReload);
        if (botReloadSkill?.Progress > 0)
        {
            _logger.Warning(_localisationService.GetText("server_start_player_active_botreload_skill"));
        }
    }

    /// <summary>
    ///     When player logs in, iterate over all active effects and reduce timer
    /// </summary>
    /// <param name="pmcProfile">Profile to adjust values for</param>
    protected void UpdateProfileHealthValues(PmcData pmcProfile)
    {
        var healthLastUpdated = pmcProfile.Health?.UpdateTime;
        var currentTimeStamp = _timeUtil.GetTimeStamp();
        var diffSeconds = currentTimeStamp - healthLastUpdated;

        // Update just occurred
        if (healthLastUpdated >= currentTimeStamp)
        {
            return;
        }

        // Base values
        double energyRegenPerHour = 60;
        double hydrationRegenPerHour = 60;
        var hpRegenPerHour = 456.6;

        // Set new values, whatever is smallest
        energyRegenPerHour += pmcProfile.Bonuses!
            .Where(bonus => bonus.Type == BonusType.EnergyRegeneration)
            .Aggregate(0d, (sum, bonus) => sum + bonus.Value!.Value);

        hydrationRegenPerHour += pmcProfile.Bonuses!
            .Where(bonus => bonus.Type == BonusType.HydrationRegeneration)
            .Aggregate(0d, (sum, bonus) => sum + bonus.Value!.Value);

        hpRegenPerHour += pmcProfile.Bonuses!
            .Where(bonus => bonus.Type == BonusType.HealthRegeneration)
            .Aggregate(0d, (sum, bonus) => sum + bonus.Value!.Value);

        // Player has energy deficit
        if (Math.Abs(pmcProfile.Health?.Energy?.Current - pmcProfile.Health?.Energy?.Maximum ?? 1) <= _deviation)
        {
            // Set new value, whatever is smallest
            pmcProfile.Health!.Energy!.Current += Math.Round(energyRegenPerHour * (diffSeconds!.Value / 3600));
            if (pmcProfile.Health.Energy.Current > pmcProfile.Health.Energy.Maximum)
            {
                pmcProfile.Health.Energy.Current = pmcProfile.Health.Energy.Maximum;
            }
        }

        // Player has hydration deficit
        if (Math.Abs(pmcProfile.Health?.Hydration?.Current - pmcProfile.Health?.Hydration?.Maximum ?? 1) <= _deviation)
        {
            pmcProfile.Health!.Hydration!.Current += Math.Round(hydrationRegenPerHour * (diffSeconds!.Value / 3600));
            if (pmcProfile.Health.Hydration.Current > pmcProfile.Health.Hydration.Maximum)
            {
                pmcProfile.Health.Hydration.Current = pmcProfile.Health.Hydration.Maximum;
            }
        }

        // Check all body parts
        DecreaseBodyPartEffectTimes(pmcProfile, hpRegenPerHour, diffSeconds.Value);

        // Update both values as they've both been updated
        pmcProfile.Health.UpdateTime = currentTimeStamp;
    }

    /// <summary>
    ///     Check for and update any timers on effect found on body parts
    /// </summary>
    /// <param name="pmcProfile">Player</param>
    /// <param name="hpRegenPerHour"></param>
    /// <param name="diffSeconds"></param>
    protected void DecreaseBodyPartEffectTimes(PmcData pmcProfile, double hpRegenPerHour, double diffSeconds)
    {
        foreach (var bodyPart in pmcProfile.Health!.BodyParts!
                     .Select(bodyPartKvP => bodyPartKvP.Value))
        {
            // Check part hp
            if (bodyPart.Health!.Current < bodyPart.Health.Maximum)
            {
                bodyPart.Health.Current += Math.Round(hpRegenPerHour * (diffSeconds / 3600));
            }

            if (bodyPart.Health.Current > bodyPart.Health.Maximum)
            {
                bodyPart.Health.Current = bodyPart.Health.Maximum;
            }


            if (bodyPart.Effects is null || bodyPart.Effects.Count == 0)
            {
                continue;
            }

            // Look for effects
            foreach (var effectKvP in bodyPart.Effects)
            {
                // remove effects below 1, .e.g. bleeds at -1
                if (effectKvP.Value.Time < 1)
                {
                    // More than 30 minutes has passed
                    if (diffSeconds > 1800)
                    {
                        bodyPart.Effects.Remove(effectKvP.Key);
                    }

                    continue;
                }

                // Decrement effect time value by difference between current time and time health was last updated
                effectKvP.Value.Time -= diffSeconds;
                if (effectKvP.Value.Time < 1)
                    // Effect time was sub 1, set floor it can be
                {
                    effectKvP.Value.Time = 1;
                }
            }
        }
    }

    /// <summary>
    ///     Send starting gifts to profile after x days
    /// </summary>
    /// <param name="pmcProfile">Profile to add gifts to</param>
    protected void SendPraporGiftsToNewProfiles(PmcData pmcProfile)
    {
        var timeStampProfileCreated = pmcProfile.Info?.RegistrationDate;
        var oneDaySeconds = _timeUtil.GetHoursAsSeconds(24);
        var currentTimeStamp = _timeUtil.GetTimeStamp();

        // One day post-profile creation
        if (currentTimeStamp > timeStampProfileCreated + oneDaySeconds)
        {
            _giftService.SendPraporStartingGift(pmcProfile.SessionId!, 1);
        }

        // Two day post-profile creation
        if (currentTimeStamp > timeStampProfileCreated + oneDaySeconds * 2)
        {
            _giftService.SendPraporStartingGift(pmcProfile.SessionId!, 2);
        }
    }

    /// <summary>
    /// Mechanic sends players a measuring tape on profile start for some reason
    /// </summary>
    /// <param name="pmcProfile"></param>
    protected void SendMechanicGiftsToNewProfile(PmcData pmcProfile)
    {
        _giftService.SendGiftWithSilentReceivedCheck("MechanicGiftDay1", pmcProfile.SessionId, 1);
    }

    /// <summary>
    ///     Get a list of installed mods and save their details to the profile being used
    /// </summary>
    /// <param name="fullProfile">Profile to add mod details to</param>
    protected void SaveActiveModsToProfile(SptProfile fullProfile)
    {
        fullProfile.SptData!.Mods ??= [];
        var mods = _applicationContext?.GetLatestValue(ContextVariableType.LOADED_MOD_ASSEMBLIES).GetValue<List<SptMod>>();

        foreach (var mod in mods)
        {
            if (
                fullProfile.SptData.Mods.Any(
                    m =>
                        m.Author == mod.PackageJson.Author && m.Version == mod.PackageJson.Version && m.Name == mod.PackageJson.Name
                )
            )
            {
                // exists already, skip
                continue;
            }

            fullProfile.SptData.Mods.Add(
                new ModDetails
                {
                    Author = mod.PackageJson.Author,
                    Version = mod.PackageJson.Version,
                    Name = mod.PackageJson.Name,
                    Url = mod.PackageJson.Url,
                    DateAdded = _timeUtil.GetTimeStamp()
                }
            );
        }
    }

    /// <summary>
    ///     Add the logged in players name to PMC name pool
    /// </summary>
    /// <param name="pmcProfile">Profile of player to get name from</param>
    protected void AddPlayerToPmcNames(PmcData pmcProfile)
    {
        var playerName = pmcProfile.Info?.Nickname;
        if (playerName is not null)
        {
            var bots = _databaseService.GetBots().Types;

            // Official names can only be 15 chars in length
            if (playerName.Length > _botConfig.BotNameLengthLimit)
            {
                return;
            }

            // Skip if player name exists already
            if (bots!.TryGetValue("bear", out var bearBot))
            {
                if (bearBot is not null && bearBot.FirstNames!.Any(x => x == playerName))
                {
                    bearBot.FirstNames!.Add(playerName);
                }
            }

            if (bots.TryGetValue("bear", out var usecBot))
            {
                if (usecBot is not null && usecBot.FirstNames!.Any(x => x == playerName))
                {
                    usecBot.FirstNames!.Add(playerName);
                }
            }
        }
    }

    /// <summary>
    ///     Check for a dialog with the key 'undefined', and remove it
    /// </summary>
    /// <param name="fullProfile">Profile to check for dialog in</param>
    protected void CheckForAndRemoveUndefinedDialogues(SptProfile fullProfile)
    {
        if (fullProfile.DialogueRecords!.TryGetValue("undefined", out _))
        {
            fullProfile.DialogueRecords.Remove("undefined");
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="fullProfile"></param>
    protected void LogProfileDetails(SptProfile fullProfile)
    {
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Profile made with: {fullProfile.SptData?.Version}");
            _logger.Debug($"Server version: {ProgramStatics.SPT_VERSION() ?? _coreConfig.SptVersion} {ProgramStatics.COMMIT()}");
            _logger.Debug($"Debug enabled: {ProgramStatics.DEBUG()}");
            _logger.Debug($"Mods enabled: {ProgramStatics.MODS()}");
        }
    }

    public void Load()
    {
        _postDbLoadService.PerformPostDbLoadActions();
    }
}
