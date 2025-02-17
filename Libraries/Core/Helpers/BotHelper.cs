using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers;

[Injectable]
public class BotHelper(
    ISptLogger<BotHelper> _logger,
    DatabaseService _databaseService,
    RandomUtil _randomUtil,
    ConfigServer _configServer
)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();
    protected HashSet<string?> _pmcNames = ["usec", "bear", "pmc", "pmcbear", "pmcusec"];

    /// <summary>
    ///     Get a template object for the specified botRole from bots.types db
    /// </summary>
    /// <param name="role">botRole to get template for</param>
    /// <returns>BotType object</returns>
    public BotType? GetBotTemplate(string role)
    {
        if (!_databaseService.GetBots().Types.TryGetValue(role?.ToLower(), out var bot))
        {
            _logger.Error($"Unable to get bot of type: {role} from DB");

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
        return _pmcNames.Contains(botRole?.ToLower());
    }

    public bool IsBotBoss(string botRole)
    {
        return !IsBotFollower(botRole) && _botConfig.Bosses.Any(x => string.Equals(x, botRole, StringComparison.CurrentCultureIgnoreCase));
    }

    public bool IsBotFollower(string botRole)
    {
        return botRole?.StartsWith("follower", StringComparison.CurrentCultureIgnoreCase) ?? false;
    }

    public bool IsBotZombie(string botRole)
    {
        return botRole?.StartsWith("infected", StringComparison.CurrentCultureIgnoreCase) ?? false;
    }

    /// <summary>
    ///     Add a bot to the FRIENDLY_BOT_TYPES list
    /// </summary>
    /// <param name="difficultySettings">bot settings to alter</param>
    /// <param name="typeToAdd">bot type to add to friendly list</param>
    public void AddBotToFriendlyList(DifficultyCategories difficultySettings, string typeToAdd)
    {
        var friendlyBotTypesKey = "FRIENDLY_BOT_TYPES";

        // Null guard
        if (difficultySettings.Mind[friendlyBotTypesKey] is null)
        {
            difficultySettings.Mind[friendlyBotTypesKey] = new List<string>();
        }

        ((List<string>) difficultySettings.Mind[friendlyBotTypesKey]).Add(typeToAdd);
    }

    /// <summary>
    ///     Add a bot to the REVENGE_BOT_TYPES list
    /// </summary>
    /// <param name="difficultySettings">bot settings to alter</param>
    /// <param name="typesToAdd">bot type to add to revenge list</param>
    public void AddBotToRevengeList(DifficultyCategories difficultySettings, string[] typesToAdd)
    {
        var revengePropKey = "REVENGE_BOT_TYPES";

        // Nothing to add
        if (typesToAdd is null)
        {
            return;
        }

        // Null guard
        if (difficultySettings.Mind[revengePropKey] is null)
        {
            difficultySettings.Mind[revengePropKey] = new List<string>();
        }

        var revengeArray = (List<string>) difficultySettings.Mind[revengePropKey];
        foreach (var botTypeToAdd in typesToAdd)
        {
            if (!revengeArray.Contains(botTypeToAdd))
            {
                revengeArray.Add(botTypeToAdd);
            }
        }
    }

    /// <summary>
    ///     is the provided role a PMC, case-agnostic
    /// </summary>
    /// <param name="botRole">Role to check</param>
    /// <returns>True if role is PMC</returns>
    public bool BotRoleIsPmc(string botRole)
    {
        HashSet<string> listToCheck = [_pmcConfig.UsecType.ToLower(), _pmcConfig.BearType.ToLower()];
        return listToCheck.Contains(
            botRole.ToLower()
        );
    }

    /// <summary>
    ///     Get randomization settings for bot from config/bot.json
    /// </summary>
    /// <param name="botLevel">level of bot</param>
    /// <param name="botEquipConfig">bot equipment json</param>
    /// <returns>RandomisationDetails</returns>
    public RandomisationDetails GetBotRandomizationDetails(int botLevel, EquipmentFilters botEquipConfig)
    {
        // No randomisation details found, skip
        if (botEquipConfig is null || botEquipConfig.Randomisation is null)
        {
            return null;
        }

        return botEquipConfig.Randomisation.FirstOrDefault(
            randDetails => botLevel >= randDetails.LevelRange.Min && botLevel <= randDetails.LevelRange.Max
        );
    }

    /// <summary>
    ///     Choose between pmcBEAR and pmcUSEC at random based on the % defined in pmcConfig.isUsec
    /// </summary>
    /// <returns>pmc role</returns>
    public string GetRandomizedPmcRole()
    {
        return _randomUtil.GetChance100(_pmcConfig.IsUsec) ? _pmcConfig.UsecType : _pmcConfig.BearType;
    }

    /// <summary>
    ///     Get the corresponding side when pmcBEAR or pmcUSEC is passed in
    /// </summary>
    /// <param name="botRole">role to get side for</param>
    /// <returns>side (usec/bear)</returns>
    public string GetPmcSideByRole(string botRole)
    {
        if (_pmcConfig.BearType.ToLower() == botRole.ToLower())
        {
            return "Bear";
        }

        if (_pmcConfig.UsecType.ToLower() == botRole.ToLower())
        {
            return "Usec";
        }

        return GetRandomizedPmcSide();
    }

    /// <summary>
    ///     Get a randomized PMC side based on bot config value 'isUsec'
    /// </summary>
    /// <returns>pmc side as string</returns>
    protected string GetRandomizedPmcSide()
    {
        return _randomUtil.GetChance100(_pmcConfig.IsUsec) ? "Usec" : "Bear";
    }

    /// <summary>
    ///     Get a name from a PMC that fits the desired length
    /// </summary>
    /// <param name="maxLength">Max length of name, inclusive</param>
    /// <param name="side">OPTIONAL - what side PMC to get name from (usec/bear)</param>
    /// <returns>name of PMC</returns>
    public string GetPmcNicknameOfMaxLength(int maxLength, string side = null)
    {
        var randomType = side is not null ? side : _randomUtil.GetInt(0, 1) == 0 ? "usec" : "bear";
        var allNames = _databaseService.GetBots().Types[randomType.ToLower()].FirstNames;
        var filteredNames = allNames.Where(name => name.Length <= maxLength);
        if (filteredNames.Count() == 0)
        {
            _logger.Warning(
                $"Unable to filter: {randomType} PMC names to only those under: {maxLength}, none found that match that criteria, selecting from entire name pool instead`,\n"
            );

            return _randomUtil.GetStringCollectionValue(allNames);
        }

        return _randomUtil.GetStringCollectionValue(filteredNames);
    }
}
