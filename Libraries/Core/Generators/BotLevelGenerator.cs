using Core.Models.Common;
using Core.Models.Eft.Bot;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

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
    ///     Return a randomised bot level and exp value
    /// </summary>
    /// <param name="levelDetails">Min and max of level for bot</param>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="bot">Bot the level is being generated for</param>
    /// <returns>IRandomisedBotLevelResult object</returns>
    public RandomisedBotLevelResult GenerateBotLevel(MinMax levelDetails, BotGenerationDetails botGenerationDetails, BotBase bot)
    {
        if (!botGenerationDetails.IsPmc.GetValueOrDefault(false))
        {
            return new RandomisedBotLevelResult
            {
                Exp = 0,
                Level = 1
            };
        }

        var expTable = _databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable;
        var botLevelRange = GetRelativePmcBotLevelRange(botGenerationDetails, levelDetails, expTable.Length);

        // Get random level based on the exp table.
        var exp = 0;
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

        return new RandomisedBotLevelResult
        {
            Level = level,
            Exp = exp
        };
    }

    public double ChooseBotLevel(double min, double max, int shift, double number)
    {
        return _randomUtil.GetBiasedRandomNumber(min, max, shift, number);
    }

    /// <summary>
    ///     Return the min and max level a PMC can be
    /// </summary>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="levelDetails"></param>
    /// <param name="maxAvailableLevel">Max level allowed</param>
    /// <returns>A MinMax of the lowest and highest level to generate the bots</returns>
    public MinMax GetRelativePmcBotLevelRange(BotGenerationDetails botGenerationDetails, MinMax levelDetails, int maxAvailableLevel)
    {
        var levelOverride = botGenerationDetails.LocationSpecificPmcLevelOverride;

        // Create a min limit PMCs level cannot fall below
        var minPossibleLevel = levelOverride is not null
            ? Math.Min(
                Math.Max(levelDetails.Min.Value, levelOverride.Min.Value), // Biggest between json min and the botgen min
                maxAvailableLevel // Fallback if value above is crazy (default is 79)
            )
            : Math.Min(levelDetails.Min.Value, maxAvailableLevel); // Not pmc with override or non-pmc

        // Create a max limit PMCs level cannot go above
        var maxPossibleLevel = levelOverride is not null
            ? Math.Min(levelOverride.Max.Value, maxAvailableLevel) // Was a PMC and they have a level override
            : Math.Min(levelDetails.Max.Value, maxAvailableLevel); // Not pmc with override or non-pmc

        // Get min level relative to player if value exists
        var minLevel = botGenerationDetails.PlayerLevel.HasValue
            ? botGenerationDetails.PlayerLevel.Value - botGenerationDetails.BotRelativeLevelDeltaMin.Value
            : 1 - botGenerationDetails.BotRelativeLevelDeltaMin.Value;

        // Get max level relative to player if value exists
        var maxLevel = botGenerationDetails.PlayerLevel.HasValue
            ? botGenerationDetails.PlayerLevel.Value + botGenerationDetails.BotRelativeLevelDeltaMax.Value
            : 1 + botGenerationDetails.BotRelativeLevelDeltaMin.Value;

        // Bound the level to the min/max possible
        maxLevel = Math.Min(Math.Max(maxLevel, minPossibleLevel), maxPossibleLevel);
        minLevel = Math.Min(Math.Max(minLevel, minPossibleLevel), maxPossibleLevel);

        return new MinMax(minLevel, maxLevel);
    }
}
