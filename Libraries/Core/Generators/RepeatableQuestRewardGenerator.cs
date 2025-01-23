using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Player;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Repeatable;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using System.Linq;

namespace Core.Generators;

[Injectable]
public class RepeatableQuestRewardGenerator(
    ISptLogger<RepeatableQuestRewardGenerator> _logger,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    MathUtil _mathUtil,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    PresetHelper _presetHelper,
    HandbookHelper _handbookHelper,
    LocalisationService _localisationService,
    ItemFilterService _itemFilterService,
    SeasonalEventService _seasonalEventService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();

    /**
     * Generate the reward for a mission. A reward can consist of:
     * - Experience
     * - Money
     * - GP coins
     * - Weapon preset
     * - Items
     * - Trader Reputation
     * - Skill level experience
     *
     * The reward is dependent on the player level as given by the wiki. The exact mapping of pmcLevel to
     * experience / money / items / trader reputation can be defined in QuestConfig.js
     *
     * There's also a random variation of the reward the spread of which can be also defined in the config
     *
     * Additionally, a scaling factor w.r.t. quest difficulty going from 0.2...1 can be used
     * @param pmcLevel Level of player reward is being generated for
     * @param difficulty Reward scaling factor from 0.2 to 1
     * @param traderId Trader reward will be given by
     * @param repeatableConfig Config for quest type (daily, weekly)
     * @param questConfig
     * @param rewardTplBlacklist OPTIONAL: list of tpls to NOT use when picking a reward
     * @returns IQuestRewards
     */
    public QuestRewards GenerateReward(
        int pmcLevel,
        double difficulty,
        string traderId,
        RepeatableQuestConfig repeatableConfig,
        BaseQuestConfig eliminationConfig,
        List<string>? rewardTplBlacklist = null)
    {
        // Get vars to configure rewards with
        var rewardParams = GetQuestRewardValues(repeatableConfig.RewardScaling, difficulty, pmcLevel);

        // Get budget to spend on item rewards (copy of raw roubles given)
        var itemRewardBudget = rewardParams.RewardRoubles;

        // Possible improvement -> draw trader-specific items e.g. with _itemHelper.isOfBaseclass(val._id, ItemHelper.BASECLASS.FoodDrink)
        QuestRewards rewards = new() { Started = [], Success = [], Fail = [] };

        // Start reward index to keep track
        var rewardIndex = -1;

        // Add xp reward
        if (rewardParams.RewardXP > 0)
        {
            rewards.Success.Add(
                new()
                {
                    Id = _hashUtil.Generate(),
                    Unknown = false,
                    GameMode = [],
                    AvailableInGameEditions = [],
                    Index = rewardIndex,
                    Value = rewardParams.RewardXP,
                    Type = RewardType.Experience
                }
            );
            rewardIndex++;
        }

        // Add money reward
        rewards.Success.Add(GetMoneyReward(traderId, rewardParams.RewardRoubles.Value, rewardIndex));
        rewardIndex++;

        // Add GP coin reward
        rewards.Success.Add(GenerateItemReward(Money.GP, rewardParams.GpCoinRewardCount.Value, rewardIndex));
        rewardIndex++;

        // Add preset weapon to reward if checks pass
        var traderWhitelistDetails = repeatableConfig.TraderWhitelist.FirstOrDefault(
            (traderWhitelist) => traderWhitelist.TraderId == traderId
        );
        if (traderWhitelistDetails?.RewardCanBeWeapon ?? false && _randomUtil.GetChance100(traderWhitelistDetails.WeaponRewardChancePercent ?? 0)
           )
        {
            var chosenWeapon = GetRandomWeaponPresetWithinBudget(itemRewardBudget, rewardIndex);
            if (chosenWeapon is not null)
            {
                rewards.Success.Add(chosenWeapon.Value.Key);

                // Subtract price of preset from item budget so we dont give player too much stuff
                itemRewardBudget -= (int)chosenWeapon.Value.Value;
                rewardIndex++;
            }
        }

        var inBudgetRewardItemPool = ChooseRewardItemsWithinBudget(repeatableConfig, itemRewardBudget, traderId);
        if (rewardTplBlacklist is not null)
        {
            // Filter reward pool of items from blacklist, only use if there's at least 1 item remaining
            var filteredRewardItemPool = inBudgetRewardItemPool.Where(
                (item) => !rewardTplBlacklist.Contains(item.Id)
            );
            if (filteredRewardItemPool.Count() > 0)
            {
                inBudgetRewardItemPool = filteredRewardItemPool.ToList();
            }
        }


        _logger.Debug(
            $"Generating: {repeatableConfig.Name} quest for: {traderId} with budget: {itemRewardBudget} totalling: {rewardParams.RewardNumItems} items"
        );
        if (inBudgetRewardItemPool.Count > 0)
        {
            var itemsToReward = GetRewardableItemsFromPoolWithinBudget(
                inBudgetRewardItemPool,
                rewardParams.RewardNumItems,
                itemRewardBudget,
                repeatableConfig
            );

            // Add item rewards
            foreach (var itemReward in itemsToReward)
            {
                rewards.Success.Add(GenerateItemReward(itemReward.Key.Id, itemReward.Value, rewardIndex));
                rewardIndex++;
            }
        }

        // Add rep reward to rewards array
        if (rewardParams.RewardReputation > 0)
        {
            Reward reward = new()
            {
                Id = _hashUtil.Generate(),
                Unknown = false,
                GameMode = [],
                AvailableInGameEditions = [],
                Target = traderId,
                Value = rewardParams.RewardReputation,
                Type = RewardType.TraderStanding,
                Index = rewardIndex
            };
            rewards.Success.Add(reward);
            rewardIndex++;

            _logger.Debug($"Adding: {rewardParams.RewardReputation} {traderId} trader reputation reward");
        }

        // Chance of adding skill reward
        if (_randomUtil.GetChance100((double)rewardParams.SkillRewardChance * 100))
        {
            var targetSkill = _randomUtil.GetArrayValue(eliminationConfig.PossibleSkillRewards);
            Reward reward = new()
            {
                Id = _hashUtil.Generate(),
                Unknown = false,
                GameMode = [],
                AvailableInGameEditions = [],
                Target = targetSkill,
                Value = rewardParams.SkillPointReward,
                Type = RewardType.Skill,
                Index = rewardIndex
            };
            rewards.Success.Add(reward);

            _logger.Debug($"Adding {rewardParams.SkillPointReward} skill points to {targetSkill}");
        }

        return rewards;
    }

    private QuestRewardValues GetQuestRewardValues(RewardScaling? rewardScaling, double? difficulty, int pmcLevel)
    {
        // difficulty could go from 0.2 ... -> for lowest difficulty receive 0.2*nominal reward
        var levelsConfig = rewardScaling.Levels;
        var roublesConfig = rewardScaling.Roubles;
        var gpCoinConfig = rewardScaling.GpCoins;
        var xpConfig = rewardScaling.Experience;
        var itemsConfig = rewardScaling.Items;
        var rewardSpreadConfig = rewardScaling.RewardSpread;
        var skillRewardChanceConfig = rewardScaling.SkillRewardChance;
        var skillPointRewardConfig = rewardScaling.SkillPointReward;
        var reputationConfig = rewardScaling.Reputation;

        var effectiveDifficulty = difficulty is null ? 1 : difficulty;
        if (difficulty is null)
        {
            _logger.Warning(_localisationService.GetText("repeatable-difficulty_was_nan"));
        }

        return new()
        {
            SkillPointReward = _mathUtil.Interp1(pmcLevel, levelsConfig, skillPointRewardConfig),
            SkillRewardChance = _mathUtil.Interp1(pmcLevel, levelsConfig, skillRewardChanceConfig),
            RewardReputation = GetRewardRep(effectiveDifficulty, pmcLevel, levelsConfig, reputationConfig, rewardSpreadConfig),
            RewardNumItems = GetRewardNumItems(pmcLevel, levelsConfig, itemsConfig),
            RewardRoubles = GetRewardRoubles(effectiveDifficulty, pmcLevel, levelsConfig, roublesConfig, rewardSpreadConfig),
            GpCoinRewardCount = GetGpCoinRewardCount(effectiveDifficulty, pmcLevel, levelsConfig, gpCoinConfig, rewardSpreadConfig),
            RewardXP = GetRewardXp(effectiveDifficulty, pmcLevel, levelsConfig, xpConfig, rewardSpreadConfig),
        };
    }

    private double GetRewardXp(double? effectiveDifficulty, int pmcLevel, List<double>? levelsConfig, List<double>? xpConfig, double? rewardSpreadConfig)
    {
        return Math.Floor(
            (effectiveDifficulty *
             _mathUtil.Interp1(pmcLevel, levelsConfig, xpConfig) *
             _randomUtil.GetFloat((float)(1 - rewardSpreadConfig), (float)(1 + rewardSpreadConfig))) ??
            0
        );
    }

    private double GetGpCoinRewardCount(double? effectiveDifficulty, int pmcLevel, List<double>? levelsConfig, List<double>? gpCoinConfig,
        double? rewardSpreadConfig)
    {
        return Math.Ceiling(
            (effectiveDifficulty *
             _mathUtil.Interp1(pmcLevel, levelsConfig, gpCoinConfig) *
             _randomUtil.GetFloat((float)(1 - rewardSpreadConfig), (float)(1 + rewardSpreadConfig))) ??
            0
        );
    }

    private double GetRewardRep(double? effectiveDifficulty, int pmcLevel, List<double>? levelsConfig, List<double>? reputationConfig,
        double? rewardSpreadConfig)
    {
        return Math.Round(
                   100 *
                   effectiveDifficulty *
                   _mathUtil.Interp1(pmcLevel, levelsConfig, reputationConfig) *
                   _randomUtil.GetFloat((float)(1 - rewardSpreadConfig), (float)(1 + rewardSpreadConfig)) ??
                   0
               ) /
               100;
    }

    private double GetRewardNumItems(int pmcLevel, List<double>? levelsConfig, List<double>? itemsConfig)
    {
        return _randomUtil.RandInt(1, (int)Math.Round(_mathUtil.Interp1(pmcLevel, levelsConfig, itemsConfig) ?? 0) + 1);
    }

    private double GetRewardRoubles(double? effectiveDifficulty, int pmcLevel, List<double>? levelsConfig, List<double>? roublesConfig,
        double? rewardSpreadConfig)
    {
        return Math.Floor(
            (effectiveDifficulty *
             _mathUtil.Interp1(pmcLevel, levelsConfig, roublesConfig) *
             _randomUtil.GetFloat((float)(1 - rewardSpreadConfig), (float)(1 + rewardSpreadConfig))) ??
            0
        );
    }

    private List<KeyValuePair<TemplateItem, double>> GetRewardableItemsFromPoolWithinBudget(List<TemplateItem> inBudgetRewardItemPool,
        object rewardNumItems, double? itemRewardBudget, RepeatableQuestConfig repeatableConfig)
    {
        throw new NotImplementedException();
    }

    private List<TemplateItem> ChooseRewardItemsWithinBudget(RepeatableQuestConfig repeatableConfig, double? roublesBudget, string traderId)
    {
        // First filter for type and baseclass to avoid lookup in handbook for non-available items
        var rewardableItemPool = GetRewardableItems(repeatableConfig, traderId);
        var minPrice = Math.Min(25000, 0.5 * roublesBudget.Value);

        var rewardableItemPoolWithinBudget = FilterRewardPoolWithinBudget(
            rewardableItemPool,
            roublesBudget.Value,
            minPrice);

        if (rewardableItemPoolWithinBudget.Count == 0)
        {
            _logger.Warning(_localisationService.GetText("repeatable-no_reward_item_found_in_price_range", new {
                minPrice = minPrice,
                roublesBudget = roublesBudget }));

            // In case we don't find any items in the price range
            rewardableItemPoolWithinBudget = rewardableItemPool
                .Where((x) => _itemHelper.GetItemPrice(x.Id) < roublesBudget)
                .ToList();
        }

        return rewardableItemPoolWithinBudget;
    }

    private List<TemplateItem> FilterRewardPoolWithinBudget(List<TemplateItem> rewardItems, double roublesBudget, double minPrice)
    {
        return rewardItems.Where((item) => {
            var itemPrice = _presetHelper.GetDefaultPresetOrItemPrice(item.Id);
            return itemPrice < roublesBudget && itemPrice > minPrice;
        }).ToList();
    }

    private KeyValuePair<Reward, double>? GetRandomWeaponPresetWithinBudget(double? itemRewardBudget, double rewardIndex)
    {
        throw new NotImplementedException();
    }

    private Reward GenerateItemReward(string tpl, double count, int index, bool foundInRaid = true)
    {
        var id = _hashUtil.Generate();
        var questRewardItem = new Reward{
            Id = _hashUtil.Generate(),
            Unknown = false,
            GameMode = [],
            AvailableInGameEditions = [],
            Index = index,
            Target = id,
            Value = count,
            IsEncoded = false,
            FindInRaid = foundInRaid,
            Type = RewardType.Item,
            Items = [],
        };

        var rootItem = new Item { Id = id, Template = tpl, Upd = new Upd { StackObjectsCount = count, SpawnedInSession = foundInRaid }
        };
        questRewardItem.Items = [rootItem];

        return questRewardItem;
    }

    private Reward GetMoneyReward(string traderId, double rewardRoubles, int rewardIndex)
    {
        // Determine currency based on trader
        // PK and Fence use Euros, everyone else is Roubles
        var currency = traderId is Traders.PEACEKEEPER or Traders.FENCE ? Money.EUROS : Money.ROUBLES;

        // Convert reward amount to Euros if necessary
        var rewardAmountToGivePlayer =
        currency == Money.EUROS ? _handbookHelper.FromRUB(rewardRoubles, Money.EUROS) : rewardRoubles;

        // Get chosen currency + amount and return
        return GenerateItemReward(currency, rewardAmountToGivePlayer, rewardIndex, false);
    }


    public List<TemplateItem> GetRewardableItems(RepeatableQuestConfig repeatableQuestConfig, string traderId)
    {
        // Get an array of seasonal items that should not be shown right now as seasonal event is not active
        var seasonalItems = _seasonalEventService.GetInactiveSeasonalEventItems();

        // Check for specific baseclasses which don't make sense as reward item
        // also check if the price is greater than 0; there are some items whose price can not be found
        // those are not in the game yet (e.g. AGS grenade launcher)
        return _databaseService.GetItems().Values.Where((itemTemplate => {
            // Base "Item" item has no parent, ignore it
            if (itemTemplate.Parent == "")
            {
                return false;
            }

            if (seasonalItems.Contains(itemTemplate.Id))
            {
                return false;
            }

            var traderWhitelist = repeatableQuestConfig.TraderWhitelist.FirstOrDefault(
                (trader) => trader.TraderId == traderId);

            return IsValidRewardItem(itemTemplate.Id, repeatableQuestConfig, traderWhitelist?.RewardBaseWhitelist);
        })).ToList();
    }

    private bool IsValidRewardItem(string tpl, RepeatableQuestConfig repeatableQuestConfig, List<string>? itemBaseWhitelist = null)
    {
        // Return early if not valid item to give as reward
        if (!_itemHelper.isValidItem(tpl))
        {
            return false;
        }

        // Check item is not blacklisted
        if (
            _itemFilterService.IsItemBlacklisted(tpl) ||
            _itemFilterService.IsItemRewardBlacklisted(tpl) ||
            repeatableQuestConfig.RewardBlacklist.Contains(tpl) ||
            _itemFilterService.IsItemBlacklisted(tpl)
        )
        {
            return false;
        }

        // Item has blacklisted base types
        if (_itemHelper.IsOfBaseclasses(tpl, repeatableQuestConfig.RewardBaseTypeBlacklist ))
        {
            return false;
        }

        // Skip boss items
        if (_itemFilterService.IsBossItem(tpl))
        {
            return false;
        }

        // Trader has specific item base types they can give as rewards to player
        if (itemBaseWhitelist is not null && !_itemHelper.IsOfBaseclasses(tpl, itemBaseWhitelist))
        {
            return false;
        }

        return true;
    }
}
