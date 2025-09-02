using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class FenceBaseAssortGenerator(
    ISptLogger<FenceBaseAssortGenerator> logger,
    DatabaseService databaseService,
    HandbookHelper handbookHelper,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    ItemFilterService itemFilterService,
    SeasonalEventService seasonalEventService,
    ServerLocalisationService localisationService,
    ConfigServer configServer,
    FenceService fenceService,
    ICloner cloner
)
{
    protected readonly TraderConfig TraderConfig = configServer.GetConfig<TraderConfig>();

    /// <summary>
    ///     Create base fence assorts dynamically and store in memory
    /// </summary>
    public void GenerateFenceBaseAssorts()
    {
        var blockedSeasonalItems = seasonalEventService.GetInactiveSeasonalEventItems();
        var baseFenceAssort = databaseService.GetTrader(Traders.FENCE)?.Assort;

        foreach (var (itemId, rootItemDb) in databaseService.GetItems())
        {
            if (!string.Equals(rootItemDb.Type, "Item", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Skip blacklisted items
            if (itemFilterService.IsItemBlacklisted(itemId))
            {
                continue;
            }

            // Skip reward item blacklist
            if (itemFilterService.IsItemRewardBlacklisted(itemId))
            {
                continue;
            }

            // Invalid
            if (!itemHelper.IsValidItem(itemId))
            {
                continue;
            }

            // Item base type blacklisted
            if (TraderConfig.Fence.Blacklist.Count > 0)
            {
                if (TraderConfig.Fence.Blacklist.Contains(itemId) || itemHelper.IsOfBaseclasses(itemId, TraderConfig.Fence.Blacklist))
                {
                    continue;
                }
            }

            // Only allow rigs with no slots (carrier rigs)
            if (
                itemHelper.IsOfBaseclass(itemId, BaseClasses.VEST)
                && (rootItemDb.Properties?.Slots is not null && rootItemDb.Properties.Slots.Any())
            )
            {
                continue;
            }

            // Skip seasonal event items when not in seasonal event
            if (TraderConfig.Fence.BlacklistSeasonalItems && blockedSeasonalItems.Contains(itemId))
            {
                continue;
            }

            // Create item object in array
            var itemWithChildrenToAdd = new List<Item>
            {
                new()
                {
                    Id = new MongoId(),
                    Template = itemId,
                    ParentId = "hideout",
                    SlotId = "hideout",
                    Upd = new Upd { StackObjectsCount = 9999999 },
                },
            };

            // Ensure ammo is not above penetration limit value
            if (itemHelper.IsOfBaseclasses(itemId, [BaseClasses.AMMO_BOX, BaseClasses.AMMO]))
            {
                if (IsAmmoAbovePenetrationLimit(rootItemDb))
                {
                    continue;
                }
            }

            if (itemHelper.IsOfBaseclass(itemId, BaseClasses.AMMO_BOX))
            // Only add cartridges to box if box has no children
            {
                if (itemWithChildrenToAdd.Count == 1)
                {
                    itemHelper.AddCartridgesToAmmoBox(itemWithChildrenToAdd, rootItemDb);
                }
            }

            // Ensure IDs are unique
            itemWithChildrenToAdd.RemapRootItemId();
            if (itemWithChildrenToAdd.Count > 1)
            {
                itemHelper.ReparentItemAndChildren(itemWithChildrenToAdd[0], itemWithChildrenToAdd);
                itemWithChildrenToAdd[0].ParentId = "hideout";
            }

            // Create barter scheme (price)
            var barterSchemeToAdd = new BarterScheme
            {
                Count = Math.Round((double)fenceService.GetItemPrice(itemId, itemWithChildrenToAdd)),
                Template = Money.ROUBLES,
            };

            // Add barter data to base
            baseFenceAssort.BarterScheme[itemWithChildrenToAdd[0].Id] =
            [
                [barterSchemeToAdd],
            ];

            // Add item to base
            baseFenceAssort.Items.AddRange(itemWithChildrenToAdd);

            // Add loyalty data to base
            baseFenceAssort.LoyalLevelItems[itemWithChildrenToAdd[0].Id] = 1;
        }

        // Add all default presets to base fence assort
        var defaultPresets = presetHelper.GetDefaultPresets().Values;
        foreach (var defaultPreset in defaultPresets)
        {
            // Construct preset + mods
            var itemAndChildren = cloner.Clone(defaultPreset.Items).ReplaceIDs();

            // Find root item and add some properties to it
            var rootItem = itemAndChildren.FirstOrDefault(item => string.IsNullOrEmpty(item.ParentId));
            rootItem.ParentId = "hideout";
            rootItem.SlotId = "hideout";
            rootItem.Upd = new Upd
            {
                StackObjectsCount = 1,
                SptPresetId = defaultPreset.Id, // Store preset id here so we can check it later to prevent preset dupes
            };

            // Add constructed preset to assorts
            baseFenceAssort.Items.AddRange(itemAndChildren);

            // Calculate preset price (root item + child items)
            var price = handbookHelper.GetTemplatePriceForItems(itemAndChildren);
            var itemQualityModifier = itemHelper.GetItemQualityModifierForItems(itemAndChildren);

            // Multiply weapon+mods rouble price by quality modifier
            baseFenceAssort.BarterScheme[itemAndChildren.First().Id] =
            [
                new()
                {
                    new BarterScheme { Template = Money.ROUBLES, Count = Math.Round(price * itemQualityModifier) },
                },
            ];

            baseFenceAssort.LoyalLevelItems[itemAndChildren.First().Id] = 1;
        }
    }

    /// <summary>
    ///     Check ammo in boxes + loose ammos has a penetration value above the configured value in trader.json / ammoMaxPenLimit
    /// </summary>
    /// <param name="rootItemDb"> Ammo box or ammo item from items.db </param>
    /// <returns>True if penetration value is above limit set in config</returns>
    protected bool IsAmmoAbovePenetrationLimit(TemplateItem rootItemDb)
    {
        var ammoPenetrationPower = GetAmmoPenetrationPower(rootItemDb);
        if (ammoPenetrationPower == null)
        {
            logger.Warning(localisationService.GetText("fence-unable_to_get_ammo_penetration_value", rootItemDb.Id));
            return false;
        }

        return ammoPenetrationPower > TraderConfig.Fence.AmmoMaxPenLimit;
    }

    /// <summary>
    ///     Get the penetration power value of an ammo, works with ammo boxes and raw ammos
    /// </summary>
    /// <param name="rootItemDb"> Ammo box or ammo item from items.db </param>
    /// <returns> Penetration power of passed in item, undefined if it doesn't have a power </returns>
    protected double? GetAmmoPenetrationPower(TemplateItem rootItemDb)
    {
        if (itemHelper.IsOfBaseclass(rootItemDb.Id, BaseClasses.AMMO_BOX))
        {
            // Get the cartridge tpl found inside ammo box
            var cartridgeTplInBox = rootItemDb.Properties.StackSlots.First().Properties.Filters.First().Filter.FirstOrDefault();

            // Look up cartridge tpl in db
            var ammoItemDb = itemHelper.GetItem(cartridgeTplInBox);
            if (!ammoItemDb.Key)
            {
                logger.Warning(localisationService.GetText("fence-ammo_not_found_in_db", cartridgeTplInBox));
                return null;
            }

            return ammoItemDb.Value.Properties.PenetrationPower;
        }

        // Plain old ammo, get its pen property
        if (itemHelper.IsOfBaseclass(rootItemDb.Id, BaseClasses.AMMO))
        {
            return rootItemDb.Properties.PenetrationPower;
        }

        // Not an ammobox or ammo
        return null;
    }

    /// <summary>
    ///     Add soft inserts + armor plates to an armor
    /// </summary>
    /// <param name="armor"> Armor item array to add mods into </param>
    /// <param name="itemDbDetails">Armor items db template</param>
    protected void AddChildrenToArmorModSlots(List<Item> armor, TemplateItem itemDbDetails)
    {
        // Armor has no mods, make no additions
        var hasMods = itemDbDetails.Properties?.Slots is not null && itemDbDetails.Properties.Slots.Any();
        if (!hasMods)
        {
            return;
        }

        // Check for and add required soft inserts to armors
        var requiredSlots = itemDbDetails.Properties.Slots.Where(slot => slot.Required ?? false).ToList();
        var hasRequiredSlots = requiredSlots.Count > 0;
        if (hasRequiredSlots)
        {
            foreach (var requiredSlot in requiredSlots)
            {
                var modItemDbDetails = itemHelper.GetItem(requiredSlot.Properties.Filters.First().Plate.Value).Value;
                var plateTpl = requiredSlot.Properties.Filters.First().Plate; // `Plate` property appears to be the 'default' item for slot
                if (plateTpl is null || plateTpl.Value.IsEmpty)
                // Some bsg plate properties are empty, skip mod
                {
                    continue;
                }

                var mod = new Item
                {
                    Id = new MongoId(),
                    Template = plateTpl.Value,
                    ParentId = armor[0].Id,
                    SlotId = requiredSlot.Name,
                    Upd = new Upd
                    {
                        Repairable = new UpdRepairable
                        {
                            Durability = modItemDbDetails.Properties.MaxDurability,
                            MaxDurability = modItemDbDetails.Properties.MaxDurability,
                        },
                    },
                };

                armor.Add(mod);
            }
        }

        // Check for and add plate items
        var plateSlots = itemDbDetails.Properties.Slots.Where(slot => itemHelper.IsRemovablePlateSlot(slot.Name)).ToList();
        if (plateSlots.Count > 0)
        {
            foreach (var plateSlot in plateSlots)
            {
                var plateTpl = plateSlot.Properties.Filters.First().Plate;
                if (string.IsNullOrEmpty(plateTpl))
                // Bsg data lacks a default plate, skip adding mod
                {
                    continue;
                }

                var modItemDbDetails = itemHelper.GetItem(plateTpl.Value).Value;
                armor.Add(
                    new Item
                    {
                        Id = new MongoId(),
                        Template = plateSlot.Properties.Filters.First().Plate.Value, // `Plate` property appears to be the 'default' item for slot
                        ParentId = armor[0].Id,
                        SlotId = plateSlot.Name,
                        Upd = new Upd
                        {
                            Repairable = new UpdRepairable
                            {
                                Durability = modItemDbDetails.Properties.MaxDurability,
                                MaxDurability = modItemDbDetails.Properties.MaxDurability,
                            },
                        },
                    }
                );
            }
        }
    }

    /// <summary>
    ///     Check if item is valid for being added to fence assorts
    /// </summary>
    /// <param name="item"> Item to check </param>
    /// <returns> True if valid fence item </returns>
    protected bool IsValidFenceItem(TemplateItem item)
    {
        return string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase);
    }
}
