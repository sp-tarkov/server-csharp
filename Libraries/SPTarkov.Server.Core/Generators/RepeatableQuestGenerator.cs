using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Repeatable;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Collections;
using SPTarkov.Server.Core.Utils.Json;
using BodyParts = SPTarkov.Server.Core.Constants.BodyParts;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class RepeatableQuestGenerator(
    ISptLogger<RepeatableQuestGenerator> _logger,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    MathUtil _mathUtil,
    RepeatableQuestHelper _repeatableQuestHelper,
    ItemHelper _itemHelper,
    RepeatableQuestRewardGenerator _repeatableQuestRewardGenerator,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    /// <summary>
    /// Body parts to present to the client as opposed to the body part information in quest data.
    /// </summary>
    private static readonly Dictionary<string, List<string>> _bodyPartsToClient = new()
    {
        {
            BodyParts.Arms, [
                BodyParts.LeftArm,
                BodyParts.RightArm
            ]
        },
        {
            BodyParts.Legs, [
                BodyParts.LeftLeg,
                BodyParts.RightLeg
            ]
        },
        {
            BodyParts.Head, [
                BodyParts.Head
            ]
        },
        {
            BodyParts.Chest, [
                BodyParts.Chest,
                BodyParts.Stomach
            ]
        },
    };

    protected int _maxRandomNumberAttempts = 6;
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();

    /// <summary>
    ///     This method is called by /GetClientRepeatableQuests/ and creates one element of quest type format (see
    ///     assets/database/templates/repeatableQuests.json).
    ///     It randomly draws a quest type (currently Elimination, Completion or Exploration) as well as a trader who is
    ///     providing the quest
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="pmcTraderInfo">Players trader standing/rep levels</param>
    /// <param name="questTypePool">Possible quest types pool</param>
    /// <param name="repeatableConfig">Repeatable quest config</param>
    /// <returns>RepeatableQuest</returns>
    public RepeatableQuest? GenerateRepeatableQuest(
        string sessionId,
        int pmcLevel,
        Dictionary<string, TraderInfo> pmcTraderInfo,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var questType = _randomUtil.DrawRandomFromList(questTypePool.Types).First();

        // Get traders from whitelist and filter by quest type availability
        var traders = repeatableConfig.TraderWhitelist
            .Where(x =>
            {
                return x.QuestTypes.Contains(questType);
            })
            .Select(x =>
            {
                return x.TraderId;
            })
            .ToList();
        // filter out locked traders
        traders = traders.Where(x =>
        {
            return pmcTraderInfo[x].Unlocked.GetValueOrDefault(false);
        }).ToList();
        var traderId = _randomUtil.DrawRandomFromList(traders).FirstOrDefault();

        return questType switch
        {
            "Elimination" => GenerateEliminationQuest(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            "Completion" => GenerateCompletionQuest(sessionId, pmcLevel, traderId, repeatableConfig),
            "Exploration" => GenerateExplorationQuest(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            "Pickup" => GeneratePickupQuest(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            _ => null
        };
    }

    /// <summary>
    ///     Generate a randomised Elimination quest
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">
    ///     The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig
    ///     for the requestd quest
    /// </param>
    /// <returns>Object of quest type format for "Elimination" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest GenerateEliminationQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var rand = new Random();

        var eliminationConfig = _repeatableQuestHelper.GetEliminationConfigByPmcLevel(pmcLevel, repeatableConfig);
        var locationsConfig = repeatableConfig.Locations;
        var targetsConfig = new ProbabilityObjectArray<string, BossInfo>(_mathUtil, _cloner, eliminationConfig.Targets);
        var bodyPartsConfig = new ProbabilityObjectArray<string, List<string>>(_mathUtil, _cloner, eliminationConfig.BodyParts);
        var weaponCategoryRequirementConfig =
            new ProbabilityObjectArray<string, List<string>>(_mathUtil, _cloner, eliminationConfig.WeaponCategoryRequirements);
        var weaponRequirementConfig = new ProbabilityObjectArray<string, List<string>>(_mathUtil, _cloner, eliminationConfig.WeaponRequirements);

        // the difficulty of the quest varies in difficulty depending on the condition
        // possible conditions are
        // - amount of npcs to kill
        // - type of npc to kill (scav, boss, pmc)
        // - with hit to what body part they should be killed
        // - from what distance they should be killed
        // a random combination of listed conditions can be required
        // possible conditions elements and their relative probability can be defined in QuestConfig.js
        // We use ProbabilityObjectArray to draw by relative probability. e.g. for targets:
        // "targets": {
        //    "Savage": 7,
        //    "AnyPmc": 2,
        //    "bossBully": 0.5
        // }
        // higher is more likely. We define the difficulty to be the inverse of the relative probability.

        // We want to generate a reward which is scaled by the difficulty of this mission. To get a upper bound with which we scale
        // the actual difficulty we calculate the minimum and maximum difficulty (max being the sum of max of each condition type
        // times the number of kills we have to perform):

        // the minimum difficulty is the difficulty for the most probable (= easiest target) with no additional conditions
        var minDifficulty =
            1 / targetsConfig.MaxProbability(); // min difficulty is lowest amount of scavs without any constraints

        // Target on bodyPart max. difficulty is that of the least probable element
        var maxTargetDifficulty = 1 / targetsConfig.MinProbability();
        var maxBodyPartsDifficulty = eliminationConfig.MinKills / bodyPartsConfig.MinProbability();

        // maxDistDifficulty is defined by 2, this could be a tuning parameter if we don't like the reward generation
        const int maxDistDifficulty = 2;

        var maxKillDifficulty = eliminationConfig.MaxKills;

        var targetPool = questTypePool.Pool.Elimination;
        targetsConfig = targetsConfig.Filter(x =>
        {
            return questTypePool.Pool.Elimination.Targets.ContainsKey(x.Key);
        });

        if (targetsConfig.Count == 0 || targetsConfig.All(x =>
        {
            return x.Data.IsBoss.GetValueOrDefault(false);
        }))
        {
            // There are no more targets left for elimination; delete it as a possible quest type
            // also if only bosses are left we need to leave otherwise it's a guaranteed boss elimination
            // -> then it would not be a quest with low probability anymore
            questTypePool.Types = questTypePool.Types.Where(t =>
            {
                return t != "Elimination";
            }).ToList();
            return null;
        }

        var botTypeToEliminate = targetsConfig.Draw()[0];
        var targetDifficulty = 1 / targetsConfig.Probability(botTypeToEliminate);

        questTypePool.Pool.Elimination.Targets.TryGetValue(botTypeToEliminate, out var targetLocationPool);
        var locations = targetLocationPool.Locations;

        // we use any as location if "any" is in the pool and we don't hit the specific location random
        // we use any also if the random condition is not met in case only "any" was in the pool
        var locationKey = "any";
        if (locations.Contains("any") &&
            (eliminationConfig.SpecificLocationProbability < rand.NextDouble() || locations.Count <= 1)
           )
        {
            locationKey = "any";
            questTypePool.Pool.Elimination.Targets.Remove(botTypeToEliminate);
        }
        else
        {
            // Specific location
            locations = locations.Where(l =>
            {
                return l != "any";
            }).ToList();
            if (locations.Count > 0)
            {
                // Get name of location we want elimination to occur on
                locationKey = _randomUtil.DrawRandomFromList(locations).FirstOrDefault();

                // Get a pool of locations the chosen bot type can be eliminated on
                if (!questTypePool.Pool.Elimination.Targets.TryGetValue(
                        botTypeToEliminate,
                        out var possibleLocationPool
                    ))
                {
                    _logger.Warning($"Bot to kill: {botTypeToEliminate} not found in elimination dict");
                }

                // Filter locations bot can be killed on to just those not chosen by key
                possibleLocationPool.Locations = possibleLocationPool.Locations
                    .Where(location =>
                    {
                        return location != locationKey;
                    })
                    .ToList();

                // None left after filtering
                if (possibleLocationPool.Locations.Count == 0)
                {
                    // TODO: Why do any of this?!
                    // Remove chosen bot to eliminate from pool
                    questTypePool.Pool.Elimination.Targets.Remove(botTypeToEliminate);
                }
            }
            else
            {
                // never should reach this if everything works out
                _logger.Error("Encountered issue when creating Elimination quest. Please report.");
            }
        }

        // draw the target body part and calculate the difficulty factor
        var bodyPartsToClient = new List<string>();
        var bodyPartDifficulty = 0d;
        if (eliminationConfig.BodyPartProbability > rand.NextDouble())
        {
            // if we add a bodyPart condition, we draw randomly one or two parts
            // each bodyPart of the BODYPARTS ProbabilityObjectArray includes the string(s) which need to be presented to the client in ProbabilityObjectArray.data
            // e.g. we draw "Arms" from the probability array but must present ["LeftArm", "RightArm"] to the client
            bodyPartsToClient = [];
            var bodyParts = bodyPartsConfig.Draw(_randomUtil.RandInt(1, 3), false);
            double probability = 0;
            foreach (var bodyPart in bodyParts)
            {
                // more than one part lead to an "OR" condition hence more parts reduce the difficulty
                probability += bodyPartsConfig.Probability(bodyPart).Value;

                if (_bodyPartsToClient.TryGetValue(bodyPart, out var bodyPartListToClient))
                {
                    bodyPartsToClient.AddRange(bodyPartListToClient);
                }
                else
                {
                    bodyPartsToClient.Add(bodyPart);
                }
            }

            bodyPartDifficulty = 1 / probability;
        }

        // Draw a distance condition
        int? distance = null;
        var distanceDifficulty = 0;
        var isDistanceRequirementAllowed = !eliminationConfig.DistLocationBlacklist.Contains(locationKey);

        if (targetsConfig.Data(botTypeToEliminate).IsBoss.GetValueOrDefault(false))
        {
            // Get all boss spawn information
            var bossSpawns = _databaseService.GetLocations()
                .GetDictionary()
                .Select(x =>
                {
                    return x.Value;
                })
                .Where(x =>
                {
                    return x.Base?.Id != null;
                })
                .Select(x =>
                {
                    return new
                    {
                        x.Base.Id,
                        BossSpawn = x.Base.BossLocationSpawn
                    };
                });
            // filter for the current boss to spawn on map
            var thisBossSpawns = bossSpawns
                .Select(x =>
                {
                    return new
                    {
                        x.Id,
                        BossSpawn = x.BossSpawn
                                                .Where(e =>
                                                {
                                                    return e.BossName == botTypeToEliminate;
                                                })
                    };
                })
                .Where(x =>
                {
                    return x.BossSpawn.Count() > 0;
                });
            // remove blacklisted locations
            var allowedSpawns = thisBossSpawns.Where(x =>
            {
                return !eliminationConfig.DistLocationBlacklist.Contains(x.Id);
            });
            // if the boss spawns on nom-blacklisted locations and the current location is allowed we can generate a distance kill requirement
            isDistanceRequirementAllowed = isDistanceRequirementAllowed && allowedSpawns.Count() > 0;
        }

        if (eliminationConfig.DistanceProbability > rand.NextDouble() && isDistanceRequirementAllowed)
        {
            // Random distance with lower values more likely; simple distribution for starters...
            distance = (int) Math.Floor(
                Math.Abs(rand.NextDouble() - rand.NextDouble()) *
                (1 + eliminationConfig.MaxDistance - eliminationConfig.MinDistance) +
                eliminationConfig.MinDistance ??
                0
            );

            distance = (int) Math.Ceiling((decimal) (distance / 5)) * 5;
            distanceDifficulty = (int) (maxDistDifficulty * distance / eliminationConfig.MaxDistance);
        }

        string? allowedWeaponsCategory = null;
        if (eliminationConfig.WeaponCategoryRequirementProbability > rand.NextDouble())
        {
            // Filter out close range weapons from far distance requirement
            if (distance > 50)
            {
                List<string> weaponTypeBlacklist = ["Shotgun", "Pistol"];

                // Filter out close range weapons from long distance requirement
                weaponCategoryRequirementConfig
                    .RemoveAll(category =>
                    {
                        return weaponTypeBlacklist
                                                .Contains(category.Key);
                    });
            }
            else if (distance < 20)
            {
                List<string> weaponTypeBlacklist = ["MarksmanRifle", "DMR"];

                // Filter out far range weapons from close distance requirement
                weaponCategoryRequirementConfig
                    .RemoveAll(category =>
                    {
                        return weaponTypeBlacklist
                                                .Contains(category.Key);
                    });
            }

            // Pick a weighted weapon category
            var weaponRequirement = weaponCategoryRequirementConfig.Draw(1, false);

            // Get the hideout id value stored in the .data array
            allowedWeaponsCategory = weaponCategoryRequirementConfig.Data(weaponRequirement[0])[0];
        }

        // Only allow a specific weapon requirement if a weapon category was not chosen
        string? allowedWeapon = null;
        if (allowedWeaponsCategory is not null && eliminationConfig.WeaponRequirementProbability > rand.NextDouble())
        {
            var weaponRequirement = weaponRequirementConfig.Draw(1, false);
            var specificAllowedWeaponCategory = weaponRequirementConfig.Data(weaponRequirement[0]);
            var allowedWeapons = _itemHelper.GetItemTplsOfBaseType(specificAllowedWeaponCategory[0]);
            allowedWeapon = _randomUtil.GetArrayValue(allowedWeapons);
        }

        // Draw how many npm kills are required
        var desiredKillCount = GetEliminationKillCount(botTypeToEliminate, targetsConfig, eliminationConfig);
        var killDifficulty = desiredKillCount;

        // not perfectly happy here; we give difficulty = 1 to the quest reward generation when we have the most difficult mission
        // e.g. killing reshala 5 times from a distance of 200m with a headshot.
        var maxDifficulty = DifficultyWeighing(1, 1, 1, 1, 1);
        var curDifficulty = DifficultyWeighing(
            targetDifficulty.Value / maxTargetDifficulty,
            bodyPartDifficulty / maxBodyPartsDifficulty.Value,
            distanceDifficulty / maxDistDifficulty,
            killDifficulty / maxKillDifficulty.Value,
            allowedWeaponsCategory is not null || allowedWeapon is not null ? 1 : 0
        );

        // Aforementioned issue makes it a bit crazy since now all easier quests give significantly lower rewards than Completion / Exploration
        // I therefore moved the mapping a bit up (from 0.2...1 to 0.5...2) so that normal difficulty still gives good reward and having the
        // crazy maximum difficulty will lead to a higher difficulty reward gain factor than 1
        var difficulty = _mathUtil.MapToRange(curDifficulty, minDifficulty, maxDifficulty, 0.5, 2);

        var quest = GenerateRepeatableTemplate("Elimination", traderId, repeatableConfig.Side, sessionId);

        // ASSUMPTION: All fence quests are for scavs
        if (traderId == Traders.FENCE)
        {
            quest.Side = "Scav";
        }

        var availableForFinishCondition = quest.Conditions.AvailableForFinish[0];
        availableForFinishCondition.Counter.Id = _hashUtil.Generate();
        availableForFinishCondition.Counter.Conditions = [];

        // Only add specific location condition if specific map selected
        if (locationKey != "any")
        {
            var locationId = Enum.Parse<ELocationName>(locationKey);
            availableForFinishCondition.Counter.Conditions.Add(
                GenerateEliminationLocation(locationsConfig[locationId])
            );
        }

        availableForFinishCondition.Counter.Conditions.Add(
            GenerateEliminationCondition(
                botTypeToEliminate,
                bodyPartsToClient,
                distance,
                allowedWeapon,
                allowedWeaponsCategory
            )
        );
        availableForFinishCondition.Value = desiredKillCount;
        availableForFinishCondition.Id = _hashUtil.Generate();
        quest.Location = GetQuestLocationByMapId(locationKey);

        quest.Rewards = _repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            Math.Min(difficulty, 1),
            traderId,
            repeatableConfig,
            eliminationConfig
        );

        return quest;
    }

    /// <summary>
    ///     Get a number of kills needed to complete elimination quest
    /// </summary>
    /// <param name="targetKey"> Target type desired e.g. anyPmc/bossBully/Savage </param>
    /// <param name="targetsConfig"> Config of the target </param>
    /// <param name="eliminationConfig"> Config of the elimination </param>
    /// <returns> Number of AI to kill </returns>
    protected int GetEliminationKillCount(
        string targetKey,
        ProbabilityObjectArray<string, BossInfo> targetsConfig,
        EliminationConfig eliminationConfig)
    {
        if (targetsConfig.Data(targetKey).IsBoss.GetValueOrDefault(false))
        {
            return _randomUtil.RandInt(eliminationConfig.MinBossKills.Value, eliminationConfig.MaxBossKills + 1);
        }

        if (targetsConfig.Data(targetKey).IsPmc.GetValueOrDefault(false))
        {
            return _randomUtil.RandInt(eliminationConfig.MinPmcKills.Value, eliminationConfig.MaxPmcKills + 1);
        }

        return _randomUtil.RandInt(eliminationConfig.MinKills.Value, eliminationConfig.MaxKills + 1);
    }

    protected double DifficultyWeighing(
        double target,
        double bodyPart,
        int dist,
        int kill,
        int weaponRequirement)
    {
        return Math.Sqrt(Math.Sqrt(target) + bodyPart + dist + weaponRequirement) * kill;
    }

    /// <summary>
    ///     A repeatable quest, besides some more or less static components, exists of reward and condition (see
    ///     assets/database/templates/repeatableQuests.json)
    ///     This is a helper method for GenerateEliminationQuest to create a location condition.
    /// </summary>
    /// <param name="location">the location on which to fulfill the elimination quest</param>
    /// <returns>Elimination-location-subcondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationLocation(List<string> location)
    {
        return new QuestConditionCounterCondition
        {
            Id = _hashUtil.Generate(),
            DynamicLocale = true,
            Target = new ListOrT<string>(location, null),
            ConditionType = "Location"
        };
    }

    /// <summary>
    ///     Create kill condition for an elimination quest
    /// </summary>
    /// <param name="target">Bot type target of elimination quest e.g. "AnyPmc", "Savage"</param>
    /// <param name="targetedBodyParts">Body parts player must hit</param>
    /// <param name="distance">Distance from which to kill (currently only >= supported)</param>
    /// <param name="allowedWeapon">What weapon must be used - undefined = any</param>
    /// <param name="allowedWeaponCategory">What category of weapon must be used - undefined = any</param>
    /// <returns>EliminationCondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationCondition(
        string target,
        List<string>? targetedBodyParts,
        double? distance,
        string? allowedWeapon,
        string? allowedWeaponCategory
    )
    {
        var killConditionProps = new QuestConditionCounterCondition
        {
            Id = _hashUtil.Generate(),
            DynamicLocale = true,
            Target = new ListOrT<string>(null, target), // e,g, "AnyPmc"
            Value = 1,
            ResetOnSessionEnd = false,
            EnemyHealthEffects = [],
            Daytime = new DaytimeCounter
            {
                From = 0,
                To = 0
            },
            ConditionType = "Kills"
        };

        if (target.StartsWith("boss"))
        {
            killConditionProps.Target = new ListOrT<string>(null, "Savage");
            killConditionProps.SavageRole = [target];
        }

        // Has specific body part hit condition
        if (targetedBodyParts is not null)
        {
            killConditionProps.BodyPart = targetedBodyParts;
        }

        // Don't allow distance + melee requirement
        if (distance is not null && allowedWeaponCategory != "5b5f7a0886f77409407a7f96")
        {
            killConditionProps.Distance = new CounterConditionDistance
            {
                CompareMethod = ">=",
                Value = distance.Value
            };
        }

        // Has specific weapon requirement
        if (allowedWeapon is not null)
        {
            killConditionProps.Weapon = [allowedWeapon];
        }

        // Has specific weapon category requirement
        if (allowedWeaponCategory?.Length > 0)
        {
            // TODO - fix - does weaponCategories exist?
            // killConditionProps.weaponCategories = [allowedWeaponCategory];
        }

        return killConditionProps;
    }

    /// <summary>
    ///     Generates a valid Completion quest
    /// </summary>
    /// <param name="pmcLevel">player's level for requested items and reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="repeatableConfig">
    ///     The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig
    ///     for the requested quest
    /// </param>
    /// <returns>quest type format for "Completion" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest? GenerateCompletionQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var completionConfig = repeatableConfig.QuestConfig.Completion;
        var levelsConfig = repeatableConfig.RewardScaling.Levels;
        var roublesConfig = repeatableConfig.RewardScaling.Roubles;

        var quest = GenerateRepeatableTemplate("Completion", traderId, repeatableConfig.Side, sessionId);

        // Filter the items.json items to items the player must retrieve to complete quest: shouldn't be a quest item or "non-existant"
        var possibleItemsToRetrievePool = _repeatableQuestRewardGenerator.GetRewardableItems(
            repeatableConfig,
            traderId
        );

        // Be fair, don't var the items be more expensive than the reward
        var multi = _randomUtil.GetDouble(0.5, 1);
        var roublesBudget = Math.Floor(
            (double) (_mathUtil.Interp1(pmcLevel, levelsConfig, roublesConfig) * multi)
        );
        roublesBudget = Math.Max(roublesBudget, 5000d);
        var itemSelection = possibleItemsToRetrievePool.Where(x =>
        {
            return _itemHelper.GetItemPrice(x.Id) < roublesBudget;
        })
            .ToList();

        // We also have the option to use whitelist and/or blacklist which is defined in repeatableQuests.json as
        // [{"minPlayerLevel": 1, "itemIds": ["id1",...]}, {"minPlayerLevel": 15, "itemIds": ["id3",...]}]
        if (repeatableConfig.QuestConfig.Completion.UseWhitelist.GetValueOrDefault(false))
        {
            var itemWhitelist = _databaseService.GetTemplates().RepeatableQuests.Data.Completion.ItemsWhitelist;

            // Filter and concatenate the arrays according to current player level
            var itemIdsWhitelisted = itemWhitelist
                .Where(p =>
                {
                    return p.MinPlayerLevel <= pmcLevel;
                })
                .SelectMany(x =>
                {
                    return x.ItemIds;
                })
                .ToHashSet(); //.Aggregate((a, p) => a.Concat(p.ItemIds), []);
            itemSelection = itemSelection.Where(x =>
                    {
                        // Whitelist can contain item tpls and item base type ids
                        return itemIdsWhitelisted.Any(v =>
                        {
                            return _itemHelper.IsOfBaseclass(x.Id, v);
                        }) ||
                               itemIdsWhitelisted.Contains(x.Id);
                    }
                )
                .ToList();
            // check if items are missing
            // var flatList = itemSelection.reduce((a, il) => a.concat(il[0]), []);
            // var missing = itemIdsWhitelisted.filter(l => !flatList.includes(l));
        }

        if (repeatableConfig.QuestConfig.Completion.UseBlacklist.GetValueOrDefault(false))
        {
            var itemBlacklist = _databaseService.GetTemplates().RepeatableQuests.Data.Completion.ItemsBlacklist;

            // we filter and concatenate the arrays according to current player level
            var itemIdsBlacklisted = itemBlacklist
                .Where(p =>
                {
                    return p.MinPlayerLevel <= pmcLevel;
                })
                .SelectMany(x =>
                {
                    return x.ItemIds;
                })
                .ToHashSet(); //.Aggregate(List<ItemsBlacklist> , (a, p) => a.Concat(p.ItemIds) );

            itemSelection = itemSelection.Where(x =>
                    {
                        return itemIdsBlacklisted.All(v =>
                        {
                            return !_itemHelper.IsOfBaseclass(x.Id, v);
                        }) ||
                               !itemIdsBlacklisted.Contains(x.Id);
                    }
                )
                .ToList();
        }

        if (!itemSelection.Any())
        {
            _logger.Error(
                _localisationService.GetText(
                    "repeatable-completion_quest_whitelist_too_small_or_blacklist_too_restrictive"
                )
            );

            return null;
        }

        // Draw items to ask player to retrieve
        var isAmmo = 0;

        // Store the indexes of items we are asking player to provide
        var distinctItemsToRetrieveCount = _randomUtil.GetInt(1, completionConfig.UniqueItemCount.Value);
        var chosenRequirementItemsTpls = new List<string>();
        var usedItemIndexes = new HashSet<int>();
        for (var i = 0; i < distinctItemsToRetrieveCount; i++)
        {
            var chosenItemIndex = _randomUtil.RandInt(itemSelection.Count);
            var found = false;

            for (var j = 0; j < _maxRandomNumberAttempts; j++)
            {
                if (usedItemIndexes.Contains(chosenItemIndex))
                {
                    chosenItemIndex = _randomUtil.RandInt(itemSelection.Count);
                }
                else
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "repeatable-no_reward_item_found_in_price_range",
                        new
                        {
                            minPrice = 0,
                            roublesBudget
                        }
                    )
                );

                return null;
            }

            usedItemIndexes.Add(chosenItemIndex);

            var itemSelected = itemSelection[chosenItemIndex];
            var itemUnitPrice = _itemHelper.GetItemPrice(itemSelected.Id).Value;
            var minValue = completionConfig.MinimumRequestedAmount.Value;
            var maxValue = completionConfig.MaximumRequestedAmount.Value;
            if (_itemHelper.IsOfBaseclass(itemSelected.Id, BaseClasses.AMMO))
            {
                // Prevent multiple ammo requirements from being picked
                if (isAmmo > 0 && isAmmo < _maxRandomNumberAttempts)
                {
                    isAmmo++;
                    i--;

                    continue;
                }

                isAmmo++;
                minValue = completionConfig.MinimumRequestedBulletAmount.Value;
                maxValue = completionConfig.MaximumRequestedBulletAmount.Value;
            }

            var value = minValue;

            // Get the value range within budget
            var x = (int) Math.Floor(roublesBudget / itemUnitPrice);
            maxValue = Math.Min(maxValue, x);
            if (maxValue > minValue)
            // If it doesn't blow the budget we have for the request, draw a random amount of the selected
            // Item type to be requested
            {
                value = _randomUtil.RandInt(minValue, maxValue + 1);
            }

            roublesBudget -= value * itemUnitPrice;

            // Push a CompletionCondition with the item and the amount of the item
            chosenRequirementItemsTpls.Add(itemSelected.Id);
            quest.Conditions.AvailableForFinish.Add(GenerateCompletionAvailableForFinish(itemSelected.Id, value, repeatableConfig.QuestConfig.Completion));

            if (roublesBudget > 0)
            {
                // Reduce the list possible items to fulfill the new budget constraint
                itemSelection = itemSelection.Where(dbItem =>
                {
                    return _itemHelper.GetItemPrice(dbItem.Id) < roublesBudget;
                })
                    .ToList();
                if (!itemSelection.Any())
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        quest.Rewards = _repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            1,
            traderId,
            repeatableConfig,
            completionConfig,
            chosenRequirementItemsTpls
        );

        return quest;
    }

    /// <summary>
    ///     A repeatable quest, besides some more or less static components, exists of reward and condition (see
    ///     assets/database/templates/repeatableQuests.json)
    ///     This is a helper method for GenerateCompletionQuest to create a completion condition (of which a completion quest
    ///     theoretically can have many)
    /// </summary>
    /// <param name="itemTpl">Id of the item to request</param>
    /// <param name="value">Amount of items of this specific type to request</param>
    /// <param name="completionConfig">Completion config from quest.json</param>
    /// <returns>object of "Completion"-condition</returns>
    protected QuestCondition GenerateCompletionAvailableForFinish(string itemTpl,
        double value,
        Completion completionConfig)
    {
        var onlyFoundInRaid = completionConfig.RequiredItemsAreFiR;
        var minDurability = _itemHelper.IsOfBaseclasses(itemTpl, [BaseClasses.WEAPON, BaseClasses.ARMOR])
            ? _randomUtil.GetArrayValue([completionConfig.RequiredItemMinDurabilityMinMax.Min, completionConfig.RequiredItemMinDurabilityMinMax.Max])
            : 0;

        // Dog tags MUST NOT be FiR for them to work
        if (_itemHelper.IsDogtag(itemTpl))
        {
            onlyFoundInRaid = false;
        }

        return new QuestCondition
        {
            Id = _hashUtil.Generate(),
            Index = 0,
            ParentId = "",
            DynamicLocale = true,
            VisibilityConditions = [],
            GlobalQuestCounterId = "",
            Target = new ListOrT<string>([itemTpl], null),
            Value = value,
            MinDurability = minDurability,
            MaxDurability = 100,
            DogtagLevel = 0,
            OnlyFoundInRaid = onlyFoundInRaid,
            IsEncoded = false,
            ConditionType = "HandoverItem"
        };
    }

    /// <summary>
    ///     Generates a valid Exploration quest
    /// </summary>
    /// <param name="sessionId">session id for the quest</param>
    /// <param name="pmcLevel">player's level for reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">
    ///     The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig
    ///     for the requested quest
    /// </param>
    /// <returns>object of quest type format for "Exploration" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest? GenerateExplorationQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig)
    {
        var explorationConfig = repeatableConfig.QuestConfig.Exploration;
        var requiresSpecificExtract =
            _randomUtil.Random.Next() < repeatableConfig.QuestConfig.Exploration.SpecificExits.Probability;

        if (questTypePool.Pool.Exploration.Locations.Count == 0)
        {
            // there are no more locations left for exploration; delete it as a possible quest type
            questTypePool.Types = questTypePool.Types.Where(t =>
            {
                return t != "Exploration";
            }).ToList();
            return null;
        }

        // If location drawn is factory, it's possible to either get factory4_day and factory4_night or only one
        // of the both
        var locationKey = _randomUtil.DrawRandomFromDict(questTypePool.Pool.Exploration.Locations)[0];
        var locationTarget = questTypePool.Pool.Exploration.Locations[locationKey];

        // Remove the location from the available pool
        questTypePool.Pool.Exploration.Locations.Remove(locationKey);

        // Different max extract count when specific extract needed
        var exitTimesMax = requiresSpecificExtract
            ? explorationConfig.MaximumExtractsWithSpecificExit
            : explorationConfig.MaximumExtracts + 1;
        var numExtracts = _randomUtil.RandInt(1, exitTimesMax);

        var quest = GenerateRepeatableTemplate("Exploration", traderId, repeatableConfig.Side, sessionId);

        var exitStatusCondition = new QuestConditionCounterCondition
        {
            Id = _hashUtil.Generate(),
            DynamicLocale = true,
            Status = ["Survived"],
            ConditionType = "ExitStatus"
        };
        var locationCondition = new QuestConditionCounterCondition
        {
            Id = _hashUtil.Generate(),
            DynamicLocale = true,
            Target = new ListOrT<string>(locationTarget, null),
            ConditionType = "Location"
        };

        quest.Conditions.AvailableForFinish[0].Counter.Id = _hashUtil.Generate();
        quest.Conditions.AvailableForFinish[0].Counter.Conditions = [exitStatusCondition, locationCondition];
        quest.Conditions.AvailableForFinish[0].Value = numExtracts;
        quest.Conditions.AvailableForFinish[0].Id = _hashUtil.Generate();
        quest.Location = GetQuestLocationByMapId(locationKey.ToString());

        if (requiresSpecificExtract)
        {
            // Fetch extracts for the requested side
            var mapExits = GetLocationExitsForSide(locationKey.ToString(), repeatableConfig.Side);

            // Only get exits that have a greater than 0% chance to spawn
            var exitPool = mapExits.Where(exit =>
            {
                return exit.Chance > 0;
            }).ToList();

            // Exclude exits with a requirement to leave (e.g. car extracts)
            var possibleExits = exitPool.Where(exit =>
            {
                return exit.PassageRequirement is not null ||
                                    repeatableConfig.QuestConfig.Exploration.SpecificExits.PassageRequirementWhitelist.Contains(
                                        "PassageRequirement"
                                    );
            })
                .ToList();

            if (possibleExits.Count == 0)
            {
                _logger.Error($"Unable to choose specific exit on map: {locationKey}, Possible exit pool was empty");
            }
            else
            {
                // Choose one of the exits we filtered above
                var chosenExit = _randomUtil.DrawRandomFromList(possibleExits)[0];

                // Create a quest condition to leave raid via chosen exit
                var exitCondition = GenerateExplorationExitCondition(chosenExit);
                quest.Conditions.AvailableForFinish[0].Counter.Conditions.Add(exitCondition);
            }
        }

        // Difficulty for exploration goes from 1 extract to maxExtracts
        // Difficulty for reward goes from 0.2...1 -> map
        var difficulty = _mathUtil.MapToRange(numExtracts, 1, explorationConfig.MaximumExtracts.Value, 0.2, 1);
        quest.Rewards = _repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            difficulty,
            traderId,
            repeatableConfig,
            explorationConfig
        );

        return quest;
    }

    /// <summary>
    ///     Filter a maps exits to just those for the desired side
    /// </summary>
    /// <param name="locationKey">Map id (e.g. factory4_day)</param>
    /// <param name="playerSide">Scav/Pmc</param>
    /// <returns>List of Exit objects</returns>
    protected List<Exit> GetLocationExitsForSide(string locationKey, string playerSide)
    {
        var mapExtracts = _databaseService.GetLocation(locationKey.ToLower()).AllExtracts;

        return mapExtracts.Where(exit =>
        {
            return exit.Side == playerSide;
        }).ToList();
    }

    protected RepeatableQuest GeneratePickupQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig)
    {
        var pickupConfig = repeatableConfig.QuestConfig.Pickup;

        var quest = GenerateRepeatableTemplate("Pickup", traderId, repeatableConfig.Side, sessionId);

        var itemTypeToFetchWithCount = _randomUtil.GetArrayValue(pickupConfig.ItemTypeToFetchWithMaxCount);
        var itemCountToFetch = _randomUtil.RandInt(
            itemTypeToFetchWithCount.MinimumPickupCount.Value,
            itemTypeToFetchWithCount.MaximumPickupCount + 1
        );
        // Choose location - doesnt seem to work for anything other than 'any'
        // var locationKey: string = this.randomUtil.drawRandomFromDict(questTypePool.pool.Pickup.locations)[0];
        // var locationTarget = questTypePool.pool.Pickup.locations[locationKey];

        var findCondition = quest.Conditions.AvailableForFinish.FirstOrDefault(x =>
        {
            return x.ConditionType == "FindItem";
        });
        findCondition.Target = new ListOrT<string>([itemTypeToFetchWithCount.ItemType], null);
        findCondition.Value = itemCountToFetch;

        var counterCreatorCondition = quest.Conditions.AvailableForFinish.FirstOrDefault(x =>
        {
            return x.ConditionType == "CounterCreator";
        });
        // var locationCondition = counterCreatorCondition._props.counter.conditions.find(x => x._parent === "Location");
        // (locationCondition._props as ILocationConditionProps).target = [...locationTarget];

        var equipmentCondition = counterCreatorCondition.Counter.Conditions.FirstOrDefault(x =>
        {
            return x.ConditionType == "Equipment";
        });
        equipmentCondition.EquipmentInclusive = [[itemTypeToFetchWithCount.ItemType]];

        // Add rewards
        quest.Rewards = _repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            1,
            traderId,
            repeatableConfig,
            pickupConfig
        );

        return quest;
    }

    /// <summary>
    ///     Convert a location into an quest code can read (e.g. factory4_day into 55f2d3fd4bdc2d5f408b4567)
    /// </summary>
    /// <param name="locationKey">e.g factory4_day</param>
    /// <returns>guid</returns>
    protected string GetQuestLocationByMapId(string locationKey)
    {
        return _questConfig.LocationIdMap[locationKey];
    }

    /// <summary>
    ///     Exploration repeatable quests can specify a required extraction point.
    ///     This method creates the according object which will be appended to the conditions list
    /// </summary>
    /// <param name="exit">The exit name to generate the condition for</param>
    /// <returns>Exit condition</returns>
    protected QuestConditionCounterCondition GenerateExplorationExitCondition(Exit exit)
    {
        return new QuestConditionCounterCondition
        {
            Id = _hashUtil.Generate(),
            DynamicLocale = true,
            ExitName = exit.Name,
            ConditionType = "ExitName"
        };
    }

    /// <summary>
    ///     Generates the base object of quest type format given as templates in
    ///     assets/database/templates/repeatableQuests.json
    ///     The templates include Elimination, Completion and Extraction quest types
    /// </summary>
    /// <param name="type">Quest type: "Elimination", "Completion" or "Extraction"</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="side">Scav daily or pmc daily/weekly quest</param>
    /// <returns>
    ///     Object which contains the base elements for repeatable quests of the requests type
    ///     (needs to be filled with reward and conditions by called to make a valid quest)
    /// </returns>
    protected RepeatableQuest GenerateRepeatableTemplate(
        string type,
        string traderId,
        string side,
        string sessionId)
    {
        RepeatableQuest questData = null;
        switch (type)
        {
            case "Elimination":
                questData = _databaseService.GetTemplates().RepeatableQuests.Templates.Elimination;
                break;
            case "Completion":
                questData = _databaseService.GetTemplates().RepeatableQuests.Templates.Completion;
                break;
            case "Exploration":
                questData = _databaseService.GetTemplates().RepeatableQuests.Templates.Exploration;
                break;
            case "Pickup":
                questData = _databaseService.GetTemplates().RepeatableQuests.Templates.Pickup;
                break;
        }

        var questClone = _cloner.Clone(questData);
        questClone.Id = _hashUtil.Generate();
        questClone.TraderId = traderId;

        /*  in locale, these id correspond to the text of quests
            template ids -pmc  : Elimination = 616052ea3054fc0e2c24ce6e / Completion = 61604635c725987e815b1a46 / Exploration = 616041eb031af660100c9967
            template ids -scav : Elimination = 62825ef60e88d037dc1eb428 / Completion = 628f588ebb558574b2260fe5 / Exploration = 62825ef60e88d037dc1eb42c
        */

        // Get template id from config based on side and type of quest
        var typeIds = string.Equals(side, "pmc", StringComparison.OrdinalIgnoreCase)
            ? _questConfig.QuestTemplateIds.Pmc
            : _questConfig.QuestTemplateIds.Scav;

        var templateId = string.Empty;
        switch (type)
        {
            case "Completion":
                templateId = typeIds.Completion;
                break;
            case "Elimination":
                templateId = typeIds.Elimination;
                break;
            case "Exploration":
                templateId = typeIds.Exploration;
                break;
            case "Pickup":
                templateId = typeIds.Pickup;
                break;
        }

        questClone.TemplateId = templateId;

        // Force REF templates to use prapors ID - solves missing text issue
        var desiredTraderId = traderId == Traders.REF ? Traders.PRAPOR : traderId;

        questClone.Name = questClone.Name
            .Replace("{traderId}", traderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.Note = questClone.Note
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.Description = questClone.Description
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.SuccessMessageText = questClone.SuccessMessageText
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.FailMessageText = questClone.FailMessageText
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.StartedMessageText = questClone.StartedMessageText
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.ChangeQuestMessageText = questClone.ChangeQuestMessageText
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.AcceptPlayerMessage = questClone.AcceptPlayerMessage
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.DeclinePlayerMessage = questClone.DeclinePlayerMessage
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);
        questClone.CompletePlayerMessage = questClone.CompletePlayerMessage
            .Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questClone.TemplateId);

        questClone.QuestStatus.Id = _hashUtil.Generate();
        questClone.QuestStatus.Uid = sessionId; // Needs to match user id
        questClone.QuestStatus.QId = questClone.Id; // Needs to match quest id

        return questClone;
    }
}
