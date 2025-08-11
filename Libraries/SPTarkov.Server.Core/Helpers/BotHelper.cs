using System.Collections.Concurrent;
using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class BotHelper(ISptLogger<BotHelper> logger, DatabaseService databaseService, RandomUtil randomUtil, ConfigServer configServer)
{
    private static readonly FrozenSet<string> _pmcTypeIds =
    [
        Sides.Usec.ToLowerInvariant(),
        Sides.Bear.ToLowerInvariant(),
        Sides.PmcBear.ToLowerInvariant(),
        Sides.PmcUsec.ToLowerInvariant(),
    ];

    private readonly BotConfig _botConfig = configServer.GetConfig<BotConfig>();
    private readonly PmcConfig _pmcConfig = configServer.GetConfig<PmcConfig>();
    private readonly ConcurrentDictionary<string, List<string>> _pmcNameCache = new();

    /// <summary>
    ///     Get a template object for the specified botRole from bots.types db
    /// </summary>
    /// <param name="role">botRole to get template for</param>
    /// <returns>BotType object</returns>
    public BotType? GetBotTemplate(string role)
    {
        if (!databaseService.GetBots().Types.TryGetValue(role.ToLowerInvariant(), out var bot))
        {
            logger.Error($"Unable to get bot of type: {role} from DB");

            return null;
        }

        return bot;
    }

    /// <summary>
    ///     Is the passed in bot role a PMC (USEC/Bear/PMC)
    /// </summary>
    /// <param name="botRole">bot role to check</param>
    /// <returns>true if is pmc</returns>
    public bool IsBotPmc(string? botRole)
    {
        return _pmcTypeIds.Contains(botRole?.ToLowerInvariant() ?? string.Empty);
    }

    public bool IsBotBoss(string botRole)
    {
        return !IsBotFollower(botRole) && _botConfig.Bosses.Any(x => string.Equals(x, botRole, StringComparison.CurrentCultureIgnoreCase));
    }

    public bool IsBotFollower(string botRole)
    {
        return botRole.StartsWith("follower", StringComparison.CurrentCultureIgnoreCase);
    }

    public bool IsBotZombie(string botRole)
    {
        return botRole.StartsWith("infected", StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    ///     Add a bot to the FRIENDLY_BOT_TYPES list
    /// </summary>
    /// <param name="difficultySettings">bot settings to alter</param>
    /// <param name="typeToAdd">bot type to add to friendly list</param>
    public void AddBotToFriendlyList(DifficultyCategories difficultySettings, string typeToAdd)
    {
        const string friendlyBotTypesKey = "FRIENDLY_BOT_TYPES";

        // Null guard
        if (!difficultySettings.Mind.ContainsKey(friendlyBotTypesKey))
        {
            difficultySettings.Mind[friendlyBotTypesKey] = new List<string>();
        }

        ((List<string>)difficultySettings.Mind[friendlyBotTypesKey]).Add(typeToAdd);
    }

    /// <summary>
    ///     Add a bot to the REVENGE_BOT_TYPES list
    /// </summary>
    /// <param name="difficultySettings">bot settings to alter</param>
    /// <param name="typesToAdd">bot type to add to revenge list</param>
    public void AddBotToRevengeList(DifficultyCategories difficultySettings, string[] typesToAdd)
    {
        const string revengePropKey = "REVENGE_BOT_TYPES";

        // Nothing to add
        if (typesToAdd.Length == 0)
        {
            return;
        }

        // Null guard
        if (!difficultySettings.Mind.ContainsKey(revengePropKey))
        {
            difficultySettings.Mind[revengePropKey] = new List<string>();
        }

        var revengeArray = (List<string>)difficultySettings.Mind[revengePropKey];
        foreach (var botTypeToAdd in typesToAdd)
        {
            if (!revengeArray.Contains(botTypeToAdd))
            {
                revengeArray.Add(botTypeToAdd);
            }
        }
    }

    /// <summary>
    ///     Get randomization settings for bot from config/bot.json
    /// </summary>
    /// <param name="botLevel">level of bot</param>
    /// <param name="botEquipConfig">bot equipment json</param>
    /// <returns>RandomisationDetails</returns>
    public RandomisationDetails? GetBotRandomizationDetails(int botLevel, EquipmentFilters botEquipConfig)
    {
        // No randomisation details found, skip

        return botEquipConfig.Randomisation?.FirstOrDefault(randDetails =>
            botLevel >= randDetails.LevelRange.Min && botLevel <= randDetails.LevelRange.Max
        );
    }

    /// <summary>
    ///     Get the corresponding side when pmcBEAR or pmcUSEC is passed in
    /// </summary>
    /// <param name="botRole">role to get side for</param>
    /// <returns>side (usec/bear)</returns>
    public string GetPmcSideByRole(string botRole)
    {
        if (string.Equals(_pmcConfig.BearType, botRole, StringComparison.OrdinalIgnoreCase))
        {
            return Sides.Bear;
        }

        if (string.Equals(_pmcConfig.UsecType, botRole, StringComparison.OrdinalIgnoreCase))
        {
            return Sides.Usec;
        }

        return GetRandomizedPmcSide();
    }

    /// <summary>
    ///     Get the corresponding side when pmcBEAR or pmcUSEC is passed in
    /// </summary>
    /// <param name="botRole">role to get side for</param>
    /// <returns>side (usec/bear)</returns>
    public string GetPmcSideByRole(WildSpawnType botRole)
    {
        switch (botRole)
        {
            case WildSpawnType.pmcBEAR:
                return Sides.Bear;
            case WildSpawnType.pmcUSEC:
                return Sides.Usec;
            default:
                return GetRandomizedPmcSide();
        }
    }

    /// <summary>
    ///     Get a randomized PMC side based on bot config value 'isUsec'
    /// </summary>
    /// <returns>pmc side as string</returns>
    protected string GetRandomizedPmcSide()
    {
        return randomUtil.GetChance100(_pmcConfig.IsUsec) ? Sides.Usec : Sides.Bear;
    }

    /// <summary>
    ///     Get a PMC name that fits the desired length
    /// </summary>
    /// <param name="maxLength">Max length of name, inclusive</param>
    /// <param name="side">OPTIONAL - what side PMC to get name from (usec/bear)</param>
    /// <returns>name of PMC</returns>
    public string GetPmcNicknameOfMaxLength(int maxLength, string? side = null)
    {
        var chosenFaction = GetPmcFactionBySide(side);
        var eligibleNames = GetOrAddEligiblePmcNamesFromCache(chosenFaction, maxLength);

        return randomUtil.GetRandomElement(eligibleNames);
    }

    /// <summary>
    /// Choose a faction based on the passed in side (usec/bear) or choose randomly if not provided
    /// </summary>
    /// <param name="side">usec/bear</param>
    /// <returns>usec/bear</returns>
    protected string GetPmcFactionBySide(string? side)
    {
        var chosenFaction = (side ?? (randomUtil.GetInt(0, 1) == 0 ? Sides.Usec : Sides.Bear)).ToLowerInvariant();
        return chosenFaction;
    }

    /// <summary>
    /// Cache Pmcs against their length and faction, return values that match requirement
    /// </summary>
    /// <param name="chosenFaction">bear/usec</param>
    /// <param name="maxLength">Max length of name</param>
    /// <returns>Collection of names</returns>
    protected List<string> GetOrAddEligiblePmcNamesFromCache(string chosenFaction, int maxLength)
    {
        var cacheKey = $"{chosenFaction}{maxLength.ToString()}";
        if (_pmcNameCache.TryGetValue(cacheKey, out var eligibleNames))
        {
            // Exists in cache, return
            return eligibleNames;
        }

        // Not found in cache, generate and store
        var generatedNames = GatherPmcNamesOfLength(chosenFaction, maxLength);
        _pmcNameCache.TryAdd(cacheKey, generatedNames);

        return generatedNames;
    }

    /// <summary>
    /// Get names that match the side and length defined in parameters
    /// </summary>
    /// <param name="chosenFaction">bear/usec</param>
    /// <param name="maxLength">max length of name to return</param>
    /// <returns></returns>
    protected List<string> GatherPmcNamesOfLength(string chosenFaction, int maxLength)
    {
        // Ensure faction is legit before gathering
        if (!databaseService.GetBots().Types.TryGetValue(chosenFaction, out var chosenFactionDetails))
        {
            logger.Error($"Unknown faction: {chosenFaction} Defaulting to: {Sides.Usec}");
            chosenFaction = Sides.Usec.ToLowerInvariant();
            chosenFactionDetails = databaseService.GetBots().Types[chosenFaction];
        }

        var matchingNames = chosenFactionDetails.FirstNames.Where(name => name.Length <= maxLength).ToList();
        if (matchingNames.Count != 0)
        {
            return matchingNames;
        }

        logger.Warning(
            $"Unable to filter: {chosenFaction} PMC names to only those under: {maxLength}, none found that match that criteria, selecting from entire name pool instead"
        );

        // Return a random string from names
        return chosenFactionDetails.FirstNames.ToList();
    }
}
