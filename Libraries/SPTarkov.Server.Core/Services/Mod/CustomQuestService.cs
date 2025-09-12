using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Mods;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Services.Mod;

// TODO: LOCALIZE THE ERRORS

[Injectable]
public class CustomQuestService(ISptLogger<CustomQuestService> logger, DatabaseService databaseService, ConfigServer configServer)
{
    /// <summary>
    ///     Create a new custom quest from a NewQuestDetails object.
    /// </summary>
    /// <param name="newQuestDetails">Quest details to be used for creation</param>
    /// <returns>Result of the quest creation, if this is returned and no exceptions are thrown its safe to assume the quest was added successfully</returns>
    /// <exception cref="NewCustomQuestException">Thrown if the id already exists, or no languages have been added.</exception>
    public CreateQuestResult CreateQuest(NewQuestDetails newQuestDetails)
    {
        var quest = newQuestDetails.NewQuest;
        var result = new CreateQuestResult(false, newQuestDetails.NewQuest.Id, []);

        var databaseQuests = databaseService.GetTables().Templates.Quests;
        if (!databaseQuests.TryAdd(quest.Id, quest))
        {
            result.Errors?.Add($"A quest with the id: {quest.Id.ToString()} already exists.");
            return result;
        }

        var locales = newQuestDetails.Locales;
        if (locales.Count == 0)
        {
            result.Errors?.Add($"No languages have been added for custom quest id: {quest.Id.ToString()}");
            return result;
        }

        AddQuestLocales(locales, result);

        var side = newQuestDetails.LockedToSide;
        if (side.HasValue)
        {
            RestrictQuestSide(quest.Id, side.Value, result);
        }

        // No errors mean success
        result.Success = result.Errors?.Count == 0;
        return result;
    }

    /// <summary>
    ///     TODO: Not implemented
    /// </summary>
    /// <param name="clonedDetails">Cloned quest details to use for quest creation</param>
    /// <returns>Result of the quest creation, if this is returned and no exceptions are thrown its safe to assume the quest was added successfully</returns>
    /// <exception cref="NotImplementedException"></exception>
    public CreateQuestResult CreateQuestFromClone(NewQuestFromCloneDetails clonedDetails)
    {
        throw new NotImplementedException();
    }

    private void AddQuestLocales(Dictionary<string, Dictionary<string, string>> locales, CreateQuestResult result)
    {
        var globalLocales = databaseService.GetLocales().Global;

        foreach (var (languageKey, entries) in locales)
        {
            if (entries.Count == 0)
            {
                result.Errors?.Add($"No locale entries have been added for language key: {languageKey}, was this intentional?");
                continue;
            }

            if (!globalLocales.TryGetValue(languageKey, out var lazyLoadedLocales))
            {
                result.Errors?.Add(
                    $"Could not find language key: {languageKey} in global locales when adding a custom quest. This is either a typo, or this language is not supported."
                );
                continue;
            }

            lazyLoadedLocales.AddTransformer(localeData =>
            {
                if (localeData is null)
                {
                    result.Errors?.Add($"Locale data is null for language: {languageKey}");
                    return null;
                }

                foreach (var entry in entries)
                {
                    localeData[entry.Key] = entry.Value;
                }

                return localeData;
            });
        }
    }

    /// <summary>
    ///     Restricts a custom quest to a specific side.
    /// </summary>
    /// <param name="questId">Quest id to restrict</param>
    /// <param name="side">Side to restrict it to</param>
    /// <param name="result">Result of the quest creation</param>
    /// <exception cref="NewCustomQuestException"></exception>
    private void RestrictQuestSide(MongoId questId, PlayerSide side, CreateQuestResult result)
    {
        var questConfig = configServer.GetConfig<QuestConfig>();

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (side)
        {
            case PlayerSide.Usec:
                questConfig.UsecOnlyQuests.Add(questId);
                break;

            case PlayerSide.Bear:
                questConfig.BearOnlyQuests.Add(questId);
                break;

            case PlayerSide.Savage:
                result.Errors?.Add($"QuestId: {questId.ToString()} Savage is not a valid side for a side locked quest.");
                break;
        }
    }
}
