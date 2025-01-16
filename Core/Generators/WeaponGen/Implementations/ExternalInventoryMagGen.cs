using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;

namespace Core.Generators.WeaponGen.Implementations;

[Injectable]
public class ExternalInventoryMagGen : InventoryMagGen, IInventoryMagGen
{
    private readonly ISptLogger<ExternalInventoryMagGen> _logger;
    private readonly ItemHelper _itemHelper;
    private readonly LocalisationService _localisationService;
    private readonly BotWeaponGeneratorHelper _botWeaponGeneratorHelper;
    private readonly BotGeneratorHelper _botGeneratorHelper;
    private readonly RandomUtil _randomUtil;

    public ExternalInventoryMagGen
    (
        ISptLogger<ExternalInventoryMagGen> logger,
        ItemHelper itemHelper,
        LocalisationService localisationService,
        BotWeaponGeneratorHelper botWeaponGeneratorHelper,
        BotGeneratorHelper botGeneratorHelper,
        RandomUtil randomUtil
    )
    {
        _logger = logger;
        _itemHelper = itemHelper;
        _localisationService = localisationService;
        _botWeaponGeneratorHelper = botWeaponGeneratorHelper;
        _botGeneratorHelper = botGeneratorHelper;
        _randomUtil = randomUtil;
    }

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
        // Cout of attempts to fit a magazine into bot inventory
        var fitAttempts = 0;

        // Magazine Db template
        var magTemplate = inventoryMagGen.GetMagazineTemplate();
        var magazineTpl = magTemplate.Id;
        var weapon = inventoryMagGen.GetWeaponTemplate();
        List<string> attemptedMagBlacklist = [];
        var defaultMagazineTpl = _botWeaponGeneratorHelper.GetWeaponsDefaultMagazineTpl(weapon);
        var randomizedMagazineCount = _botWeaponGeneratorHelper.GetRandomizedMagazineCount(inventoryMagGen.GetMagCount());
        for (var i = 0; i < randomizedMagazineCount; i++) {
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

            if (fitsIntoInventory == ItemAddedResult.NO_CONTAINERS) {
                // No containers to fit magazines, stop trying
                break;
            }

            // No space for magazine and we haven't reached desired magazine count
            if (fitsIntoInventory == ItemAddedResult.NO_SPACE && i < randomizedMagazineCount) {
                // Prevent infinite loop by only allowing 5 attempts at fitting a magazine into inventory
                if (fitAttempts > 5) {
                    _logger.Debug($"Failed {fitAttempts} times to add magazine {magazineTpl} to bot inventory, stopping");

                    break;
                }

                /* We were unable to fit at least the minimum amount of magazines,
                 * so we fallback to default magazine and try again.
                 * Temporary workaround to Killa spawning with no extra mags if he spawns with a drum mag */

                if (magazineTpl == defaultMagazineTpl) {
                    // We were already on default - stop here to prevent infinite looping
                    break;
                }

                // Add failed magazine tpl to blacklist
                attemptedMagBlacklist.Add(magazineTpl);

                // Set chosen magazine tpl to the weapons default magazine tpl and try to fit into inventory next loop
                magazineTpl = defaultMagazineTpl;
                magTemplate = _itemHelper.GetItem(magazineTpl).Value;
                if (magTemplate is null) {
                    _logger.Error(
                        _localisationService.GetText("bot-unable_to_find_default_magazine_item", magazineTpl)
                    );

                    break;
                }

                // Edge case - some weapons (SKS) have an internal magazine as default, choose random non-internal magazine to add to bot instead
                if (magTemplate.Properties.ReloadMagType == "InternalMagazine") {
                    var result = GetRandomExternalMagazineForInternalMagazineGun(
                        inventoryMagGen.GetWeaponTemplate().Id,
                        attemptedMagBlacklist
                    );
                    if (result?.Id is null) {
                        _logger.Debug($"Unable to add additional magazine into bot inventory for weapon: {weapon.Name}, attempted: {fitAttempts} times");

                        break;
                    }

                    magazineTpl = result.Id;
                    magTemplate = result;
                    fitAttempts++;
                }

                // Reduce loop counter by 1 to ensure we get full cout of desired magazines
                i--;
            }

            if (fitsIntoInventory == ItemAddedResult.SUCCESS) {
                // Reset fit counter now it succeeded
                fitAttempts = 0;
            }
        }
    }

    public TemplateItem? GetRandomExternalMagazineForInternalMagazineGun(string weaponTpl, List<string> magazineBlacklist)
    {
        // The mag Slot data for the weapon
        var magSlot = _itemHelper.GetItem(weaponTpl).Value.Properties.Slots.FirstOrDefault((x) => x.Name == "mod_magazine");
        if (magSlot is null) {
            return null;
        }

        // All possible mags that fit into the weapon excluding blacklisted
        var magazinePool = magSlot.Props.Filters[0].Filter.Where((x) => !magazineBlacklist.Contains(x)).Select(
            (x) => _itemHelper.GetItem(x).Value
        );
        if (magazinePool is null) {
            return null;
        }

        // Non-internal magazines that fit into the weapon
        var externalMagazineOnlyPool = magazinePool.Where((x) => x.Properties.ReloadMagType != "InternalMagazine");
        if (externalMagazineOnlyPool is null || externalMagazineOnlyPool?.Count() == 0) {
            return null;
        }

        // Randomly chosen external magazine
        return _randomUtil.GetArrayValue(externalMagazineOnlyPool);
    }
}
