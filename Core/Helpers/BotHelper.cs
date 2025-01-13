using Core.Annotations;
using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Services;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Helpers;

[Injectable]
public class BotHelper
{
    private readonly ILogger _logger;
    private readonly DatabaseService _databaseService;

    public BotHelper(
        ILogger logger,
        DatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }
    /// <summary>
    /// Get a template object for the specified botRole from bots.types db
    /// </summary>
    /// <param name="role">botRole to get template for</param>
    /// <returns>BotType object</returns>
    public BotType GetBotTemplate(string role)
    {
        if (!_databaseService.GetBots().Types.TryGetValue(role.ToLower(), out var bot))
        {
            _logger.Error($"Unable to get bot of type: {role} from DB");

            return null;
        }

        return bot;
    }

    /// <summary>
    /// Is the passed in bot role a PMC (usec/bear/pmc)
    /// </summary>
    /// <param name="botRole">bot role to check</param>
    /// <returns>true if is pmc</returns>
    public bool IsBotPmc(string botRole)
    {
        throw new NotImplementedException();
    }

    public bool IsBotBoss(string botRole)
    {
        throw new NotImplementedException();
    }

    public bool IsBotFollower(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a bot to the FRIENDLY_BOT_TYPES list
    /// </summary>
    /// <param name="difficultySettings">bot settings to alter</param>
    /// <param name="typeToAdd">bot type to add to friendly list</param>
    public void AddBotToFriendlyList(DifficultyCategories difficultySettings, string typeToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a bot to the REVENGE_BOT_TYPES list
    /// </summary>
    /// <param name="difficultySettings">bot settings to alter</param>
    /// <param name="typesToAdd">bot type to add to revenge list</param>
    public void AddBotToRevengeList(DifficultyCategories difficultySettings, string[] typesToAdd)
    {
        throw new NotImplementedException();
    }

    public bool RollChanceToBePmc(MinMax botConvertMinMax)
    {
        throw new NotImplementedException();
    }

    protected void GetPmcConversionValuesForLocation(string location)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// is the provided role a PMC, case-agnostic
    /// </summary>
    /// <param name="botRole">Role to check</param>
    /// <returns>True if role is PMC</returns>
    public bool BotRoleIsPmc(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get randomization settings for bot from config/bot.json
    /// </summary>
    /// <param name="botLevel">level of bot</param>
    /// <param name="botEquipConfig">bot equipment json</param>
    /// <returns>RandomisationDetails</returns>
    public RandomisationDetails GetBotRandomizationDetails(int botLevel, EquipmentFilters botEquipConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose between pmcBEAR and pmcUSEC at random based on the % defined in pmcConfig.isUsec
    /// </summary>
    /// <returns>pmc role</returns>
    public string GetRandomizedPmcRole()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the corresponding side when pmcBEAR or pmcUSEC is passed in
    /// </summary>
    /// <param name="botRole">role to get side for</param>
    /// <returns>side (usec/bear)</returns>
    public string GetPmcSideByRole(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a randomized PMC side based on bot config value 'isUsec'
    /// </summary>
    /// <returns>pmc side as string</returns>
    protected string GetRandomizedPmcSide()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a name from a PMC that fits the desired length
    /// </summary>
    /// <param name="maxLength">Max length of name, inclusive</param>
    /// <param name="side">OPTIONAL - what side PMC to get name from (usec/bear)</param>
    /// <returns>name of PMC</returns>
    public string GetPmcNicknameOfMaxLength(int maxLength, string side = null)
    {
        throw new NotImplementedException();
    }
}
