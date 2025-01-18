using Core.Annotations;
using Core.Models.Common;
using Core.Models.Eft.Bot;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;


namespace Core.Generators;

[Injectable]
public class BotLevelGenerator(
    ISptLogger<BotLevelGenerator> _logger,
    RandomUtil _randomUtil,
    MathUtil _mathUtil,
    DatabaseService _databaseService
)
{
    /// <summary>
    /// Return a randomised bot level and exp value
    /// </summary>
    /// <param name="levelDetails">Min and max of level for bot</param>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="bot">Bot the level is being generated for</param>
    /// <returns>IRandomisedBotLevelResult object</returns>
    public RandomisedBotLevelResult GenerateBotLevel(MinMax levelDetails, BotGenerationDetails botGenerationDetails, BotBase bot)
    {
        var expTable = _databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable;
        var botLevelRange = GetRelativeBotLevelRange(botGenerationDetails, levelDetails, expTable.Length);

        // Get random level based on the exp table.
        int exp = 0;
        var level = int.Parse(
            ChooseBotLevel(botLevelRange.Min.Value, botLevelRange.Max.Value, 1, 1.15)
                .ToString()
        ); // TODO - nasty double to string to int conversion
        for (var i = 0; i < level; i++)
        {
            exp += expTable[i].Experience.Value;
        }

        // Sprinkle in some random exp within the level, unless we are at max level.
        if (level < expTable.Length - 1)
        {
            exp += _randomUtil.GetInt(0, expTable[level].Experience.Value - 1);
        }

        return new RandomisedBotLevelResult { Level = level, Exp = exp };
    }

    public double ChooseBotLevel(double min, double max, int shift, double number)
    {
        return _randomUtil.GetBiasedRandomNumber(min, max, shift, number);
    }

    /// <summary>
    /// Return the min and max bot level based on a relative delta from the PMC level
    /// </summary>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="levelDetails"></param>
    /// <param name="maxAvailableLevel">Max level allowed</param>
    /// <returns>A MinMax of the lowest and highest level to generate the bots</returns>
    public MinMax GetRelativeBotLevelRange(BotGenerationDetails botGenerationDetails, MinMax levelDetails, int maxAvailableLevel)
    {
        var isPmc = botGenerationDetails.IsPmc.GetValueOrDefault(false);
        var pmcOverride = botGenerationDetails.LocationSpecificPmcLevelOverride;

        var minPossibleLevel = isPmc && pmcOverride is not null
            ? Math.Min(
                Math.Max(levelDetails.Min.Value, pmcOverride.Min.Value), // Biggest between json min and the botgen min
                maxAvailableLevel // Fallback if value above is crazy (default is 79)
            )
            : Math.Min(levelDetails.Min.Value, maxAvailableLevel); // Not pmc with override or non-pmc

        var maxPossibleLevel = isPmc && pmcOverride is not null
            ? Math.Min(pmcOverride.Max.Value, maxAvailableLevel) // Was a PMC and they have a level override
            : Math.Min(levelDetails.Max.Value, maxAvailableLevel); // Not pmc with override or non-pmc

        var minLevel = botGenerationDetails.PlayerLevel.HasValue
            ? botGenerationDetails.PlayerLevel.Value
            : 0 - botGenerationDetails.BotRelativeLevelDeltaMin.Value;
        var maxLevel = botGenerationDetails.PlayerLevel.HasValue
            ? botGenerationDetails.PlayerLevel.Value
            : 0 + botGenerationDetails.BotRelativeLevelDeltaMin.Value;

        // Bound the level to the min/max possible
        maxLevel = Math.Min(Math.Max(maxLevel, minPossibleLevel), maxPossibleLevel);
        minLevel = Math.Min(Math.Max(minLevel, minPossibleLevel), maxPossibleLevel);

        return new MinMax
        {
            Min = minLevel,
            Max = maxLevel,
        };
    }
}
