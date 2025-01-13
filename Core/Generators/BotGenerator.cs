using Core.Annotations;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Enums.RaidSettings;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Services;
using Core.Utils.Cloners;
using BodyPart = Core.Models.Eft.Common.Tables.BodyPart;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Generators;

[Injectable]
public class BotGenerator
{
    private readonly ILogger _logger;
    private readonly DatabaseService _databaseService;
    private readonly ICloner _cloner;
    private BotConfig _botConfig;
    private PmcConfig _pmcConfig;

    public BotGenerator(
        ILogger logger,
        DatabaseService databaseService,
        ICloner cloner
        )
    {
        _logger = logger;
        _databaseService = databaseService;
        _cloner = cloner;
    }

    /// <summary>
    /// Generate a player scav bot object
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="role">e.g. assault / pmcbot</param>
    /// <param name="difficulty">easy/normal/hard/impossible</param>
    /// <param name="botTemplate">base bot template to use  (e.g. assault/pmcbot)</param>
    /// <param name="profile">profile of player generating pscav</param>
    /// <returns>BotBase</returns>
    public PmcData GeneratePlayerScav(string sessionId, string role, string difficulty, BotType botTemplate, PmcData profile)
    {
        var bot = GetCloneOfBotBase();
        bot.Info.Settings.BotDifficulty = difficulty;
        bot.Info.Settings.Role = role;
        bot.Info.Side = "Savage";

        var botGenDetails = new BotGenerationDetails{
            IsPmc = false,
            Side = "Savage",
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
            Savage = bot.Savage,
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
            Prestige = new Prestige()
        };
    }

