using SPTarkov.DI.Annotations;
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
    ISptLogger<BotGenerator> logger,
    HashUtil hashUtil,
    RandomUtil randomUtil,
    DatabaseService databaseService,
    BotInventoryGenerator botInventoryGenerator,
    BotLevelGenerator botLevelGenerator,
    BotEquipmentFilterService botEquipmentFilterService,
    WeightedRandomHelper weightedRandomHelper,
    BotHelper botHelper,
    SeasonalEventService seasonalEventService,
    ItemFilterService itemFilterService,
    BotNameService botNameService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly PmcConfig PMCConfig = configServer.GetConfig<PmcConfig>();

    /// <summary>
    ///     Generate a player scav bot object
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="role">e.g. assault / pmcbot</param>
    /// <param name="difficulty">easy/normal/hard/impossible</param>
    /// <param name="botTemplate">base bot template to use  (e.g. assault/pmcbot)</param>
    /// <param name="profile">profile of player generating pscav</param>
    /// <returns>BotBase</returns>
    public PmcData GeneratePlayerScav(MongoId sessionId, string role, string difficulty, BotType botTemplate, PmcData profile)
    {
        var bot = GetBotBaseClone();
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
            ClearBotContainerCacheAfterGeneration = false,
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
    public BotBase PrepareAndGenerateBot(MongoId sessionId, BotGenerationDetails botGenerationDetails)
    {
        var botBaseClone = GetPreparedBotBaseClone(
            botGenerationDetails.EventRole ?? botGenerationDetails.Role, // Use eventRole if provided
            botGenerationDetails.Side,
            botGenerationDetails.BotDifficulty
        );

        // Get raw json data for bot (Cloned)
        var botRole = botGenerationDetails.IsPmc
            ? botBaseClone.Info.Side // Use side to get usec.json or bear.json when bot will be PMC
            : botGenerationDetails.Role;
        var botJsonTemplateClone = cloner.Clone(botHelper.GetBotTemplate(botRole));
        if (botJsonTemplateClone is null)
        {
            logger.Error($"Unable to retrieve: {botRole} bot template, cannot generate bot of this type");
        }

        return GenerateBot(sessionId, botBaseClone, botJsonTemplateClone, botGenerationDetails);
    }

    /// <summary>
    ///     Get a clone of the default bot base object and adjust its role/side/difficulty values
    /// </summary>
    /// <param name="botRole">Role bot should have</param>
    /// <param name="botSide">Side bot should have</param>
    /// <param name="difficulty">Difficult bot should have</param>
    /// <returns>Cloned bot base</returns>
    protected BotBase GetPreparedBotBaseClone(string botRole, string botSide, string difficulty)
    {
        var botBaseClone = GetBotBaseClone();
        botBaseClone.Info.Settings.Role = botRole;
        botBaseClone.Info.Side = botSide;
        botBaseClone.Info.Settings.BotDifficulty = difficulty;

        return botBaseClone;
    }

    /// <summary>
    ///     Get a clone of the database\bots\base.json file
    /// </summary>
    /// <returns>BotBase object</returns>
    protected BotBase GetBotBaseClone()
    {
        return cloner.Clone(databaseService.GetBots().Base);
    }

    /// <summary>
    ///     Create a IBotBase object with equipment/loot/exp etc
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="bot">Bots base file</param>
    /// <param name="botJsonTemplate">Bot template from db/bots/x.json</param>
    /// <param name="botGenerationDetails">details on how to generate the bot</param>
    /// <returns>BotBase object</returns>
    protected BotBase GenerateBot(MongoId sessionId, BotBase bot, BotType botJsonTemplate, BotGenerationDetails botGenerationDetails)
    {
        botGenerationDetails.RoleLowercase = botGenerationDetails.Role.ToLowerInvariant();

        var botLevelDetails = botLevelGenerator.GenerateBotLevel(botJsonTemplate.BotExperience.Level, botGenerationDetails, bot);

        // Assign value for later use
        botGenerationDetails.BotLevel = botLevelDetails.Level.GetValueOrDefault();

        // Generate Id/AId for bot
        AddIdsToBot(bot);

        // Only filter bot equipment, never players
        if (!botGenerationDetails.IsPlayerScav)
        {
            botEquipmentFilterService.FilterBotEquipment(sessionId, botJsonTemplate, botGenerationDetails);
        }

        bot.Info.Nickname = botNameService.GenerateUniqueBotNickname(
            botJsonTemplate,
            botGenerationDetails,
            BotConfig.BotRolesThatMustHaveUniqueName
        );

        // Only PMCs need a lower nickname
        bot.Info.LowerNickname = botGenerationDetails.IsPmc ? bot.Info.Nickname.ToLowerInvariant() : string.Empty;

        // Only run when generating a 'fake' playerscav, not actual player scav
        if (!botGenerationDetails.IsPlayerScav && ShouldSimulatePlayerScav(botGenerationDetails.RoleLowercase))
        {
            botNameService.AddRandomPmcNameToBotMainProfileNicknameProperty(bot);
            SetRandomisedGameVersionAndCategory(bot.Info);
        }

        if (!seasonalEventService.ChristmasEventEnabled())
        // Process all bots EXCEPT gifter, he needs christmas items
        {
            if (botGenerationDetails.Role != "gifter")
            {
                seasonalEventService.RemoveChristmasItemsFromBotInventory(botJsonTemplate.BotInventory, botGenerationDetails.Role);
            }
        }

        RemoveBlacklistedLootFromBotTemplate(botJsonTemplate.BotInventory);

        // Remove hideout data if bot is not a PMC or pscav - match what live sends
        if (!(botGenerationDetails.IsPmc || botGenerationDetails.IsPlayerScav))
        {
            bot.Hideout = null;
        }

        bot.Info.Experience = botLevelDetails.Exp;
        bot.Info.Level = botLevelDetails.Level;
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
        bot.Info.Settings.UseSimpleAnimator = botJsonTemplate.BotExperience.UseSimpleAnimator;
        bot.Customization.Voice = weightedRandomHelper.GetWeightedValue(botJsonTemplate.BotAppearance.Voice);
        bot.Health = GenerateHealth(botJsonTemplate.BotHealth, botGenerationDetails.IsPlayerScav);
        bot.Skills = GenerateSkills(botJsonTemplate.BotSkills);
        bot.Info.PrestigeLevel = 0;

        if (botGenerationDetails.IsPmc)
        {
            bot.Info.IsStreamerModeAvailable = true; // Set to true so client patches can pick it up later - client sometimes alters botrole to assaultGroup
            SetRandomisedGameVersionAndCategory(bot.Info);
            if (bot.Info.GameVersion == GameEditions.UNHEARD)
            {
                AddAdditionalPocketLootWeightsForUnheardBot(botJsonTemplate);
            }

            botGenerationDetails.GameVersion = bot.Info.GameVersion;
        }

        // Add drip
        SetBotAppearance(bot, botJsonTemplate.BotAppearance, botGenerationDetails);

        bot.Inventory = botInventoryGenerator.GenerateInventory(bot.Id.Value, sessionId, botJsonTemplate, botGenerationDetails);

        if (BotConfig.BotRolesWithDogTags.Contains(botGenerationDetails.RoleLowercase))
        {
            AddDogtagToBot(bot);
        }

        // Generate new inventory ID
        GenerateInventoryId(bot);

        // Set role back to originally requested now it has been generated
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
    protected bool ShouldSimulatePlayerScav(string botRole)
    {
        return botRole == Roles.Assault && randomUtil.GetChance100(BotConfig.ChanceAssaultScavHasPlayerScavName);
    }

    /// <summary>
    ///     Get exp for kill by bot difficulty
    /// </summary>
    /// <param name="experiences">Dict of difficulties and experience</param>
    /// <param name="botDifficulty">the killed bots difficulty</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Experience for kill</returns>
    protected int GetExperienceRewardForKillByDifficulty(Dictionary<string, MinMax<int>> experiences, string botDifficulty, string role)
    {
        if (!experiences.TryGetValue(botDifficulty.ToLowerInvariant(), out var result))
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Unable to find experience: {botDifficulty} for {role} bot, falling back to `normal`");
            }

            return randomUtil.GetInt(experiences["normal"].Min, experiences["normal"].Max);
        }

        // Some bots have -1/-1, shortcut result

        if (result.Max == -1)
        {
            return -1;
        }

        return randomUtil.GetInt(result.Min, result.Max);
    }

    /// <summary>
    ///     Get the standing value change when player kills a bot
    /// </summary>
    /// <param name="standingsForKill">Dictionary of standing values keyed by bot difficulty</param>
    /// <param name="botDifficulty">Difficulty of bot to look up</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Standing change value</returns>
    protected double GetStandingChangeForKillByDifficulty(Dictionary<string, double> standingsForKill, string botDifficulty, string role)
    {
        if (standingsForKill.TryGetValue(botDifficulty.ToLowerInvariant(), out var result))
        {
            return result;
        }

        logger.Debug($"Unable to find 'standing for kill' value for: {role} {botDifficulty}, using `normal` value");

        return standingsForKill["normal"];
    }

    /// <summary>
    ///     Get the aggressor bonus value when player kills a bot
    /// </summary>
    /// <param name="aggressorBonuses">Dictionary of standing values keyed by bot difficulty</param>
    /// <param name="botDifficulty">Difficulty of bot to look up</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Standing change value</returns>
    protected double GetAggressorBonusByDifficulty(Dictionary<string, double> aggressorBonuses, string botDifficulty, string role)
    {
        if (aggressorBonuses.TryGetValue(botDifficulty.ToLowerInvariant(), out var result))
        {
            return result;
        }

        logger.Debug($"Unable to find 'aggressor bonus for kill' value for: {role} {botDifficulty}, using `normal` value");

        return aggressorBonuses["normal"];
    }

    /// <summary>
    ///     Unheard PMCs need their pockets expanded
    /// </summary>
    /// <param name="botJsonTemplate">Bot data to adjust</param>
    protected void AddAdditionalPocketLootWeightsForUnheardBot(BotType botJsonTemplate)
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
    protected void RemoveBlacklistedLootFromBotTemplate(BotTypeInventory botInventory)
    {
        var containersToProcess = new List<Dictionary<MongoId, double>>
        {
            botInventory.Items.Backpack,
            botInventory.Items.Pockets,
            botInventory.Items.TacticalVest,
        };

        foreach (var container in containersToProcess)
        {
            if (!container.Any())
            {
                // Nothing in container, skip
                continue;
            }

            // Create list of blacklisted keys to remove
            var keysToRemove = container
                .Where(item => itemFilterService.IsLootableItemBlacklisted(item.Key))
                .Select(item => item.Key)
                .ToList();

            // Remove from container by key
            foreach (var key in keysToRemove)
            {
                container.Remove(key);
            }
        }
    }

    /// <summary>
    ///     Choose various appearance settings for a bot using weights: head/body/feet/hands
    /// </summary>
    /// <param name="bot">Bot to adjust</param>
    /// <param name="appearance">Appearance settings to choose from</param>
    /// <param name="botGenerationDetails">Generation details</param>
    protected void SetBotAppearance(BotBase bot, Appearance appearance, BotGenerationDetails botGenerationDetails)
    {
        // Choose random values by weight
        bot.Customization.Head = weightedRandomHelper.GetWeightedValue(appearance.Head);
        bot.Customization.Feet = weightedRandomHelper.GetWeightedValue(appearance.Feet);
        bot.Customization.Body = weightedRandomHelper.GetWeightedValue(appearance.Body);

        var bodyGlobalDictDb = databaseService.GetGlobals().Configuration.Customization.Body;
        var chosenBodyTemplate = databaseService.GetCustomization()[bot.Customization.Body.Value];

        // Some bodies have matching hands, look up body to see if this is the case
        var chosenBody = bodyGlobalDictDb.FirstOrDefault(c => c.Key == chosenBodyTemplate?.Name.Trim());
        bot.Customization.Hands =
            chosenBody.Value?.IsNotRandom ?? false
                ? chosenBody.Value.Hands // Has fixed hands for chosen body, update to match
                : weightedRandomHelper.GetWeightedValue(appearance.Hands); // Hands can be random, choose any from weighted dict
    }

    /// <summary>
    ///     Converts health object to the required format
    /// </summary>
    /// <param name="healthObj">health object from bot json</param>
    /// <param name="playerScav">Is a pscav bot being generated</param>
    /// <returns>Health object</returns>
    protected BotBaseHealth GenerateHealth(BotTypeHealth healthObj, bool playerScav = false)
    {
        var bodyParts = playerScav ? GetLowestHpBodyPart(healthObj.BodyParts) : randomUtil.GetArrayValue(healthObj.BodyParts);

        BotBaseHealth health = new()
        {
            Hydration = new CurrentMinMax
            {
                Current = randomUtil.GetDouble(healthObj.Hydration.Min, healthObj.Hydration.Max),
                Maximum = healthObj.Hydration.Max,
            },
            Energy = new CurrentMinMax
            {
                Current = randomUtil.GetDouble(healthObj.Energy.Min, healthObj.Energy.Max),
                Maximum = healthObj.Energy.Max,
            },
            Temperature = new CurrentMinMax
            {
                Current = randomUtil.GetDouble(healthObj.Temperature.Min, healthObj.Temperature.Max),
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
                            Current = randomUtil.GetDouble(bodyParts.Head.Min, bodyParts.Head.Max),
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
                            Current = randomUtil.GetDouble(bodyParts.Chest.Min, bodyParts.Chest.Max),
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
                            Current = randomUtil.GetDouble(bodyParts.Stomach.Min, bodyParts.Stomach.Max),
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
                            Current = randomUtil.GetDouble(bodyParts.LeftArm.Min, bodyParts.LeftArm.Max),
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
                            Current = randomUtil.GetDouble(bodyParts.RightArm.Min, bodyParts.RightArm.Max),
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
                            Current = randomUtil.GetDouble(bodyParts.LeftLeg.Min, bodyParts.LeftLeg.Max),
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
                            Current = randomUtil.GetDouble(bodyParts.RightLeg.Min, bodyParts.RightLeg.Max),
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
    ///     Get the bodyPart with the lowest hp
    /// </summary>
    /// <param name="bodyParts">Body parts</param>
    /// <returns>Part with the lowest hp</returns>
    protected BodyPart? GetLowestHpBodyPart(IEnumerable<BodyPart> bodyParts)
    {
        if (!bodyParts.Any())
        {
            return null;
        }

        return bodyParts
            .Select(bp => new
            {
                BodyPart = bp,
                TotalMaxHp = bp.Head.Max + bp.Chest.Max + bp.LeftArm.Max + bp.RightArm.Max + bp.LeftLeg.Max + bp.RightLeg.Max,
            })
            .OrderBy(x => x.TotalMaxHp)
            .FirstOrDefault()
            ?.BodyPart;
    }

    /// <summary>
    ///     Get a bots skills with randomised progress value between the min and max values
    /// </summary>
    /// <param name="botSkills">Skills that should have their progress value randomised</param>
    /// <returns>Skills</returns>
    protected Skills GenerateSkills(BotDbSkills botSkills)
    {
        var skillsToReturn = new Skills
        {
            Common = GetCommonSkillsWithRandomisedProgressValue(botSkills.Common),
            Mastering = GetMasteringSkillsWithRandomisedProgressValue(botSkills.Mastering),
            Points = 0,
        };

        return skillsToReturn;
    }

    /// <summary>
    ///     Randomise the progress value of passed in skills based on the min/max value
    /// </summary>
    /// <param name="skills">Skills to randomise</param>
    /// <returns>Skills with randomised progress values as a collection</returns>
    protected List<CommonSkill> GetCommonSkillsWithRandomisedProgressValue(Dictionary<string, MinMax<double>> skills)
    {
        return skills
            .Select(kvp =>
            {
                // Get skill from dict, skip if not found
                var skill = kvp.Value;
                if (skill == null)
                {
                    return null;
                }

                return new CommonSkill
                {
                    Id = Enum.Parse<SkillTypes>(kvp.Key),
                    Progress = randomUtil.GetDouble(skill.Min, skill.Max),
                    PointsEarnedDuringSession = 0,
                    LastAccess = 0,
                };
            })
            .Where(baseSkill => baseSkill != null)
            .ToList();
    }

    /// <summary>
    /// Randomise the progress value of passed in skills based on the min/max value
    /// </summary>
    /// <param name="masteringSkills">Skills to randomise</param>
    /// <returns>Skills with randomised progress values as a collection</returns>
    protected List<MasterySkill> GetMasteringSkillsWithRandomisedProgressValue(Dictionary<string, MinMax<double>>? masteringSkills)
    {
        if (masteringSkills is null)
        {
            return [];
        }

        return masteringSkills
            .Select(kvp =>
            {
                // Get skill from dict, skip if not found
                var skill = kvp.Value;
                if (skill == null)
                {
                    return null;
                }

                // All skills have id and progress props
                return new MasterySkill { Id = kvp.Key, Progress = randomUtil.GetDouble(skill.Min, skill.Max) };
            })
            .Where(baseSkill => baseSkill != null)
            .ToList();
    }

    /// <summary>
    ///     Generate an id+aid for a bot and apply
    /// </summary>
    /// <param name="bot">bot to update</param>
    /// <returns></returns>
    protected void AddIdsToBot(BotBase bot)
    {
        bot.Id = new MongoId();
        bot.Aid = 0;
    }

    /// <summary>
    ///     Update a profiles profile.Inventory.equipment value with a freshly generated one.
    ///     Update all inventory items that make use of this value too.
    /// </summary>
    /// <param name="profile">Profile to update</param>
    protected void GenerateInventoryId(BotBase profile)
    {
        var newInventoryItemId = new MongoId();

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
    protected string SetRandomisedGameVersionAndCategory(Info botInfo)
    {
        // Special case
        if (string.Equals(botInfo.Nickname, "nikita", StringComparison.OrdinalIgnoreCase))
        {
            botInfo.GameVersion = GameEditions.UNHEARD;
            botInfo.MemberCategory = MemberCategory.Developer;

            return botInfo.GameVersion;
        }

        // Choose random weighted game version for bot
        botInfo.GameVersion = weightedRandomHelper.GetWeightedValue(PMCConfig.GameVersionWeight);

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
                botInfo.MemberCategory = weightedRandomHelper.GetWeightedValue(PMCConfig.AccountTypeWeight);
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
    protected void AddDogtagToBot(BotBase bot)
    {
        Item inventoryItem = new()
        {
            Id = new MongoId(),
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
    protected MongoId GetDogtagTplByGameVersionAndSide(string side, string gameVersion)
    {
        PMCConfig.DogtagSettings.TryGetValue(side.ToLower(), out var gameVersionWeights);
        if (!gameVersionWeights.TryGetValue(gameVersion, out var possibleDogtags))
        {
            gameVersionWeights.TryGetValue("default", out possibleDogtags);
        }

        return weightedRandomHelper.GetWeightedValue(possibleDogtags);
    }
}
