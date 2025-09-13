using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Mods;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services.Mod;

// TODO: LOCALIZE THE ERRORS

[Injectable]
public class CustomQuestService(
    ISptLogger<CustomQuestService> logger,
    DatabaseService databaseService,
    ConfigServer configServer,
    ICloner cloner
)
{
    /// <summary>
    ///     Create a new custom quest from a NewQuestDetails object.
    /// </summary>
    /// <param name="newQuestDetails">Quest details to be used for creation</param>
    /// <returns>Result of the quest creation, remember to check it for errors!</returns>
    /// <exception cref="NewCustomQuestException">Thrown if the id already exists, or no languages have been added.</exception>
    public CreateQuestResult CreateQuest(NewQuestDetails newQuestDetails)
    {
        var quest = newQuestDetails.NewQuest;
        var result = new CreateQuestResult(false, newQuestDetails.NewQuest.Id);

        var databaseQuests = databaseService.GetTables().Templates.Quests;
        if (!databaseQuests.TryAdd(quest.Id, quest))
        {
            result.Errors.Add($"A quest with the id: {quest.Id.ToString()} already exists.");
            return result;
        }

        var locales = newQuestDetails.Locales;
        if (locales.Count == 0)
        {
            result.Errors.Add($"No languages have been added for custom quest id: {quest.Id.ToString()}");
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
    ///     TODO: Not implemented
    /// </summary>
    /// <param name="clonedDetails">Cloned quest details to use for quest creation</param>
    /// <returns>Result of the quest creation, if this is returned and no exceptions are thrown its safe to assume the quest was added successfully</returns>
    /// <exception cref="NotImplementedException"></exception>
    public CreateQuestResult CreateQuestFromClone(NewQuestFromCloneDetails clonedDetails)
    {
        var questTable = databaseService.GetTables().Templates.Quests;
        var result = new CreateQuestResult(false, null);

        if (questTable.ContainsKey(clonedDetails.NewQuestId))
        {
            result.Errors.Add($"A quest with id: {clonedDetails.NewQuestId.ToString()} already exists.");
            return result;
        }

        result.QuestId = clonedDetails.NewQuestId;

        if (!questTable.TryGetValue(clonedDetails.QuestTplToClone, out var quest))
        {
            result.Errors.Add($"Could not find quest: {clonedDetails.QuestTplToClone.ToString()} to clone in the database.");
            return result;
        }

        var questClone = cloner.Clone(quest);
        if (questClone is null)
        {
            result.Errors.Add($"Cloned quest: {quest.Id} was null after cloning. This should never happen. Open an issue.");
            return result;
        }

        questClone.Id = clonedDetails.NewQuestId;
        OverrideQuestData(clonedDetails, questClone, result);

        var side = clonedDetails.LockedToSide;
        var questConfig = configServer.GetConfig<QuestConfig>();

        // No overriden value, use the original quests side lock
        if (!side.HasValue)
        {
            if (questConfig.UsecOnlyQuests.Contains(quest.Id))
            {
                questConfig.UsecOnlyQuests.Add(questClone.Id);
            }

            if (questConfig.BearOnlyQuests.Contains(quest.Id))
            {
                questConfig.BearOnlyQuests.Add(questClone.Id);
            }
        }
        // Use overriden value
        else
        {
            RestrictQuestSide(questClone.Id, side.Value, result);
        }

        return result;
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

                foreach (var (key, entry) in entries)
                {
                    localeData[key] = entry;
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
                result.Errors.Add($"QuestId: {questId.ToString()} Savage is not a valid side for a side locked quest.");
                break;
        }
    }

    /// <summary>
    ///     Overrides properties of the quest.
    /// </summary>
    /// <param name="clonedDetails">Cloned details to pull the new data from</param>
    /// <param name="clonedQuest">Quest to update</param>
    /// <param name="result">Result of the modification</param>
    private void OverrideQuestData(NewQuestFromCloneDetails clonedDetails, Quest clonedQuest, CreateQuestResult result)
    {
        foreach (var member in typeof(Quest).GetMembers())
        {
            switch (member.Name)
            {
                case "Conditions":
                    OverrideQuestConditionMembers(clonedDetails, clonedQuest, result);
                    continue;
                case "Rewards":
                    OverrideQuestRewardMembers(clonedDetails, clonedQuest, result);
                    continue;
            }

            // Get the value for this member from the cloned quest
            var overrideMemberObj = GetMemberObject(member, clonedDetails.QuestOverrideData);
            if (overrideMemberObj is null)
            {
                // Nothing to set
                continue;
            }

            SetMemberObjectValue(member, clonedQuest, overrideMemberObj);
        }
    }

    private void OverrideQuestConditionMembers(NewQuestFromCloneDetails clonedDetails, Quest clonedQuest, CreateQuestResult result) { }

    private void OverrideQuestRewardMembers(NewQuestFromCloneDetails clonedDetails, Quest clonedQuest, CreateQuestResult result) { }

    private object? GetMemberObject(MemberInfo member, object instance)
    {
        return member.MemberType switch
        {
            MemberTypes.Field => ((PropertyInfo)member).GetValue(instance),
            MemberTypes.Property => ((FieldInfo)member).GetValue(instance),
            _ => null,
        };
    }

    private void SetMemberObjectValue(MemberInfo member, object instance, object value)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                ((FieldInfo)member).SetValue(instance, value);
                break;
            case MemberTypes.Property:
                ((PropertyInfo)member).SetValue(instance, value);
                break;
        }
    }
}
