using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using BodyPart = SPTarkov.Server.Core.Models.Eft.Common.Tables.BodyPart;
using BodyParts = SPTarkov.Server.Core.Constants.BodyParts;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class BotGenerator(
    ISptLogger<BotGenerator> _logger,
    HashUtil _hashUtil,
    RandomUtil _randomUtil,
    DatabaseService _databaseService,
    BotInventoryGenerator _botInventoryGenerator,
    BotLevelGenerator _botLevelGenerator,
    BotEquipmentFilterService _botEquipmentFilterService,
    WeightedRandomHelper _weightedRandomHelper,
    BotHelper _botHelper,
    BotGeneratorHelper _botGeneratorHelper,
    SeasonalEventService _seasonalEventService,
    ItemFilterService _itemFilterService,
    BotNameService _botNameService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();

    /// <summary>
    ///     Generate a player scav bot object
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="role">e.g. assault / pmcbot</param>
    /// <param name="difficulty">easy/normal/hard/impossible</param>
    /// <param name="botTemplate">base bot template to use  (e.g. assault/pmcbot)</param>
    /// <param name="profile">profile of player generating pscav</param>
    /// <returns>BotBase</returns>
    public PmcData GeneratePlayerScav(
        string sessionId,
        string role,
        string difficulty,
        BotType botTemplate,
        PmcData profile
    )
    {
        var bot = GetCloneOfBotBase();
        bot.Info.Settings.BotDifficulty = difficulty;
        bot.Info.Settings.Role = role;
        bot.Info.Side = Sides.Savage;

        var botGenDetails = new BotGenerationDetails
        {
            IsPmc = false,
            Side = Sides.Savage,
            Role = role,
            BotRelativeLevelDeltaMax = 0,
            BotRelativeLevelDeltaMin = 0,
            BotCountToGenerate = 1,
            BotDifficulty = difficulty,
            IsPlayerScav = true,
        };

        bot = GenerateBot(sessionId, bot, botTemplate, botGenDetails);

        // Sets the name after scav name shown in parentheses
        bot.Info.MainProfileNickname = profile.Info.Nickname;

        return new PmcData
        {
            Id = bot.Id,
            Aid = bot.Aid,
            SessionId = bot.SessionId,
            Savage = null,
            KarmaValue = bot.KarmaValue,
            Info = bot.Info,
            Customization = bot.Customization,
            Health = bot.Health,
            Inventory = bot.Inventory,
            Skills = bot.Skills,
            Stats = bot.Stats,
            Encyclopedia = bot.Encyclopedia,
            TaskConditionCounters = bot.TaskConditionCounters,
            InsuredItems = bot.InsuredItems,
            Hideout = bot.Hideout,
            Quests = bot.Quests,
            TradersInfo = bot.TradersInfo,
            UnlockedInfo = bot.UnlockedInfo,
            RagfairInfo = bot.RagfairInfo,
            Achievements = bot.Achievements,
            RepeatableQuests = bot.RepeatableQuests,
            Bonuses = bot.Bonuses,
            Notes = bot.Notes,
            CarExtractCounts = bot.CarExtractCounts,
            CoopExtractCounts = bot.CoopExtractCounts,
            SurvivorClass = bot.SurvivorClass,
            WishList = bot.WishList,
            MoneyTransferLimitData = bot.MoneyTransferLimitData,
            IsPmc = bot.IsPmc,
            Prestige = new Dictionary<string, long>(),
        };
    }

    /// <summary>
    ///     Create 1 bot of the type/side/difficulty defined in botGenerationDetails
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="botGenerationDetails">details on how to generate bots</param>
    /// <returns>constructed bot</returns>
    public BotBase PrepareAndGenerateBot(
        string sessionId,
        BotGenerationDetails? botGenerationDetails
    )
    {
        var preparedBotBase = GetPreparedBotBase(
            botGenerationDetails.EventRole ?? botGenerationDetails.Role, // Use eventRole if provided
            botGenerationDetails.Side,
            botGenerationDetails.BotDifficulty
        );

        // Get raw json data for bot (Cloned)
        var botRole =
            botGenerationDetails.IsPmc ?? false
                ? preparedBotBase.Info.Side // Use side to get usec.json or bear.json when bot will be PMC
                : botGenerationDetails.Role;
        var botJsonTemplateClone = _cloner.Clone(_botHelper.GetBotTemplate(botRole));
        if (botJsonTemplateClone is null)
        {
            _logger.Error(
                $"Unable to retrieve: {botRole} bot template, cannot generate bot of this type"
            );
        }

        return GenerateBot(sessionId, preparedBotBase, botJsonTemplateClone, botGenerationDetails);
    }

    /// <summary>
    ///     Get a clone of the default bot base object and adjust its role/side/difficulty values
    /// </summary>
    /// <param name="botRole">Role bot should have</param>
    /// <param name="botSide">Side bot should have</param>
    /// <param name="difficulty">Difficult bot should have</param>
    /// <returns>Cloned bot base</returns>
    public BotBase GetPreparedBotBase(string botRole, string botSide, string difficulty)
    {
        var botBaseClone = GetCloneOfBotBase();
        botBaseClone.Info.Settings.Role = botRole;
        botBaseClone.Info.Side = botSide;
        botBaseClone.Info.Settings.BotDifficulty = difficulty;

        return botBaseClone;
    }

    /// <summary>
    ///     Get a clone of the database\bots\base.json file
    /// </summary>
    /// <returns>BotBase object</returns>
    public BotBase GetCloneOfBotBase()
    {
        return _cloner.Clone(_databaseService.GetBots().Base);
    }

    /// <summary>
    ///     Create a IBotBase object with equipment/loot/exp etc
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="bot">Bots base file</param>
    /// <param name="botJsonTemplate">Bot template from db/bots/x.json</param>
    /// <param name="botGenerationDetails">details on how to generate the bot</param>
    /// <returns>BotBase object</returns>
    public BotBase GenerateBot(
        string sessionId,
        BotBase bot,
        BotType botJsonTemplate,
        BotGenerationDetails botGenerationDetails
    )
    {
        var botRoleLowercase = botGenerationDetails.Role.ToLower();
        var botLevel = _botLevelGenerator.GenerateBotLevel(
            botJsonTemplate.BotExperience.Level,
            botGenerationDetails,
            bot
        );

        // Only filter bot equipment, never players
        if (!botGenerationDetails.IsPlayerScav.GetValueOrDefault(false))
        {
            _botEquipmentFilterService.FilterBotEquipment(
                sessionId,
                botJsonTemplate,
                botLevel.Level.Value,
                botGenerationDetails
            );
        }

        bot.Info.Nickname = _botNameService.GenerateUniqueBotNickname(
            botJsonTemplate,
            botGenerationDetails,
            botRoleLowercase,
            _botConfig.BotRolesThatMustHaveUniqueName
        );

        // Only Pmcs should have a lower nickname
        bot.Info.LowerNickname = botGenerationDetails.IsPmc.GetValueOrDefault(false)
            ? bot.Info.Nickname.ToLower()
            : string.Empty;

        // Only run when generating a 'fake' playerscav, not actual player scav
        if (
            !botGenerationDetails.IsPlayerScav.GetValueOrDefault(false)
            && ShouldSimulatePlayerScav(botRoleLowercase)
        )
        {
            _botNameService.AddRandomPmcNameToBotMainProfileNicknameProperty(bot);
            SetRandomisedGameVersionAndCategory(bot.Info);
        }

        if (!_seasonalEventService.ChristmasEventEnabled())
        // Process all bots EXCEPT gifter, he needs christmas items
        {
            if (botGenerationDetails.Role != "gifter")
            {
                _seasonalEventService.RemoveChristmasItemsFromBotInventory(
                    botJsonTemplate.BotInventory,
                    botGenerationDetails.Role
                );
            }
        }

        RemoveBlacklistedLootFromBotTemplate(botJsonTemplate.BotInventory);

        // Remove hideout data if bot is not a PMC or pscav - match what live sends
        if (
            !(
                botGenerationDetails.IsPmc.GetValueOrDefault(false)
                || botGenerationDetails.IsPlayerScav.GetValueOrDefault(false)
            )
        )
        {
            bot.Hideout = null;
        }

        bot.Info.Experience = botLevel.Exp;
        bot.Info.Level = botLevel.Level;
        bot.Info.Settings.Experience = GetExperienceRewardForKillByDifficulty(
            botJsonTemplate.BotExperience.Reward,
            botGenerationDetails.BotDifficulty,
            botGenerationDetails.Role
        );
        bot.Info.Settings.StandingForKill = GetStandingChangeForKillByDifficulty(
            botJsonTemplate.BotExperience.StandingForKill,
            botGenerationDetails.BotDifficulty,
            botGenerationDetails.Role
        );
        bot.Info.Settings.AggressorBonus = GetAggressorBonusByDifficulty(
            botJsonTemplate.BotExperience.StandingForKill,
            botGenerationDetails.BotDifficulty,
            botGenerationDetails.Role
        );
        bot.Info.Settings.UseSimpleAnimator =
            botJsonTemplate.BotExperience.UseSimpleAnimator ?? false;
        bot.Info.Voice = _weightedRandomHelper.GetWeightedValue(
            botJsonTemplate.BotAppearance.Voice
        );
        bot.Health = GenerateHealth(
            botJsonTemplate.BotHealth,
            botGenerationDetails.IsPlayerScav.GetValueOrDefault(false)
        );
        bot.Skills = GenerateSkills(botJsonTemplate.BotSkills);
        bot.Info.PrestigeLevel = 0;

        if (botGenerationDetails.IsPmc.GetValueOrDefault(false))
        {
            bot.Info.IsStreamerModeAvailable = true; // Set to true so client patches can pick it up later - client sometimes alters botrole to assaultGroup
            SetRandomisedGameVersionAndCategory(bot.Info);
            if (bot.Info.GameVersion == GameEditions.UNHEARD)
            {
                AddAdditionalPocketLootWeightsForUnheardBot(botJsonTemplate);
            }
        }

        // Add drip
        SetBotAppearance(bot, botJsonTemplate.BotAppearance, botGenerationDetails);

        // Filter out blacklisted gear from the base template
        FilterBlacklistedGear(botJsonTemplate, botGenerationDetails);

        bot.Inventory = _botInventoryGenerator.GenerateInventory(
            sessionId,
            botJsonTemplate,
            botRoleLowercase,
            botGenerationDetails.IsPmc.GetValueOrDefault(false),
            bot.Info.Level.Value,
            bot.Info.GameVersion
        );

        if (_botConfig.BotRolesWithDogTags.Contains(botRoleLowercase))
        {
            AddDogtagToBot(bot);
        }

        // Generate new bot ID
        AddIdsToBot(bot, botGenerationDetails);

        // Generate new inventory ID
        GenerateInventoryId(bot);

        // Set role back to originally requested now its been generated
        if (botGenerationDetails.EventRole is not null)
        {
            bot.Info.Settings.Role = botGenerationDetails.EventRole;
        }

        return bot;
    }

    /// <summary>
    ///     Should this bot have a name like "name (Pmc Name)" and be altered by client patch to be hostile to player
    /// </summary>
    /// <param name="botRole">Role bot has</param>
    /// <returns>True if name should be simulated pscav</returns>
    public bool ShouldSimulatePlayerScav(string botRole)
    {
        return botRole == Roles.Assault
            && _randomUtil.GetChance100(_botConfig.ChanceAssaultScavHasPlayerScavName);
    }

    /// <summary>
    ///     Get exp for kill by bot difficulty
    /// </summary>
    /// <param name="experiences">Dict of difficulties and experience</param>
    /// <param name="botDifficulty">the killed bots difficulty</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Experience for kill</returns>
    public int GetExperienceRewardForKillByDifficulty(
        Dictionary<string, MinMax<int>> experiences,
        string botDifficulty,
        string role
    )
    {
        if (!experiences.TryGetValue(botDifficulty.ToLower(), out var result))
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Unable to find experience: {botDifficulty} for {role} bot, falling back to `normal`"
                );
            }

            return _randomUtil.GetInt(experiences["normal"].Min, experiences["normal"].Max);
        }

        // Some bots have -1/-1, shortcut result

        if (result.Max == -1)
        {
            return -1;
        }

        return _randomUtil.GetInt(result.Min, result.Max);
    }

    /// <summary>
    ///     Get the standing value change when player kills a bot
    /// </summary>
    /// <param name="standingsForKill">Dictionary of standing values keyed by bot difficulty</param>
    /// <param name="botDifficulty">Difficulty of bot to look up</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Standing change value</returns>
    public double GetStandingChangeForKillByDifficulty(
        Dictionary<string, double> standingsForKill,
        string botDifficulty,
        string role
    )
    {
        if (!standingsForKill.TryGetValue(botDifficulty.ToLower(), out var result))
        {
            _logger.Warning(
                $"Unable to find standing for kill value for: {role} {botDifficulty}, falling back to `normal`"
            );

            return standingsForKill["normal"];
        }

        return result;
    }

    /// <summary>
    ///     Get the aggressor bonus value when player kills a bot
    /// </summary>
    /// <param name="aggressorBonuses">Dictionary of standing values keyed by bot difficulty</param>
    /// <param name="botDifficulty">Difficulty of bot to look up</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Standing change value</returns>
    public double GetAggressorBonusByDifficulty(
        Dictionary<string, double> aggressorBonuses,
        string botDifficulty,
        string role
    )
    {
        if (!aggressorBonuses.TryGetValue(botDifficulty.ToLower(), out var result))
        {
            _logger.Warning(
                $"Unable to find aggressor bonus for kill value for: {role} {botDifficulty}, falling back to `normal`"
            );

            return aggressorBonuses["normal"];
        }

        return result;
    }

    /// <summary>
    ///     Set weighting of flagged equipment to 0
    /// </summary>
    /// <param name="botJsonTemplate">Bot data to adjust</param>
    /// <param name="botGenerationDetails">Generation details of bot</param>
    public void FilterBlacklistedGear(
        BotType botJsonTemplate,
        BotGenerationDetails botGenerationDetails
    )
    {
        var blacklist = _botEquipmentFilterService.GetBotEquipmentBlacklist(
            _botGeneratorHelper.GetBotEquipmentRole(botGenerationDetails.Role),
            botGenerationDetails.PlayerLevel.GetValueOrDefault(1)
        );

        if (blacklist?.Gear is null)
        // Nothing to filter by
        {
            return;
        }

        foreach (var (equipmentSlot, blacklistedTpls) in blacklist.Gear)
        {
            var equipmentDict = botJsonTemplate.BotInventory.Equipment[equipmentSlot];

            foreach (var blacklistedTpl in blacklistedTpls)
            // Set weighting to 0, will never be picked
            {
                equipmentDict[blacklistedTpl] = 0;
            }
        }
    }

    /// <summary>
    ///     Unheard PMCs need their pockets expanded
    /// </summary>
    /// <param name="botJsonTemplate">Bot data to adjust</param>
    public void AddAdditionalPocketLootWeightsForUnheardBot(BotType botJsonTemplate)
    {
        // Adjust pocket loot weights to allow for 5 or 6 items
        var pocketWeights = botJsonTemplate.BotGeneration.Items.PocketLoot.Weights;
        pocketWeights[5] = 1;
        pocketWeights[6] = 1;
    }

    /// <summary>
    ///     Remove items from item.json/lootableItemBlacklist from bots inventory
    /// </summary>
    /// <param name="botInventory">Bot to filter</param>
    public void RemoveBlacklistedLootFromBotTemplate(BotTypeInventory botInventory)
    {
        List<string> lootContainersToFilter = ["Backpack", "Pockets", "TacticalVest"];
        var props = botInventory.Items.GetType().GetProperties();

        // Remove blacklisted loot from loot containers
        foreach (var lootContainerKey in lootContainersToFilter)
        {
            var propInfo = props.FirstOrDefault(x =>
                string.Equals(x.Name, lootContainerKey, StringComparison.CurrentCultureIgnoreCase)
            );
            var prop = (Dictionary<string, double>?)propInfo.GetValue(botInventory.Items);

            // No container, skip
            if (prop is null)
            {
                continue;
            }

            var newProp = prop.Where(tpl =>
                {
                    return !_itemFilterService.IsLootableItemBlacklisted(tpl.Key);
                })
                .ToDictionary();
            propInfo.SetValue(botInventory.Items, newProp);
        }
    }

    /// <summary>
    ///     Choose various appearance settings for a bot using weights: head/body/feet/hands
    /// </summary>
    /// <param name="bot">Bot to adjust</param>
    /// <param name="appearance">Appearance settings to choose from</param>
    /// <param name="botGenerationDetails">Generation details</param>
    public void SetBotAppearance(
        BotBase bot,
        Appearance appearance,
        BotGenerationDetails botGenerationDetails
    )
    {
        // Choose random values by weight
        bot.Customization.Head = _weightedRandomHelper.GetWeightedValue<string>(appearance.Head);
        bot.Customization.Feet = _weightedRandomHelper.GetWeightedValue<string>(appearance.Feet);
        bot.Customization.Body = _weightedRandomHelper.GetWeightedValue<string>(appearance.Body);

        var bodyGlobalDictDb = _databaseService.GetGlobals().Configuration.Customization.Body;
        var chosenBodyTemplate = _databaseService.GetCustomization()[bot.Customization.Body];

        // Some bodies have matching hands, look up body to see if this is the case
        var chosenBody = bodyGlobalDictDb.FirstOrDefault(c =>
            c.Key == chosenBodyTemplate?.Name.Trim()
        );
        bot.Customization.Hands =
            chosenBody.Value?.IsNotRandom ?? false
                ? chosenBody.Value.Hands // Has fixed hands for chosen body, update to match
                : _weightedRandomHelper.GetWeightedValue<string>(appearance.Hands); // Hands can be random, choose any from weighted dict
    }

    /// <summary>
    ///     Converts health object to the required format
    /// </summary>
    /// <param name="healthObj">health object from bot json</param>
    /// <param name="playerScav">Is a pscav bot being generated</param>
    /// <returns>Health object</returns>
    public BotBaseHealth GenerateHealth(BotTypeHealth healthObj, bool playerScav = false)
    {
        var bodyParts = playerScav
            ? GetLowestHpBody(healthObj.BodyParts)
            : _randomUtil.GetArrayValue(healthObj.BodyParts);

        BotBaseHealth health = new()
        {
            Hydration = new CurrentMinMax
            {
                Current = _randomUtil.GetDouble(healthObj.Hydration.Min, healthObj.Hydration.Max),
                Maximum = healthObj.Hydration.Max,
            },
            Energy = new CurrentMinMax
            {
                Current = _randomUtil.GetDouble(healthObj.Energy.Min, healthObj.Energy.Max),
                Maximum = healthObj.Energy.Max,
            },
            Temperature = new CurrentMinMax
            {
                Current = _randomUtil.GetDouble(
                    healthObj.Temperature.Min,
                    healthObj.Temperature.Max
                ),
                Maximum = healthObj.Temperature.Max,
            },
            BodyParts = new Dictionary<string, BodyPartHealth>
            {
                {
                    BodyParts.Head,
                    new BodyPartHealth
                    {
                        Health = new CurrentMinMax
                        {
                            Current = _randomUtil.GetDouble(bodyParts.Head.Min, bodyParts.Head.Max),
                            Maximum = Math.Round(bodyParts.Head.Max),
                        },
                    }
                },
                {
                    BodyParts.Chest,
                    new BodyPartHealth
                    {
                        Health = new CurrentMinMax
                        {
                            Current = _randomUtil.GetDouble(
                                bodyParts.Chest.Min,
                                bodyParts.Chest.Max
                            ),
                            Maximum = Math.Round(bodyParts.Chest.Max),
                        },
                    }
                },
                {
                    BodyParts.Stomach,
                    new BodyPartHealth
                    {
                        Health = new CurrentMinMax
                        {
                            Current = _randomUtil.GetDouble(
                                bodyParts.Stomach.Min,
                                bodyParts.Stomach.Max
                            ),
                            Maximum = Math.Round(bodyParts.Stomach.Max),
                        },
                    }
                },
                {
                    BodyParts.LeftArm,
                    new BodyPartHealth
                    {
                        Health = new CurrentMinMax
                        {
                            Current = _randomUtil.GetDouble(
                                bodyParts.LeftArm.Min,
                                bodyParts.LeftArm.Max
                            ),
                            Maximum = Math.Round(bodyParts.LeftArm.Max),
                        },
                    }
                },
                {
                    BodyParts.RightArm,
                    new BodyPartHealth
                    {
                        Health = new CurrentMinMax
                        {
                            Current = _randomUtil.GetDouble(
                                bodyParts.RightArm.Min,
                                bodyParts.RightArm.Max
                            ),
                            Maximum = Math.Round(bodyParts.RightArm.Max),
                        },
                    }
                },
                {
                    BodyParts.LeftLeg,
                    new BodyPartHealth
                    {
                        Health = new CurrentMinMax
                        {
                            Current = _randomUtil.GetDouble(
                                bodyParts.LeftLeg.Min,
                                bodyParts.LeftLeg.Max
                            ),
                            Maximum = Math.Round(bodyParts.LeftLeg.Max),
                        },
                    }
                },
                {
                    BodyParts.RightLeg,
                    new BodyPartHealth
                    {
                        Health = new CurrentMinMax
                        {
                            Current = _randomUtil.GetDouble(
                                bodyParts.RightLeg.Min,
                                bodyParts.RightLeg.Max
                            ),
                            Maximum = Math.Round(bodyParts.RightLeg.Max),
                        },
                    }
                },
            },
            UpdateTime = 0, // 0 for player-scav too
            Immortal = false,
        };

        return health;
    }

    /// <summary>
    ///     Sum up body parts max hp values, return the bodyPart collection with the lowest value
    /// </summary>
    /// <param name="bodies">Body parts to sum up</param>
    /// <returns>Lowest hp collection</returns>
    public BodyPart? GetLowestHpBody(List<BodyPart> bodies)
    {
        if (bodies.Count == 0)
        {
            return null;
        }

        BodyPart result = new();
        var props = result.GetType().GetProperties();
        double? currentHighest = double.MaxValue;
        foreach (var bodyPart in bodies)
        {
            double? hpTotal = 0;

            foreach (var prop in props)
            {
                var value = (MinMax<double>)prop.GetValue(bodyPart);
                hpTotal += value.Max;
            }

            if (hpTotal < currentHighest)
            {
                // Found collection with lower value that previous, use it
                currentHighest = hpTotal;
                result = bodyPart;
            }
        }

        return result;
    }

    /// <summary>
    ///     Get a bots skills with randomised progress value between the min and max values
    /// </summary>
    /// <param name="botSkills">Skills that should have their progress value randomised</param>
    /// <returns>Skills</returns>
    public Skills GenerateSkills(BotDbSkills botSkills)
    {
        var skillsToReturn = new Skills
        {
            Common = GetSkillsWithRandomisedProgressValue(botSkills.Common, true),
            Mastering = GetSkillsWithRandomisedProgressValue(botSkills.Mastering, false),
            Points = 0,
        };

        return skillsToReturn;
    }

    /// <summary>
    ///     Randomise the progress value of passed in skills based on the min/max value
    /// </summary>
    /// <param name="skills">Skills to randomise</param>
    /// <param name="isCommonSkills">Are the skills 'common' skills</param>
    /// <returns>Skills with randomised progress values as an array</returns>
    public List<BaseSkill> GetSkillsWithRandomisedProgressValue(
        Dictionary<string, MinMax<double>>? skills,
        bool isCommonSkills
    )
    {
        if (skills is null)
        {
            return [];
        }

        return skills
            .Select(kvp =>
            {
                // Get skill from dict, skip if not found
                var skill = kvp.Value;
                if (skill == null)
                {
                    return null;
                }

                // All skills have id and progress props
                var skillToAdd = new BaseSkill
                {
                    Id = kvp.Key,
                    Progress = _randomUtil.GetDouble(skill.Min, skill.Max),
                };

                // Common skills have additional props
                if (isCommonSkills)
                {
                    skillToAdd.PointsEarnedDuringSession = 0;
                    skillToAdd.LastAccess = 0;
                }

                return skillToAdd;
            })
            .Where(baseSkill => baseSkill != null)
            .ToList();
    }

    /// <summary>
    ///     Generate an id+aid for a bot and apply
    /// </summary>
    /// <param name="bot">bot to update</param>
    /// <param name="botGenerationDetails"></param>
    /// <returns></returns>
    public void AddIdsToBot(BotBase bot, BotGenerationDetails botGenerationDetails)
    {
        var botId = _hashUtil.Generate();

        bot.Id = botId;
        bot.Aid = botGenerationDetails.IsPmc.GetValueOrDefault(false)
            ? _hashUtil.GenerateAccountId()
            : 0;
    }

    /// <summary>
    ///     Update a profiles profile.Inventory.equipment value with a freshly generated one.
    ///     Update all inventory items that make use of this value too.
    /// </summary>
    /// <param name="profile">Profile to update</param>
    public void GenerateInventoryId(BotBase profile)
    {
        var newInventoryItemId = _hashUtil.Generate();

        foreach (var item in profile.Inventory.Items)
        {
            // Root item found, update its _id value to newly generated id
            if (item.Template == ItemTpl.INVENTORY_DEFAULT)
            {
                item.Id = newInventoryItemId;

                continue;
            }

            // Optimisation - skip items without a parentId
            // They are never linked to root inventory item + we already handled root item above
            if (item.ParentId is null)
            {
                continue;
            }

            // Item is a child of root inventory item, update its parentId value to newly generated id
            if (item.ParentId == profile.Inventory.Equipment)
            {
                item.ParentId = newInventoryItemId;
            }
        }

        // Update inventory equipment id to new one we generated
        profile.Inventory.Equipment = newInventoryItemId;
    }

    /// <summary>
    ///     Randomise a bots game version and account category.
    ///     Chooses from all the game versions (standard, eod etc).
    ///     Chooses account type (default, Sherpa, etc).
    /// </summary>
    /// <param name="botInfo">bot info object to update</param>
    /// <returns>Chosen game version</returns>
    public string SetRandomisedGameVersionAndCategory(Info botInfo)
    {
        // Special case
        if (string.Equals(botInfo.Nickname, "nikita", StringComparison.OrdinalIgnoreCase))
        {
            botInfo.GameVersion = GameEditions.UNHEARD;
            botInfo.MemberCategory = MemberCategory.Developer;

            return botInfo.GameVersion;
        }

        // Choose random weighted game version for bot
        botInfo.GameVersion = _weightedRandomHelper.GetWeightedValue(_pmcConfig.GameVersionWeight);

        // Choose appropriate member category value
        switch (botInfo.GameVersion)
        {
            case GameEditions.EDGE_OF_DARKNESS:
                botInfo.MemberCategory = MemberCategory.UniqueId;
                break;
            case GameEditions.UNHEARD:
                botInfo.MemberCategory = MemberCategory.Unheard;
                break;
            default:
                // Everyone else gets a weighted randomised category
                botInfo.MemberCategory = _weightedRandomHelper.GetWeightedValue(
                    _pmcConfig.AccountTypeWeight
                );
                break;
        }

        // Ensure selected category matches
        botInfo.SelectedMemberCategory = botInfo.MemberCategory;

        return botInfo.GameVersion;
    }

    /// <summary>
    ///     Add a side-specific (usec/bear) dogtag item to a bots inventory
    /// </summary>
    /// <param name="bot">bot to add dogtag to</param>
    /// <returns></returns>
    public void AddDogtagToBot(BotBase bot)
    {
        Item inventoryItem = new()
        {
            Id = _hashUtil.Generate(),
            Template = GetDogtagTplByGameVersionAndSide(bot.Info.Side, bot.Info.GameVersion),
            ParentId = bot.Inventory.Equipment,
            SlotId = Slots.Dogtag,
            Upd = new Upd { SpawnedInSession = true },
        };

        bot.Inventory.Items.Add(inventoryItem);
    }

    /// <summary>
    ///     Get a dogtag tpl that matches the bots game version and side
    /// </summary>
    /// <param name="side">Usec/Bear</param>
    /// <param name="gameVersion">edge_of_darkness / standard</param>
    /// <returns>item tpl</returns>
    public string GetDogtagTplByGameVersionAndSide(string side, string gameVersion)
    {
        if (string.Equals(side, Sides.Usec, StringComparison.OrdinalIgnoreCase))
        {
            switch (gameVersion)
            {
                case GameEditions.EDGE_OF_DARKNESS:
                    return ItemTpl.BARTER_DOGTAG_USEC_EOD;
                case GameEditions.UNHEARD:
                    return ItemTpl.BARTER_DOGTAG_USEC_TUE;
                default:
                    return ItemTpl.BARTER_DOGTAG_USEC;
            }
        }

        switch (gameVersion)
        {
            case GameEditions.EDGE_OF_DARKNESS:
                return ItemTpl.BARTER_DOGTAG_BEAR_EOD;
            case GameEditions.UNHEARD:
                return ItemTpl.BARTER_DOGTAG_BEAR_TUE;
            default:
                return ItemTpl.BARTER_DOGTAG_BEAR;
        }
    }
}
