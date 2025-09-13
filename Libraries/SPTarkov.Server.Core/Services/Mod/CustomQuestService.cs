using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Services.Mod;

[Injectable]
public class CustomQuestService(
    DatabaseService databaseService,
    ConfigServer configServer,
    ServerLocalisationService serverLocalisationService
)
{
    /// <summary>
    ///     Create a new custom quest from a NewQuestDetails object.
    /// </summary>
    /// <param name="newQuestDetails">Quest details to be used for creation</param>
    /// <returns>Result of the quest creation, remember to check it for errors!</returns>
    public CreateQuestResult CreateQuest(NewQuestDetails newQuestDetails)
    {
        var quest = newQuestDetails.NewQuest;
        var result = new CreateQuestResult(false, newQuestDetails.NewQuest.Id);

        var databaseQuests = databaseService.GetTables().Templates.Quests;
        if (!databaseQuests.TryAdd(quest.Id, quest))
        {
            result.Errors.Add(serverLocalisationService.GetText("custom-quest-service_quest_id_already_exists", quest.Id));
            return result;
        }

        var locales = newQuestDetails.Locales;
        if (locales.Count == 0)
        {
            result.Errors.Add(serverLocalisationService.GetText("custom-quest-service_no_languages_for_quest", quest.Id));
            return result;
        }

        AddQuestLocales(locales, result);

        var side = newQuestDetails.LockedToSide;
        if (side.HasValue)
        {
            RestrictQuestSide(quest.Id, side.Value, result);
        }

        // No errors mean success
        result.Success = result.Errors.Count == 0;
        return result;
    }

    /// <summary>
    ///     Adds quest locales to the database
    /// </summary>
    /// <param name="locales">locales to add</param>
    /// <param name="result">create quest result</param>
    private void AddQuestLocales(Dictionary<string, Dictionary<string, string>> locales, CreateQuestResult result)
    {
        var globalLocales = databaseService.GetLocales().Global;

        foreach (var (languageKey, entries) in locales)
        {
            if (entries.Count == 0)
            {
                result.Errors?.Add(serverLocalisationService.GetText("custom-quest-service_no_entries_for_language", languageKey));
                continue;
            }

            if (!globalLocales.TryGetValue(languageKey, out var lazyLoadedLocales))
            {
                result.Errors?.Add(serverLocalisationService.GetText("custom-quest-service_could_not_find_language_key", languageKey));
                continue;
            }

            lazyLoadedLocales.AddTransformer(localeData =>
            {
                if (localeData is null)
                {
                    result.Errors?.Add(serverLocalisationService.GetText("custom-quest-service_locale_data_null", languageKey));
                    return null;
                }

                foreach (var (key, entry) in entries)
                {
                    localeData.TryAdd(key, entry);
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
                result.Errors.Add(serverLocalisationService.GetText("custom-quest-service_invalid_side", result.QuestId));
                break;
        }
    }
}
