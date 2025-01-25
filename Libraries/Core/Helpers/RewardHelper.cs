using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;

namespace Core.Helpers
{
    [Injectable]
    public class RewardHelper
    {
        private readonly ISptLogger<RewardHelper> _logger;
        private readonly HashUtil _hashUtil;
        private readonly TimeUtil _timeUtil;
        private readonly ItemHelper _itemHelper;
        private readonly DatabaseService _databaseService;
        private readonly ProfileHelper _profileHelper;
        private readonly LocalisationService _localisationService;
        private readonly TraderHelper _traderHelper;
        private readonly PresetHelper _presetHelper;
        private readonly ICloner _cloner;

        public RewardHelper(
            ISptLogger<RewardHelper> logger,
            HashUtil hashUtil,
            TimeUtil timeUtil,
            ItemHelper itemHelper,
            DatabaseService databaseService,
            ProfileHelper profileHelper,
            LocalisationService localisationService,
            TraderHelper traderHelper,
            PresetHelper presetHelper,
            ICloner cloner
        )
        {
            _logger = logger;
            _hashUtil = hashUtil;
            _timeUtil = timeUtil;
            _itemHelper = itemHelper;
            _databaseService = databaseService;
            _profileHelper = profileHelper;
            _localisationService = localisationService;
            _traderHelper = traderHelper;
            _presetHelper = presetHelper;
            _cloner = cloner;
        }

        /**
         * Apply the given rewards to the passed in profile
         * @param rewards List of rewards to apply
         * @param source The source of the rewards (Achievement, quest)
         * @param fullProfile The full profile to apply the rewards to
         * @param questId The quest or achievement ID, used for finding production unlocks
         * @param questResponse Response to quest completion when a production is unlocked
         * @returns List of items that were rewarded
         */
        public List<Item> ApplyRewards(
            List<Reward> rewards,
            string source,
            SptProfile fullProfile,
            PmcData profileData,
            string questId,
            ItemEventRouterResponse questResponse = null
        )
        {
            var sessionId = fullProfile?.ProfileInfo?.ProfileId;
            var pmcProfile = fullProfile?.CharacterData.PmcData;
            if (pmcProfile is null)
            {
                _logger.Error($"Unable to get pmc profile for: {sessionId}, no rewards given");
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
                        _profileHelper.AddSkillPointsToPlayer(
                            profileData,
                            Enum.Parse<SkillTypes>(reward.Target),
                            reward.Value as double?
                        );
                        break;
                    case RewardType.Experience:
                        _profileHelper.AddExperienceToPmc(
                            sessionId,
                            int.Parse(reward.Value.ToString())
                        ); // this must occur first as the output object needs to take the modified profile exp value
                        break;
                    case RewardType.TraderStanding:
                        _traderHelper.AddStandingToTrader(
                            sessionId,
                            reward.Target,
                            double.Parse(reward.Value.ToString())
                        );
                        break;
                    case RewardType.TraderUnlock:
                        _traderHelper.SetTraderUnlockedState(reward.Target, true, sessionId);
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
                        _profileHelper.AddStashRowsBonusToProfile(
                            sessionId,
                            (int)reward.Value
                        ); // Add specified stash rows from reward - requires client restart
                        break;
                    case RewardType.ProductionScheme:
                        FindAndAddHideoutProductionIdToProfile(pmcProfile, reward, questId, sessionId, questResponse);
                        break;
                    case RewardType.Pockets:
                        _profileHelper.ReplaceProfilePocketTpl(pmcProfile, reward.Target);
                        break;
                    case RewardType.CustomizationDirect:
                        _profileHelper.AddHideoutCustomisationUnlock(fullProfile, reward, source);
                        break;
                    default:
                        _logger.Error(
                            _localisationService.GetText(
                                "reward-type_not_handled",
                                new
                                {
                                    rewardType = reward.Type,
                                    questId = questId,
                                }
                            )
                        );
                        break;
                }
            }

            return GetRewardItems(rewards, gameVersion);
        }

        /**
     * Does the provided reward have a game version requirement to be given and does it match
     * @param reward Reward to check
     * @param gameVersion Version of game to check reward against
     * @returns True if it has requirement, false if it doesnt pass check
     */
        public bool RewardIsForGameEdition(Reward reward, string gameVersion)
        {
            if (reward.AvailableInGameEditions?.Count > 0 && !reward.AvailableInGameEditions.Contains(gameVersion))
            {
                // Reward has edition whitelist and game version isn't in it
                return false;
            }

            if (reward.NotAvailableInGameEditions?.Count > 0 &&
                reward.NotAvailableInGameEditions.Contains(gameVersion))
            {
                // Reward has edition blacklist and game version is in it
                return false;
            }

            // No whitelist/blacklist or reward isn't blacklisted/whitelisted
            return true;
        }

