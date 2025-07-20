using MongoIdTplGenerator.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using Path = System.IO.Path;

namespace MongoIdTplGenerator.Generators;

[Injectable]
public class QuestTplMongoIdGenerator(
    ISptLogger<QuestTplMongoIdGenerator> logger,
    DatabaseServer databaseServer,
    LocaleService localeService,
    FileUtil fileUtil,
    LocaleUtil localeUtil
) : IMongoIdGenerator
{
    private string? _enumDir;
    private Dictionary<string, Quest>? _quests;

    public Task Run()
    {
        // Figure out our source and target directories
        var projectDir = Directory.GetParent("./").Parent.Parent.Parent.Parent.Parent;
        _enumDir = Path.Combine(
            projectDir.FullName,
            "Libraries",
            "SPTarkov.Server.Core",
            "Models",
            "Enums"
        );

        _quests = databaseServer.GetTables().Templates.Quests;
        var questTplObject = GenerateQuestTplObject();
        var questTplOutPath = Path.Combine(_enumDir, "QuestTpl.cs");

        WriteEnumToFile(questTplOutPath, questTplObject);

        return Task.CompletedTask;
    }

    private Dictionary<string, string> GenerateQuestTplObject()
    {
        var result = new Dictionary<string, string>();

        foreach (var quest in _quests)
        {
            var id = quest.Key;

            if (QuestOverrides.NameOverridesDictionary.TryGetValue(id, out var nameOverride))
            {
                if (!result.TryAdd(nameOverride, id))
                {
                    logger.Warning(
                        $"Duplicate locale name: {nameOverride} with id: {id} in quest list"
                    );
                }

                continue;
            }

            var locale = localeService
                .GetLocaleDb()[$"{id} name"]
                .Replace(" ", "_")
                .Replace("-", "_");

            locale = localeUtil.SanitizeEnumKey(locale);

            if (!result.TryAdd(locale, id))
            {
                logger.Warning($"Duplicate locale name: {locale} with id: {id} in quest list");
            }
        }

        return result;
    }

    private void WriteEnumToFile(string outputPath, Dictionary<string, string> enumEntries)
    {
        var enumFileData =
            "using SPTarkov.Server.Core.Models.Common;\n\n"
            + "// This is an auto generated file, do not modify. Re-generate by running MongoIdTplGenerator.exe";

        enumFileData += $"\npublic static class QuestTpl\n{{\n";

        foreach (var (enumName, data) in enumEntries)
        {
            enumFileData +=
                $"    public static readonly MongoId {enumName} = new MongoId(\"{data}\");\n";
        }

        enumFileData += "}\n";

        fileUtil.WriteFile(outputPath, enumFileData);
    }
}
