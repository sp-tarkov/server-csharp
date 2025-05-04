using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Models.Eft.Player;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Services;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable]
public class AirdropService(
    ConfigServer configServer,
    ISptLogger<AirdropService> _logger,
    LootGenerator _lootGenerator,
    HashUtil _hashUtil,
    WeightedRandomHelper _weightedRandomHelper,
    ContainerHelper _containerHelper,
    LocalisationService _localisationService,
    ItemFilterService _itemFilterService,
    ItemHelper _itemHelper
)
{
    protected AirdropConfig _airdropConfig = configServer.GetConfig<AirdropConfig>();

    public GetAirdropLootResponse GenerateCustomAirdropLoot(GetAirdropLootRequest request)
    {
        if (
            !_airdropConfig.CustomAirdropMapping.TryGetValue(
                request.ContainerId,
                out var customAirdropInformation
            )
        )
        {
            _logger.Warning(
                $"Unable to find data for custom airdrop {request.ContainerId}, returning random airdrop instead"
            );

            return GenerateAirdropLoot();
        }

        return GenerateAirdropLoot(customAirdropInformation);
    }

    /// <summary>
    ///     Handle client/location/getAirdropLoot
    ///     Get loot for an airdrop container
    ///     Generates it randomly based on config/airdrop.json values
    /// </summary>
    /// <param name="forcedAirdropType">OPTIONAL - Desired airdrop type, randomised when not provided</param>
    /// <returns>List of LootItem objects</returns>
    public GetAirdropLootResponse GenerateAirdropLoot(SptAirdropTypeEnum? forcedAirdropType = null)
    {
        var airdropType = forcedAirdropType ?? ChooseAirdropType();
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Chose: {airdropType} for airdrop loot");
        }

        // Common/weapon/etc
        var airdropConfig = GetAirdropLootConfigByType(airdropType);

        // generate loot to put into airdrop crate
        var crateLootPool = airdropConfig.UseForcedLoot.GetValueOrDefault(false)
            ? _lootGenerator.CreateForcedLoot(airdropConfig.ForcedLoot)
            : _lootGenerator.CreateRandomLoot(airdropConfig);

        // Create airdrop crate and add to result in first spot
        var airdropCrateItem = GetAirdropCrateItem(airdropType);

        // Filter loot pool to just items that fit crate
        var crateLoot = GetLootThatFitsContainer(airdropCrateItem, crateLootPool);

        // Flatten loot into single array ready to be returned
        var flattenedCrateLoot = crateLoot.SelectMany(x => x).ToList();

        // Add crate to front of loot rewards
        flattenedCrateLoot.Insert(0, airdropCrateItem);

        // Re-parent loot items to crate we just added
        foreach (var item in flattenedCrateLoot)
        {
            if (item.Id == airdropCrateItem.Id)
            // Crate itself, don't alter
            {
                continue;
            }

            // no parentId = root item, make item have crate as parent
            if (string.IsNullOrEmpty(item.ParentId))
            {
                item.ParentId = airdropCrateItem.Id;
                item.SlotId = "main";
            }
        }

        return new GetAirdropLootResponse
        {
            Icon = airdropConfig.Icon,
            Container = flattenedCrateLoot,
        };
    }

    /// <summary>
    /// Check if the items provided fit into the passed in container
    /// </summary>
    /// <param name="container">Crate item to fit items into</param>
    /// <param name="crateLootPool">Item pool to try and fit into container</param>
    /// <returns>Items that will fit container</returns>
    protected List<List<Item>> GetLootThatFitsContainer(
        Item container,
        List<List<Item>> crateLootPool
    )
    {
        var lootResult = new List<List<Item>>();
        var containerMap = _itemHelper.GetContainerMapping(container.Template);

        var failedToFitAttemptCount = 0;
        foreach (var itemAndChildren in crateLootPool)
        {
            var itemSize = _itemHelper.GetItemSize(itemAndChildren, itemAndChildren[0].Id);

            // look for open slot to put chosen item into
            var result = _containerHelper.FindSlotForItem(
                containerMap,
                itemSize.Width,
                itemSize.Height
            );
            if (result.Success.GetValueOrDefault(false))
            {
                // It Fits!
                lootResult.AddRange(itemAndChildren);

                continue;
            }

            if (failedToFitAttemptCount > 3)
            // x attempts to fit an item, container is probably full, stop trying to add more
            {
                break;
            }

            // Can't fit item, skip
            failedToFitAttemptCount++;
        }

        return lootResult;
    }

    /// <summary>
    ///     Create a container create item based on passed in airdrop type
    /// </summary>
    /// <param name="airdropType">What type of container: weapon/common etc</param>
    /// <returns>Item</returns>
    protected Item GetAirdropCrateItem(SptAirdropTypeEnum airdropType)
    {
        var airdropContainer = new Item
        {
            Id = _hashUtil.Generate(),
            Template = string.Empty, // Picked later
            Upd = new Upd { SpawnedInSession = true, StackObjectsCount = 1 },
        };

        switch (airdropType)
        {
            case SptAirdropTypeEnum.foodMedical:
                airdropContainer.Template = ItemTpl.LOOTCONTAINER_AIRDROP_MEDICAL_CRATE;
                break;
            case SptAirdropTypeEnum.barter:
                airdropContainer.Template = ItemTpl.LOOTCONTAINER_AIRDROP_SUPPLY_CRATE;
                break;
            case SptAirdropTypeEnum.weaponArmor:
                airdropContainer.Template = ItemTpl.LOOTCONTAINER_AIRDROP_WEAPON_CRATE;
                break;
            case SptAirdropTypeEnum.mixed:
                airdropContainer.Template = ItemTpl.LOOTCONTAINER_AIRDROP_COMMON_SUPPLY_CRATE;
                break;
            case SptAirdropTypeEnum.radar:
                airdropContainer.Template =
                    ItemTpl.LOOTCONTAINER_AIRDROP_TECHNICAL_SUPPLY_CRATE_EVENT_1;
                break;
            default:
                airdropContainer.Template = ItemTpl.LOOTCONTAINER_AIRDROP_COMMON_SUPPLY_CRATE;
                break;
        }

        return airdropContainer;
    }

    /// <summary>
    ///     Randomly pick a type of airdrop loot using weighted values from config
    /// </summary>
    /// <returns>airdrop type value</returns>
    protected SptAirdropTypeEnum ChooseAirdropType()
    {
        var possibleAirdropTypes = _airdropConfig.AirdropTypeWeightings;

        return _weightedRandomHelper.GetWeightedValue(possibleAirdropTypes);
    }

    /// <summary>
    ///     Get the configuration for a specific type of airdrop
    /// </summary>
    /// <param name="airdropType">Type of airdrop to get settings for</param>
    /// <returns>LootRequest</returns>
    protected AirdropLootRequest GetAirdropLootConfigByType(SptAirdropTypeEnum? airdropType)
    {
        var lootSettingsByType = _airdropConfig.Loot[airdropType.ToString()];
        if (lootSettingsByType is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "location-unable_to_find_airdrop_drop_config_of_type",
                    airdropType
                )
            );

            // TODO: Get Radar airdrop to work. Atm Radar will default to common supply drop (mixed)
            // Default to common
            lootSettingsByType = _airdropConfig.Loot[AirdropTypeEnum.Common.ToString()];
        }

        // Get all items that match the blacklisted types and fold into item blacklist
        var itemTypeBlacklist = _itemFilterService.GetItemRewardBaseTypeBlacklist();
        var itemsMatchingTypeBlacklist = _itemHelper
            .GetItems()
            .Where(templateItem => !string.IsNullOrEmpty(templateItem.Parent))
            .Where(templateItem =>
                _itemHelper.IsOfBaseclasses(templateItem.Parent, itemTypeBlacklist)
            )
            .Select(templateItem => templateItem.Id)
            .ToHashSet();
        var itemBlacklist = new HashSet<string>();
        itemBlacklist.UnionWith(lootSettingsByType.ItemBlacklist);
        itemBlacklist.UnionWith(_itemFilterService.GetItemRewardBlacklist());
        itemBlacklist.UnionWith(_itemFilterService.GetBossItems());
        itemBlacklist.UnionWith(itemsMatchingTypeBlacklist);

        return new AirdropLootRequest
        {
            Icon = lootSettingsByType.Icon,
            WeaponPresetCount = lootSettingsByType.WeaponPresetCount,
            ArmorPresetCount = lootSettingsByType.ArmorPresetCount,
            ItemCount = lootSettingsByType.ItemCount,
            WeaponCrateCount = lootSettingsByType.WeaponCrateCount,
            ItemBlacklist = itemBlacklist,
            ItemTypeWhitelist = lootSettingsByType.ItemTypeWhitelist,
            ItemLimits = lootSettingsByType.ItemLimits,
            ItemStackLimits = lootSettingsByType.ItemStackLimits,
            ArmorLevelWhitelist = lootSettingsByType.ArmorLevelWhitelist,
            AllowBossItems = lootSettingsByType.AllowBossItems,
            UseForcedLoot = lootSettingsByType.UseForcedLoot,
            ForcedLoot = lootSettingsByType.ForcedLoot,
            UseRewardItemBlacklist = lootSettingsByType.UseRewardItemBlacklist,
            BlockSeasonalItemsOutOfSeason = lootSettingsByType.BlockSeasonalItemsOutOfSeason,
        };
    }
}
