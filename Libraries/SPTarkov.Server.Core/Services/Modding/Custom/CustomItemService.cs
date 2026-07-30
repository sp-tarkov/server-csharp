using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Items;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services.Modding.Custom;

[Injectable]
public class CustomItemService(
    ISptLogger<CustomItemService> logger,
    TemplateTable templateTable,
    LocaleTable locales,
    BotTable botTable,
    ItemHelper itemHelper,
    PmcConfig pmcConfig,
    ItemBaseClassService itemBaseClassService,
    ModItemCacheService modItemCacheService,
    ICloner cloner
)
{
    /// <summary>
    ///     Create a new item from a cloned item base <br />
    ///     WARNING - If no item id is supplied, an id will be generated, this id will be random every time you add an item and will not be the same on each subsequent server start <br />
    ///     Add to the items db <br />
    ///     Add to the flea market <br />
    ///     Add to the handbook <br />
    ///     Add to the locales
    /// </summary>
    /// <param name="newItemDetails"> Item details for the new item to be created </param>
    /// <param name="callingAssembly"> The ability to assign another calling assembly, mostly useful for libraries </param>
    /// <returns> tplId of the new item created </returns>
    public CreateItemResult CreateItemFromClone(NewItemFromCloneDetails newItemDetails, Assembly? callingAssembly = null)
    {
        // Generate new id for item if none supplied
        var newItemId = newItemDetails.NewId;

        // Fail if itemId already exists
        if (templateTable.Items.TryGetValue(newItemId, out var item))
        {
            return new CreateItemResult
            {
                ItemId = newItemId,
                Errors = [$"ItemId already exists. {item.Name}"],
                Success = false,
            };
        }

        // Clone existing item
        templateTable.Items.TryGetValue(newItemDetails.ItemTplToClone, out var itemToClone);
        var itemClone = cloner.Clone(itemToClone) ?? throw new InvalidOperationException($"Could not clone {nameof(itemToClone)}");

        // Update id and parentId of item
        itemClone.Id = newItemId;
        itemClone.Parent = newItemDetails.ParentId;
        itemClone.Name = newItemDetails.NewItemName;

        UpdateBaseItemPropertiesWithOverrides(newItemDetails.OverrideProperties, itemClone);

        AddToItemsDb(newItemId, itemClone);

        if (newItemDetails.AddToHandbook)
        {
            if (newItemDetails.HandbookParentId is null)
            {
                throw new InvalidOperationException($"{nameof(newItemDetails.HandbookParentId)} is null while trying to add to handbook!");
            }

            AddToHandbookDb(newItemId, newItemDetails.HandbookParentId, newItemDetails.HandbookPriceRoubles);
        }

        AddToLocaleDbs(newItemDetails.Locales, newItemId);

        if (newItemDetails.AddToFleaPriceDb)
        {
            if (newItemDetails.FleaPriceRoubles is null or <= 0)
            {
                throw new InvalidOperationException($"{nameof(newItemDetails.FleaPriceRoubles)} is null or 0 while trying to add to flea!");
            }

            AddToFleaPriceDb(newItemId, newItemDetails.FleaPriceRoubles);
        }
        else
        {
            // Prevent item with no flea entry being added to pmc loot
            pmcConfig.GlobalLootBlacklist.Add(newItemId);
        }

        itemBaseClassService.AddItemToCache(newItemId);

        if (newItemDetails.AddToWeaponShelf && itemHelper.IsOfBaseclass(itemClone.Id, BaseClasses.WEAPON))
        {
            AddToWeaponShelf(newItemId);
        }

        if (callingAssembly is not null)
        {
            modItemCacheService.AddModItem(callingAssembly, newItemId);
        }
        else
        {
            modItemCacheService.AddModItem(Assembly.GetCallingAssembly(), newItemId);
        }

        return new CreateItemResult
        {
            Errors = [],
            Success = true,
            ItemId = newItemId,
        };
    }

    /// <summary>
    ///     Create a new item without using an existing item as a template <br />
    ///     Add to the items db <br />
    ///     Add to the flea market <br />
    ///     Add to the handbook <br />
    ///     Add to the locales <br />
    /// </summary>
    /// <param name="newItemDetails"> Details on what the item to be created </param>
    /// <param name="callingAssembly"> The ability to assign another calling assembly, mostly useful for libraries </param>
    /// <returns> CreateItemResult containing the completed items ID </returns>
    public CreateItemResult CreateItem(NewItemDetails newItemDetails, Assembly? callingAssembly = null)
    {
        var newItem = newItemDetails.NewItem;

        // Fail if itemId already exists
        if (templateTable.Items.TryGetValue(newItem.Id, out var item))
        {
            return new CreateItemResult
            {
                ItemId = newItem.Id,
                Errors = [$"ItemId already exists. {item.Name}"],
                Success = false,
            };
        }

        AddToItemsDb(newItem.Id, newItem);

        if (newItemDetails.AddToHandbook)
        {
            if (newItemDetails.HandbookParentId is null)
            {
                throw new InvalidOperationException($"{nameof(newItemDetails.HandbookParentId)} is null while trying to add to handbook!");
            }

            AddToHandbookDb(newItem.Id, newItemDetails.HandbookParentId, newItemDetails.HandbookPriceRoubles);
        }

        AddToLocaleDbs(newItemDetails.Locales, newItem.Id);

        if (newItemDetails.AddToFleaPriceDb)
        {
            if (newItemDetails.FleaPriceRoubles is null or <= 0)
            {
                throw new InvalidOperationException($"{nameof(newItemDetails.FleaPriceRoubles)} is null or 0 while trying to add to flea!");
            }

            AddToFleaPriceDb(newItem.Id, newItemDetails.FleaPriceRoubles);
        }
        else
        {
            // Prevent item with no flea entry being added to pmc loot
            pmcConfig.GlobalLootBlacklist.Add(newItem.Id);
        }

        itemBaseClassService.AddItemToCache(newItem.Id);

        if (newItemDetails.AddToWeaponShelf && itemHelper.IsOfBaseclass(newItem.Id, BaseClasses.WEAPON))
        {
            AddToWeaponShelf(newItem.Id);
        }

        if (callingAssembly is not null)
        {
            modItemCacheService.AddModItem(callingAssembly, newItem.Id);
        }
        else
        {
            modItemCacheService.AddModItem(Assembly.GetCallingAssembly(), newItem.Id);
        }

        return new CreateItemResult
        {
            Errors = [],
            Success = true,
            ItemId = newItemDetails.NewItem.Id,
        };
    }

    /// <summary>
    ///     Iterates through supplied properties and updates the cloned items properties with them
    /// </summary>
    /// <param name="overrideProperties"> New properties to apply </param>
    /// <param name="itemClone"> Item to update </param>
    protected void UpdateBaseItemPropertiesWithOverrides(TemplateItemProperties? overrideProperties, TemplateItem itemClone)
    {
        if (overrideProperties is null || itemClone?.Properties is null)
            return;

        var target = itemClone.Properties;
        var targetType = target.GetType();

        foreach (var member in overrideProperties.GetType().GetMembers())
        {
            var value = member.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)member).GetValue(overrideProperties),
                MemberTypes.Field => ((FieldInfo)member).GetValue(overrideProperties),
                _ => null,
            };

            if (value is null)
            {
                continue;
            }

            var targetMember = targetType.GetMember(member.Name).FirstOrDefault();
            if (targetMember is null)
            {
                continue;
            }

            switch (targetMember.MemberType)
            {
                case MemberTypes.Property:
                    var prop = (PropertyInfo)targetMember;
                    if (prop.CanWrite)
                    {
                        prop.SetValue(target, value);
                    }

                    break;

                case MemberTypes.Field:
                    var field = (FieldInfo)targetMember;
                    if (!field.IsInitOnly)
                    {
                        field.SetValue(target, value);
                    }

                    break;
            }
        }
    }

    /// <summary>
    ///     Add a new item object to the in-memory representation of items.json
    /// </summary>
    /// <param name="newItemId"> ID of the item to add to items.json </param>
    /// <param name="itemToAdd"> Item to add against the new id </param>
    protected void AddToItemsDb(string newItemId, TemplateItem itemToAdd)
    {
        if (!templateTable.Items.TryAdd(newItemId, itemToAdd))
        {
            logger.Warning($"Unable to add: {newItemId} To Database");
        }
    }

    /// <summary>
    ///     Add a handbook price for an item
    /// </summary>
    /// <param name="newItemId"> ID of the item being added </param>
    /// <param name="parentId"> Parent ID of the item being added </param>
    /// <param name="priceRoubles"> Price of the item being added </param>
    protected void AddToHandbookDb(MongoId newItemId, string parentId, double? priceRoubles)
    {
        templateTable.Handbook.Items.Add(
            new HandbookItem
            {
                Id = newItemId,
                ParentId = parentId,
                Price = priceRoubles,
            }
        );
        // TODO: would we want to keep this the same or get them to send a HandbookItem
    }

    /// <summary>
    ///     Iterate through the passed in locale data and add to each locale in turn <br />
    ///     If data is not provided for each language EFT uses, the first object will be used in its place <br />
    ///     e.g. <br />
    ///     en[0] <br />
    ///     fr[1] <br />
    ///     <br />
    ///     No jp provided, so english will be used as a substitute
    /// </summary>
    /// <param name="localeDetails"> key is language, value are the new locale details </param>
    /// <param name="newItemId"> ID of the item being created </param>
    protected void AddToLocaleDbs(Dictionary<string, LocaleDetails> localeDetails, string newItemId)
    {
        // Validate that there's atleast one locale to use as a default
        var defaultLocale = localeDetails.Keys.FirstOrDefault();
        if (defaultLocale == null)
        {
            return;
        }

        var languages = locales.Languages;
        foreach (var shortNameKey in languages)
        {
            // Get locale details passed in, if not provided by caller use first record in newItemDetails.locales
            localeDetails.TryGetValue(shortNameKey.Key, out var newLocaleDetails);

            newLocaleDetails ??= localeDetails[defaultLocale];

            // If there's no name defined, don't add a transformer, the mod is probably handling locales elsewhere
            if (newLocaleDetails.Name == null)
            {
                continue;
            }

            if (locales.Global.TryGetValue(shortNameKey.Key, out var lazyLoad))
            {
                lazyLoad.AddTransformer(localeData =>
                {
                    localeData![$"{newItemId} Name"] = newLocaleDetails.Name;
                    localeData[$"{newItemId} ShortName"] = newLocaleDetails.ShortName ?? "";
                    localeData[$"{newItemId} Description"] = newLocaleDetails.Description ?? "";

                    return localeData;
                });
            }
        }
    }

    /// <summary>
    ///     Add a price to the in-memory representation of prices.json, used to inform the flea of an items price on the market
    /// </summary>
    /// <param name="newItemId"> ID of the new item </param>
    /// <param name="fleaPriceRoubles"> Price of the new item </param>
    protected void AddToFleaPriceDb(string newItemId, double? fleaPriceRoubles)
    {
        templateTable.Prices[newItemId] = fleaPriceRoubles ?? 0;
    }

    /// <summary>
    ///     Add a weapon to the hideout weapon shelf whitelist
    /// </summary>
    /// <param name="newItemId"> Weapon ID to add </param>
    protected void AddToWeaponShelf(string newItemId)
    {
        // Ids for wall stashes in db
        List<MongoId> wallStashIds =
        [
            ItemTpl.HIDEOUTAREACONTAINER_WEAPONSTAND_STASH_1,
            ItemTpl.HIDEOUTAREACONTAINER_WEAPONSTAND_STASH_2,
            ItemTpl.HIDEOUTAREACONTAINER_WEAPONSTAND_STASH_3,
        ];
        foreach (var wallId in wallStashIds)
        {
            var wall = itemHelper.GetItem(wallId);
            if (wall.Key)
            {
                wall.Value.Properties.Grids.First().Properties.Filters.First().Filter.Add(newItemId);
            }
        }
    }

    /// <summary>
    ///     Add a custom weapon to PMCs loadout
    /// </summary>
    /// <param name="weaponTpl"> Custom weapon tpl to add to PMCs </param>
    /// <param name="weaponWeight"> The weighting for the weapon to be picked vs other weapons </param>
    /// <param name="weaponSlot"> The slot the weapon should be added to (e.g. FirstPrimaryWeapon/SecondPrimaryWeapon/Holster) </param>
    public void AddCustomWeaponToPMCs(string weaponTpl, double weaponWeight, string weaponSlot)
    {
        var weapon = itemHelper.GetItem(weaponTpl);
        if (!weapon.Key)
        {
            logger.Warning($"Unable to add custom weapon {weaponTpl} to PMCs as it cannot be found in the Item db");

            return;
        }

        var baseWeaponModObject = new Dictionary<string, HashSet<MongoId>?>();

        // Get all slots weapon has and create a dictionary of them with possible mods that slot into each
        var weaponSlots = weapon.Value.Properties.Slots;
        foreach (var slot in weaponSlots)
        {
            baseWeaponModObject[slot.Name] = [.. slot.Properties.Filters.First().Filter];
        }

        // Get PMCs
        var botTypes = botTable.Types;

        // Add weapon base+mods into bear/usec data
        botTypes["usec"].BotInventory.Mods[weaponTpl] = baseWeaponModObject;
        botTypes["bear"].BotInventory.Mods[weaponTpl] = baseWeaponModObject;

        // Add weapon to array of allowed weapons + weighting to be picked
        botTypes["usec"].BotInventory.Equipment[Enum.Parse<EquipmentSlots>(weaponSlot)][weaponTpl] = weaponWeight;
        botTypes["bear"].BotInventory.Equipment[Enum.Parse<EquipmentSlots>(weaponSlot)][weaponTpl] = weaponWeight;
    }
}
