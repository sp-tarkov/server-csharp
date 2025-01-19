using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Mod;

namespace Core.Services.Mod;

public class CustomItemService
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /**
     * If the id provided is an empty string, return a randomly generated guid, otherwise return the newId parameter
     * @param newId id supplied to code
     * @returns item id
     */
    protected string GetOrGenerateIdForItem(string newId)
    {
        throw new NotImplementedException();
    }

    /**
     * Iterates through supplied properties and updates the cloned items properties with them
     * Complex objects cannot have overrides, they must be fully hydrated with values if they are to be used
     * @param overrideProperties new properties to apply
     * @param itemClone item to update
     */
    protected void UpdateBaseItemPropertiesWithOverrides(Props overrideProperties, TemplateItem itemClone)
    {
        throw new NotImplementedException();
    }

    /**
     * Addd a new item object to the in-memory representation of items.json
     * @param newItemId id of the item to add to items.json
     * @param itemToAdd Item to add against the new id
     */
    protected void AddToItemsDb(string newItemId, TemplateItem itemToAdd)
    {
        throw new NotImplementedException();
    }

    /**
     * Add a handbook price for an item
     * @param newItemId id of the item being added
     * @param parentId parent id of the item being added
     * @param priceRoubles price of the item being added
     */
    protected void AddToHandbookDb(string newItemId, string parentId, decimal priceRoubles)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /**
     * Add a price to the in-memory representation of prices.json, used to inform the flea of an items price on the market
     * @param newItemId id of the new item
     * @param fleaPriceRoubles Price of the new item
     */
    protected void AddToFleaPriceDb(string newItemId, decimal fleaPriceRoubles)
    {
        throw new NotImplementedException();
    }

    /**
     * Add a weapon to the hideout weapon shelf whitelist
     * @param newItemId Weapon id to add
     */
    protected void AddToWeaponShelf(string newItemId)
    {
        throw new NotImplementedException();
    }

    /**
     * Add a custom weapon to PMCs loadout
     * @param weaponTpl Custom weapon tpl to add to PMCs
     * @param weaponWeight The weighting for the weapon to be picked vs other weapons
     * @param weaponSlot The slot the weapon should be added to (e.g. FirstPrimaryWeapon/SecondPrimaryWeapon/Holster)
     */
    public void AddCustomWeaponToPMCs(string weaponTpl, decimal weaponWeight, string weaponSlot)
    {
        throw new NotImplementedException();
    }
}
