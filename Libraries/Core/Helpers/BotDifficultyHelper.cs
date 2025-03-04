using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;

namespace Core.Helpers;

[Injectable]
public class BotDifficultyHelper(
    ISptLogger<BotDifficultyHelper> _logger,
    DatabaseService _databaseService,
    RandomUtil _randomUtil,
    LocalisationService _localisationService,
    BotHelper _botHelper,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();

    /// <summary>
    ///     Get difficulty settings for desired bot type, if not found use assault bot types
    /// </summary>
    /// <param name="type">bot type to retrieve difficulty of</param>
    /// <param name="desiredDifficulty">difficulty to get settings for (easy/normal etc)</param>
    /// <param name="botDb">bots from database</param>
    /// <returns>Difficulty object</returns>
    public DifficultyCategories GetBotDifficultySettings(string type, string desiredDifficulty, Bots botDb)
    {
        var desiredType = _botHelper.IsBotPmc(type)
            ? _botHelper.GetPmcSideByRole(type).ToLower()
            : type.ToLower();
        if (!botDb.Types.ContainsKey(desiredType))
        {
            // No bot found, get fallback difficulty values
            _logger.Warning(_localisationService.GetText("bot-unable_to_get_bot_fallback_to_assault", type));
            botDb.Types[desiredType] = _cloner.Clone(botDb.Types["assault"]);
        }

        // Get settings from raw bot json template file
        var botTemplate = _botHelper.GetBotTemplate(desiredType);
        botTemplate.BotDifficulty.TryGetValue(desiredDifficulty, out var difficultySettings);
        if (difficultySettings is null)
        {
            // No bot settings found, use 'assault' bot difficulty instead
            _logger.Warning(
                _localisationService.GetText(
                    "bot-unable_to_get_bot_difficulty_fallback_to_assault",
                    new
                    {
                        botType = desiredType,
                        difficulty = desiredDifficulty
                    }
                )
            );
            botDb.Types[desiredType].BotDifficulty[desiredDifficulty] = _cloner.Clone(
                botDb.Types["assault"].BotDifficulty[desiredDifficulty]
            );
        }

        return _cloner.Clone(difficultySettings);
    }

    /// <summary>
    ///     Get difficulty settings for a PMC
    /// </summary>
    /// <param name="type">"usec" / "bear"</param>
    /// <param name="difficulty">what difficulty to retrieve</param>
    /// <returns>Difficulty object</returns>
    protected DifficultyCategories GetDifficultySettings(string type, string difficulty)
    {
        var difficultySetting =
            string.Equals(_pmcConfig.Difficulty, "asonline", StringComparison.OrdinalIgnoreCase)
                ? difficulty
                : _pmcConfig.Difficulty.ToLower();

        difficultySetting = ConvertBotDifficultyDropdownToBotDifficulty(difficultySetting);

        return _cloner.Clone(_databaseService.GetBots().Types[type].BotDifficulty[difficultySetting]);
    }

    /// <summary>
    ///     Translate chosen value from pre-raid difficulty dropdown into bot difficulty value
    /// </summary>
    /// <param name="dropDownDifficulty">Dropdown difficulty value to convert</param>
    /// <returns>bot difficulty</returns>
    public string ConvertBotDifficultyDropdownToBotDifficulty(string dropDownDifficulty)
    {
        switch (dropDownDifficulty.ToLower())
        {
            case "medium":
                return "normal";
            case "random":
                return ChooseRandomDifficulty();
            default:
                return dropDownDifficulty.ToLower();
        }
    }

    /// <summary>
    ///     Choose a random difficulty from - easy/normal/hard/impossible
    /// </summary>
    /// <returns>random difficulty</returns>
    public string ChooseRandomDifficulty()
    {
        return _randomUtil.GetArrayValue(["easy", "normal", "hard", "impossible"]);
    }
}
