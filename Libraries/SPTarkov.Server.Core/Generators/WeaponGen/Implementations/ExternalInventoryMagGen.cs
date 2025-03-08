using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators.WeaponGen.Implementations;

[Injectable]
public class ExternalInventoryMagGen(
    ISptLogger<ExternalInventoryMagGen> _logger,
    ItemHelper _itemHelper,
    LocalisationService _localisationService,
    BotWeaponGeneratorHelper _botWeaponGeneratorHelper,
    BotGeneratorHelper _botGeneratorHelper,
    RandomUtil _randomUtil
) : InventoryMagGen, IInventoryMagGen
{
    public int GetPriority()
    {
        return 99;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        return true; // Fallback, if code reaches here it means no other implementation can handle this type of magazine
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        // Count of attempts to fit a magazine into bot inventory
        var fitAttempts = 0;

        // Magazine Db template
        var magTemplate = inventoryMagGen.GetMagazineTemplate();
        var magazineTpl = magTemplate.Id;
        var weapon = inventoryMagGen.GetWeaponTemplate();
        List<string> attemptedMagBlacklist = [];
        var defaultMagazineTpl = _botWeaponGeneratorHelper.GetWeaponsDefaultMagazineTpl(weapon);
        var isShotgun = _itemHelper.IsOfBaseclass(weapon.Id, BaseClasses.SHOTGUN);

        var randomizedMagazineCount = _botWeaponGeneratorHelper.GetRandomizedMagazineCount(inventoryMagGen.GetMagCount());
        for (var i = 0; i < randomizedMagazineCount; i++)
        {
            var magazineWithAmmo = _botWeaponGeneratorHelper.CreateMagazineWithAmmo(
                magazineTpl,
                inventoryMagGen.GetAmmoTemplate().Id,
                magTemplate
            );

            var fitsIntoInventory = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                [EquipmentSlots.TacticalVest, EquipmentSlots.Pockets],
                magazineWithAmmo[0].Id,
                magazineTpl,
                magazineWithAmmo,
                inventoryMagGen.GetPmcInventory()
            );

            if (fitsIntoInventory == ItemAddedResult.NO_CONTAINERS)
                // No containers to fit magazines, stop trying
            {
                break;
            }

            // No space for magazine and we haven't reached desired magazine count
            if (fitsIntoInventory == ItemAddedResult.NO_SPACE && i < randomizedMagazineCount)
            {
                // Prevent infinite loop by only allowing 5 attempts at fitting a magazine into inventory
                if (fitAttempts > 5)
                {
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"Failed {fitAttempts} times to add magazine {magazineTpl} to bot inventory, stopping");
                    }

                    break;
                }

                /* We were unable to fit at least the minimum amount of magazines,
                 * so we fallback to default magazine and try again.
                 * Temporary workaround to Killa spawning with no extra mags if he spawns with a drum mag */

                if (magazineTpl == defaultMagazineTpl)
                    // We were already on default - stop here to prevent infinite loop
                {
                    break;
                }

                // Add failed magazine tpl to blacklist
                attemptedMagBlacklist.Add(magazineTpl);

                if (defaultMagazineTpl is null)
                    // No default to fall back to, stop trying to add mags
                {
                    break;
                }

                if (defaultMagazineTpl == BaseClasses.MAGAZINE)
                    // Magazine base type, do not use
                {
                    break;
                }

                // Set chosen magazine tpl to the weapons default magazine tpl and try to fit into inventory next loop
                magazineTpl = defaultMagazineTpl;
                magTemplate = _itemHelper.GetItem(magazineTpl).Value;
                if (magTemplate is null)
                {
                    _logger.Error(
                        _localisationService.GetText("bot-unable_to_find_default_magazine_item", magazineTpl)
                    );

                    break;
                }

                // Edge case - some weapons (SKS + shotguns) have an internal magazine as default, choose random non-internal magazine to add to bot instead
                if (magTemplate.Properties.ReloadMagType == ReloadMode.InternalMagazine)
                {
                    var result = GetRandomExternalMagazineForInternalMagazineGun(
                        inventoryMagGen.GetWeaponTemplate().Id,
                        attemptedMagBlacklist
                    );

                    if (result?.Id is null)
                    {
                        // Highly likely shotgun has no external mags
                        if (isShotgun)
                        {
                            break;
                        }

                        if (_logger.IsLogEnabled(LogLevel.Debug))
                        {
                            _logger.Debug(
                                $"Unable to add additional magazine into bot inventory: vest/pockets for weapon: {weapon.Name}, attempted: {fitAttempts} times. Reason: {fitsIntoInventory}"
                            );
                        }

                        break;
                    }

                    magazineTpl = result.Id;
                    magTemplate = result;
                    fitAttempts++;
                }

                // Reduce loop counter by 1 to ensure we get full cout of desired magazines
                i--;
            }

            if (fitsIntoInventory == ItemAddedResult.SUCCESS)
                // Reset fit counter now it succeeded
            {
                fitAttempts = 0;
            }
        }
    }
    /// <summary>
    /// Get a random compatible external magazine for a weapon, exclude internal magazines from possible pool
    /// </summary>
    /// <param name="weaponTpl"> Weapon to get mag for </param>
    /// <param name="magazineBlacklist"> Blacklisted magazines </param>
    /// <returns> Item of chosen magazine </returns>
    public TemplateItem? GetRandomExternalMagazineForInternalMagazineGun(string weaponTpl, List<string> magazineBlacklist)
    {
        // The mag Slot data for the weapon
        var magSlot = _itemHelper.GetItem(weaponTpl).Value.Properties.Slots.FirstOrDefault(x => x.Name == "mod_magazine");
        if (magSlot is null)
        {
            return null;
        }

        // All possible mags that fit into the weapon excluding blacklisted
        var magazinePool = magSlot.Props.Filters[0]
            .Filter.Where(x => !magazineBlacklist.Contains(x))
            .Select(
                x => _itemHelper.GetItem(x).Value
            );
        if (magazinePool is null)
        {
            return null;
        }

        // Non-internal magazines that fit into the weapon
        var externalMagazineOnlyPool = magazinePool.Where(x => x.Properties.ReloadMagType != ReloadMode.InternalMagazine);
        if (externalMagazineOnlyPool is null || !externalMagazineOnlyPool.Any())
        {
            return null;
        }

        // Randomly chosen external magazine
        return _randomUtil.GetArrayValue(externalMagazineOnlyPool);
    }
}
