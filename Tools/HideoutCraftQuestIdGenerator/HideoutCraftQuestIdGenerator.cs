using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using Path = System.IO.Path;

namespace HideoutCraftQuestIdGenerator;

[Injectable]
public class HideoutCraftQuestIdGenerator(
    ISptLogger<HideoutCraftQuestIdGenerator> _logger,
    FileUtil _fileUtil,
    JsonUtil _jsonUtil,
    DatabaseServer _databaseServer,
    ItemHelper _itemHelper,
    DatabaseImporter _databaseImporter
)
{
    private static readonly HashSet<MongoId> _blacklistedProductions =
    [
        "6617cdb6b24b0ea24505f618", // Old event quest production "Radio Repeater" alt recipe
        "66140c4a9688754de10dac07", // Old event quest production "Documents with decrypted data"
        "661e6c26750e453380391f55", // Old event quest production "Documents with decrypted data"
        "660c2dbaa2a92e70cc074863", // Old event quest production "Decrypted flash drive"
        "67093210d514d26f8408612b", // Old event quest production "TG-Vi-24 true vaccine"
    ];

    private static readonly Dictionary<MongoId, MongoId> _forcedQuestToProductionAssociations = new()
    {
        // KEY = PRODUCTION, VALUE = QUEST
        { new MongoId("63a571802116d261d2336cd1"), new MongoId("625d6ffaf7308432be1d44c5") }, // Network Provider - Part 2)
    };

    private readonly Dictionary<MongoId, MongoId> _questProductionMap = new();
    private readonly List<QuestProductionOutput> _questProductionOutputList = [];

    public async Task Run()
    {
        await _databaseImporter.OnLoad();

        // Build up our dataset
        BuildQuestProductionList();
        UpdateProductionQuests();

        // Figure out our source and target directories
        var projectDir = Directory.GetParent("./").Parent.Parent.Parent.Parent.Parent;
        const string productionPath = "Libraries\\SPTarkov.Server.Assets\\SPT_Data\\database\\hideout\\production.json";
        var productionFilePath = Path.Combine(projectDir.FullName, productionPath);

        var updatedProductionJson = _jsonUtil.Serialize(_databaseServer.GetTables().Hideout.Production, true);
        await _fileUtil.WriteFileAsync(productionFilePath, updatedProductionJson);
    }

    // Build a list of all quests and what production they unlock
    private void BuildQuestProductionList()
    {
        foreach (var (questId, quest) in _databaseServer.GetTables().Templates.Quests)
        {
            var combinedRewards = CombineRewards(quest.Rewards).Where(x => x.Type == RewardType.ProductionScheme).ToList();
            foreach (var reward in combinedRewards)
            {
                // Assume all productions only output a single item template
                var output = new QuestProductionOutput
                {
                    QuestId = questId,
                    ItemTemplate = reward.Items[0].Template,
                    Quantity = 0,
                    QuestName = quest.QuestName,
                };

                // Loop over root items only, ignore children
                foreach (var item in reward.Items.Where(x => x.ParentId is null))
                {
                    if (item.Template != output.ItemTemplate)
                    {
                        _logger.Error($"Production scheme has multiple output items. " + $"{output.ItemTemplate} != {item.Template}");

                        continue;
                    }

                    output.Quantity += item.Upd.StackObjectsCount.Value;
                }

                _questProductionOutputList.Add(output);
            }
        }
    }

    private void UpdateProductionQuests()
    {
        // Loop through all productions, and try to associate any with a `QuestComplete` type with its quest
        foreach (var production in _databaseServer.GetTables().Hideout.Production.Recipes)
        {
            // Skip blacklisted productions
            if (_blacklistedProductions.Contains(production.Id))
            {
                _logger.Debug($"Skipped blacklisted production: {production.Id}");
                continue;
            }

            // Look for a 'quest completion' requirement
            var questCompleteRequirements = production.Requirements.Where(req => req.Type == "QuestComplete").ToList();
            if (questCompleteRequirements.Count == 0)
            {
                // Production has no quest requirement
                continue;
            }

            if (questCompleteRequirements.Count > 1)
            {
                _logger.Error($"Error, prodId: {production.Id} contains multiple QuestComplete requirements");

                // Production has no multiple quest requirements
                continue;
            }

            // Check for forced ids
            if (_forcedQuestToProductionAssociations.TryGetValue(production.Id, out var associatedQuestIdToComplete))
            {
                var enLocale = _databaseServer.GetTables().Locales.Global["en"].Value;
                var questName = enLocale[$"{associatedQuestIdToComplete} name"];
                // Found one, move to next production
                _logger.Success(
                    $"FORCED - Updated: prodId: {production.Id} endProd: {production.EndProduct} ({_itemHelper.GetItemName(production.EndProduct)}) with quantity: {production.Count} to quest: {associatedQuestIdToComplete} {questName}"
                );
                questCompleteRequirements[0].QuestId = associatedQuestIdToComplete;

                continue;
            }

            // Try to find the quest that matches this production
            var questProductionOutputs = _questProductionOutputList
                .Where(output => output.ItemTemplate == production.EndProduct && output.Quantity == production.Count)
                .ToList();

            // Make sure we found valid data
            if (!IsValidQuestProduction(production, questProductionOutputs, questCompleteRequirements[0]))
            {
                continue;
            }

            // Update the production quest ID
            _questProductionMap[questProductionOutputs[0].QuestId] = production.Id;
            questCompleteRequirements[0].QuestId = questProductionOutputs[0].QuestId;
            _logger.Success(
                $"Updated prodId: {production.Id}, endProd: {production.EndProduct} quantity: {production.Count} to quest: {questProductionOutputs[0].QuestId} {questProductionOutputs[0].QuestName}"
            );
        }
    }

    private bool IsValidQuestProduction(
        HideoutProduction production,
        List<QuestProductionOutput> questProductionOutputs,
        Requirement questComplete
    )
    {
        // A lot of error handling for edge cases
        if (!questProductionOutputs.Any())
        {
            _logger.Error(
                $"Error: Unable to find matching quest for prodId: {production.Id}, endProduct: {production.EndProduct} ({_itemHelper.GetItemName(production.EndProduct)}) quantity: {production.Count}. Potential new or removed quest?"
            );
            return false;
        }

        if (questProductionOutputs.Count > 1)
        {
            var questNamesCSV = string.Join(",", questProductionOutputs.Select(x => x.QuestName));
            _logger.Error(
                $"Error: Multiple quests match prodId: {production.Id}, endProduct: {production.EndProduct} with quantity: {production.Count}, quests: {questNamesCSV}"
            );
            return false;
        }

        if (questComplete.QuestId is not null && questComplete.QuestId != questProductionOutputs[0].QuestId)
        {
            _logger.Error(
                $"Error: Multiple productions match quest. EndProduct: {production.EndProduct} with quantity {production.Count}, existing quest: {questComplete.QuestId} {questProductionOutputs[0].QuestName}"
            );

            return false;
        }

        if (_questProductionMap.ContainsKey(questProductionOutputs[0].QuestId))
        {
            var recipies = _databaseServer.GetTables().Hideout.Production.Recipes;
            var prodId = _questProductionMap[questProductionOutputs[0].QuestId];
            var prod = recipies.FirstOrDefault(x => x.Id == prodId);
            var prodItemName = _itemHelper.GetItemName(prod.EndProduct);
            _logger.Warning(
                $"Error: Quest {questProductionOutputs[0].QuestId} {questProductionOutputs[0].QuestName} already associated with production: {prodId} {prodItemName}. Potential conflict"
            );
        }

        return true;
    }

    /// <summary>
    /// Merge all rewards together into one collection
    /// </summary>
    /// <param name="questRewards">Rewards to merge</param>
    /// <returns>IEnumerable</returns>
    protected IEnumerable<Reward> CombineRewards(Dictionary<string, List<Reward>>? questRewards)
    {
        return questRewards?.SelectMany(rewardKvP => rewardKvP.Value) ?? [];
    }
}

public class QuestProductionOutput
{
    public MongoId QuestId { get; set; }

    public MongoId ItemTemplate { get; set; }

    public double Quantity { get; set; }
    public string QuestName { get; set; }
}
