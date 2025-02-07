using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Mod;
using Core.Models.Utils;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using SptCommon.Extensions;

namespace Core.Services.Mod;

[Injectable]
public class CustomItemService(
    ISptLogger<CustomItemService> _logger,
    HashUtil _hashUtil,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    ItemBaseClassService _itemBaseClassService,
    ICloner _cloner
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
        var tables = _databaseService.GetTables();

        // Generate new id for item if none supplied
        var newItemId = GetOrGenerateIdForItem(newItemDetails.NewId);

        // Fail if itemId already exists
        if (tables.Templates.Items.ContainsKey(newItemId))
        {
            result.Errors.Add($"ItemId already exists. {tables.Templates.Items[newItemId].Name}");
            result.Success = false;
            result.ItemId = newItemId;

            return result;
        }

        // Clone existing item
        var itemClone = _cloner.Clone(tables.Templates.Items[newItemDetails.ItemTplToClone]);

        // Update id and parentId of item
        itemClone.Id = newItemId;
        itemClone.Parent = newItemDetails.ParentId;

        UpdateBaseItemPropertiesWithOverrides(newItemDetails.OverrideProperties, itemClone);

        AddToItemsDb(newItemId, itemClone);

        AddToHandbookDb(newItemId, newItemDetails.HandbookParentId, newItemDetails.HandbookPriceRoubles);

        AddToLocaleDbs(newItemDetails.Locales, newItemId);

        AddToFleaPriceDb(newItemId, newItemDetails.FleaPriceRoubles);

        _itemBaseClassService.HydrateItemBaseClassCache();

        if (_itemHelper.IsOfBaseclass(itemClone.Id, BaseClasses.WEAPON))
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
        var tables = _databaseService.GetTables();

        var newItem = newItemDetails.NewItem;

        // Fail if itemId already exists
        if (tables.Templates.Items.ContainsKey(newItem.Id))
        {
            result.Errors.Add($"ItemId already exists. {tables.Templates.Items[newItem.Id].Name}");
            return result;
        }

        AddToItemsDb(newItem.Id, newItem);

        AddToHandbookDb(newItem.Id, newItemDetails.HandbookParentId, newItemDetails.HandbookPriceRoubles);

        AddToLocaleDbs(newItemDetails.Locales, newItem.Id);

        AddToFleaPriceDb(newItem.Id, newItemDetails.FleaPriceRoubles);

        _itemBaseClassService.HydrateItemBaseClassCache();

        if (_itemHelper.IsOfBaseclass(newItem.Id, BaseClasses.WEAPON))
        {
            AddToWeaponShelf(newItem.Id);
        }

        result.ItemId = newItemDetails.NewItem.Id;
        result.Success = true;

        return result;
    }

    /**
     * If the id provided is an empty string, return a randomly generated guid, otherwise return the newId parameter
     * @param newId id supplied to code
     * @returns item id
     */
    protected string GetOrGenerateIdForItem(string newId)
    {
        return newId == "" ? _hashUtil.Generate() : newId;
    }

    /**
     * Iterates through supplied properties and updates the cloned items properties with them
     * Complex objects cannot have overrides, they must be fully hydrated with values if they are to be used
     * @param overrideProperties new properties to apply
     * @param itemClone item to update
     */
    protected void UpdateBaseItemPropertiesWithOverrides(Props? overrideProperties, TemplateItem itemClone)
    {
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
        if (!_databaseService.GetItems().TryAdd(newItemId, itemToAdd))
        {
            _logger.Warning($"Unable to add: {newItemId} To Database");
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
        _databaseService
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
     * If data is not provided for each langauge eft uses, the first object will be used in its place
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
        var languages = _databaseService.GetLocales().Languages;
        foreach (var shortNameKey in languages)
        {
            // Get locale details passed in, if not provided by caller use first record in newItemDetails.locales
            localeDetails.TryGetValue(shortNameKey.Key, out var newLocaleDetails);

            if (newLocaleDetails is null)
            {
                newLocaleDetails = localeDetails[localeDetails.Keys.FirstOrDefault()];
            }

            // Create new record in locale file
            var globals = _databaseService.GetLocales();
            globals.Global[shortNameKey.Key].Value[$"{newItemId} Name"] = newLocaleDetails.Name;
            globals.Global[shortNameKey.Key].Value[$"{newItemId} ShortName"] = newLocaleDetails.ShortName;
            globals.Global[shortNameKey.Key].Value[$"{newItemId} Description"] = newLocaleDetails.Description;
        }
    }

    /**
     * Add a price to the in-memory representation of prices.json, used to inform the flea of an items price on the market
     * @param newItemId id of the new item
     * @param fleaPriceRoubles Price of the new item
     */
    protected void AddToFleaPriceDb(string newItemId, double? fleaPriceRoubles)
    {
        _databaseService.GetTemplates().Prices[newItemId] = fleaPriceRoubles ?? 0;
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
            var wall = _itemHelper.GetItem(wallId);
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
        var weapon = _itemHelper.GetItem(weaponTpl);
        if (!weapon.Key)
        {
            _logger.Warning($"Unable to add custom weapon {weaponTpl} to PMCs as it cannot be found in the Item db");

            return;
        }

        Dictionary<string, HashSet<string>?> baseWeaponModObject = new Dictionary<string, HashSet<string>?>();

        // Get all slots weapon has and create a dictionary of them with possible mods that slot into each
        var weaponSlots = weapon.Value.Properties.Slots;
        foreach (var slot in weaponSlots)
        {
            baseWeaponModObject[slot.Name] = new HashSet<string>(slot.Props.Filters[0].Filter);
        }

        // Get PMCs
        var botTypes = _databaseService.GetBots().Types;

        // Add weapon base+mods into bear/usec data
        botTypes["usec"].BotInventory.Mods[weaponTpl] = baseWeaponModObject;
        botTypes["bear"].BotInventory.Mods[weaponTpl] = baseWeaponModObject;

        // Add weapon to array of allowed weapons + weighting to be picked
        botTypes["usec"].BotInventory.Equipment.GetByJsonProp<Dictionary<string, double>>(weaponSlot)[weaponTpl] = weaponWeight;
        botTypes["bear"].BotInventory.Equipment.GetByJsonProp<Dictionary<string, double>>(weaponSlot)[weaponTpl] = weaponWeight;
    }
}
