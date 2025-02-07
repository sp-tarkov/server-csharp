using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;

namespace Core.Generators;

[Injectable]
public class FenceBaseAssortGenerator(
    ISptLogger<FenceBaseAssortGenerator> logger,
    HashUtil hashUtil,
    DatabaseService databaseService,
    HandbookHelper handbookHelper,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    ItemFilterService itemFilterService,
    SeasonalEventService seasonalEventService,
    LocalisationService localisationService,
    ConfigServer configServer,
    FenceService fenceService,
    ICloner _cloner
)
{
    protected TraderConfig traderConfig = configServer.GetConfig<TraderConfig>();

    /**
     * Create base fence assorts dynamically and store in memory
     */
    public void GenerateFenceBaseAssorts()
    {
        var blockedSeasonalItems = seasonalEventService.GetInactiveSeasonalEventItems();
        var baseFenceAssort = databaseService.GetTrader(Traders.FENCE).Assort;

        foreach (var rootItemDb in itemHelper.GetItems().Where(IsValidFenceItem))
        {
            // Skip blacklisted items
            if (itemFilterService.IsItemBlacklisted(rootItemDb.Id))
            {
                continue;
            }

            // Skip reward item blacklist
            if (itemFilterService.IsItemRewardBlacklisted(rootItemDb.Id))
            {
                continue;
            }

            // Invalid
            if (!itemHelper.IsValidItem(rootItemDb.Id))
            {
                continue;
            }

            // Item base type blacklisted
            if (traderConfig.Fence.Blacklist.Count > 0)
            {
                if (traderConfig.Fence.Blacklist.Contains(rootItemDb.Id) ||
                    itemHelper.IsOfBaseclasses(rootItemDb.Id, traderConfig.Fence.Blacklist)
                   )
                {
                    continue;
                }
            }

            // Only allow rigs with no slots (carrier rigs)
            if (itemHelper.IsOfBaseclass(rootItemDb.Id, BaseClasses.VEST) &&
                (rootItemDb.Properties?.Slots?.Count ?? 0) > 0
               )
            {
                continue;
            }

            // Skip seasonal event items when not in seasonal event
            if (traderConfig.Fence.BlacklistSeasonalItems && blockedSeasonalItems.Contains(rootItemDb.Id))
            {
                continue;
            }

            // Create item object in array
            var itemWithChildrenToAdd = new List<Item>
            {
                new()
                {
                    Id = hashUtil.Generate(),
                    Template = rootItemDb.Id,
                    ParentId = "hideout",
                    SlotId = "hideout",
                    Upd = new Upd
                    {
                        StackObjectsCount = 9999999
                    }
                }
            };

            // Ensure ammo is not above penetration limit value
            if (itemHelper.IsOfBaseclasses(rootItemDb.Id, [BaseClasses.AMMO_BOX, BaseClasses.AMMO]))
            {
                if (IsAmmoAbovePenetrationLimit(rootItemDb))
                {
                    continue;
                }
            }

            if (itemHelper.IsOfBaseclass(rootItemDb.Id, BaseClasses.AMMO_BOX))
                // Only add cartridges to box if box has no children
            {
                if (itemWithChildrenToAdd.Count == 1)
                {
                    itemHelper.AddCartridgesToAmmoBox(itemWithChildrenToAdd, rootItemDb);
                }
            }

            // Ensure IDs are unique
            itemHelper.RemapRootItemId(itemWithChildrenToAdd);
            if (itemWithChildrenToAdd.Count > 1)
            {
                itemHelper.ReparentItemAndChildren(itemWithChildrenToAdd[0], itemWithChildrenToAdd);
                itemWithChildrenToAdd[0].ParentId = "hideout";
            }

            // Create barter scheme (price)
            var barterSchemeToAdd = new BarterScheme
            {
                Count = Math.Round((double) fenceService.GetItemPrice(rootItemDb.Id, itemWithChildrenToAdd)),
                Template = Money.ROUBLES
            };

            // Add barter data to base
            baseFenceAssort.BarterScheme[itemWithChildrenToAdd[0].Id] = [[barterSchemeToAdd]];

            // Add item to base
            baseFenceAssort.Items.AddRange(itemWithChildrenToAdd);

            // Add loyalty data to base
            baseFenceAssort.LoyalLevelItems[itemWithChildrenToAdd[0].Id] = 1;
        }

        // Add all default presets to base fence assort
        var defaultPresets = presetHelper.GetDefaultPresets().Values;
        foreach (var defaultPreset in defaultPresets)
        {
            // Skip presets we've already added
            if (baseFenceAssort.Items.Any(item => item.Upd != null && item.Upd.SptPresetId == defaultPreset.Id))
            {
                continue;
            }

            // Construct preset + mods
            var itemAndChildren = itemHelper.ReplaceIDs(_cloner.Clone(defaultPreset.Items));

            // Find root item and add some properties to it
            for (var i = 0; i < itemAndChildren.Count; i++)
            {
                var mod = itemAndChildren[i];

                // Build root Item info
                if (string.IsNullOrEmpty(mod.ParentId))
                {
                    mod.ParentId = "hideout";
                    mod.SlotId = "hideout";
                    mod.Upd = new Upd
                    {
                        StackObjectsCount = 1,
                        SptPresetId =
                            defaultPreset.Id // Store preset id here so we can check it later to prevent preset dupes
                    };

                    // Updated root item, exit loop
                    break;
                }
            }

            // Add constructed preset to assorts
            baseFenceAssort.Items.AddRange(itemAndChildren);

            // Calculate preset price (root item + child items)
            var price = handbookHelper.GetTemplatePriceForItems(itemAndChildren);
            var itemQualityModifier = itemHelper.GetItemQualityModifierForItems(itemAndChildren);

            // Multiply weapon+mods rouble price by quality modifier
            baseFenceAssort.BarterScheme[itemAndChildren[0].Id] = new List<List<BarterScheme>>
            {
                new()
                {
                    new BarterScheme
                    {
                        Template = Money.ROUBLES,
                        Count = Math.Round(price * itemQualityModifier)
                    }
                }
            };

            baseFenceAssort.LoyalLevelItems[itemAndChildren[0].Id] = 1;
        }
    }

    /**
     * Check ammo in boxes + loose ammos has a penetration value above the configured value in trader.json / ammoMaxPenLimit
     * @param rootItemDb Ammo box or ammo item from items.db
     * @returns True if penetration value is above limit set in config
     */
    protected bool IsAmmoAbovePenetrationLimit(TemplateItem rootItemDb)
    {
        var ammoPenetrationPower = GetAmmoPenetrationPower(rootItemDb);
        if (ammoPenetrationPower == null)
        {
            logger.Warning(localisationService.GetText("fence-unable_to_get_ammo_penetration_value", rootItemDb.Id));
            return false;
        }

        return ammoPenetrationPower > traderConfig.Fence.AmmoMaxPenLimit;
    }

    /**
     * Get the penetration power value of an ammo, works with ammo boxes and raw ammos
     * @param rootItemDb Ammo box or ammo item from items.db
     * @returns Penetration power of passed in item, undefined if it doesnt have a power
     */
    protected double? GetAmmoPenetrationPower(TemplateItem rootItemDb)
    {
        if (itemHelper.IsOfBaseclass(rootItemDb.Id, BaseClasses.AMMO_BOX))
        {
            // Get the cartridge tpl found inside ammo box
            var cartridgeTplInBox = rootItemDb.Properties.StackSlots[0].Props.Filters[0].Filter[0];

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

    /**
     * Add soft inserts + armor plates to an armor
     * @param armor Armor item array to add mods into
     * @param itemDbDetails Armor items db template
     */
    protected void AddChildrenToArmorModSlots(List<Item> armor, TemplateItem itemDbDetails)
    {
        // Armor has no mods, make no additions
        var hasMods = itemDbDetails.Properties.Slots.Count > 0;
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
                var modItemDbDetails = itemHelper.GetItem(requiredSlot.Props.Filters[0].Plate).Value;
                var plateTpl =
                    requiredSlot.Props.Filters[0].Plate; // `Plate` property appears to be the 'default' item for slot
                if (string.IsNullOrEmpty(plateTpl))
                    // Some bsg plate properties are empty, skip mod
                {
                    continue;
                }

                var mod = new Item
                {
                    Id = hashUtil.Generate(),
                    Template = plateTpl,
                    ParentId = armor[0].Id,
                    SlotId = requiredSlot.Name,
                    Upd = new Upd
                    {
                        Repairable = new UpdRepairable
                        {
                            Durability = modItemDbDetails.Properties.MaxDurability,
                            MaxDurability = modItemDbDetails.Properties.MaxDurability
                        }
                    }
                };

                armor.Add(mod);
            }
        }

        // Check for and add plate items
        var plateSlots = itemDbDetails.Properties.Slots.Where(slot => itemHelper.IsRemovablePlateSlot(slot.Name))
            .ToList();
        if (plateSlots.Count > 0)
        {
            foreach (var plateSlot in plateSlots)
            {
                var plateTpl = plateSlot.Props.Filters[0].Plate;
                if (string.IsNullOrEmpty(plateTpl))
                    // Bsg data lacks a default plate, skip adding mod
                {
                    continue;
                }

                var modItemDbDetails = itemHelper.GetItem(plateTpl).Value;
                armor.Add(
                    new Item
                    {
                        Id = hashUtil.Generate(),
                        Template = plateSlot.Props.Filters[0].Plate, // `Plate` property appears to be the 'default' item for slot
                        ParentId = armor[0].Id,
                        SlotId = plateSlot.Name,
                        Upd = new Upd
                        {
                            Repairable = new UpdRepairable
                            {
                                Durability = modItemDbDetails.Properties.MaxDurability,
                                MaxDurability = modItemDbDetails.Properties.MaxDurability
                            }
                        }
                    }
                );
            }
        }
    }

    /**
     * Check if item is valid for being added to fence assorts
     * @param item Item to check
     * @returns true if valid fence item
     */
    protected bool IsValidFenceItem(TemplateItem item)
    {
        return item.Type == "Item";
    }
}
