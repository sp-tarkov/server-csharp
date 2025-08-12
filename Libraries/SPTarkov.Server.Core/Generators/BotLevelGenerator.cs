using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class BotLevelGenerator(RandomUtil randomUtil, DatabaseService databaseService)
{
    /// <summary>
    ///     Return a randomised bot level and exp value
    /// </summary>
    /// <param name="levelDetails">Min and max of level for bot</param>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="bot">Bot the level is being generated for</param>
    /// <returns>IRandomisedBotLevelResult object</returns>
    public RandomisedBotLevelResult GenerateBotLevel(MinMax<int> levelDetails, BotGenerationDetails botGenerationDetails, BotBase bot)
    {
        if (!botGenerationDetails.IsPmc)
        {
            return new RandomisedBotLevelResult { Exp = 0, Level = 1 };
        }

        var expTable = databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable;
        var botLevelRange = GetRelativePmcBotLevelRange(botGenerationDetails, levelDetails, expTable.Length);

        // ChooseBotLevel now returns int directly
        var level = ChooseBotLevel(botLevelRange.Min, botLevelRange.Max, 1, 1.15);
        var maxLevelIndex = expTable.Length - 1;

        // Clamp chosen level to max
        level = Math.Clamp(level, 0, maxLevelIndex + 1);

        // Sum up total exp required for all full levels before desired
        var baseExp = expTable.Take(level).Sum(entry => entry.Experience);

        // Sprinkle in some random exp within the level, unless we are at max level
        var fractionalExp = level < maxLevelIndex ? randomUtil.GetInt(0, expTable[level].Experience - 1) : 0;

        return new RandomisedBotLevelResult { Exp = baseExp + fractionalExp, Level = level };
    }

    public int ChooseBotLevel(double min, double max, int shift, double number)
    {
        return (int)randomUtil.GetBiasedRandomNumber(min, max, shift, number);
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
        var playerLevel = botGenerationDetails.PlayerLevel ?? 1;

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

        // Get min level relative to player level
        // May be negative, is clamped to 1 below
        var minLevel = playerLevel - botGenerationDetails.BotRelativeLevelDeltaMin;

        // Get max level relative to player level
        var maxLevel = playerLevel + botGenerationDetails.BotRelativeLevelDeltaMax;

        // Clamp the level to the min/max possible
        maxLevel = Math.Clamp(maxLevel, minPossibleLevel, maxPossibleLevel);
        minLevel = Math.Clamp(minLevel, minPossibleLevel, maxPossibleLevel);

        return new MinMax<int>(minLevel, maxLevel);
    }
}
