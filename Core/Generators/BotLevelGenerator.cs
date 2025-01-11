using Core.Annotations;
using Core.Models.Common;
using Core.Models.Eft.Bot;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;

namespace Core.Generators;

[Injectable]
public class BotLevelGenerator
{
    public BotLevelGenerator()
    {
    }

    /// <summary>
    /// Return a randomised bot level and exp value
    /// </summary>
    /// <param name="levelDetails">Min and max of level for bot</param>
    /// <param name="botGenerationDetails">Details to help generate a bot</param>
    /// <param name="bot">Bot the level is being generated for</param>
    /// <returns>IRandomisedBotLevelResult object</returns>
    public RandomisedBotLevelResult GenerateBotLevel(MinMax levelDetails, BotGenerationDetails botGenerationDetails, BotBase bot)
    {
        throw new NotImplementedException();
    }

    public int ChooseBotLevel(int min, int max, int shift, int number)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}
