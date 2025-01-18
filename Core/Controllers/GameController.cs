using Core.Annotations;
using Core.Context;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Game;
using Core.Models.Eft.Match;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using System.Diagnostics;
using Server;


namespace Core.Controllers;

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
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    protected HttpConfig _httpConfig = _configServer.GetConfig<HttpConfig>();
    protected RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
    protected HideoutConfig _hideoutConfig = _configServer.GetConfig<HideoutConfig>();
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();

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

            if (fullProfile.SptData.Version.Contains("3.9.") && fullProfile.SptData.Migrations.All(m => m.Key != "39x"))
            {
                _inventoryHelper.ValidateInventoryUsesMongoIds(fullProfile.CharacterData.PmcData.Inventory.Items);
                Migrate39xProfile(fullProfile);

                // flag as migrated
                fullProfile.SptData.Migrations.Add("39x", _timeUtil.GetTimeStamp());
                _logger.Info($"Migration of 3.9.x profile: {fullProfile.ProfileInfo.Username} completed successfully");
            }

            //3.10 migrations
            if (fullProfile.SptData.Version.Contains("3.10.") && fullProfile.SptData.Migrations.All(m => m.Key != "310x"))
            {
                Migrate310xProfile(fullProfile);

                // Flag as migrated
                fullProfile.SptData.Migrations["310x"] = _timeUtil.GetTimeStamp();

                _logger.Success($"Migration of 3.10.x profile: ${fullProfile.ProfileInfo.Username} completed successfully");
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

    private void Migrate310xProfile(SptProfile fullProfile)
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
        var gameTime = profile?.Stats?.Eft?.OverallCounters?.Items?.FirstOrDefault(
                               c =>
                                   c.Key.Contains("LifeTime") &&
                                   c.Key.Contains("Pmc")
                           )
                           ?.Value ??
                       0D;

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
            BackendUrl = _httpServerHelper.GetBackendUrl()
        };
    }

    /// <summary>
    /// Handle client/server/list
    /// </summary>
    /// <param name="sessionId"></param>
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
    /// Handle client/match/group/current
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public CurrentGroupResponse GetCurrentGroup(string sessionId)
    {
        return new CurrentGroupResponse
        {
            Squad = []
        };
    }


    /// <summary>
    /// Handle client/checkVersion
    /// </summary>
    /// <param name="sessionId"></param>
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
    /// Handle client/game/keepalive
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GameKeepAliveResponse GetKeepAlive(string sessionId)
    {
        _profileActivityService.SetActivityTimestamp(sessionId);
        return new GameKeepAliveResponse() { Message = "OK", UtcTime = _timeUtil.GetTimeStamp() };
    }

    /// <summary>
    /// Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetRaidTimeResponse GetRaidTime(string sessionId, GetRaidTimeRequest request)
    {
        // Set interval times to in-raid value
        _ragfairConfig.RunIntervalSeconds = _ragfairConfig.RunIntervalValues.InRaid;

        _hideoutConfig.RunIntervalSeconds = _hideoutConfig.RunIntervalValues.InRaid;

        return _raidTimeAdjustmentService.GetRaidAdjustments(sessionId, request);
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
        var botReloadSkill = _profileHelper.GetSkillFromProfile(pmcProfile, SkillTypes.BotReload);
        if (botReloadSkill?.Progress > 0)
        {
            _logger.Warning(_localisationService.GetText("server_start_player_active_botreload_skill"));
        }
    }

    /// <summary>
    /// When player logs in, iterate over all active effects and reduce timer
    /// </summary>
    /// <param name="pmcProfile">Profile to adjust values for</param>
    private void UpdateProfileHealthValues(PmcData pmcProfile)
    {
        var healthLastUpdated = pmcProfile.Health.UpdateTime;
        var currentTimeStamp = _timeUtil.GetTimeStamp();
        var diffSeconds = currentTimeStamp - healthLastUpdated;

        // Last update is in past
        if (healthLastUpdated < currentTimeStamp)
        {
            // Base values
            double energyRegenPerHour = 60;
            double hydrationRegenPerHour = 60;
            double hpRegenPerHour = 456.6;

            // Set new values, whatever is smallest
            energyRegenPerHour += pmcProfile.Bonuses
                .Where((bonus) => bonus.Type == BonusType.EnergyRegeneration)
                .Aggregate(0d, (sum, bonus) => sum + (bonus.Value.Value));
            hydrationRegenPerHour += pmcProfile.Bonuses
                .Where((bonus) => bonus.Type == BonusType.HydrationRegeneration)
                .Aggregate(0d, (sum, bonus) => sum + (bonus.Value.Value));
            hpRegenPerHour += pmcProfile.Bonuses
                .Where((bonus) => bonus.Type == BonusType.HealthRegeneration)
                .Aggregate(0d, (sum, bonus) => sum + (bonus.Value.Value));

            // Player has energy deficit
            if (pmcProfile.Health.Energy.Current != pmcProfile.Health.Energy.Maximum)
            {
                // Set new value, whatever is smallest
                pmcProfile.Health.Energy.Current += Math.Round(energyRegenPerHour * (diffSeconds.Value / 3600));
                if (pmcProfile.Health.Energy.Current > pmcProfile.Health.Energy.Maximum)
                {
                    pmcProfile.Health.Energy.Current = pmcProfile.Health.Energy.Maximum;
                }
            }

            // Player has hydration deficit
            if (pmcProfile.Health.Hydration.Current != pmcProfile.Health.Hydration.Maximum)
            {
                pmcProfile.Health.Hydration.Current += Math.Round(hydrationRegenPerHour * (diffSeconds.Value / 3600));
                if (pmcProfile.Health.Hydration.Current > pmcProfile.Health.Hydration.Maximum)
                {
                    pmcProfile.Health.Hydration.Current = pmcProfile.Health.Hydration.Maximum;
                }
            }

            // Check all body parts
            foreach (var bodyPart in pmcProfile.Health.BodyParts
                         .Select(bodyPartKvP => bodyPartKvP.Value))
            {
                // Check part hp
                if (bodyPart.Health.Current < bodyPart.Health.Maximum)
                {
                    bodyPart.Health.Current += Math.Round(hpRegenPerHour * (diffSeconds.Value / 3600));
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
                        // More than 30 mins has passed
                        if (diffSeconds > 1800)
                        {
                            bodyPart.Effects.Remove(effectKvP.Key);
                        }

                        continue;
                    }

                    // Decrement effect time value by difference between current time and time health was last updated
                    effectKvP.Value.Time -= diffSeconds;
                    if (effectKvP.Value.Time < 1)
                    {
                        // Effect time was sub 1, set floor it can be
                        effectKvP.Value.Time = 1;
                    }
                }
            }

            // Update both values as they've both been updated
            pmcProfile.Health.UpdateTime = currentTimeStamp;
        }
    }

    /// <summary>
    /// Send starting gifts to profile after x days
    /// </summary>
    /// <param name="pmcProfile">Profile to add gifts to</param>
    private void SendPraporGiftsToNewProfiles(PmcData pmcProfile)
    {
        var timeStampProfileCreated = pmcProfile.Info.RegistrationDate;
        var oneDaySeconds = _timeUtil.GetHoursAsSeconds(24);
        var currentTimeStamp = _timeUtil.GetTimeStamp();

        // One day post-profile creation
        if (currentTimeStamp > timeStampProfileCreated + oneDaySeconds)
        {
            _giftService.SendPraporStartingGift(pmcProfile.SessionId, 1);
        }

        // Two day post-profile creation
        if (currentTimeStamp > timeStampProfileCreated + oneDaySeconds * 2)
        {
            _giftService.SendPraporStartingGift(pmcProfile.SessionId, 2);
        }
    }

    /// <summary>
    /// Get a list of installed mods and save their details to the profile being used
    /// </summary>
    /// <param name="fullProfile">Profile to add mod details to</param>
    private void SaveActiveModsToProfile(SptProfile fullProfile)
    {
        // Add empty mod array if undefined
        if (fullProfile.SptData.Mods is null)
        {
            fullProfile.SptData.Mods = [];
        }

        // Get active mods
        _logger.Error("NOT IMPLEMENTED - _preSptModLoader SaveActiveModsToProfile()");
        //var activeMods = _preSptModLoader.GetImportedModDetails(); //TODO IMPLEMENT _preSptModLoader
        var activeMods = new Dictionary<string, ModDetails>();
        foreach (var modKvP in activeMods)
        {
            var modDetails = modKvP.Value;
            if (
                fullProfile.SptData.Mods.Any(
                    (mod) =>
                        mod.Author == modDetails.Author &&
                        mod.Name == modDetails.Name &&
                        mod.Version == modDetails.Version
                ))
            {
                // Exists already, skip
                continue;
            }

            fullProfile.SptData.Mods.Add(
                new ModDetails
                {
                    Author = modDetails.Author,
                    DateAdded = _timeUtil.GetTimeStamp(),
                    Name = modDetails.Name,
                    Version = modDetails.Version,
                    Url = modDetails.Url,
                }
            );
        }
    }

    /// <summary>
    /// Add the logged in players name to PMC name pool
    /// </summary>
    /// <param name="pmcProfile">Profile of player to get name from</param>
    private void AddPlayerToPmcNames(PmcData pmcProfile)
    {
        var playerName = pmcProfile.Info.Nickname;
        if (playerName is not null)
        {
            var bots = _databaseService.GetBots().Types;

            // Official names can only be 15 chars in length
            if (playerName.Length > _botConfig.BotNameLengthLimit)
            {
                return;
            }

            // Skip if player name exists already
            if (bots.TryGetValue("bear", out var bearBot))
            {
                if (bearBot is not null && bearBot.FirstNames.Any(x => x == playerName))
                {
                    bearBot.FirstNames.Add(playerName);
                }
            }

            if (bots.TryGetValue("bear", out var usecBot))
            {
                if (usecBot is not null && usecBot.FirstNames.Any(x => x == playerName))
                {
                    usecBot.FirstNames.Add(playerName);
                }
            }
        }
    }

    /// <summary>
    /// Check for a dialog with the key 'undefined', and remove it
    /// </summary>
    /// <param name="fullProfile">Profile to check for dialog in</param>
    private void CheckForAndRemoveUndefinedDialogues(SptProfile fullProfile)
    {
        if (fullProfile.DialogueRecords.TryGetValue("undefined", out var undefinedDialog))
        {
            fullProfile.DialogueRecords.Remove("undefined");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fullProfile"></param>
    private void LogProfileDetails(SptProfile fullProfile)
    {
        _logger.Debug($"Profile made with: {fullProfile.SptData.Version}");
        _logger.Debug($"Server version: {(ProgramStatics.SPT_VERSION()) ?? _coreConfig.SptVersion} {ProgramStatics.COMMIT}");
        _logger.Debug($"Debug enabled: {ProgramStatics.DEBUG}");
        _logger.Debug($"Mods enabled: {ProgramStatics.MODS}");
    }

    public void Load()
    {
        _postDbLoadService.PerformPostDbLoadActions();
    }
}
