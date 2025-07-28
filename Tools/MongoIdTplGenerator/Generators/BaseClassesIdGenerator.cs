using System.Text.RegularExpressions;
using MongoIdTplGenerator.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using Path = System.IO.Path;

namespace MongoIdTplGenerator.Generators;

[Injectable]
public class BaseClassesIdGenerator(
    ISptLogger<BaseClassesIdGenerator> logger,
    DatabaseServer databaseServer,
    FileUtil fileUtil,
    LocaleUtil localeUtil
) : IMongoIdGenerator
{
    private string _enumDir;

    private Dictionary<MongoId, TemplateItem> _items;

    public Task Run()
    {
        // Figure out our source and target directories
        var projectDir = Directory.GetParent("./").Parent.Parent.Parent.Parent.Parent;
        _enumDir = Path.Combine(projectDir.FullName, "Libraries", "SPTarkov.Server.Core", "Models", "Enums");
        _items = databaseServer.GetTables().Templates.Items;

        // Generate an object containing all item name to ID associations
        var orderedItemsObject = GenerateItemsObject();

        // Log any changes to enum values, so the source can be updated as required
        LogEnumValueChanges(orderedItemsObject, "BaseClasses", typeof(BaseClasses));
        var itemTplOutPath = Path.Combine(_enumDir, "BaseClasses.cs");
        WriteEnumsToFile(
            itemTplOutPath,
            new Dictionary<string, Dictionary<string, MongoId>> { { nameof(BaseClasses), orderedItemsObject } }
        );

        logger.Info("Generating items finished");

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Return an object containing all items in the game with a generated name
    /// </summary>
    /// <returns>An object containing a generated item name to item ID association</returns>
    private Dictionary<string, MongoId> GenerateItemsObject()
    {
        var itemsObject = new Dictionary<string, MongoId>();
        foreach (var item in _items.Values)
        {
            // Skip invalid items (Non-Item types, and shrapnel)
            if (item.Type != "Node")
            {
                continue;
            }

            var underscoredName = Regex.Replace(item.Name, @"(?<!_|^)([A-Z])", "_$1");
            var itemKey = $"{underscoredName.ToUpperInvariant()}";

            // Strip out any remaining special characters
            itemKey = localeUtil.SanitizeEnumKey(itemKey);

            itemsObject[itemKey] = item.Id;
        }

        // Sort the items object
        var itemList = itemsObject.ToList();
        itemList.Sort((kv1, kv2) => kv1.Key.CompareTo(kv2.Key));
        var orderedItemsObject = itemList.ToDictionary(kv => kv.Key, kv => kv.Value);

        return orderedItemsObject;
    }

    private void LogEnumValueChanges(Dictionary<string, MongoId> data, string enumName, Type originalEnum)
    {
        // First generate a mapping of the original enum values to names
        var originalEnumValues = new Dictionary<string, string>();
        foreach (var field in originalEnum.GetFields())
        {
            originalEnumValues.Add(field.GetValue(null)!.ToString()!, field.Name);
        }

        // Loop through our new data, and find anywhere the given ID's name doesn't match the original enum
        foreach (var kv in data)
        {
            if (originalEnumValues.ContainsKey(kv.Value) && originalEnumValues[kv.Value] != kv.Key)
            {
                logger.Warning($"Enum {enumName} key has changed for {kv.Value}, {originalEnumValues[kv.Value]} => {kv.Key}");
            }
        }
    }

    private void WriteEnumsToFile(string outputPath, Dictionary<string, Dictionary<string, MongoId>> enumEntries)
    {
        var enumFileData =
            "using SPTarkov.Server.Core.Models.Common;\n\n"
            + "// This is an auto generated file, do not modify. Re-generate by running MongoIdTplGenerator.exe";

        foreach (var (enumName, data) in enumEntries)
        {
            enumFileData += $"\npublic static class {enumName}\n{{\n";

            foreach (var (key, value) in data)
            {
                enumFileData += $"    public static readonly MongoId {key} = new MongoId(\"{value}\");\n";
            }

            enumFileData += "}\n";
        }

        fileUtil.WriteFile(outputPath, enumFileData);
    }
}
