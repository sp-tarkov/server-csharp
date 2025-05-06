using System.Text.Json.Serialization;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using BonusSettings = SPTarkov.Server.Core.Models.Spt.Config.BonusSettings;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RepairService(
    ISptLogger<RepairService> _logger,
    RandomUtil _randomUtil,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    TraderHelper _traderHelper,
    PaymentService _paymentService,
    ProfileHelper _profileHelper,
    RepairHelper _repairHelper,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    WeightedRandomHelper _weightedRandomHelper)
{
    protected RepairConfig _repairConfig = _configServer.GetConfig<RepairConfig>();


    /// <summary>
    ///     Use trader to repair an items durability
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="pmcData">Profile to find item to repair in</param>
    /// <param name="repairItemDetails">Details of the item to repair</param>
    /// <param name="traderId">Trader being used to repair item</param>
    /// <returns>RepairDetails object</returns>
    public RepairDetails RepairItemByTrader(
        string sessionID,
        PmcData pmcData,
        RepairItem repairItemDetails,
        string traderId
    )
    {
        var itemToRepair = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == repairItemDetails.Id);
        if (itemToRepair is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "repair-unable_to_find_item_in_inventory_cant_repair",
                    repairItemDetails.Id
                )
            );
        }

        var priceCoef = _traderHelper.GetLoyaltyLevel(traderId, pmcData).RepairPriceCoefficient;
        var traderRepairDetails = _traderHelper.GetTrader(traderId, sessionID)?.Repair;
        if (traderRepairDetails is null)
        {
            _logger.Error(_localisationService.GetText("repair-unable_to_find_trader_details_by_id", traderId));
        }

        var repairQualityMultiplier = traderRepairDetails.Quality;
        var repairRate = priceCoef <= 0 ? 1 : priceCoef / 100 + 1;

        var items = _databaseService.GetItems();
        var itemToRepairDetails = items[itemToRepair.Template];
        var repairItemIsArmor = itemToRepairDetails.Properties.ArmorMaterial is not null;

        _repairHelper.UpdateItemDurability(
            itemToRepair,
            itemToRepairDetails,
            repairItemIsArmor,
            repairItemDetails.Count.Value,
            false,
            repairQualityMultiplier.Value,
            repairQualityMultiplier != 0 && _repairConfig.ApplyRandomizeDurabilityLoss
        );

        // get repair price
        var itemRepairCost = items[itemToRepair.Template].Properties.RepairCost;
        if (itemRepairCost is null)
        {
            _logger.Error(
                _localisationService.GetText("repair-unable_to_find_item_repair_cost", itemToRepair.Template)
            );
        }

        var repairCost = Math.Round(
            itemRepairCost.Value * repairItemDetails.Count.Value * repairRate.Value * _repairConfig.PriceMultiplier
        );

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"item base repair cost: {itemRepairCost}");
            _logger.Debug($"price multiplier: {_repairConfig.PriceMultiplier}");
            _logger.Debug($"repair cost: {repairCost}");
        }

        return new RepairDetails
        {
            RepairCost = repairCost,
            RepairedItem = itemToRepair,
            RepairedItemIsArmor = repairItemIsArmor,
            RepairAmount = repairItemDetails.Count,
            RepairedByKit = false
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="pmcData">Profile to take money from</param>
    /// <param name="repairedItemId">Repaired item id</param>
    /// <param name="repairCost">Cost to repair item in roubles</param>
    /// <param name="traderId">Id of the trader who repaired the item / who is paid</param>
    /// <param name="output">Client response</param>
    public void PayForRepair(
        string sessionID,
        PmcData pmcData,
        string repairedItemId,
        double repairCost,
        string traderId,
        ItemEventRouterResponse output
    )
    {
        var options = new ProcessBuyTradeRequestData
        {
            SchemeItems =
            [
                new IdWithCount
                {
                    Count = Math.Round(repairCost),
                    Id = Money.ROUBLES
                }
            ],
            TransactionId = traderId,
            Action = "SptRepair",
            Type = "",
            ItemId = "",
            Count = 0,
            SchemeId = 0
        };

        _paymentService.PayMoney(pmcData, options, sessionID, output);
    }

    /// <summary>
    ///     Add skill points to profile after repairing an item
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="repairDetails">Details of item repaired, cost/item</param>
    /// <param name="pmcData">Profile to add points to</param>
    public void AddRepairSkillPoints(string sessionId, RepairDetails repairDetails, PmcData pmcData)
    {
        // Handle kit repair of weapon
        if (
            repairDetails.RepairedByKit.GetValueOrDefault(false) &&
            _itemHelper.IsOfBaseclass(repairDetails.RepairedItem.Template, BaseClasses.WEAPON)
        )
        {
            var skillPoints = GetWeaponRepairSkillPoints(repairDetails);

            if (skillPoints > 0)
            {
                _logger.Debug($"Added: {skillPoints} WEAPON_TREATMENT points to skill");
                _profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.WeaponTreatment, skillPoints, true);
            }
        }

        // Handle kit repair of armor
        if (
            repairDetails.RepairedByKit.GetValueOrDefault(false) &&
            _itemHelper.IsOfBaseclasses(
                repairDetails.RepairedItem.Template,
                [
                    BaseClasses.ARMOR_PLATE,
                    BaseClasses.BUILT_IN_INSERTS
                ]
            )
        )
        {
            var itemDetails = _itemHelper.GetItem(repairDetails.RepairedItem.Template);
            if (!itemDetails.Key)
            {
                // No item found
                _logger.Error(
                    _localisationService.GetText(
                        "repair-unable_to_find_item_in_db",
                        repairDetails.RepairedItem.Template
                    )
                );

                return;
            }

            var isHeavyArmor = itemDetails.Value.Properties.ArmorType == "Heavy";
            var vestSkillToLevel = isHeavyArmor ? SkillTypes.HeavyVests : SkillTypes.LightVests;
            if (repairDetails.RepairPoints is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "repair-item_has_no_repair_points",
                        repairDetails.RepairedItem.Template
                    )
                );
            }

            var pointsToAddToVestSkill =
                repairDetails.RepairPoints * _repairConfig.ArmorKitSkillPointGainPerRepairPointMultiplier;

            _logger.Debug($"Added: {pointsToAddToVestSkill} {vestSkillToLevel} skill");
            _profileHelper.AddSkillPointsToPlayer(pmcData, vestSkillToLevel, pointsToAddToVestSkill);
        }

        // Handle giving INT to player - differs if using kit/trader and weapon vs armor
        var intellectGainedFromRepair = GetIntellectGainedFromRepair(repairDetails);
        if (intellectGainedFromRepair > 0)
        {
            _logger.Debug($"Added: {intellectGainedFromRepair} intellect skill");
            _profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.Intellect, intellectGainedFromRepair);
        }
    }

    protected double GetIntellectGainedFromRepair(RepairDetails repairDetails)
    {
        if (repairDetails.RepairedByKit.GetValueOrDefault(false))
        {
            // Weapons/armor have different multipliers
            var intRepairMultiplier = _itemHelper.IsOfBaseclass(repairDetails.RepairedItem.Template, BaseClasses.WEAPON)
                ? _repairConfig.RepairKitIntellectGainMultiplier.Weapon
                : _repairConfig.RepairKitIntellectGainMultiplier.Armor;

            // Limit gain to a max value defined in config.maxIntellectGainPerRepair
            if (repairDetails.RepairPoints is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "repair-item_has_no_repair_points",
                        repairDetails.RepairedItem.Template
                    )
                );
            }

            return Math.Min(
                repairDetails.RepairPoints.Value * intRepairMultiplier,
                _repairConfig.MaxIntellectGainPerRepair.Kit
            );
        }

        // Trader repair - Not as accurate as kit, needs data from live
        return Math.Min(repairDetails.RepairAmount.Value / 10, _repairConfig.MaxIntellectGainPerRepair.Trader);
    }

    /// <summary>
    ///     Return an approximation of the amount of skill points live would return for the given repairDetails
    /// </summary>
    /// <param name="repairDetails">The repair details to calculate skill points for</param>
    /// <returns>The number of skill points to reward the user</returns>
    protected double GetWeaponRepairSkillPoints(RepairDetails repairDetails)
    {
        var random = new Random();
        // This formula and associated configs is calculated based on 30 repairs done on live
        // The points always came out 2-aligned, which is why there's a divide/multiply by 2 with ceil calls
        var gainMult = _repairConfig.WeaponTreatment.PointGainMultiplier;

        // First we get a baseline based on our repair amount, and gain multiplier with a bit of rounding
        var step1 = Math.Ceiling(repairDetails.RepairAmount.Value / 2) * gainMult;

        // Then we have to get the next even number
        var step2 = Math.Ceiling(step1 / 2) * 2;

        // Then multiply by 2 again to hopefully get to what live would give us
        var skillPoints = step2 * 2;

        // You can both crit fail and succeed at the same time, for fun (Balances out to 0 with default settings)
        // Add a random chance to crit-fail
        if (random.Next() <= _repairConfig.WeaponTreatment.CritFailureChance)
        {
            skillPoints -= _repairConfig.WeaponTreatment.CritFailureAmount;
        }

        // Add a random chance to crit-succeed
        if (random.Next() <= _repairConfig.WeaponTreatment.CritSuccessChance)
        {
            skillPoints += _repairConfig.WeaponTreatment.CritSuccessAmount;
        }

        return Math.Max(skillPoints, 0);
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Profile to update repaired item in</param>
    /// <param name="repairKits">List of Repair kits to use</param>
    /// <param name="itemToRepairId">Item id to repair</param>
    /// <param name="output">ItemEventRouterResponse</param>
    /// <returns>Details of repair, item/price</returns>
    public RepairDetails RepairItemByKit(
        string sessionId,
        PmcData pmcData,
        List<RepairKitsInfo> repairKits,
        string itemToRepairId,
        ItemEventRouterResponse output
    )
    {
        // Find item to repair in inventory
        var itemToRepair = pmcData.Inventory.Items.FirstOrDefault(x => x.Id == itemToRepairId);
        if (itemToRepair is null)
        {
            _logger.Error(_localisationService.GetText("repair-item_not_found_unable_to_repair", itemToRepairId));
        }

        var itemsDb = _databaseService.GetItems();
        var itemToRepairDetails = itemsDb[itemToRepair.Template];
        var repairItemIsArmor = itemToRepairDetails.Properties.ArmorMaterial is not null;
        var repairAmount = repairKits[0].Count / GetKitDivisor(itemToRepairDetails, repairItemIsArmor, pmcData);
        var shouldApplyDurabilityLoss = ShouldRepairKitApplyDurabilityLoss(
            pmcData,
            _repairConfig.ApplyRandomizeDurabilityLoss
        );

        _repairHelper.UpdateItemDurability(
            itemToRepair,
            itemToRepairDetails,
            repairItemIsArmor,
            repairAmount.Value,
            true,
            1,
            shouldApplyDurabilityLoss
        );

        // Find and use repair kit defined in body
        foreach (var repairKit in repairKits)
        {
            var repairKitInInventory = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == repairKit.Id);
            if (repairKitInInventory is null)
            {
                _logger.Error(
                    _localisationService.GetText("repair-repair_kit_not_found_in_inventory", repairKit.Id)
                );
            }

            var repairKitDetails = itemsDb[repairKitInInventory.Template];
            var repairKitReductionAmount = repairKit.Count;

            AddMaxResourceToKitIfMissing(repairKitDetails, repairKitInInventory);

            // reduce usages on repairkit used
            repairKitInInventory.Upd.RepairKit.Resource -= repairKitReductionAmount;

            output.ProfileChanges[sessionId].Items.ChangedItems.Add(repairKitInInventory);
        }

        return new RepairDetails
        {
            RepairPoints = repairKits[0].Count,
            RepairedItem = itemToRepair,
            RepairedItemIsArmor = repairItemIsArmor,
            RepairAmount = repairAmount,
            RepairedByKit = true
        };
    }

    /// <summary>
    ///     Calculate value repairkit points need to be divided by to get the durability points to be added to an item
    /// </summary>
    /// <param name="itemToRepairDetails">Item to repair details</param>
    /// <param name="isArmor">Is the item being repaired armor</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Number to divide kit points by</returns>
    protected double GetKitDivisor(TemplateItem itemToRepairDetails, bool isArmor, PmcData pmcData)
    {
        var globals = _databaseService.GetGlobals();
        var globalConfig = globals.Configuration;
        var globalRepairSettings = globalConfig.RepairSettings;

        var intellectRepairPointsPerLevel = globalConfig.SkillsSettings.Intellect.RepairPointsCostReduction;
        var profileIntellectLevel =
            _profileHelper.GetSkillFromProfile(pmcData, SkillTypes.Intellect)?.Progress ?? 0;
        var intellectPointReduction = intellectRepairPointsPerLevel * Math.Truncate(profileIntellectLevel / 100);

        if (isArmor)
        {
            var durabilityPointCostArmor = globalRepairSettings.DurabilityPointCostArmor;
            var repairArmorBonus = GetBonusMultiplierValue(BonusType.RepairArmorBonus, pmcData);
            var armorBonus = 1.0 - (repairArmorBonus - 1.0) - intellectPointReduction;
            var materialType = itemToRepairDetails.Properties.ArmorMaterial.Value;
            globalConfig.ArmorMaterials.TryGetValue(materialType, out var armorMaterial);
            var destructability = 1 + armorMaterial.Destructibility;
            var armorClass = itemToRepairDetails.Properties.ArmorClass.Value;
            var armorClassDivisor = globals.Configuration.RepairSettings.ArmorClassDivisor;
            var armorClassMultiplier = 1.0 + armorClass / armorClassDivisor;

            return durabilityPointCostArmor.Value *
                   armorBonus.Value *
                   destructability.Value *
                   armorClassMultiplier.Value;
        }

        var repairWeaponBonus = GetBonusMultiplierValue(BonusType.RepairWeaponBonus, pmcData) - 1;
        var repairPointMultiplier = 1.0 - repairWeaponBonus - intellectPointReduction;
        var durabilityPointCostGuns = globals.Configuration.RepairSettings.DurabilityPointCostGuns;

        return durabilityPointCostGuns.Value * repairPointMultiplier.Value;
    }

    /// <summary>
    ///     Get the bonus multiplier for a skill from a player profile
    /// </summary>
    /// <param name="skillBonus">Bonus to get multiplier of</param>
    /// <param name="pmcData">Player profile to look in for skill</param>
    /// <returns>Multiplier value</returns>
    protected double GetBonusMultiplierValue(BonusType skillBonus, PmcData pmcData)
    {
        var bonusesMatched = pmcData?.Bonuses?.Where(b => b.Type == skillBonus);
        var value = 1d;
        if (bonusesMatched is not null)
        {
            var summedPercentage = bonusesMatched.Sum(x => x.Value ?? 0);
            value = 1 + summedPercentage / 100;
        }

        return value;
    }

    /// <summary>
    ///     Should a repair kit apply total durability loss on repair
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="applyRandomizeDurabilityLoss">Value from repair config</param>
    /// <returns>True if loss should be applied</returns>
    protected bool ShouldRepairKitApplyDurabilityLoss(PmcData pmcData, bool applyRandomizeDurabilityLoss)
    {
        var shouldApplyDurabilityLoss = applyRandomizeDurabilityLoss;
        if (shouldApplyDurabilityLoss)
        {
            // Random loss not disabled via config, perform charisma check
            var hasEliteCharisma = _profileHelper.HasEliteSkillLevel(SkillTypes.Charisma, pmcData);
            if (hasEliteCharisma)
                // 50/50 chance of loss being ignored at elite level
            {
                shouldApplyDurabilityLoss = _randomUtil.GetChance100(50);
            }
        }

        return shouldApplyDurabilityLoss;
    }

    /// <summary>
    ///     Update repair kits Resource object if it doesn't exist
    /// </summary>
    /// <param name="repairKitDetails">Repair kit details from db</param>
    /// <param name="repairKitInInventory">Repair kit to update</param>
    protected void AddMaxResourceToKitIfMissing(TemplateItem repairKitDetails, Item repairKitInInventory)
    {
        var maxRepairAmount = repairKitDetails.Properties.MaxRepairResource;
        if (repairKitInInventory.Upd is null)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Repair kit: {repairKitInInventory.Id} in inventory lacks upd object, adding");
            }

            repairKitInInventory.Upd = new Upd
            {
                RepairKit = new UpdRepairKit
                {
                    Resource = maxRepairAmount
                }
            };
        }

        if (repairKitInInventory.Upd.RepairKit?.Resource is null)
        {
            repairKitInInventory.Upd.RepairKit = new UpdRepairKit
            {
                Resource = maxRepairAmount
            };
        }
    }

    /// <summary>
    ///     Chance to apply buff to an item (Armor/weapon) if repaired by armor kit
    /// </summary>
    /// <param name="repairDetails">Repair details of item</param>
    /// <param name="pmcData">Player profile</param>
    public void AddBuffToItem(RepairDetails repairDetails, PmcData pmcData)
    {
        // Buffs are repair kit only
        if (!repairDetails.RepairedByKit.GetValueOrDefault(false))
        {
            return;
        }

        if (ShouldBuffItem(repairDetails, pmcData))
        {
            if (
                _itemHelper.IsOfBaseclasses(
                    repairDetails.RepairedItem.Template,
                    [
                        BaseClasses.ARMOR,
                        BaseClasses.VEST,
                        BaseClasses.HEADWEAR,
                        BaseClasses.ARMOR_PLATE
                    ]
                )
            )
            {
                var armorConfig = _repairConfig.RepairKit.Armor;
                AddBuff(armorConfig, repairDetails.RepairedItem);
            }
            else if (_itemHelper.IsOfBaseclass(repairDetails.RepairedItem.Template, BaseClasses.WEAPON))
            {
                var weaponConfig = _repairConfig.RepairKit.Weapon;
                AddBuff(weaponConfig, repairDetails.RepairedItem);
            }
            // TODO: Knife repair kits may be added at some point, a bracket needs to be added here
        }
    }

    /// <summary>
    ///     Add random buff to item
    /// </summary>
    /// <param name="itemConfig">weapon/armor config</param>
    /// <param name="item">Item to repair</param>
    public void AddBuff(BonusSettings itemConfig, Item item)
    {
        var bonusRarityName = _weightedRandomHelper.GetWeightedValue(itemConfig.RarityWeight);
        var bonusTypeName = _weightedRandomHelper.GetWeightedValue(itemConfig.BonusTypeWeight);

        var bonusRarity = bonusRarityName == "Rare" ? itemConfig.Rare : itemConfig.Common;
        var bonusValues = bonusRarity[bonusTypeName].ValuesMinMax;
        var bonusValue = _randomUtil.GetDouble(bonusValues.Min, bonusValues.Max);

        var bonusThresholdPercents = bonusRarity[bonusTypeName].ActiveDurabilityPercentMinMax;
        var bonusThresholdPercent = _randomUtil.GetDouble(
            bonusThresholdPercents.Min,
            bonusThresholdPercents.Max
        );

        item.Upd.Buff = new UpdBuff
        {
            Rarity = bonusRarityName,
            BuffType = Enum.Parse<BuffType>(bonusTypeName),
            Value = bonusValue,
            ThresholdDurability = _randomUtil.GetPercentOfValue(bonusThresholdPercent, item.Upd.Repairable.Durability.Value, 0)
        };
    }

    /// <summary>
    ///     Check if item should be buffed by checking the item type and relevant player skill level
    /// </summary>
    /// <param name="repairDetails">Item that was repaired</param>
    /// <param name="itemTpl">tpl of item to be buffed</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>True if item should have buff applied</returns>
    protected bool ShouldBuffItem(RepairDetails repairDetails, PmcData pmcData)
    {
        var globals = _databaseService.GetGlobals();

        var hasTemplate = _itemHelper.GetItem(repairDetails.RepairedItem.Template);
        if (!hasTemplate.Key)
        {
            return false;
        }

        var template = hasTemplate.Value;

        // Returns SkillTypes.LIGHT_VESTS/HEAVY_VESTS/WEAPON_TREATMENT
        var itemSkillType = GetItemSkillType(template);
        if (itemSkillType is null)
        {
            return false;
        }

        // Skill < level 10 + repairing weapon
        if (
            itemSkillType == SkillTypes.WeaponTreatment &&
            _profileHelper.GetSkillFromProfile(pmcData, SkillTypes.WeaponTreatment)?.Progress < 1000
        )
        {
            return false;
        }

        // Skill < level 10 + repairing armor
        if (
            new HashSet<SkillTypes>
            {
                SkillTypes.LightVests,
                SkillTypes.HeavyVests
            }.Contains(itemSkillType.Value) &&
            _profileHelper.GetSkillFromProfile(pmcData, itemSkillType.Value)?.Progress < 1000
        )
        {
            return false;
        }

        var skillSettings = globals.Configuration.SkillsSettings.GetAllPropsAsDict();
        BuffSettings? buffSettings = null;
        switch (itemSkillType)
        {
            case SkillTypes.LightVests:
            case SkillTypes.HeavyVests:
                buffSettings = ((ArmorSkills) skillSettings[itemSkillType.ToString()]).BuffSettings;
                break;
            case SkillTypes.WeaponTreatment:
                buffSettings = ((WeaponTreatment) skillSettings[itemSkillType.ToString()]).BuffSettings;
                break;
            default:
                _logger.Error($"Unhandled buff type: {itemSkillType}");
                break;
        }

        var commonBuffMinChanceValue = buffSettings.CommonBuffMinChanceValue;
        var commonBuffChanceLevelBonus = buffSettings.CommonBuffChanceLevelBonus;
        var receivedDurabilityMaxPercent = buffSettings.ReceivedDurabilityMaxPercent;

        var skillLevel =
            Math.Truncate((_profileHelper.GetSkillFromProfile(pmcData, itemSkillType.Value)?.Progress ?? 0) / 100);

        if (repairDetails.RepairPoints is null)
        {
            _logger.Error(
                _localisationService.GetText("repair-item_has_no_repair_points", repairDetails.RepairedItem.Template)
            );
        }

        var durabilityToRestorePercent = repairDetails.RepairPoints / template.Properties.MaxDurability;
        var durabilityMultiplier = GetDurabilityMultiplier(
            receivedDurabilityMaxPercent.Value,
            durabilityToRestorePercent.Value
        );

        var doBuff = commonBuffMinChanceValue + commonBuffChanceLevelBonus * skillLevel * durabilityMultiplier;
        var random = new Random();
        return random.Next() <= doBuff;
    }

    /// <summary>
    ///     Based on item, what underlying skill does this item use for buff settings
    /// </summary>
    /// <param name="itemTemplate">Item to check for skill</param>
    /// <returns>Skill name</returns>
    protected SkillTypes? GetItemSkillType(TemplateItem itemTemplate)
    {
        var isArmorRelated = _itemHelper.IsOfBaseclasses(
            itemTemplate.Id,
            [
                BaseClasses.ARMOR,
                BaseClasses.VEST,
                BaseClasses.HEADWEAR,
                BaseClasses.ARMOR_PLATE
            ]
        );

        if (isArmorRelated)
        {
            var armorType = itemTemplate.Properties.ArmorType;
            if (armorType == "Light")
            {
                return SkillTypes.LightVests;
            }

            if (armorType == "Heavy")
            {
                return SkillTypes.HeavyVests;
            }
        }

        if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.WEAPON))
        {
            return SkillTypes.WeaponTreatment;
        }

        if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.KNIFE))
        {
            return SkillTypes.Melee;
        }

        return null;
    }

    /// <summary>
    ///     Ensure multiplier is between 1 and 0.01
    /// </summary>
    /// <param name="receiveDurabilityMaxPercent">Max durability percent</param>
    /// <param name="receiveDurabilityPercent">current durability percent</param>
    /// <returns>durability multiplier value</returns>
    protected double GetDurabilityMultiplier(double receiveDurabilityMaxPercent, double receiveDurabilityPercent)
    {
        // Ensure the max percent is at least 0.01
        var validMaxPercent = Math.Max(0.01, receiveDurabilityMaxPercent);
        // Calculate the ratio and constrain it between 0.01 and 1
        return Math.Min(1, Math.Max(0.01, receiveDurabilityPercent / validMaxPercent));
    }
}

public class RepairDetails
{
    [JsonPropertyName("repairCost")]
    public double? RepairCost
    {
        get;
        set;
    }

    [JsonPropertyName("repairPoints")]
    public double? RepairPoints
    {
        get;
        set;
    }

    [JsonPropertyName("repairedItem")]
    public Item? RepairedItem
    {
        get;
        set;
    }

    [JsonPropertyName("repairedItemIsArmor")]
    public bool? RepairedItemIsArmor
    {
        get;
        set;
    }

    [JsonPropertyName("repairAmount")]
    public double? RepairAmount
    {
        get;
        set;
    }

    [JsonPropertyName("repairedByKit")]
    public bool? RepairedByKit
    {
        get;
        set;
    }
}
