using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Services.Mod;

[Injectable]
public class CustomItemService(
    ISptLogger<CustomItemService> logger,
    HashUtil hashUtil,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    ItemBaseClassService itemBaseClassService,
    ICloner cloner
)
{
    /**
     * Create a new item from a cloned item base
     * WARNING - If no item id is supplied, an id will be generated, this id will be random every time you add an item and will not be the same on each subsequent server start
     * Add to the items db
     * Add to the flea market
     * Add to the handbook
     * Add to the locales
     * @param newItemDetails Item details for the new item to be created
     * @returns tplId of the new item created
     */
    public CreateItemResult CreateItemFromClone(NewItemFromCloneDetails newItemDetails)
    {
        var result = new CreateItemResult();
        var tables = databaseService.GetTables();

        // Generate new id for item if none supplied
        var newItemId = newItemDetails.NewId;

        // Fail if itemId already exists
        if (tables.Templates.Items.ContainsKey(newItemId))
        {
            result.Errors.Add($"ItemId already exists. {tables.Templates.Items[newItemId].Name}");
            result.Success = false;
            result.ItemId = newItemId;

            return result;
        }

        // Clone existing item
        var itemClone = cloner.Clone(tables.Templates.Items[newItemDetails.ItemTplToClone]);

        // Update id and parentId of item
        itemClone.Id = newItemId;
        itemClone.Parent = newItemDetails.ParentId;

        UpdateBaseItemPropertiesWithOverrides(newItemDetails.OverrideProperties, itemClone);

        AddToItemsDb(newItemId, itemClone);

        AddToHandbookDb(newItemId, newItemDetails.HandbookParentId, newItemDetails.HandbookPriceRoubles);

        AddToLocaleDbs(newItemDetails.Locales, newItemId);

        AddToFleaPriceDb(newItemId, newItemDetails.FleaPriceRoubles);

        itemBaseClassService.HydrateItemBaseClassCache();

        if (itemHelper.IsOfBaseclass((MongoId) itemClone.Id, BaseClasses.WEAPON))
        {
            AddToWeaponShelf(newItemId);
        }

        result.Success = true;
        result.ItemId = newItemId;

        return result;
    }

    /**
     * Create a new item without using an existing item as a template
     * Add to the items db
     * Add to the flea market
     * Add to the handbook
     * Add to the locales
     * @param newItemDetails Details on what the item to be created
     * @returns CreateItemResult containing the completed items Id
     */
    public CreateItemResult CreateItem(NewItemDetails newItemDetails)
    {
        var result = new CreateItemResult();
        var tables = databaseService.GetTables();

        var newItem = newItemDetails.NewItem;

        // Fail if itemId already exists
        if (tables.Templates.Items.ContainsKey((MongoId) newItem.Id))
        {
            result.Errors.Add($"ItemId already exists. {tables.Templates.Items[(MongoId) newItem.Id].Name}");
            return result;
        }

        AddToItemsDb(newItem.Id, newItem);

        AddToHandbookDb(newItem.Id, newItemDetails.HandbookParentId, newItemDetails.HandbookPriceRoubles);

        AddToLocaleDbs(newItemDetails.Locales, newItem.Id);

        AddToFleaPriceDb(newItem.Id, newItemDetails.FleaPriceRoubles);

        itemBaseClassService.HydrateItemBaseClassCache();

        if (itemHelper.IsOfBaseclass((MongoId) newItem.Id, BaseClasses.WEAPON))
        {
            AddToWeaponShelf(newItem.Id);
        }

        result.ItemId = newItemDetails.NewItem.Id;
        result.Success = true;

        return result;
    }
    
    /**
     * Iterates through supplied properties and updates the cloned items properties with them
     * Complex objects cannot have overrides, they must be fully hydrated with values if they are to be used
     * @param overrideProperties new properties to apply
     * @param itemClone item to update
     */
    protected void UpdateBaseItemPropertiesWithOverrides(Props? overrideProperties, TemplateItem itemClone)
    {
        if (overrideProperties is null)
        {
            return;
        }

        foreach (var propKey in overrideProperties.GetAllPropsAsDict())
        {
            itemClone.Properties.GetAllPropsAsDict()[propKey.Key] = overrideProperties.GetAllPropsAsDict()[propKey.Key];
        }
    }

    /**
     * Add a new item object to the in-memory representation of items.json
     * @param newItemId id of the item to add to items.json
     * @param itemToAdd Item to add against the new id
     */
    protected void AddToItemsDb(string newItemId, TemplateItem itemToAdd)
    {
        if (!databaseService.GetItems().TryAdd(newItemId, itemToAdd))
        {
            logger.Warning($"Unable to add: {newItemId} To Database");
        }
    }

    /**
     * Add a handbook price for an item
     * @param newItemId id of the item being added
     * @param parentId parent id of the item being added
     * @param priceRoubles price of the item being added
     */
    protected void AddToHandbookDb(string newItemId, string parentId, double? priceRoubles)
    {
        databaseService
            .GetTemplates()
            .Handbook.Items.Add(
                new HandbookItem
                {
                    Id = newItemId,
                    ParentId = parentId,
                    Price = priceRoubles
                }
            );
        // TODO: would we want to keep this the same or get them to send a HandbookItem
    }

    /**
     * Iterate through the passed in locale data and add to each locale in turn
     * If data is not provided for each language EFT uses, the first object will be used in its place
     * e.g.
     * en[0]
     * fr[1]
     *
     * No jp provided, so english will be used as a substitute
     * @param localeDetails key is language, value are the new locale details
     * @param newItemId id of the item being created
     */
    protected void AddToLocaleDbs(Dictionary<string, LocaleDetails> localeDetails, string newItemId)
    {
        var languages = databaseService.GetLocales().Languages;
        foreach (var shortNameKey in languages)
        {
            // Get locale details passed in, if not provided by caller use first record in newItemDetails.locales
            localeDetails.TryGetValue(shortNameKey.Key, out var newLocaleDetails);

            newLocaleDetails ??= localeDetails[localeDetails.Keys.FirstOrDefault()];

            // Create new record in locale file
            if (!databaseService.GetLocales().Global.TryGetValue(shortNameKey.Key, out var desiredGlobal))
            {
                logger.Error($"Unable to add locale keys to {shortNameKey.Key}");

                return;
            }

            desiredGlobal.Value[$"{newItemId} Name"] = newLocaleDetails.Name;
            desiredGlobal.Value[$"{newItemId} ShortName"] = newLocaleDetails.ShortName;
            desiredGlobal.Value[$"{newItemId} Description"] = newLocaleDetails.Description;
        }
    }

    /**
     * Add a price to the in-memory representation of prices.json, used to inform the flea of an items price on the market
     * @param newItemId id of the new item
     * @param fleaPriceRoubles Price of the new item
     */
    protected void AddToFleaPriceDb(string newItemId, double? fleaPriceRoubles)
    {
        databaseService.GetTemplates().Prices[newItemId] = fleaPriceRoubles ?? 0;
    }

    /**
     * Add a weapon to the hideout weapon shelf whitelist
     * @param newItemId Weapon id to add
     */
    protected void AddToWeaponShelf(string newItemId)
    {
        // Ids for wall stashes in db
        List<string> wallStashIds =
        [
            ItemTpl.HIDEOUTAREACONTAINER_WEAPONSTAND_STASH_1,
            ItemTpl.HIDEOUTAREACONTAINER_WEAPONSTAND_STASH_2,
            ItemTpl.HIDEOUTAREACONTAINER_WEAPONSTAND_STASH_3
        ];
        foreach (var wallId in wallStashIds)
        {
            var wall = itemHelper.GetItem(wallId);
            if (wall.Key)
            {
                wall.Value.Properties.Grids[0].Props.Filters[0].Filter.Add(newItemId);
            }
        }
    }

    /**
     * Add a custom weapon to PMCs loadout
     * @param weaponTpl Custom weapon tpl to add to PMCs
     * @param weaponWeight The weighting for the weapon to be picked vs other weapons
     * @param weaponSlot The slot the weapon should be added to (e.g. FirstPrimaryWeapon/SecondPrimaryWeapon/Holster)
     */
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
            baseWeaponModObject[slot.Name] = [..slot.Props.Filters[0].Filter];
        }

        // Get PMCs
        var botTypes = databaseService.GetBots().Types;

        // Add weapon base+mods into bear/usec data
        botTypes["usec"].BotInventory.Mods[weaponTpl] = baseWeaponModObject;
        botTypes["bear"].BotInventory.Mods[weaponTpl] = baseWeaponModObject;

        // Add weapon to array of allowed weapons + weighting to be picked
        botTypes["usec"].BotInventory.Equipment[Enum.Parse<EquipmentSlots>(weaponSlot)][weaponTpl] = weaponWeight;
        botTypes["bear"].BotInventory.Equipment[Enum.Parse<EquipmentSlots>(weaponSlot)][weaponTpl] = weaponWeight;
    }
}
