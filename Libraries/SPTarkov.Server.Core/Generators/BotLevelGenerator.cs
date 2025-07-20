using System.Globalization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class BotLevelGenerator(
    ISptLogger<BotLevelGenerator> logger,
    RandomUtil randomUtil,
    DatabaseService databaseService
)
{
    /// <summary>
    ///     Return a randomised bot level and exp value
    /// </summary>
    /// <param name="levelDetails">Min and max of level for bot</param>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="bot">Bot the level is being generated for</param>
    /// <returns>IRandomisedBotLevelResult object</returns>
    public RandomisedBotLevelResult GenerateBotLevel(
        MinMax<int> levelDetails,
        BotGenerationDetails botGenerationDetails,
        BotBase bot
    )
    {
        if (!botGenerationDetails.IsPmc)
        {
            return new RandomisedBotLevelResult { Exp = 0, Level = 1 };
        }

        var expTable = databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable;
        var botLevelRange = GetRelativePmcBotLevelRange(
            botGenerationDetails,
            levelDetails,
            expTable.Length
        );

        // Get random level based on the exp table.
        var exp = 0;
        var level = int.Parse(
            ChooseBotLevel(botLevelRange.Min, botLevelRange.Max, 1, 1.15)
                .ToString(CultureInfo.InvariantCulture)
        ); // TODO - nasty double to string to int conversion
        for (var i = 0; i < level; i++)
        {
            exp += expTable[i].Experience;
        }

        // Sprinkle in some random exp within the level, unless we are at max level.
        if (level < expTable.Length - 1)
        {
            exp += randomUtil.GetInt(0, expTable[level].Experience - 1);
        }

        return new RandomisedBotLevelResult { Level = level, Exp = exp };
    }

    public double ChooseBotLevel(double min, double max, int shift, double number)
    {
        return randomUtil.GetBiasedRandomNumber(min, max, shift, number);
    }

    /// <summary>
    ///     Return the min and max level a PMC can be
    /// </summary>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="levelDetails"></param>
    /// <param name="maxAvailableLevel">Max level allowed</param>
    /// <returns>A MinMax of the lowest and highest level to generate the bots</returns>
    public MinMax<int> GetRelativePmcBotLevelRange(
        BotGenerationDetails botGenerationDetails,
        MinMax<int> levelDetails,
        int maxAvailableLevel
    )
    {
        var levelOverride = botGenerationDetails.LocationSpecificPmcLevelOverride;

        // Create a min limit PMCs level cannot fall below
        var minPossibleLevel = levelOverride is not null
            ? Math.Min(
                Math.Max(levelDetails.Min, levelOverride.Min), // Biggest between json min and the botgen min
                maxAvailableLevel // Fallback if value above is crazy
            )
            : Math.Clamp(levelDetails.Min, 1, maxAvailableLevel); // Not pmc with override or non-pmc

        // Create a max limit PMCs level cannot go above
        var maxPossibleLevel = levelOverride is not null
            ? Math.Min(levelOverride.Max, maxAvailableLevel) // Is PMC and have a level override
            : Math.Min(levelDetails.Max, maxAvailableLevel); // Not pmc with override or non-pmc

        // Get min level relative to player if value exists
        var minLevel = botGenerationDetails.PlayerLevel.HasValue
            ? botGenerationDetails.PlayerLevel.Value
                - botGenerationDetails.BotRelativeLevelDeltaMin.Value
            : 1 - botGenerationDetails.BotRelativeLevelDeltaMin.Value;

        // Get max level relative to player if value exists
        var maxLevel = botGenerationDetails.PlayerLevel.HasValue
            ? botGenerationDetails.PlayerLevel.Value
                + botGenerationDetails.BotRelativeLevelDeltaMax.Value
            : 1 + botGenerationDetails.BotRelativeLevelDeltaMin.Value;

        // Bound the level to the min/max possible
        maxLevel = Math.Clamp(maxLevel, minPossibleLevel, maxPossibleLevel);
        minLevel = Math.Clamp(minLevel, minPossibleLevel, maxPossibleLevel);

        return new MinMax<int>(minLevel, maxLevel);
    }
}
