using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RewardHelper(
    ISptLogger<RewardHelper> logger,
    TimeUtil timeUtil,
    ItemHelper itemHelper,
    DatabaseService databaseService,
    ProfileHelper profileHelper,
    ServerLocalisationService serverLocalisationService,
    TraderHelper traderHelper,
    PresetHelper presetHelper,
    NotifierHelper notifierHelper,
    NotificationSendHelper notificationSendHelper,
    ICloner cloner
)
{
    /// <summary>
    /// Apply the given rewards to the passed in profile.
    /// </summary>
    /// <param name="rewards">List of rewards to apply.</param>
    /// <param name="rewardSource">The source of the rewards (Achievement, quest).</param>
    /// <param name="fullProfile">The full profile to apply the rewards to.</param>
    /// <param name="profileData">The profile data (could be the scav profile).</param>
    /// <param name="rewardSourceId">The quest or achievement ID, used for finding production unlocks.</param>
    /// <param name="questResponse">Response to quest completion when a production is unlocked.</param>
    /// <returns>List of items that is the reward.</returns>
    public List<Item> ApplyRewards(
        IEnumerable<Reward> rewards,
        string rewardSource,
        SptProfile fullProfile,
        PmcData profileData,
        MongoId rewardSourceId,
        ItemEventRouterResponse? questResponse = null
    )
    {
        var sessionId = fullProfile?.ProfileInfo?.ProfileId;
        var pmcProfile = fullProfile?.CharacterData?.PmcData;
        if (pmcProfile is null)
        {
            logger.Error($"Unable to get PMC profile for: {sessionId}, no rewards given");

            return [];
        }

        var gameVersion = pmcProfile.Info.GameVersion;

        foreach (var reward in rewards)
        {
            // Handle reward availability for different game versions, notAvailableInGameEditions currently not used
            if (!RewardIsForGameEdition(reward, gameVersion))
            {
                continue;
            }

            switch (reward.Type)
            {
                case RewardType.Skill:
                    // This needs to use the passed in profileData, as it could be the scav profile
                    profileHelper.AddSkillPointsToPlayer(
                        profileData,
                        Enum.Parse<SkillTypes>(reward.Target),
                        reward.Value.GetValueOrDefault(0)
                    );
                    break;
                case RewardType.Experience:
                    profileHelper.AddExperienceToPmc(sessionId.Value, int.Parse(reward.Value.ToString())); // this must occur first as the output object needs to take the modified profile exp value
                    // Recalculate level in event player leveled up
                    pmcProfile.Info.Level = pmcProfile.CalculateLevel(databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable);
                    break;
                case RewardType.TraderStanding:
                    traderHelper.AddStandingToTrader(sessionId.Value, reward.Target, reward.Value.Value);
                    break;
                case RewardType.TraderUnlock:
                    traderHelper.SetTraderUnlockedState(reward.Target, true, sessionId.Value);
                    break;
                case RewardType.Item:
                    // Item rewards are retrieved by getRewardItems() below, and returned to be handled by caller
                    break;
                case RewardType.AssortmentUnlock:
                    // Handled by getAssort(), locked assorts are stripped out by `assortHelper.stripLockedLoyaltyAssort()` before being sent to player
                    break;
                case RewardType.Achievement:
                    AddAchievementToProfile(fullProfile, reward.Target);
                    break;
                case RewardType.StashRows:
                    var bonusId = profileHelper.AddStashRowsBonusToProfile(sessionId.Value, (int)reward.Value); // Add specified stash rows from reward - requires client restart

                    notificationSendHelper.SendMessage(
                        sessionId.Value,
                        new WsProfileChangeEvent
                        {
                            EventIdentifier = new MongoId(),
                            EventType = NotificationEventType.StashRows,
                            Changes = new Dictionary<string, double?> { { bonusId, reward.Value } },
                        }
                    );

                    break;
                case RewardType.ProductionScheme:
                    FindAndAddHideoutProductionIdToProfile(pmcProfile, reward, rewardSourceId, sessionId.Value, questResponse);
                    break;
                case RewardType.Pockets:
                    profileHelper.ReplaceProfilePocketTpl(pmcProfile, reward.Target);
                    break;
                case RewardType.CustomizationDirect:
                    profileHelper.AddHideoutCustomisationUnlock(fullProfile, reward, rewardSource);
                    notificationSendHelper.SendMessage(
                        sessionId.Value,
                        new WsNotificationEvent
                        {
                            EventIdentifier = new MongoId(),
                            EventType = NotificationEventType.CustomizationUpdateRequired,
                        }
                    );

                    break;
                case RewardType.NotificationPopup:
                    var notification = notifierHelper.CreateNotificationPopup(reward.IllustrationConfig, reward.Message.Value);
                    notificationSendHelper.SendMessage(sessionId.Value, notification);
                    break;
                case RewardType.WebPromoCode:
                    // TODO: ??? (Free arena trial from Balancing - Part 1)
                    logger.Error("UNHANDLED: RewardType.WebPromoCode");
                    break;
                default:
                    logger.Error(
                        serverLocalisationService.GetText(
                            "reward-type_not_handled",
                            new { rewardType = reward.Type, questId = rewardSourceId }
                        )
                    );
                    break;
            }
        }

        return GetRewardItems(rewards, gameVersion).ToList();
    }

    /// <summary>
    /// Does the provided reward have a game version requirement to be given and does it match.
    /// </summary>
    /// <param name="reward">Reward to check.</param>
    /// <param name="gameVersion">Version of game to check reward against.</param>
    /// <returns>True if it has requirement, false if it doesn't pass check.</returns>
    public bool RewardIsForGameEdition(Reward reward, string gameVersion)
    {
        if (reward.AvailableInGameEditions?.Count > 0 && !reward.AvailableInGameEditions.Contains(gameVersion))
        // Reward has edition whitelist and game version isn't in it
        {
            return false;
        }

        if (reward.NotAvailableInGameEditions?.Count > 0 && reward.NotAvailableInGameEditions.Contains(gameVersion))
        // Reward has edition blacklist and game version is in it
        {
            return false;
        }

        // No whitelist/blacklist or reward isn't blacklisted/whitelisted
        return true;
    }

    /// <summary>
    /// WIP - Find hideout craft id and add to unlockedProductionRecipe array in player profile
    /// also update client response recipeUnlocked array with craft id
    /// </summary>
    /// <param name="pmcData">Player profile.</param>
    /// <param name="craftUnlockReward">Reward with craft unlock details.</param>
    /// <param name="questId">Quest or achievement ID with craft unlock reward.</param>
    /// <param name="sessionID">Session id.</param>
    /// <param name="response">Response to send back to client.</param>
    protected void FindAndAddHideoutProductionIdToProfile(
        PmcData pmcData,
        Reward craftUnlockReward,
        MongoId questId,
        MongoId sessionID,
        ItemEventRouterResponse response
    )
    {
        var matchingProductions = GetRewardProductionMatch(craftUnlockReward, questId);
        if (matchingProductions.Count != 1)
        {
            logger.Error(
                serverLocalisationService.GetText(
                    "reward-unable_to_find_matching_hideout_production",
                    new { questId, matchCount = matchingProductions.Count }
                )
            );

            return;
        }

        // Add above match to pmc profile + client response
        var matchingCraftId = matchingProductions[0].Id;
        pmcData.UnlockedInfo.UnlockedProductionRecipe.Add(matchingCraftId);

        // Update Inform client of change
        response.ProfileChanges[sessionID].RecipeUnlocked ??= new();
        response.ProfileChanges[sessionID].RecipeUnlocked[matchingCraftId] = true;
    }

    /// <summary>
    /// Find hideout craft for the specified reward.
    /// </summary>
    /// <param name="craftUnlockReward">Reward with craft unlock details.</param>
    /// <param name="questId">Quest or achievement ID with craft unlock reward.</param>
    /// <returns>List of matching HideoutProduction objects.</returns>
    public List<HideoutProduction> GetRewardProductionMatch(Reward craftUnlockReward, MongoId questId)
    {
        // Get hideout crafts and find those that match by areaType/required level/end product tpl - hope for just one match

        // "TraderId" holds area ID that will be used to craft unlocked item
        var desiredHideoutAreaType = (HideoutAreas)int.Parse(craftUnlockReward.TraderId.ToString());

        var matchingProductions = GetMatchingProductions(desiredHideoutAreaType, questId, craftUnlockReward);

        // More/less than single match, above filtering wasn't strict enough
        if (matchingProductions.Count != 1)
        // Multiple matches were found, last ditch attempt to match by questid (value we add manually to production.json via `gen:productionquests` command)
        {
            matchingProductions = matchingProductions
                .Where(prod => prod.Requirements.Any(requirement => requirement.QuestId == questId))
                .ToList();
        }

        return matchingProductions;
    }

    /// <summary>
    /// Find a hideout craft (production) based on input parameter data
    /// </summary>
    /// <param name="desiredHideoutAreaType">Hideout area craft is for</param>
    /// <param name="questId">Id of quest with production unlock</param>
    /// <param name="craftUnlockReward">Reward given by quest</param>
    /// <returns>Hideout crafts that match input parameters</returns>
    protected List<HideoutProduction> GetMatchingProductions(HideoutAreas desiredHideoutAreaType, MongoId questId, Reward craftUnlockReward)
    {
        var rewardItemTpl = craftUnlockReward.Items.FirstOrDefault()?.Template;

        var craftingRecipes = databaseService.GetHideout().Production.Recipes;
        return craftingRecipes
            .Where(production =>
                // Some crafts have the questId, easy match
                production.Requirements.Any(req => req.QuestId == questId)
                ||
                // Couldn't get craft by questId, get the closest match based on information we know
                (
                    rewardItemTpl is not null
                    && production.AreaType == desiredHideoutAreaType
                    && production.EndProduct == rewardItemTpl.Value
                    && production.Requirements.Any(req => req.Type is "QuestComplete")
                    && production.Requirements.Any(req => req.RequiredLevel == craftUnlockReward.LoyaltyLevel)
                )
            )
            .ToList();
    }

    /// <summary>
    /// Gets a flat list of reward items from the given rewards for the specified game version.
    /// </summary>
    /// <param name="rewards">Array of rewards to get the items from.</param>
    /// <param name="gameVersion">The game version of the profile.</param>
    /// <returns>Array of items with the correct maxStack.</returns>
    protected IEnumerable<Item> GetRewardItems(IEnumerable<Reward> rewards, string gameVersion)
    {
        // Iterate over all rewards with the desired status, flatten out items that have a type of Item
        var rewardItems = rewards.SelectMany(reward =>
            reward.Type == RewardType.Item && RewardIsForGameEdition(reward, gameVersion) ? ProcessReward(reward) : []
        );

        return rewardItems;
    }

    /// <summary>
    /// Take reward item and set FiR status, fix stack sizes, and fix mod Ids.
    /// </summary>
    /// <param name="reward">Reward item to fix.</param>
    /// <returns>Fixed rewards.</returns>
    protected List<Item> ProcessReward(Reward reward)
    {
        // item with mods to return
        List<Item> rewardItems = [];
        List<Item> targets = [];
        List<Item> mods = [];

        // Is armor item that may need inserts / plates
        if (reward.Items.Count == 1 && itemHelper.ArmorItemCanHoldMods(reward.Items[0].Template))
        // Only process items with slots
        {
            if (itemHelper.ItemHasSlots(reward.Items.FirstOrDefault().Template))
            // Attempt to pull default preset from globals and add child items to reward (clones reward.items)
            {
                GenerateArmorRewardChildSlots(reward.Items.FirstOrDefault(), reward);
            }
        }

        foreach (var rewardItem in reward.Items)
        {
            rewardItem.AddUpd();

            // Reward items are granted Found in Raid status
            itemHelper.SetFoundInRaid(rewardItem);

            // Is root item, fix stacks
            if (rewardItem.Id == reward.Target)
            {
                // Is base reward item
                if (
                    rewardItem.ParentId != null
                    && rewardItem.ParentId == "hideout"
                    && // Has parentId of hideout
                    rewardItem.Upd != null
                    && rewardItem.Upd.StackObjectsCount != null
                    && // Has upd with stackobject count
                    rewardItem.Upd.StackObjectsCount > 1 // More than 1 item in stack
                )
                {
                    rewardItem.Upd.StackObjectsCount = 1;
                }

                targets = itemHelper.SplitStack(rewardItem);
                // splitStack created new ids for the new stacks. This would destroy the relation to possible children.
                // Instead, we reset the id to preserve relations and generate a new id in the downstream loop, where we are also reparenting if required
                foreach (var target in targets)
                {
                    target.Id = rewardItem.Id;
                }
            }
            else
            {
                // Is child mod
                if (reward.Items.FirstOrDefault().Upd.SpawnedInSession.GetValueOrDefault(false))
                // Propagate FiR status into child items
                {
                    if (!itemHelper.IsOfBaseclasses(rewardItem.Template, [BaseClasses.AMMO, BaseClasses.MONEY]))
                    {
                        rewardItem.Upd.SpawnedInSession = reward.Items.FirstOrDefault()?.Upd.SpawnedInSession;
                    }
                }

                mods.Add(rewardItem);
            }
        }

        // Add mods to the base items, fix ids
        foreach (var target in targets)
        {
            // This has all the original id relations since we reset the id to the original after the splitStack
            var itemsClone = new List<Item> { cloner.Clone(target) };
            // Here we generate a new id for the root item
            target.Id = new MongoId();

            // Add cloned mods to root item array
            var clonedMods = cloner.Clone(mods);
            foreach (var mod in clonedMods)
            {
                itemsClone.Add(mod);
            }

            // Re-parent items + generate new ids to ensure valid ids
            var itemsToAdd = itemHelper.ReparentItemAndChildren(target, itemsClone);
            rewardItems.AddRange(itemsToAdd);
        }

        return rewardItems;
    }

    /// <summary>
    /// Add missing mod items to an armor reward.
    /// </summary>
    /// <param name="originalRewardRootItem">Original armor reward item from IReward.items object.</param>
    /// <param name="reward">Armor reward.</param>
    protected void GenerateArmorRewardChildSlots(Item originalRewardRootItem, Reward reward)
    {
        // Look for a default preset from globals for armor
        var defaultPreset = presetHelper.GetDefaultPreset(originalRewardRootItem.Template);
        if (defaultPreset is not null)
        {
            // Found preset, use mods to hydrate reward item
            var presetAndMods = cloner.Clone(defaultPreset.Items).ReplaceIDs().ToList();
            var newRootId = presetAndMods.RemapRootItemId();

            reward.Items = presetAndMods;

            // Find root item and set its stack count
            var rootItem = reward.Items.FirstOrDefault(item => item.Id == newRootId);

            // Remap target id to the new presets root id
            reward.Target = rootItem.Id;

            // Copy over stack count otherwise reward shows as missing in client
            rootItem.AddUpd();
            rootItem.Upd.StackObjectsCount = originalRewardRootItem.Upd.StackObjectsCount;
            return;
        }

        logger.Warning($"Unable to find default preset for armor: {originalRewardRootItem.Template}, adding mods manually");
        var itemDbData = itemHelper.GetItem(originalRewardRootItem.Template).Value;

        // Hydrate reward with only 'required' mods - necessary for things like helmets otherwise you end up with nvgs/visors etc
        reward.Items = itemHelper.AddChildSlotItems(reward.Items, itemDbData, null, true);
    }

    /// <summary>
    /// Add an achievement to player profile and handle any rewards for the achievement.
    /// Triggered from a quest, or another achievement.
    /// </summary>
    /// <param name="fullProfile">Profile to add achievement to.</param>
    /// <param name="achievementId">Id of achievement to add.</param>
    public void AddAchievementToProfile(SptProfile fullProfile, MongoId achievementId)
    {
        // Add achievement id to profile with timestamp it was unlocked
        fullProfile.CharacterData.PmcData.Achievements.TryAdd(achievementId, timeUtil.GetTimeStamp());

        // Check for any customisation unlocks
        var achievementDataDb = databaseService.GetTemplates().Achievements.FirstOrDefault(achievement => achievement.Id == achievementId);
        if (achievementDataDb is null)
        {
            return;
        }

        // Note: At the moment, we don't know the exact quest and achievement data layout for an achievement
        //       that is triggered by a quest, that gives an item, because BSG has only done this once. However
        //       based on deduction, I am going to assume that the *quest* will handle the initial item reward,
        //       and the achievement reward should only be handled post-wipe.
        // All of that is to say, we are going to ignore the list of returned reward items here
        var pmcProfile = fullProfile.CharacterData.PmcData;
        ApplyRewards(achievementDataDb.Rewards, CustomisationSource.ACHIEVEMENT, fullProfile, pmcProfile, achievementDataDb.Id);
    }
}