    /// <summary>
    /// Create 1 bot of the type/side/difficulty defined in botGenerationDetails
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="botGenerationDetails">details on how to generate bots</param>
    /// <returns>constructed bot</returns>
    public BotBase PrepareAndGenerateBot(string sessionId, BotGenerationDetails botGenerationDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a clone of the default bot base object and adjust its role/side/difficulty values
    /// </summary>
    /// <param name="botRole">Role bot should have</param>
    /// <param name="botSide">Side bot should have</param>
    /// <param name="difficulty">Difficult bot should have</param>
    /// <returns>Cloned bot base</returns>
    public BotBase GetPreparedBotBase(string botRole, string botSide, string difficulty)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a clone of the database\bots\base.json file
    /// </summary>
    /// <returns>BotBase object</returns>
    public BotBase GetCloneOfBotBase()
    {
        return _cloner.Clone(_databaseService.GetBots().Base);
    }

    /// <summary>
    /// Create a IBotBase object with equipment/loot/exp etc
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="bot">Bots base file</param>
    /// <param name="botJsonTemplate">Bot template from db/bots/x.json</param>
    /// <param name="botGenerationDetails">details on how to generate the bot</param>
    /// <returns>BotBase object</returns>
    public BotBase GenerateBot(string sessionId, BotBase bot, BotType botJsonTemplate, BotGenerationDetails botGenerationDetails)
    {
        _logger.Error("NOT IMPLEMENTED BotGenerator.GenerateBot");

        bot.Inventory.Items = [];

        return bot;
    }

    /// <summary>
    /// Should this bot have a name like "name (Pmc Name)" and be alterd by client patch to be hostile to player
    /// </summary>
    /// <param name="botRole">Role bot has</param>
    /// <returns>True if name should be simulated pscav</returns>
    public bool ShouldSimulatePlayerScav(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get exp for kill by bot difficulty
    /// </summary>
    /// <param name="experience">Dict of difficulties and experience</param>
    /// <param name="botDifficulty">the killed bots difficulty</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Experience for kill</returns>
    public int GetExperienceRewardForKillByDifficulty(Dictionary<string, MinMax> experience, string botDifficulty, string role)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the standing value change when player kills a bot
    /// </summary>
    /// <param name="standingForKill">Dictionary of standing values keyed by bot difficulty</param>
    /// <param name="botDifficulty">Difficulty of bot to look up</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Standing change value</returns>
    public int GetStandingChangeForKillByDifficulty(Dictionary<string, int> standingForKill, string botDifficulty, string role)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the agressor bonus value when player kills a bot
    /// </summary>
    /// <param name="aggressorBonus">Dictionary of standing values keyed by bot difficulty</param>
    /// <param name="botDifficulty">Difficulty of bot to look up</param>
    /// <param name="role">Role of bot (optional, used for error logging)</param>
    /// <returns>Standing change value</returns>
    public int GetAgressorBonusByDifficulty(Dictionary<string, int> aggressorBonus, string botDifficulty, string role)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Set weighting of flagged equipment to 0
    /// </summary>
    /// <param name="botJsonTemplate">Bot data to adjust</param>
    /// <param name="botGenerationDetails">Generation details of bot</param>
    public void FilterBlacklistedGear(BotType botJsonTemplate, BotGenerationDetails botGenerationDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// TODO: Complete Summary
    /// </summary>
    /// <param name="botJsonTemplate">Bot data to adjust</param>
    public void AddAdditionalPocketLootWeightsForUnheardBot(BotType botJsonTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove items from item.json/lootableItemBlacklist from bots inventory
    /// </summary>
    /// <param name="botInventory">Bot to filter</param>
    public void RemoveBlacklistedLootFromBotTemplate(BotTypeInventory botInventory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose various appearance settings for a bot using weights: head/body/feet/hands
    /// </summary>
    /// <param name="bot">Bot to adjust</param>
    /// <param name="appearance">Appearance settings to choose from</param>
    /// <param name="botGenerationDetails">Generation details</param>
    public void SetBotAppearance(BotBase bot, Appearance appearance, BotGenerationDetails botGenerationDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Log the number of PMCs generated to the debug console
    /// </summary>
    /// <param name="output">Generated bot array, ready to send to client</param>
    public void LogPmcGeneratedCount(List<BotBase> output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts health object to the required format
    /// </summary>
    /// <param name="healthObj">health object from bot json</param>
    /// <param name="playerScav">Is a pscav bot being generated</param>
    /// <returns>Health object</returns>
    public Health GenerateHealth(Health healthObj, bool playerScav = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sum up body parts max hp values, return the bodypart collection with lowest value
    /// </summary>
    /// <param name="bodies">Body parts to sum up</param>
    /// <returns>Lowest hp collection</returns>
    public BodyPart? GetLowestHpBody(List<BodyPart> bodies) // TODO: there are two types of body parts
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a bots skills with randomsied progress value between the min and max values
    /// </summary>
    /// <param name="botSkills">Skills that should have their progress value randomised</param>
    /// <returns>Skills</returns>
    public Skills GenerateSkills(BaseJsonSkills botSkills)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise the progress value of passed in skills based on the min/max value
    /// </summary>
    /// <param name="skills">Skills to randomise</param>
    /// <param name="isCommonSkills">Are the skills 'common' skills</param>
    /// <returns>Skills with randomised progress values as an array</returns>
    public List<BaseSkill> GetSkillsWithRandomisedProgressValue(Dictionary<string, BaseSkill> skills, bool isCommonSkills)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate an id+aid for a bot and apply
    /// </summary>
    /// <param name="bot">bot to update</param>
    /// <returns>updated IBotBase object</returns> // TODO: Node server claims this in summary but is void
    public void AddIdsToBot(BotBase bot)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a profiles profile.Inventory.equipment value with a freshly generated one.
    /// Update all inventory items that make use of this value too.
    /// </summary>
    /// <param name="profile">Profile to update</param>
    public void GenerateInventoryId(BotBase profile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise a bots game version and account category.
    /// Chooses from all the game versions (standard, eod etc).
    /// Chooses account type (default, Sherpa, etc).
    /// </summary>
    /// <param name="botInfo">bot info object to update</param>
    /// <returns>Chosen game version</returns>
    public string SetRandomisedGameVersionAndCategory(Info botInfo) // TODO: there are two types of Info
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a side-specific (usec/bear) dogtag item to a bots inventory
    /// </summary>
    /// <param name="bot">bot to add dogtag to</param>
    /// <returns>Bot with dogtag added</returns> // TODO: Node server claims this in summary but is void
    public void AddDogtagToBot(BotBase bot)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a dogtag tpl that matches the bots game version and side
    /// </summary>
    /// <param name="side">Usec/Bear</param>
    /// <param name="gameVersion">edge_of_darkness / standard</param>
    /// <returns>item tpl</returns>
    public string GetDogtagTplByGameVersionAndSide(string side, string gameVersion)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust a PMCs pocket tpl to UHD if necessary, otherwise do nothing
    /// </summary>
    /// <param name="bot">Pmc object to adjust</param>
    public void SetPmcPocketsByGameVersion(BotBase bot)
    {
        throw new NotImplementedException();
    }
}
