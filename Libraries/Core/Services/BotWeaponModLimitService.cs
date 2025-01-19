using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotWeaponModLimitService(
    ISptLogger<BotWeaponModLimitService> _logger,
    ConfigServer _configServer,
    ItemHelper _itemHelper
)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();

    /// <summary>
    /// Initalise mod limits to be used when generating a weapon
    /// </summary>
    /// <param name="botRole">"assault", "bossTagilla" or "pmc"</param>
    /// <returns>BotModLimits object</returns>
    public BotModLimits GetWeaponModLimits(string botRole)
    {
        return new()
        {
            Scope = new() { Count = 0 },
            ScopeMax = _botConfig.Equipment[botRole]?.WeaponModLimits?.ScopeLimit,
            ScopeBaseTypes =
            [
                BaseClasses.OPTIC_SCOPE,
                BaseClasses.ASSAULT_SCOPE,
                BaseClasses.COLLIMATOR,
                BaseClasses.COMPACT_COLLIMATOR,
                BaseClasses.SPECIAL_SCOPE
            ],
            FlashlightLaser = new() { Count = 0 },
            FlashlightLaserMax = _botConfig.Equipment[botRole]?.WeaponModLimits?.LightLaserLimit,
            FlashlightLaserBaseTypes =
            [
                BaseClasses.TACTICAL_COMBO,
                BaseClasses.FLASHLIGHT,
                BaseClasses.PORTABLE_RANGE_FINDER,
            ]
        };
    }

    /// <summary>
    /// Check if weapon mod item is on limited list + has surpassed the limit set for it
    /// Exception: Always allow ncstar backup mount
    /// Exception: Always allow scopes with a scope for a parent
    /// Exception: Always disallow mounts that hold only scopes once scope limit reached
    /// Exception: Always disallow mounts that hold only flashlights once flashlight limit reached
    /// </summary>
    /// <param name="botRole">role the bot has e.g. assault</param>
    /// <param name="modTemplate">mods template data</param>
    /// <param name="modLimits">limits set for weapon being generated for this bot</param>
    /// <param name="modsParent">The parent of the mod to be checked</param>
    /// <param name="weapon">Array of IItem</param>
    /// <returns>true if over item limit</returns>
    public bool WeaponModHasReachedLimit(
        string botRole,
        TemplateItem modTemplate,
        BotModLimits modLimits,
        TemplateItem modsParent,
        List<Item> weapon)
    {
        // If mod or mods parent is the NcSTAR MPR45 Backup mount, allow it as it looks cool
        if (modsParent.Id == ItemTpl.MOUNT_NCSTAR_MPR45_BACKUP || modTemplate.Id == ItemTpl.MOUNT_NCSTAR_MPR45_BACKUP)
        {
            // If weapon already has a longer ranged scope on it, allow ncstar to be spawned
            if (weapon.Any(
                    (item) =>
                        _itemHelper.IsOfBaseclasses(
                            item.Template,
                            [
                                BaseClasses.ASSAULT_SCOPE,
                                BaseClasses.OPTIC_SCOPE,
                                BaseClasses.SPECIAL_SCOPE,
                            ]
                        )
                ))
            {
                return false;
            }

            return true;
        }

        // Mods parent is scope and mod is scope, allow it (adds those mini-sights to the tops of sights)
        var modIsScope = _itemHelper.IsOfBaseclasses(modTemplate.Id, modLimits.ScopeBaseTypes);
        if (_itemHelper.IsOfBaseclasses(modsParent.Id, modLimits.ScopeBaseTypes) && modIsScope)
        {
            return false;
        }

        // If mod is a scope , Exit early
        if (modIsScope)
        {
            return WeaponModLimitReached(modTemplate.Id, modLimits.Scope, modLimits.ScopeMax ?? 0, botRole);
        }

        // Don't allow multple mounts on a weapon (except when mount is on another mount)
        // Fail when:
        // Over or at scope limit on weapon
        // Item being added is a mount but the parent item is NOT a mount (Allows red dot sub-mounts on mounts)
        // Mount has one slot and its for a mod_scope
        if (modLimits.Scope.Count >= modLimits.ScopeMax &&
            modTemplate.Properties.Slots?.Count() == 1 &&
            _itemHelper.IsOfBaseclass(modTemplate.Id, BaseClasses.MOUNT) &&
            !_itemHelper.IsOfBaseclass(modsParent.Id, BaseClasses.MOUNT) &&
            modTemplate.Properties.Slots.Any((slot) => slot.Name == "mod_scope")
           )
        {
            return true;
        }

        // If mod is a light/laser, return if limit reached
        var modIsLightOrLaser = _itemHelper.IsOfBaseclasses(modTemplate.Id, modLimits.FlashlightLaserBaseTypes);
        if (modIsLightOrLaser)
        {
            return WeaponModLimitReached(
                modTemplate.Id,
                modLimits.FlashlightLaser,
                modLimits.FlashlightLaserMax ?? 0,
                botRole
            );
        }

        // Mod is a mount that can hold only flashlights ad limit is reached (dont want to add empty mounts if limit is reached)
        if (modLimits.Scope.Count >= modLimits.ScopeMax &&
            modTemplate.Properties.Slots?.Count() == 1 &&
            _itemHelper.IsOfBaseclass(modTemplate.Id, BaseClasses.MOUNT) &&
            modTemplate.Properties.Slots.Any((slot) => slot.Name == "mod_flashlight")
           )
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if the specific item type on the weapon has reached the set limit
    /// </summary>
    /// <param name="modTpl">log mod tpl if over type limit</param>
    /// <param name="currentCount">current number of this item on gun</param>
    /// <param name="maxLimit">mod limit allowed</param>
    /// <param name="botRole">role of bot we're checking weapon of</param>
    /// <returns>true if limit reached</returns>
    protected bool WeaponModLimitReached(
        string modTpl,
        ItemCount currentCount,
        int? maxLimit,
        string botRole)
    {
        // No value or 0
        if (maxLimit is null || maxLimit is 0)
        {
            return false;
        }

        // Has mod limit for bot type been reached
        if (currentCount.Count >= maxLimit)
        {
            // this.logger.debug($"[{botRole}] scope limit reached! tried to add {modTpl} but scope count is {currentCount.count}`);
            return true;
        }

        // Increment scope count
        currentCount.Count++;

        return false;
    }
}
