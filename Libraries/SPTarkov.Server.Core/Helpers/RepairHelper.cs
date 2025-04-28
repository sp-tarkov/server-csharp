using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RepairHelper(
    ISptLogger<RepairHelper> _logger,
    RandomUtil _randomUtil,
    DatabaseService _databaseService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected RepairConfig _repairConfig = _configServer.GetConfig<RepairConfig>();

    /// <summary>
    ///     Alter an items durability after a repair by trader/repair kit
    /// </summary>
    /// <param name="itemToRepair">item to update durability details</param>
    /// <param name="itemToRepairDetails">db details of item to repair</param>
    /// <param name="isArmor">Is item being repaired a piece of armor</param>
    /// <param name="amountToRepair">how many unit of durability to repair</param>
    /// <param name="useRepairKit">Is item being repaired with a repair kit</param>
    /// <param name="traderQualityMultiplier">Trader quality value from traders base json</param>
    /// <param name="applyMaxDurabilityDegradation">should item have max durability reduced</param>
    public void UpdateItemDurability(
        Item itemToRepair,
        TemplateItem itemToRepairDetails,
        bool isArmor,
        double amountToRepair,
        bool useRepairKit,
        double traderQualityMultiplier,
        bool applyMaxDurabilityDegradation = true
    )
    {
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Adding {amountToRepair} to {itemToRepairDetails.Name} using kit: {useRepairKit}");
        }

        var itemMaxDurability = _cloner.Clone(itemToRepair.Upd.Repairable.MaxDurability);
        var itemCurrentDurability = _cloner.Clone(itemToRepair.Upd.Repairable.Durability);
        var itemCurrentMaxDurability = _cloner.Clone(itemToRepair.Upd.Repairable.MaxDurability);

        var newCurrentDurability = itemCurrentDurability + amountToRepair;
        var newCurrentMaxDurability = itemCurrentMaxDurability + amountToRepair;

        // Ensure new max isnt above items max
        if (newCurrentMaxDurability > itemMaxDurability)
        {
            newCurrentMaxDurability = itemMaxDurability;
        }

        // Ensure new current isnt above items max
        if (newCurrentDurability > itemMaxDurability)
        {
            newCurrentDurability = itemMaxDurability;
        }

        // Update Repairable properties with new values after repair
        itemToRepair.Upd.Repairable = new UpdRepairable
        {
            Durability = newCurrentDurability,
            MaxDurability = newCurrentMaxDurability
        };

        // when modders set the repair coefficient to 0 it means that they dont want to lose durability on items
        // the code below generates a random degradation on the weapon durability
        if (applyMaxDurabilityDegradation)
        {
            var randomisedWearAmount = isArmor
                ? GetRandomisedArmorRepairDegradationValue(
                    itemToRepairDetails.Properties.ArmorMaterial.Value,
                    useRepairKit,
                    itemCurrentMaxDurability ?? 0,
                    traderQualityMultiplier
                )
                : GetRandomisedWeaponRepairDegradationValue(
                    itemToRepairDetails.Properties,
                    useRepairKit,
                    itemCurrentMaxDurability ?? 0,
                    traderQualityMultiplier
                );

            // Apply wear to durability
            itemToRepair.Upd.Repairable.MaxDurability -= randomisedWearAmount;

            // After adjusting max durability with degradation, ensure current dura isnt above max
            if (itemToRepair.Upd.Repairable.Durability > itemToRepair.Upd.Repairable.MaxDurability)
            {
                itemToRepair.Upd.Repairable.Durability = itemToRepair.Upd.Repairable.MaxDurability;
            }
        }

        // Repair mask cracks
        if (itemToRepair.Upd.FaceShield is not null && itemToRepair.Upd.FaceShield?.Hits > 0)
        {
            itemToRepair.Upd.FaceShield.Hits = 0;
        }
    }

    /// <summary>
    ///     Repairing armor reduces the total durability value slightly, get a randomised (to 2dp) amount based on armor material
    /// </summary>
    /// <param name="material">What material is the armor being repaired made of</param>
    /// <param name="isRepairKit">Was a repair kit used</param>
    /// <param name="armorMax">Max amount of durability item can have</param>
    /// <param name="traderQualityMultiplier">Different traders produce different loss values</param>
    /// <returns>Amount to reduce max durability by</returns>
    protected double GetRandomisedArmorRepairDegradationValue(
        ArmorMaterial material,
        bool isRepairKit,
        double armorMax,
        double traderQualityMultiplier
    )
    {
        // Degradation value is based on the armor material
        if (!_databaseService.GetGlobals().Configuration.ArmorMaterials.TryGetValue(material, out var armorMaterialSettings))
        {
            _logger.Error($"Unable to find armor with a type of: {material}");
        }

        var minMultiplier = isRepairKit
            ? armorMaterialSettings.MinRepairKitDegradation
            : armorMaterialSettings.MinRepairDegradation;

        var maxMultiplier = isRepairKit
            ? armorMaterialSettings.MaxRepairKitDegradation
            : armorMaterialSettings.MaxRepairDegradation;

        var duraLossPercent = _randomUtil.GetDouble((double) minMultiplier, (double) maxMultiplier);
        var duraLossMultipliedByTraderMultiplier = duraLossPercent * armorMax * traderQualityMultiplier;

        return Math.Round(duraLossMultipliedByTraderMultiplier, 2);
    }

    /// <summary>
    ///     Repairing weapons reduces the total durability value slightly, get a randomised (to 2dp) amount
    /// </summary>
    /// <param name="itemProps">Weapon properties</param>
    /// <param name="isRepairKit">Was a repair kit used</param>
    /// <param name="weaponMax">Max amount of durability item can have</param>
    /// <param name="traderQualityMultipler">Different traders produce different loss values</param>
    /// <returns>Amount to reduce max durability by</returns>
    protected double GetRandomisedWeaponRepairDegradationValue(
        Props itemProps,
        bool isRepairKit,
        double weaponMax,
        double traderQualityMultipler
    )
    {
        var minRepairDeg = isRepairKit ? itemProps.MinRepairKitDegradation : itemProps.MinRepairDegradation;
        var maxRepairDeg = isRepairKit ? itemProps.MaxRepairKitDegradation : itemProps.MaxRepairDegradation;

        // WORKAROUND: Some items are always 0 when repairkit is true
        if (maxRepairDeg == 0)
        {
            maxRepairDeg = itemProps.MaxRepairDegradation;
        }

        var duraLossPercent = _randomUtil.GetDouble((double) minRepairDeg, (double) maxRepairDeg);
        var duraLossMultipliedByTraderMultiplier = duraLossPercent * weaponMax * traderQualityMultipler;

        return Math.Round(duraLossMultipliedByTraderMultiplier, 2);
    }
}
