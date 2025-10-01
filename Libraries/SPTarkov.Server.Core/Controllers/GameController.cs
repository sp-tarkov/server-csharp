using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Location;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class GameController(
    ISptLogger<GameController> logger,
    IReadOnlyList<SptMod> loadedMods,
    ConfigServer configServer,
    DatabaseService databaseService,
    TimeUtil timeUtil,
    HttpServerHelper httpServerHelper,
    HideoutHelper hideoutHelper,
    ProfileHelper profileHelper,
    ProfileFixerService profileFixerService,
    ServerLocalisationService serverLocalisationService,
    PostDbLoadService postDbLoadService,
    SeasonalEventService seasonalEventService,
    GiftService giftService,
    RaidTimeAdjustmentService raidTimeAdjustmentService,
    ProfileActivityService profileActivityService
)
{
    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();
    protected const double Deviation = 0.0001;
    protected readonly HideoutConfig HideoutConfig = configServer.GetConfig<HideoutConfig>();
    protected readonly HttpConfig HttpConfig = configServer.GetConfig<HttpConfig>();

    /// <summary>
    ///     Handle client/game/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="startTimeStampMs"></param>
    public void GameStart(string url, MongoId sessionId, long startTimeStampMs)
    {
        profileActivityService.AddActiveProfile(sessionId, startTimeStampMs);

        if (sessionId.IsEmpty)
        {
            logger.Error($"{nameof(sessionId)} is empty on GameController.GameStart");
            return;
        }

        // repeatableQuests are stored by in profile.Quests due to the responses of the client (e.g. Quests in
        // offraidData). Since we don't want to clutter the Quests list, we need to remove all completed (failed or
        // successful) repeatable quests. We also have to remove the Counters from the repeatableQuests

        var fullProfile = profileHelper.GetFullProfile(sessionId);
        if (fullProfile is null)
        {
            logger.Error($"{nameof(fullProfile)} is null on GameController.GameStart");
            return;
        }

        fullProfile.FriendProfileIds ??= [];

        if (fullProfile.ProfileInfo?.IsWiped is not null && fullProfile.ProfileInfo.IsWiped.Value)
        {
            return;
        }

        if (fullProfile.ProfileInfo?.InvalidOrUnloadableProfile is not null && fullProfile.ProfileInfo.InvalidOrUnloadableProfile.Value)
        {
            return;
        }

        fullProfile.CharacterData!.PmcData!.WishList ??= new();
        fullProfile.CharacterData.ScavData!.WishList ??= new();

        if (fullProfile.DialogueRecords is not null)
        {
            profileFixerService.CheckForAndFixDialogueAttachments(fullProfile);
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Started game with session {sessionId} {fullProfile.ProfileInfo?.Username}");
        }

        var pmcProfile = fullProfile.CharacterData.PmcData;

        if (CoreConfig.Fixes.FixProfileBreakingInventoryItemIssues)
        {
            profileFixerService.FixProfileBreakingInventoryItemIssues(pmcProfile);
        }

        if (pmcProfile.Health is not null)
        {
            UpdateProfileHealthValues(pmcProfile);
        }

        if (pmcProfile.Inventory is not null)
        {
            SendPraporGiftsToNewProfiles(pmcProfile);
            SendMechanicGiftsToNewProfile(pmcProfile);
        }

        profileFixerService.CheckForAndRemoveInvalidTraders(fullProfile);
        profileFixerService.CheckForAndFixPmcProfileIssues(pmcProfile);

        if (pmcProfile.Hideout is not null)
        {
            profileFixerService.AddMissingHideoutBonusesToProfile(pmcProfile, databaseService.GetHideout().Areas);
            hideoutHelper.SetHideoutImprovementsToCompleted(pmcProfile);
            pmcProfile.UnlockHideoutWallInProfile();

            // Handle if player has been inactive for a long time, catch up on hideout update before the user goes to his hideout
            if (!profileActivityService.ActiveWithinLastMinutes(sessionId, HideoutConfig.UpdateProfileHideoutWhenActiveWithinMinutes))
            {
                hideoutHelper.UpdatePlayerHideout(sessionId);
            }
        }

        LogProfileDetails(fullProfile);
        SaveActiveModsToProfile(fullProfile);

        if (pmcProfile.Info is not null)
        {
            AddPlayerToPmcNames(pmcProfile);
        }

        if (pmcProfile.Skills?.Common is not null)
        {
            WarnOnActiveBotReloadSkill(pmcProfile);
        }

        seasonalEventService.GivePlayerSeasonalGifts(sessionId);

        // Set activity timestamp at the end of the method, so that code that checks for an older timestamp (Updating hideout) can still run
        profileActivityService.SetActivityTimestamp(sessionId);
    }

    /// <summary>
    ///     Handle client/game/config
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>GameConfigResponse</returns>
    public GameConfigResponse GetGameConfig(MongoId sessionId)
    {
        var profile = profileHelper.GetPmcProfile(sessionId);
        var gameTime =
            profile?.Stats?.Eft?.OverallCounters?.Items?.FirstOrDefault(c => c.Key!.Contains("LifeTime") && c.Key.Contains("Pmc"))?.Value
            ?? 0D;

        var config = new GameConfigResponse
        {
            Languages = databaseService.GetLocales().Languages,
            IsNdaFree = false,
            IsReportAvailable = false,
            IsTwitchEventMember = false,
            Language = "en",
            Aid = profile?.Aid,
            Taxonomy = 6,
            ActiveProfileId = sessionId,
            Backend = new Backend
            {
                Lobby = httpServerHelper.GetBackendUrl(),
                Trading = httpServerHelper.GetBackendUrl(),
                Messaging = httpServerHelper.GetBackendUrl(),
                Main = httpServerHelper.GetBackendUrl(),
                RagFair = httpServerHelper.GetBackendUrl(),
            },
            UseProtobuf = false,
            UtcTime = timeUtil.GetTimeStamp(),
            TotalInGame = gameTime,
            SessionMode = "pve",
            PurchasedGames = new PurchasedGames { IsEftPurchased = true, IsArenaPurchased = false },
            IsGameSynced = true,
        };

        return config;
    }

    /// <summary>
    ///     Handle client/game/mode
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="requestData"></param>
    /// <returns></returns>
    public GameModeResponse GetGameMode(MongoId sessionId, GameModeRequestData requestData)
    {
        return new GameModeResponse { GameMode = "pve", BackendUrl = httpServerHelper.GetBackendUrl() };
    }

    /// <summary>
    ///     Handle client/server/list
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public List<ServerDetails> GetServer(MongoId sessionId)
    {
        return [new ServerDetails { Ip = HttpConfig.BackendIp, Port = HttpConfig.BackendPort }];
    }

    /// <summary>
    ///     Handle client/match/group/current
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public CurrentGroupResponse GetCurrentGroup(MongoId sessionId)
    {
        return new CurrentGroupResponse { Squad = [] };
    }

    /// <summary>
    ///     Handle client/checkVersion
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public CheckVersionResponse GetValidGameVersion(MongoId sessionId)
    {
        return new CheckVersionResponse { IsValid = true, LatestVersion = CoreConfig.CompatibleTarkovVersion };
    }

    /// <summary>
    ///     Handle client/game/keepalive
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public GameKeepAliveResponse GetKeepAlive(MongoId sessionId)
    {
        profileActivityService.SetActivityTimestamp(sessionId);
        return new GameKeepAliveResponse { Message = "OK", UtcTime = timeUtil.GetTimeStamp() };
    }

    /// <summary>
    ///     Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    public RaidChanges GetRaidTime(MongoId sessionId, GetRaidTimeRequest request)
    {
        return raidTimeAdjustmentService.GetRaidAdjustments(sessionId, request);
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public SurveyResponseData GetSurvey(MongoId sessionId)
    {
        return CoreConfig.Survey;
    }

    /// <summary>
    ///     Players set botReload to a high value and don't expect the crazy fast reload speeds, give them a warn about it
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    protected void WarnOnActiveBotReloadSkill(PmcData pmcProfile)
    {
        var botReloadSkill = pmcProfile.GetSkillFromProfile(SkillTypes.BotReload);
        if (botReloadSkill?.Progress > 0)
        {
            logger.Warning(serverLocalisationService.GetText("server_start_player_active_botreload_skill"));
        }
    }

    /// <summary>
    ///     When player logs in, iterate over all active effects and reduce timer
    /// </summary>
    /// <param name="pmcProfile">Profile to adjust values for</param>
    protected void UpdateProfileHealthValues(PmcData pmcProfile)
    {
        var healthLastUpdated = pmcProfile.Health?.UpdateTime;
        var currentTimeStamp = timeUtil.GetTimeStamp();
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
        energyRegenPerHour += pmcProfile
            .Bonuses!.Where(bonus => bonus.Type == BonusType.EnergyRegeneration)
            .Aggregate(0d, (sum, bonus) => sum + bonus.Value!.Value);

        hydrationRegenPerHour += pmcProfile
            .Bonuses!.Where(bonus => bonus.Type == BonusType.HydrationRegeneration)
            .Aggregate(0d, (sum, bonus) => sum + bonus.Value!.Value);

        hpRegenPerHour += pmcProfile
            .Bonuses!.Where(bonus => bonus.Type == BonusType.HealthRegeneration)
            .Aggregate(0d, (sum, bonus) => sum + bonus.Value!.Value);

        // Player has energy deficit
        if (pmcProfile.Health?.Energy?.Current - pmcProfile.Health?.Energy?.Maximum <= Deviation)
        {
            // Set new value, whatever is smallest
            pmcProfile.Health!.Energy!.Current += Math.Round(energyRegenPerHour * (diffSeconds!.Value / 3600));
            if (pmcProfile.Health.Energy.Current > pmcProfile.Health.Energy.Maximum)
            {
                pmcProfile.Health.Energy.Current = pmcProfile.Health.Energy.Maximum;
            }
        }

        // Player has hydration deficit
        if (pmcProfile.Health?.Hydration?.Current - pmcProfile.Health?.Hydration?.Maximum <= Deviation)
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
        foreach (var bodyPart in pmcProfile.Health!.BodyParts!.Select(bodyPartKvP => bodyPartKvP.Value))
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
            foreach (var (effectId, effect) in bodyPart.Effects)
            {
                // remove effects below 1, .e.g. bleeds at -1
                if (effect.Time < 1)
                {
                    // More than 30 minutes has passed
                    if (diffSeconds > timeUtil.GetMinutesAsSeconds(30))
                    {
                        bodyPart.Effects.Remove(effectId);
                    }

                    continue;
                }

                // Decrement effect time value by difference between current time and time health was last updated
                effect.Time -= diffSeconds;
                if (effect.Time < 1)
                // Effect time was sub 1, set floor it can be
                {
                    effect.Time = 1;
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
        var oneDaySeconds = timeUtil.GetHoursAsSeconds(24);
        var currentTimeStamp = timeUtil.GetTimeStamp();

        // One day post-profile creation
        if (currentTimeStamp > timeStampProfileCreated + oneDaySeconds)
        {
            giftService.SendPraporStartingGift(pmcProfile.SessionId.Value, 1);
        }

        // Two day post-profile creation
        if (currentTimeStamp > timeStampProfileCreated + (oneDaySeconds * 2))
        {
            giftService.SendPraporStartingGift(pmcProfile.SessionId.Value, 2);
        }
    }

    /// <summary>
    ///     Mechanic sends players a measuring tape on profile start for some reason
    /// </summary>
    /// <param name="pmcProfile"></param>
    protected void SendMechanicGiftsToNewProfile(PmcData pmcProfile)
    {
        giftService.SendGiftWithSilentReceivedCheck("MechanicGiftDay1", pmcProfile.SessionId.Value, 1);
    }

    /// <summary>
    ///     Get a list of installed mods and save their details to the profile being used
    /// </summary>
    /// <param name="fullProfile">Profile to add mod details to</param>
    protected void SaveActiveModsToProfile(SptProfile fullProfile)
    {
        fullProfile.SptData!.Mods ??= [];

        foreach (var mod in loadedMods)
        {
            if (
                fullProfile.SptData.Mods.Any(m =>
                    m.Author == mod.ModMetadata.Author && m.Version == mod.ModMetadata.Version.ToString() && m.Name == mod.ModMetadata.Name
                )
            )
            {
                // exists already, skip
                continue;
            }

            fullProfile.SptData.Mods.Add(
                new ModDetails
                {
                    Author = mod.ModMetadata.Author,
                    Version = mod.ModMetadata.Version.ToString(),
                    Name = mod.ModMetadata.Name,
                    Url = mod.ModMetadata.Url,
                    DateAdded = timeUtil.GetTimeStamp(),
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
            var bots = databaseService.GetBots().Types;

            // Official names can only be 15 chars in length
            if (playerName.Length > BotConfig.BotNameLengthLimit)
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
    /// </summary>
    /// <param name="fullProfile"></param>
    protected void LogProfileDetails(SptProfile fullProfile)
    {
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Profile made with: {fullProfile.SptData?.Version}");
            logger.Debug($"Server version: {ProgramStatics.SPT_VERSION()} {ProgramStatics.COMMIT()}");
            logger.Debug($"Debug enabled: {ProgramStatics.DEBUG()}");
            logger.Debug($"Mods enabled: {ProgramStatics.MODS()}");
        }
    }

    public void Load()
    {
        postDbLoadService.PerformPostDbLoadActions();
    }
}
