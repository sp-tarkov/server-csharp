using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RepeatableQuestHelper(
    ISptLogger<RepeatableQuestHelper> _logger,
    ConfigServer _configServer
    )
{
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();

    /// <summary>
    ///     Get the relevant elimination config based on the current players PMC level
    /// </summary>
    /// <param name="pmcLevel">Level of PMC character</param>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>EliminationConfig</returns>
    public EliminationConfig? GetEliminationConfigByPmcLevel(
        int pmcLevel,
        RepeatableQuestConfig repeatableConfig
    )
    {
        return repeatableConfig.QuestConfig.Elimination.FirstOrDefault(x =>
            pmcLevel >= x.LevelRange.Min && pmcLevel <= x.LevelRange.Max
        );
    }

    /// <summary>
    ///     Returns the repeatable template ids for the provided side
    /// </summary>
    /// <param name="playerGroup">Side to get the templates for</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Dictionary<string, string>? GetRepeatableQuestTemplatesByGroup(PlayerGroup playerGroup)
    {
        var templates = _questConfig.RepeatableQuestTemplates;

        return playerGroup switch
        {
            PlayerGroup.Pmc => templates.Pmc,
            PlayerGroup.Scav => templates.Scav,
            _ => throw new ArgumentOutOfRangeException(nameof(playerGroup), playerGroup, null)
        };
    }
}
