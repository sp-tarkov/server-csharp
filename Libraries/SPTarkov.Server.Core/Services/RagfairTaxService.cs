using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairTaxService(
    ISptLogger<RagfairTaxService> logger,
    DatabaseService databaseService,
    RagfairPriceService ragfairPriceService,
    ItemHelper itemHelper,
    ICloner cloner
)
{
    protected readonly Dictionary<MongoId, StorePlayerOfferTaxAmountRequestData> _playerOfferTaxCache = new();

    public void StoreClientOfferTaxValue(MongoId sessionId, StorePlayerOfferTaxAmountRequestData offer)
    {
        _playerOfferTaxCache[offer.Id.Value] = offer;
    }

    public void ClearStoredOfferTaxById(MongoId offerIdToRemove)
    {
        _playerOfferTaxCache.Remove(offerIdToRemove);
    }

    public StorePlayerOfferTaxAmountRequestData? GetStoredClientOfferTaxValueById(MongoId offerIdToGet)
    {
        return _playerOfferTaxCache.GetValueOrDefault(offerIdToGet);
    }

    /// <summary>
    ///     This method, along with CalculateItemWorth, is trying to mirror the client-side code found in the method "CalculateTaxPrice".
    ///     It's structured to resemble the client-side code as closely as possible - avoid making any big structure changes if it's not necessary.
    /// </summary>
    /// <param name="item"> Item being sold on flea </param>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="requirementsValue"></param>
    /// <param name="offerItemCount"> Number of offers being created </param>
    /// <param name="sellInOnePiece"></param>
    /// <returns> Tax in roubles </returns>
    public double CalculateTax(Item item, PmcData pmcData, double? requirementsValue, int? offerItemCount, bool sellInOnePiece)
    {
        if (requirementsValue is null)
        {
            return 0;
        }

        if (offerItemCount is null)
        {
            return 0;
        }

        var globals = databaseService.GetGlobals();

        var itemTemplate = itemHelper.GetItem(item.Template).Value;
        var itemWorth = CalculateItemWorth(item, itemTemplate, offerItemCount.Value, pmcData);
        var requirementsPrice = requirementsValue * (sellInOnePiece ? 1 : offerItemCount);

        var itemTaxMult = globals.Configuration.RagFair.CommunityItemTax / 100.0;
        var requirementTaxMult = globals.Configuration.RagFair.CommunityRequirementTax / 100.0;

        var itemPriceMult = Math.Log10(itemWorth / requirementsPrice.Value);
        var requirementPriceMult = Math.Log10(requirementsPrice.Value / itemWorth);

        if (requirementsPrice >= itemWorth)
        {
            requirementPriceMult = Math.Pow(requirementPriceMult, 1.08);
        }
        else
        {
            itemPriceMult = Math.Pow(itemPriceMult, 1.08);
        }

        itemPriceMult = Math.Pow(4.0, itemPriceMult);
        requirementPriceMult = Math.Pow(4.0, requirementPriceMult);

        var hideoutFleaTaxDiscountBonusSum = pmcData.GetBonusValueFromProfile(BonusType.RagfairCommission);
        // A negative bonus implies a lower discount, since we subtract later, invert the value here
        var taxDiscountPercent = -(hideoutFleaTaxDiscountBonusSum / 100.0);

        var tax = itemWorth * itemTaxMult * itemPriceMult + requirementsPrice * requirementTaxMult * requirementPriceMult;
        var discountedTax = tax * (1.0 - taxDiscountPercent);
        var itemComissionMult = itemTemplate.Properties.RagFairCommissionModifier ?? 1;

        if (item.Upd.Buff is not null)
        {
            var buffType = item.Upd.Buff.BuffType;
            var itemEnhancementSettings = databaseService.GetGlobals().Configuration.RepairSettings.ItemEnhancementSettings;
            var priceModiferValue = buffType switch
            {
                BuffType.DamageReduction => itemEnhancementSettings.DamageReduction.PriceModifierValue,
                BuffType.MalfunctionProtections => itemEnhancementSettings.MalfunctionProtections.PriceModifierValue,
                BuffType.WeaponSpread => itemEnhancementSettings.WeaponSpread.PriceModifierValue,
                _ => 1d,
            };
            discountedTax *= 1.0 + Math.Abs(item.Upd.Buff.Value.Value - 1.0) * priceModiferValue;
        }

        var taxValue = Math.Round(discountedTax.Value * itemComissionMult);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Tax Calculated to be: {taxValue}");
        }

        return taxValue;
    }

    /// <summary>
    ///     This method is trying to replicate the item worth calculation method found in the client code.
    ///     Any inefficiencies or style issues are intentional and should not be fixed, to preserve the client-side code mirroring.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="itemTemplate"></param>
    /// <param name="itemCount"></param>
    /// <param name="pmcData"></param>
    /// <param name="isRootItem"></param>
    /// <returns></returns>
    protected double CalculateItemWorth(Item item, TemplateItem itemTemplate, int itemCount, PmcData pmcData, bool isRootItem = true)
    {
        var worth = ragfairPriceService.GetFleaPriceForItem(item.Template);

        // In client, all item slots are traversed and any items contained within have their values added
        if (isRootItem)
        {
            // Since we get a flat list of all child items, we only want to recurse from parent item
            var itemChildren = pmcData.Inventory.Items.GetItemWithChildren(item.Id);
            if (itemChildren.Count > 1)
            {
                var itemChildrenClone = cloner.Clone(itemChildren); // Clone is expensive, only run if necessary
                foreach (var child in itemChildrenClone.Where(child => child.Id != item.Id))
                {
                    child.Upd ??= new Upd();

                    worth += CalculateItemWorth(
                        child,
                        itemHelper.GetItem(child.Template).Value,
                        (int)(child.Upd.StackObjectsCount ?? 1),
                        pmcData,
                        false
                    );
                }
            }
        }

        var upd = item.Upd ??= new Upd();

        if (upd.Dogtag is not null)
        {
            worth *= upd.Dogtag.Level.Value;
        }

        if (itemTemplate.Properties is null)
        {
            logger.Warning($"Item: {item.Id} lacks _props and cannot have its worth calculated properly");

            return worth;
        }

        if (upd.Key is not null && (itemTemplate.Properties.MaximumNumberOfUsage ?? 0) > 0)
        {
            worth =
                worth
                / (itemTemplate.Properties.MaximumNumberOfUsage ?? 1)
                * ((itemTemplate.Properties.MaximumNumberOfUsage ?? 1) - upd.Key.NumberOfUsages.Value);
        }

        if (upd.Resource is not null && (itemTemplate.Properties.MaxResource ?? 0) > 0)
        {
            worth = (double)(worth * 0.1 + worth * 0.9 / (itemTemplate.Properties.MaxResource ?? 1) * upd.Resource.Value);
        }

        if (upd.SideEffect is not null && (itemTemplate.Properties.MaxResource ?? 0) > 0)
        {
            worth = (double)(worth * 0.1 + worth * 0.9 / (itemTemplate.Properties.MaxResource ?? 1) * upd.SideEffect.Value);
        }

        if (upd.MedKit is not null && (itemTemplate.Properties.MaxHpResource ?? 0) > 0)
        {
            worth = worth / (itemTemplate.Properties.MaxHpResource ?? 1) * upd.MedKit.HpResource.Value;
        }

        if (upd.FoodDrink is not null && (itemTemplate.Properties.MaxResource ?? 0) > 0)
        {
            worth = worth / (itemTemplate.Properties.MaxResource ?? 1) * upd.FoodDrink.HpPercent.Value;
        }

        if (upd.Repairable is not null && (itemTemplate.Properties.ArmorClass ?? 0) > 0)
        {
            var num2 = 0.01 * Math.Pow(0.0, upd.Repairable.MaxDurability.Value);
            worth =
                worth * (upd.Repairable.MaxDurability.Value / (itemTemplate.Properties.Durability ?? 1) - num2)
                - Math.Floor(
                    (itemTemplate.Properties.RepairCost ?? 0) * (upd.Repairable.MaxDurability.Value - upd.Repairable.Durability.Value)
                );
        }

        return worth * itemCount;
    }
}
