using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using System.Reflection;

namespace SPTarkov.Server.Core.Services.Mod;

[Injectable]
public class CustomItemService(
    ISptLogger<CustomItemService> logger,
    HashUtil hashUtil,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    ItemBaseClassService itemBaseClassService,
    ICloner cloner,
    LocaleService localeService
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
    /// <returns> tplId of the new item created </returns>
    public CreateItemResult CreateItemFromClone(NewItemFromCloneDetails newItemDetails)
    {
        var result = new CreateItemResult();
        var tables = databaseService.GetTables();

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

        if (itemHelper.IsOfBaseclass(itemClone.Id, BaseClasses.WEAPON))
        {
            AddToWeaponShelf(newItemId);
        }

        result.Success = true;
        result.ItemId = newItemId;

        return result;
    }

    /// <summary>
    ///     Create a new item without using an existing item as a template <br />
    ///     Add to the items db <br />
    ///     Add to the flea market <br />
    ///     Add to the handbook <br />
    ///     Add to the locales <br />
    /// </summary>
    /// <param name="newItemDetails"> Details on what the item to be created </param>
    /// <returns> CreateItemResult containing the completed items ID </returns>
    public CreateItemResult CreateItem(NewItemDetails newItemDetails)
    {
        var result = new CreateItemResult();
        var tables = databaseService.GetTables();

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

        itemBaseClassService.HydrateItemBaseClassCache();

        if (itemHelper.IsOfBaseclass(newItem.Id, BaseClasses.WEAPON))
        {
            AddToWeaponShelf(newItem.Id);
        }

        result.ItemId = newItemDetails.NewItem.Id;
        result.Success = true;

        return result;
    }

    /// <summary>
    ///     If the ID provided is an empty string, return a randomly generated guid, otherwise return the newId parameter
    /// </summary>
    /// <param name="newId"> ID supplied to code </param>
    /// <returns> ItemID </returns>
    protected string GetOrGenerateIdForItem(string newId)
    {
        return newId == "" ? hashUtil.Generate() : newId;
    }

    /// <summary>
    ///     Iterates through supplied properties and updates the cloned items properties with them
    /// </summary>
    /// <param name="overrideProperties"> New properties to apply </param>
    /// <param name="itemClone"> Item to update </param>
    protected void UpdateBaseItemPropertiesWithOverrides(Props? overrideProperties, TemplateItem itemClone)
    {
        if (overrideProperties is null || itemClone?.Properties is null) return;

        var target = itemClone.Properties;
        var targetType = target.GetType();

        foreach (var member in overrideProperties.GetType().GetMembers())
        {
            var value = member.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)member).GetValue(overrideProperties),
                MemberTypes.Field => ((FieldInfo)member).GetValue(overrideProperties),
                _ => null
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
        if (!databaseService.GetItems().TryAdd(newItemId, itemToAdd))
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
        var languages = databaseService.GetLocales().Languages;
        foreach (var shortNameKey in languages)
        {
            // Get locale details passed in, if not provided by caller use first record in newItemDetails.locales
            localeDetails.TryGetValue(shortNameKey.Key, out var newLocaleDetails);

            newLocaleDetails ??= localeDetails[localeDetails.Keys.FirstOrDefault()];

            localeService.AddCustomClientLocale(shortNameKey.Key, $"{newItemId} Name", newLocaleDetails.Name);
            localeService.AddCustomClientLocale(shortNameKey.Key, $"{newItemId} ShortName", newLocaleDetails.ShortName);
            localeService.AddCustomClientLocale(shortNameKey.Key, $"{newItemId} Description", newLocaleDetails.Description);
        }
    }

    /// <summary>
    ///     Add a price to the in-memory representation of prices.json, used to inform the flea of an items price on the market
    /// </summary>
    /// <param name="newItemId"> ID of the new item </param>
    /// <param name="fleaPriceRoubles"> Price of the new item </param>
    protected void AddToFleaPriceDb(string newItemId, double? fleaPriceRoubles)
    {
        databaseService.GetTemplates().Prices[newItemId] = fleaPriceRoubles ?? 0;
    }

    /// <summary>
    ///     Add a weapon to the hideout weapon shelf whitelist
    /// </summary>
    /// <param name="newItemId"> Weapon ID to add </param>
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

        var baseWeaponModObject = new Dictionary<string, HashSet<string>?>();

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
