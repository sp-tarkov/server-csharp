using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;

namespace Core.Helpers;

[Injectable]
public class BotDifficultyHelper
{
    /// <summary>
    /// Get difficulty settings for desired bot type, if not found use assault bot types
    /// </summary>
    /// <param name="type">bot type to retrieve difficulty of</param>
    /// <param name="difficulty">difficulty to get settings for (easy/normal etc)</param>
    /// <param name="botDb">bots from database</param>
    /// <returns>Difficulty object</returns>
    public DifficultyCategories GetBotDifficultySettings(string botType, string difficultyLevel, Bots botDatabase)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get difficulty settings for a PMC
    /// </summary>
    /// <param name="type">"usec" / "bear"</param>
    /// <param name="difficulty">what difficulty to retrieve</param>
    /// <returns>Difficulty object</returns>
    protected DifficultyCategories GetDifficultySettings(string botType, string difficultyLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Translate chosen value from pre-raid difficulty dropdown into bot difficulty value
    /// </summary>
    /// <param name="dropDownDifficulty">Dropdown difficulty value to convert</param>
    /// <returns>bot difficulty</returns>
    public string ConvertBotDifficultyDropdownToBotDifficulty(string dropDownDifficultyValue)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a random difficulty from - easy/normal/hard/impossible
    /// </summary>
    /// <returns>random difficulty</returns>
    public string ChooseRandomDifficulty()
    {
        throw new NotImplementedException();
    }
}
