using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Core.Helpers;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotNameService(
    ISptLogger<BotNameService> _logger,
    BotHelper _botHelper,
    RandomUtil _randomUtil,
    LocalisationService _localisationService,
    DatabaseService _databaseService,
    ConfigServer _configServer
)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected HashSet<string> _usedNameCache = new HashSet<string>();

    /// <summary>
    /// Clear out any entries in Name Set
    /// </summary>
    public void ClearNameCache()
    {
        _usedNameCache.Clear();
    }

    /// <summary>
    /// Create a unique bot nickname
    /// </summary>
    /// <param name="botJsonTemplate">bot JSON data from db</param>
    /// <param name="botGenerationDetails"></param>
    /// <param name="botRole">role of bot e.g. assault</param>
    /// <param name="uniqueRoles">Lowercase roles to always make unique</param>
    /// <returns>Nickname for bot</returns>
    public string GenerateUniqueBotNickname(
        BotType botJsonTemplate,
        BotGenerationDetails botGenerationDetails,
        string botRole,
        List<string> uniqueRoles = null)
    {
        var isPmc = botGenerationDetails.IsPmc;

        // Never show for players
        var showTypeInNickname = !botGenerationDetails.IsPlayerScav.GetValueOrDefault(false) && _botConfig.ShowTypeInNickname;
        var roleShouldBeUnique = uniqueRoles?.Contains(botRole.ToLower());

        var attempts = 0;
        while (attempts <= 5)
        {
            // Get bot name with leading/trailing whitespace removed
            var name = (isPmc.GetValueOrDefault(false)) // Explicit handling of PMCs, all other bots will get "first_name last_name"
                ? _botHelper.GetPmcNicknameOfMaxLength(_botConfig.BotNameLengthLimit, botGenerationDetails.Side)
                : $"{_randomUtil.GetArrayValue(botJsonTemplate.FirstNames)} {_randomUtil.GetArrayValue(botJsonTemplate.LastNames)}";

            name = name.Trim();

            // Config is set to add role to end of bot name
            if (showTypeInNickname)
            {
                name += $" {botRole}";
            }

            // Replace pmc bot names with player name + prefix
            if (botGenerationDetails.IsPmc.GetValueOrDefault(false) && botGenerationDetails.AllPmcsHaveSameNameAsPlayer.GetValueOrDefault(false))
            {
                var prefix = _localisationService.GetRandomTextThatMatchesPartialKey("pmc-name_prefix_");
                name = $"{prefix} {name}";
            }

            // Is this a role that must be unique
            if (roleShouldBeUnique.GetValueOrDefault(false))
            {
                // Check name in cache
                if (_usedNameCache.Contains(name))
                {
                    // Not unique
                    if (attempts >= 5)
                    {
                        // 5 attempts to generate a name, pool probably isn't big enough
                        var genericName = $"{botGenerationDetails.Side} {_randomUtil.GetInt(100000, 999999)}";
                        _logger.Debug($"Failed to find unique name for: {botRole} {botGenerationDetails.Side} after 5 attempts, using: {genericName}");

                        return genericName;
                    }

                    attempts++;

                    // Try again
                    continue;
                }
            }

            // Add bot name to cache to prevent being used again
            _usedNameCache.Add(name);

            return name;
        }

        // Should never reach here
        return $"BOT {botRole} {botGenerationDetails.BotDifficulty}";
    }

    /// <summary>
    /// Add random PMC name to bots MainProfileNickname property
    /// </summary>
    /// <param name="bot">Bot to update</param>
    public void AddRandomPmcNameToBotMainProfileNicknameProperty(BotBase bot)
    {
        // Simulate bot looking like a player scav with the PMC name in brackets.
        // E.g. "ScavName (PMC Name)"
        bot.Info.MainProfileNickname = GetRandomPmcName();
    }

    /// <summary>
    /// Choose a random PMC name from bear or usec bot jsons
    /// </summary>
    /// <returns>PMC name as string</returns>
    protected string GetRandomPmcName()
    {
        var bots = _databaseService.GetBots().Types;

        var pmcNames = new List<string>();
        pmcNames.AddRange(bots["usec"].FirstNames);
        pmcNames.AddRange(bots["bear"].FirstNames);

        return _randomUtil.GetArrayValue(pmcNames);
    }
}
