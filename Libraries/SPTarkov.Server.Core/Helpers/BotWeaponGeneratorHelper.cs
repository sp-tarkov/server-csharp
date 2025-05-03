using System.Collections.Frozen;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class BotWeaponGeneratorHelper(
    ISptLogger<BotWeaponGeneratorHelper> _logger,
    DatabaseServer _databaseServer,
    ItemHelper _itemHelper,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    WeightedRandomHelper _weightedRandomHelper,
    BotGeneratorHelper _botGeneratorHelper,
    LocalisationService _localisationService
)
{
    private static readonly FrozenSet<string> _magCheck = ["CylinderMagazine", "SpringDrivenCylinder"];

    /// <summary>
    ///     Get a randomized number of bullets for a specific magazine
    /// </summary>
    /// <param name="magCounts">Weights of magazines</param>
    /// <param name="magTemplate">Magazine to generate bullet count for</param>
    /// <returns>Bullet count number</returns>
    public double? GetRandomizedBulletCount(GenerationData magCounts, TemplateItem magTemplate)
    {
        var randomizedMagazineCount = GetRandomizedMagazineCount(magCounts);
        var parentItem = _itemHelper.GetItem(magTemplate.Parent).Value;
        double? chamberBulletCount = 0;
        if (MagazineIsCylinderRelated(parentItem.Name))
        {
            var firstSlotAmmoTpl = magTemplate.Properties.Cartridges.FirstOrDefault()?.Props.Filters[0].Filter.FirstOrDefault();
            var ammoMaxStackSize = _itemHelper.GetItem(firstSlotAmmoTpl).Value?.Properties?.StackMaxSize ?? 1;
            chamberBulletCount = ammoMaxStackSize == 1
                ? 1 // Rotating grenade launcher
                : magTemplate.Properties.Slots.Count; // Shotguns/revolvers. We count the number of camoras as the _max_count of the magazine is 0
        }
        else if (parentItem.Id == BaseClasses.UBGL)
        {
            // Underbarrel launchers can only have 1 chambered grenade
            chamberBulletCount = 1;
        }
        else
        {
            chamberBulletCount = magTemplate.Properties.Cartridges?[0].MaxCount;
        }

        // Get the amount of bullets that would fit in the internal magazine
        // and multiply by how many magazines were supposed to be created
        return chamberBulletCount * randomizedMagazineCount;
    }

    /// <summary>
    ///     Get a randomized count of magazines
    /// </summary>
    /// <param name="magCounts">Min and max value returned value can be between</param>
    /// <returns>Numerical value of magazine count</returns>
    public int GetRandomizedMagazineCount(GenerationData magCounts)
    {
        return (int) _weightedRandomHelper.GetWeightedValue(magCounts.Weights);
    }

    /// <summary>
    ///     Is this magazine cylinder related (revolvers and grenade launchers)
    /// </summary>
    /// <param name="magazineParentName">The name of the magazines parent</param>
    /// <returns>True if it is cylinder related</returns>
    public bool MagazineIsCylinderRelated(string magazineParentName)
    {
        return _magCheck.Contains(magazineParentName);
    }

    /// <summary>
    ///     Create a magazine using the parameters given
    /// </summary>
    /// <param name="magazineTpl">Tpl of the magazine to create</param>
    /// <param name="ammoTpl">Ammo to add to magazine</param>
    /// <param name="magTemplate">Template object of magazine</param>
    /// <returns>Item array</returns>
    public List<Item> CreateMagazineWithAmmo(string magazineTpl, string ammoTpl, TemplateItem magTemplate)
    {
        List<Item> magazine =
        [
            new()
            {
                Id = _hashUtil.Generate(),
                Template = magazineTpl
            }
        ];

        _itemHelper.FillMagazineWithCartridge(magazine, magTemplate, ammoTpl, 1);

        return magazine;
    }

    /// <summary>
    ///     Add a specific number of cartridges to a bots inventory (defaults to vest and pockets)
    /// </summary>
    /// <param name="ammoTpl">Ammo tpl to add to vest/pockets</param>
    /// <param name="cartridgeCount">Number of cartridges to add to vest/pockets</param>
    /// <param name="inventory">Bot inventory to add cartridges to</param>
    /// <param name="equipmentSlotsToAddTo">What equipment slots should bullets be added into</param>
    public void AddAmmoIntoEquipmentSlots(
        string ammoTpl,
        int cartridgeCount,
        BotBaseInventory inventory,
        HashSet<EquipmentSlots> equipmentSlotsToAddTo
    )
    {
        if (equipmentSlotsToAddTo is null)
        {
            equipmentSlotsToAddTo = [EquipmentSlots.TacticalVest, EquipmentSlots.Pockets];
        }

        var ammoItems = _itemHelper.SplitStack(
            new Item
            {
                Id = _hashUtil.Generate(),
                Template = ammoTpl,
                Upd = new Upd
                {
                    StackObjectsCount = cartridgeCount
                }
            }
        );

        foreach (var ammoItem in ammoItems)
        {
            var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                equipmentSlotsToAddTo,
                ammoItem.Id,
                ammoItem.Template,
                [ammoItem],
                inventory
            );

            if (result != ItemAddedResult.SUCCESS)
            {
                _logger.Debug($"Unable to add ammo: {ammoItem.Template} to bot inventory, {result.ToString()}");

                if (result == ItemAddedResult.NO_SPACE || result == ItemAddedResult.NO_CONTAINERS)
                // If there's no space for 1 stack or no containers to hold item, there's no space for the others
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    ///     Get a weapons default magazine template id
    /// </summary>
    /// <param name="weaponTemplate">Weapon to get default magazine for</param>
    /// <returns>Tpl of magazine</returns>
    public string? GetWeaponsDefaultMagazineTpl(TemplateItem weaponTemplate)
    {
        return weaponTemplate.Properties.DefMagType;
    }
}
