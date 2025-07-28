using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class BotDifficultyHelper(
    ISptLogger<BotDifficultyHelper> logger,
    DatabaseService databaseService,
    RandomUtil randomUtil,
    ServerLocalisationService serverLocalisationService,
    BotHelper botHelper,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly PmcConfig _pmcConfig = configServer.GetConfig<PmcConfig>();

    /// <summary>
    ///     Get difficulty settings for desired bot type, if not found use assault bot types
    /// </summary>
    /// <param name="type">bot type to retrieve difficulty of</param>
    /// <param name="desiredDifficulty">difficulty to get settings for (easy/normal etc)</param>
    /// <param name="botDb">bots from database</param>
    /// <returns>Difficulty object</returns>
    public DifficultyCategories GetBotDifficultySettings(string type, string desiredDifficulty, Bots botDb)
    {
        var desiredType = botHelper.IsBotPmc(type) ? botHelper.GetPmcSideByRole(type).ToLowerInvariant() : type.ToLowerInvariant();
        if (!botDb.Types.ContainsKey(desiredType))
        {
            // No bot found, get fallback difficulty values
            logger.Warning(serverLocalisationService.GetText("bot-unable_to_get_bot_fallback_to_assault", type));
            botDb.Types[desiredType] = cloner.Clone(botDb.Types["assault"]);
        }

        // Get settings from raw bot json template file
        var botTemplate = botHelper.GetBotTemplate(desiredType);
        botTemplate.BotDifficulty.TryGetValue(desiredDifficulty, out var difficultySettings);
        if (difficultySettings is null)
        {
            // No bot settings found, use 'assault' bot difficulty instead
            logger.Warning(
                serverLocalisationService.GetText(
                    "bot-unable_to_get_bot_difficulty_fallback_to_assault",
                    new { botType = desiredType, difficulty = desiredDifficulty }
                )
            );
            botDb.Types[desiredType].BotDifficulty[desiredDifficulty] = cloner.Clone(
                botDb.Types["assault"].BotDifficulty[desiredDifficulty]
            );
        }

        return cloner.Clone(difficultySettings);
    }

    /// <summary>
    ///     Get difficulty settings for a PMC
    /// </summary>
    /// <param name="type">"usec" / "bear"</param>
    /// <param name="difficulty">what difficulty to retrieve</param>
    /// <returns>Difficulty object</returns>
    protected DifficultyCategories GetDifficultySettings(string type, string difficulty)
    {
        var difficultySetting = string.Equals(_pmcConfig.Difficulty, "asonline", StringComparison.OrdinalIgnoreCase)
            ? difficulty
            : _pmcConfig.Difficulty.ToLowerInvariant();

        difficultySetting = ConvertBotDifficultyDropdownToBotDifficulty(difficultySetting);

        return cloner.Clone(databaseService.GetBots().Types[type].BotDifficulty[difficultySetting]);
    }

    /// <summary>
    ///     Translate chosen value from pre-raid difficulty dropdown into bot difficulty value
    /// </summary>
    /// <param name="dropDownDifficulty">Dropdown difficulty value to convert</param>
    /// <returns>bot difficulty</returns>
    public string ConvertBotDifficultyDropdownToBotDifficulty(string dropDownDifficulty)
    {
        switch (dropDownDifficulty.ToLowerInvariant())
        {
            case "medium":
                return "normal";
            case "random":
                return ChooseRandomDifficulty();
            default:
                return dropDownDifficulty.ToLowerInvariant();
        }
    }

    /// <summary>
    ///     Choose a random difficulty from - easy/normal/hard/impossible
    /// </summary>
    /// <returns>random difficulty</returns>
    public string ChooseRandomDifficulty()
    {
        return randomUtil.GetArrayValue(["easy", "normal", "hard", "impossible"]);
    }
}