        /**
         * WIP - Find hideout craft id and add to unlockedProductionRecipe array in player profile
         * also update client response recipeUnlocked array with craft id
         * @param pmcData Player profile
         * @param craftUnlockReward Reward with craft unlock details
         * @param questId Quest or achievement ID with craft unlock reward
         * @param sessionID Session id
         * @param response Response to send back to client
         */
        protected void FindAndAddHideoutProductionIdToProfile(
            PmcData pmcData,
            Reward craftUnlockReward,
            string questId,
            string sessionID,
            ItemEventRouterResponse response)
        {
            var matchingProductions = GetRewardProductionMatch(craftUnlockReward, questId);
            if (matchingProductions.Count != 1)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "reward-unable_to_find_matching_hideout_production",
                        new
                        {
                            questId = questId,
                            matchCount = matchingProductions.Count,
                        }
                    )
                );

                return;
            }

            // Add above match to pmc profile + client response
            var matchingCraftId = matchingProductions[0].Id;
            pmcData.UnlockedInfo.UnlockedProductionRecipe.Add(matchingCraftId);
            if (response is not null)
            {
                response.ProfileChanges[sessionID].RecipeUnlocked ??= new Dictionary<string, bool>();
                response.ProfileChanges[sessionID].RecipeUnlocked[matchingCraftId] = true;
            }
        }

        /**
         * Find hideout craft for the specified reward
         * @param craftUnlockReward Reward with craft unlock details
         * @param questId Quest or achievement ID with craft unlock reward
         * @returns Hideout craft
         */
        public List<HideoutProduction> GetRewardProductionMatch(Reward craftUnlockReward, string questId)
        {
            // Get hideout crafts and find those that match by areatype/required level/end product tpl - hope for just one match
            var craftingRecipes = _databaseService.GetHideout().Production.Recipes;

            // Area that will be used to craft unlocked item
            var desiredHideoutAreaType = (HideoutAreas)craftUnlockReward.TraderId;

            var matchingProductions = craftingRecipes.Where(
                    (prod) =>
                        prod.AreaType == desiredHideoutAreaType &&
                        //prod.requirements.some((requirement) => requirement.questId == questId) && // BSG don't store the quest id in requirement any more!
                        prod.Requirements.Any((requirement) => requirement.Type == "QuestComplete") &&
                        prod.Requirements.Any(
                            (requirement) => requirement.RequiredLevel == craftUnlockReward.LoyaltyLevel
                        ) &&
                        prod.EndProduct == craftUnlockReward.Items.FirstOrDefault().Template
                )
                .ToList();

            // More/less than single match, above filtering wasn't strict enough
            if (matchingProductions.Count() != 1)
            {
                // Multiple matches were found, last ditch attempt to match by questid (value we add manually to production.json via `gen:productionquests` command)
                matchingProductions = matchingProductions.Where(
                        (prod) =>
                            prod.Requirements.Any((requirement) => requirement.QuestId == questId)
                    )
                    .ToList();
            }

            return matchingProductions;
        }

        /**
         * Gets a flat list of reward items from the given rewards for the specified game version
         * @param rewards Array of rewards to get the items from
         * @param gameVersion The game version of the profile
         * @returns array of items with the correct maxStack
         */
        protected List<Item> GetRewardItems(List<Reward> rewards, string gameVersion)
        {
            // Iterate over all rewards with the desired status, flatten out items that have a type of Item
            var rewardItems = rewards.SelectMany(
                (reward) =>
                    reward.Type == RewardType.Item && RewardIsForGameEdition(reward, gameVersion)
                        ? ProcessReward(reward)
                        : []
            );

            return rewardItems.ToList();
        }

        /**
         * Take reward item and set FiR status + fix stack sizes + fix mod Ids
         * @param reward Reward item to fix
         * @returns Fixed rewards
         */
        protected List<Item> ProcessReward(Reward reward)
        {
            /** item with mods to return */
            List<Item> rewardItems = [];
            List<Item> targets = [];
            List<Item> mods = [];

            // Is armor item that may need inserts / plates
            if (reward.Items.Count == 1 && _itemHelper.ArmorItemCanHoldMods(reward.Items[0].Template))
            {
                // Only process items with slots
                if (_itemHelper.ItemHasSlots(reward.Items.FirstOrDefault().Template))
                {
                    // Attempt to pull default preset from globals and add child items to reward (clones reward.items)
                    GenerateArmorRewardChildSlots(reward.Items.FirstOrDefault(), reward);
                }
            }

            foreach (var rewardItem in reward.Items)
            {
                _itemHelper.AddUpdObjectToItem(rewardItem);

                // Reward items are granted Found in Raid status
                rewardItem.Upd.SpawnedInSession = true;

                // Is root item, fix stacks
                if (rewardItem.Id == reward.Target)
                {
                    // Is base reward item
                    if (
                        rewardItem.ParentId != null &&
                        rewardItem.ParentId == "hideout" && // Has parentId of hideout
                        rewardItem.Upd != null &&
                        rewardItem.Upd.StackObjectsCount != null && // Has upd with stackobject count
                        rewardItem.Upd.StackObjectsCount > 1 // More than 1 item in stack
                    )
                    {
                        rewardItem.Upd.StackObjectsCount = 1;
                    }

                    targets = _itemHelper.SplitStack(rewardItem);
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
                    {
                        // Propigate FiR status into child items
                        rewardItem.Upd.SpawnedInSession = reward.Items.FirstOrDefault()?.Upd.SpawnedInSession;
                    }

                    mods.Add(rewardItem);
                }
            }

            // Add mods to the base items, fix ids
            foreach (var target in targets)
            {
                // This has all the original id relations since we reset the id to the original after the splitStack
                var itemsClone = new List<Item> { _cloner.Clone(target) };
                // Here we generate a new id for the root item
                target.Id = _hashUtil.Generate();

                // Add cloned mods to root item array
                var clonedMods = _cloner.Clone(mods);
                foreach (var mod in clonedMods)
                {
                    itemsClone.Add(mod);
                }

                // Re-parent items + generate new ids to ensure valid ids
                var itemsToAdd = _itemHelper.ReparentItemAndChildren(target, itemsClone);
                rewardItems.AddRange(itemsToAdd);
            }

            return rewardItems;
        }

        /**
         * Add missing mod items to an armor reward
         * @param originalRewardRootItem Original armor reward item from IReward.items object
         * @param reward Armor reward
         */
        protected void GenerateArmorRewardChildSlots(Item originalRewardRootItem, Reward reward)
        {
            // Look for a default preset from globals for armor
            var defaultPreset = _presetHelper.GetDefaultPreset(originalRewardRootItem.Template);
            if (defaultPreset is not null)
            {
                // Found preset, use mods to hydrate reward item
                var presetAndMods = _itemHelper.ReplaceIDs(defaultPreset.Items);
                var newRootId = _itemHelper.RemapRootItemId(presetAndMods);

                reward.Items = presetAndMods;

                // Find root item and set its stack count
                var rootItem = reward.Items.FirstOrDefault((item) => item.Id == newRootId);

                // Remap target id to the new presets root id
                reward.Target = rootItem.Id;

                // Copy over stack count otherwise reward shows as missing in client
                _itemHelper.AddUpdObjectToItem(rootItem);
                rootItem.Upd.StackObjectsCount = originalRewardRootItem.Upd.StackObjectsCount;
                return;
            }

            _logger.Warning(
                "Unable to find default preset for armor {originalRewardRootItem._tpl}, adding mods manually"
            );
            var itemDbData = _itemHelper.GetItem(originalRewardRootItem.Template).Value;

            // Hydrate reward with only 'required' mods - necessary for things like helmets otherwise you end up with nvgs/visors etc
            reward.Items = _itemHelper.AddChildSlotItems(reward.Items, itemDbData, null, true);
        }

        /**
             * Add an achievement to player profile and handle any rewards for the achievement
             * Triggered from a quest, or another achievement
             * @param fullProfile Profile to add achievement to
             * @param achievementId Id of achievement to add
             */
        public void AddAchievementToProfile(SptProfile fullProfile, string achievementId)
        {
            // Add achievement id to profile with timestamp it was unlocked
            fullProfile.CharacterData.PmcData.Achievements[achievementId] = _timeUtil.GetTimeStamp();

            // Check for any customisation unlocks
            var achievementDataDb = _databaseService
                .GetTemplates()
                .Achievements.FirstOrDefault((achievement) => achievement.Id == achievementId);
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
            ApplyRewards(
                achievementDataDb.Rewards,
                CustomisationSource.ACHIEVEMENT,
                fullProfile,
                pmcProfile,
                achievementDataDb.Id
            );
        }
    }
}
