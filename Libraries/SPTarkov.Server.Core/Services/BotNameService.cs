using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotNameService(
    ISptLogger<BotNameService> logger,
    BotHelper botHelper,
    RandomUtil randomUtil,
    ServerLocalisationService serverLocalisationService,
    DatabaseService databaseService,
    ConfigServer configServer
)
{
    protected readonly Lock _lockObject = new();
    protected readonly BotConfig _botConfig = configServer.GetConfig<BotConfig>();
    protected readonly HashSet<string> _usedNameCache = [];

    /// <summary>
    ///     Clear out generated pmc names from cache
    /// </summary>
    public void ClearNameCache()
    {
        lock (_lockObject)
        {
            _usedNameCache.Clear();
        }
    }

    /// <summary>
    ///     Create a unique bot nickname
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
        HashSet<string>? uniqueRoles = null
    )
    {
        var isPmc = botGenerationDetails.IsPmc;

        // Never show for players
        var showTypeInNickname =
            !botGenerationDetails.IsPlayerScav && _botConfig.ShowTypeInNickname;
        var roleShouldBeUnique = uniqueRoles?.Contains(botRole.ToLowerInvariant());

        var attempts = 0;
        while (attempts <= 5)
        {
            // Get bot name with leading/trailing whitespace removed
            var name = isPmc // Explicit handling of PMCs, all other bots will get "first_name last_name"
                ? botHelper.GetPmcNicknameOfMaxLength(
                    _botConfig.BotNameLengthLimit,
                    botGenerationDetails.Side
                )
                : $"{randomUtil.GetArrayValue(botJsonTemplate.FirstNames)} {(botJsonTemplate.LastNames.Count > 0 ? randomUtil.GetArrayValue(botJsonTemplate.LastNames) : "")}";

            name = name.Trim();

            // Config is set to add role to end of bot name
            if (showTypeInNickname)
            {
                name += $" {botRole}";
            }

            // Replace pmc bot names with player name + prefix
            if (botGenerationDetails.IsPmc && botGenerationDetails.AllPmcsHaveSameNameAsPlayer)
            {
                var prefix = serverLocalisationService.GetRandomTextThatMatchesPartialKey(
                    "pmc-name_prefix_"
                );
                name = $"{prefix} {name}";
            }

            // Is this a role that must be unique
            if (roleShouldBeUnique.GetValueOrDefault(false))
            // Check name in cache
            {
                if (CacheContainsName(name))
                {
                    // Not unique
                    if (attempts >= 5)
                    {
                        // 5 attempts to generate a name, pool probably isn't big enough
                        var genericName =
                            $"{botGenerationDetails.Side} {randomUtil.GetInt(100000, 999999)}";
                        if (logger.IsLogEnabled(LogLevel.Debug))
                        {
                            logger.Debug(
                                $"Failed to find unique name for: {botRole} {botGenerationDetails.Side} after 5 attempts, using: {genericName}"
                            );
                        }

                        return genericName;
                    }

                    attempts++;

                    // Try again
                    continue;
                }
            }

            // Add bot name to cache to prevent being used again
            AddNameToCache(name);

            return name;
        }

        // Should never reach here
        return $"BOT {botRole} {botGenerationDetails.BotDifficulty}";
    }

    private bool AddNameToCache(string name)
    {
        lock (_lockObject)
        {
            return _usedNameCache.Add(name);
        }
    }

    protected bool CacheContainsName(string name)
    {
        lock (_lockObject)
        {
            return _usedNameCache.Contains(name);
        }
    }

    /// <summary>
    ///     Add random PMC name to bots MainProfileNickname property
    /// </summary>
    /// <param name="bot">Bot to update</param>
    public void AddRandomPmcNameToBotMainProfileNicknameProperty(BotBase bot)
    {
        // Simulate bot looking like a player scav with the PMC name in brackets.
        // E.g. "ScavName (PMC Name)"
        bot.Info.MainProfileNickname = GetRandomPmcName();
    }

    /// <summary>
    ///     Choose a random PMC name from bear or usec bot jsons
    /// </summary>
    /// <returns>PMC name as string</returns>
    protected string GetRandomPmcName()
    {
        var bots = databaseService.GetBots().Types;

        var pmcNames = new List<string>();
        pmcNames.AddRange(bots["usec"].FirstNames);
        pmcNames.AddRange(bots["bear"].FirstNames);

        return randomUtil.GetArrayValue(pmcNames);
    }
}
