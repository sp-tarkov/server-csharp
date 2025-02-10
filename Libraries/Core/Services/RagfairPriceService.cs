using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairPriceService(
    ISptLogger<RagfairPriceService> _logger,
    RandomUtil _randomUtil,
    HandbookHelper _handbookHelper,
    TraderHelper _traderHelper,
    PresetHelper _presetHelper,
    ItemHelper _itemHelper,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    ConfigServer _configServer
)
{
    private readonly RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
    protected Dictionary<string, double>? _staticPrices;

    /// <summary>
    ///     Generate static (handbook) and dynamic (prices.json) flea prices, store inside class as dictionaries
    /// </summary>
    public void Load()
    {
        RefreshStaticPrices();
        RefreshDynamicPrices();
    }

    public string GetRoute()
    {
        return "RagfairPriceService";
    }

    /// <summary>
    ///     Iterate over all items of type "Item" in db and get template price, store in cache
    /// </summary>
    public void RefreshStaticPrices()
    {
        _staticPrices = new Dictionary<string, double>();
        foreach (var item in _databaseService.GetItems().Values.Where(item => item.Type == "Item"))
        {
            _staticPrices[item.Id] = _handbookHelper.GetTemplatePrice(item.Id);
        }
    }

    /// <summary>
    ///     Copy the prices.json data into our dynamic price dictionary
    /// </summary>
    public void RefreshDynamicPrices()
    {
        // TODO: remove as redundant?
    }

    /// <summary>
    ///     Get the dynamic price for an item. If value doesn't exist, use static (handbook) value.
    ///     if no static value, return 1
    /// </summary>
    /// <param name="tplId">Item tpl id to get price for</param>
    /// <returns>price in roubles</returns>
    public double GetFleaPriceForItem(string tplId)
    {
        // Get dynamic price (templates/prices), if that doesnt exist get price from static array (templates/handbook)
        var itemPrice = _itemHelper.GetDynamicItemPrice(tplId) ?? GetStaticPriceForItem(tplId);
        if (itemPrice is null)
        {
            var itemFromDb = _itemHelper.GetItem(tplId);
            _logger.Warning(
                _localisationService.GetText(
                    "ragfair-unable_to_find_item_price_for_item_in_flea_handbook",
                    new
                    {
                        tpl = tplId,
                        name = itemFromDb.Value.Name ?? ""
                    }
                )
            );
        }

        // If no price in dynamic/static, set to 1
        if (itemPrice == 0)
        {
            itemPrice = 1;
        }

        return itemPrice.Value;
    }

    /// <summary>
    ///     Get the flea price for an offers items + children
    /// </summary>
    /// <param name="offerItems">offer item + children to process</param>
    /// <returns>Rouble price</returns>
    public double GetFleaPriceForOfferItems(List<Item> offerItems)
    {
        // Preset weapons take the direct prices.json value, otherwise they're massively inflated
        if (_itemHelper.IsOfBaseclass(offerItems[0].Template, BaseClasses.WEAPON))
        {
            return GetFleaPriceForItem(offerItems[0].Template);
        }

        return offerItems.Sum(item => GetFleaPriceForItem(item.Template));
    }

    /**
     * get the dynamic (flea) price for an item
     * @param itemTpl item template id to look up
     * @returns price in roubles
     */
    public double? GetDynamicPriceForItem(string itemTpl)
    {
        _databaseService.GetPrices().TryGetValue(itemTpl, out var value);

        return value;
    }

    /// <summary>
    ///     Grab the static (handbook) for an item by its tplId
    /// </summary>
    /// <param name="itemTpl">item template id to look up</param>
    /// <returns>price in roubles</returns>
    public double? GetStaticPriceForItem(string itemTpl)
    {
        return _handbookHelper.GetTemplatePrice(itemTpl);
    }

    /// <summary>
    ///     Get prices for all items on flea, prioritize handbook prices first, use prices from prices.json if missing
    ///     This will refresh the caches prior to building the output
    /// </summary>
    /// <returns>Dictionary of item tpls and rouble cost</returns>
    public Dictionary<string, double> GetAllFleaPrices()
    {
        var dynamicPrices = _databaseService.GetPrices();
        // Use dynamic prices first, fill in any gaps with data from static prices (handbook)
        return dynamicPrices.Concat(_staticPrices)
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.First().Value);
    }

    public Dictionary<string, double> GetAllStaticPrices()
    {
        // Refresh the cache so we include any newly added custom items
        if (_staticPrices is null)
        {
            RefreshStaticPrices();
        }

        return _staticPrices;
    }

    /// <summary>
    ///     Get the percentage difference between two values
    /// </summary>
    /// <param name="a">numerical value a</param>
    /// <param name="b">numerical value b</param>
    /// <returns>different in percent</returns>
    protected double GetPriceDifference(double a, double b)
    {
        return 100 * a / (a + b);
    }

    /// <summary>
    ///     Get the rouble price for an assorts barter scheme
    /// </summary>
    /// <param name="barterScheme"></param>
    /// <returns>Rouble price</returns>
    public double GetBarterPrice(List<BarterScheme> barterScheme)
    {
        var price = 0d;

        foreach (var item in barterScheme)
        {
            price += GetStaticPriceForItem(item.Template).Value * item.Count.Value;
        }

        return Math.Round(price);
    }

    /// <summary>
    ///     Generate a currency cost for an item and its mods
    /// </summary>
    /// <param name="offerItems">Item with mods to get price for</param>
    /// <param name="desiredCurrency">Currency price desired in</param>
    /// <param name="isPackOffer">Price is for a pack type offer</param>
    /// <returns>cost of item in desired currency</returns>
    public double GetDynamicOfferPriceForOffer(List<Item> offerItems, string desiredCurrency, bool isPackOffer)
    {
        // Price to return.
        var price = 0d;

        // Iterate over each item in the offer.
        foreach (var item in offerItems)
        {
            // Skip over armor inserts as those are not factored into item prices.
            if (_itemHelper.IsOfBaseclass(item.Template, BaseClasses.BUILT_IN_INSERTS))
            {
                continue;
            }

            price += GetDynamicItemPrice(item.Template, desiredCurrency, item, offerItems, isPackOffer).Value;

            // Check if the item is a weapon preset.
            if (
                    item?.Upd?.SptPresetId is not null &&
                    _presetHelper.IsPresetBaseClass(item.Upd.SptPresetId, BaseClasses.WEAPON)
                )
                // This is a weapon preset, which has it's own price calculation that takes into account the mods in the
                // preset. Since we've already calculated the price for the preset entire preset in
                // `getDynamicItemPrice`, we can skip the rest of the items in the offer.
            {
                break;
            }
        }

        return Math.Round(price);
    }

    /// <summary>
    /// </summary>
    /// <param name="itemTemplateId">items tpl value</param>
    /// <param name="desiredCurrency">Currency to return result in</param>
    /// <param name="item">Item object (used for weapon presets)</param>
    /// <param name="offerItems"></param>
    /// <param name="isPackOffer"></param>
    /// <returns></returns>
    public double? GetDynamicItemPrice(string itemTemplateId, string desiredCurrency, Item item = null, List<Item> offerItems = null, bool? isPackOffer = null)
    {
        var isPreset = false;
        var price = GetFleaPriceForItem(itemTemplateId);

        // Adjust price if below handbook price, based on config.
        if (_ragfairConfig.Dynamic.OfferAdjustment.AdjustPriceWhenBelowHandbookPrice)
        {
            price = AdjustPriceIfBelowHandbook(price, itemTemplateId);
        }

        // Use trader price if higher, based on config.
        if (_ragfairConfig.Dynamic.UseTraderPriceForOffersIfHigher)
        {
            var traderPrice = _traderHelper.GetHighestSellToTraderPrice(itemTemplateId);
            if (traderPrice > price)
            {
                price = traderPrice;
            }
        }

        // Prices for weapon presets are handled differently.
        if (
            item?.Upd?.SptPresetId is not null &&
            offerItems is not null &&
            _presetHelper.IsPresetBaseClass(item.Upd.SptPresetId, BaseClasses.WEAPON)
        )
        {
            price = GetWeaponPresetPrice(item, offerItems, price);
            isPreset = true;
        }

        // Check for existence of manual price adjustment multiplier
        if (_ragfairConfig.Dynamic.ItemPriceMultiplier.TryGetValue(itemTemplateId, out var multiplier))
        {
            price *= multiplier;
        }

        // The quality of the item affects the price + not on the ignore list
        if (item is not null && !_ragfairConfig.Dynamic.IgnoreQualityPriceVarianceBlacklist.Contains(itemTemplateId))
        {
            var qualityModifier = _itemHelper.GetItemQualityModifier(item);
            price *= qualityModifier;
        }

        // Make adjustments for unreasonably priced items.
        foreach (var (key, value) in _ragfairConfig.Dynamic.UnreasonableModPrices)
        {
            if (!_itemHelper.IsOfBaseclass(itemTemplateId, key) || !value.Enabled)
            {
                continue;
            }

            price = AdjustUnreasonablePrice(
                _databaseService.GetHandbook().Items,
                value,
                itemTemplateId,
                price
            );
        }

        // Vary the price based on the type of offer.
        var range = GetOfferTypeRangeValues(isPreset, isPackOffer ?? false);
        price = RandomiseOfferPrice(price, range);

        // Convert to different currency if required.
        var roublesId = Money.ROUBLES;
        if (desiredCurrency != roublesId)
        {
            price = _handbookHelper.FromRUB(price, desiredCurrency);
        }

        if (price < 1)
        {
            return 1;
        }

        return price;
    }

    /// <summary>
    ///     using data from config, adjust an items price to be relative to its handbook price
    /// </summary>
    /// <param name="handbookPrices">Prices of items in handbook</param>
    /// <param name="unreasonableItemChange">Change object from config</param>
    /// <param name="itemTpl">Item being adjusted</param>
    /// <param name="price">Current price of item</param>
    /// <returns>Adjusted price of item</returns>
    protected double AdjustUnreasonablePrice(
        List<HandbookItem> handbookPrices,
        UnreasonableModPrices unreasonableItemChange,
        string itemTpl,
        double price)
    {
        var itemHandbookPrice = handbookPrices.FirstOrDefault(handbookItem => handbookItem.Id == itemTpl);
        if (itemHandbookPrice is not null)
        {
            return price;
        }

        // Flea price is over handbook price
        if (price > itemHandbookPrice.Price * unreasonableItemChange.HandbookPriceOverMultiplier)
        {
            // Skip extreme values
            if (price <= 1)
            {
                return price;
            }

            // Price is over limit, adjust
            return itemHandbookPrice.Price.Value * unreasonableItemChange.NewPriceHandbookMultiplier;
        }

        return price;
    }

    /// <summary>
    ///     Get different min/max price multipliers for different offer types (preset/pack/default)
    /// </summary>
    /// <param name="isPreset">Offer is a preset</param>
    /// <param name="isPack">Offer is a pack</param>
    /// <returns>MinMax values</returns>
    protected MinMax<double> GetOfferTypeRangeValues(bool isPreset, bool isPack)
    {
        // Use different min/max values if the item is a preset or pack
        var priceRanges = _ragfairConfig.Dynamic.PriceRanges;
        if (isPreset)
        {
            return priceRanges.Preset;
        }

        if (isPack)
        {
            return priceRanges.Pack;
        }

        return priceRanges.Default;
    }

    /// <summary>
    ///     Check to see if an items price is below its handbook price and adjust according to values set to config/ragfair.json
    /// </summary>
    /// <param name="itemPrice">price of item</param>
    /// <param name="itemTpl">item template Id being checked</param>
    /// <returns>adjusted price value in roubles</returns>
    protected double AdjustPriceIfBelowHandbook(double itemPrice, string itemTpl)
    {
        var itemHandbookPrice = GetStaticPriceForItem(itemTpl);
        var priceDifferencePercent = GetPriceDifference(itemHandbookPrice.Value, itemPrice);
        var offerAdjustmentSettings = _ragfairConfig.Dynamic.OfferAdjustment;

        // Only adjust price if difference is > a percent AND item price passes threshold set in config
        if (
                priceDifferencePercent >
                offerAdjustmentSettings.MaxPriceDifferenceBelowHandbookPercent &&
                itemPrice >= offerAdjustmentSettings.PriceThresholdRub
            )
            // var itemDetails = this.itemHelper.getItem(itemTpl);
            // this.logger.debug(`item below handbook price {itemDetails[1]._name} handbook: {itemHandbookPrice} flea: ${itemPrice} {priceDifferencePercent}%`);
        {
            return Math.Round(itemHandbookPrice.Value * offerAdjustmentSettings.HandbookPriceMultiplier);
        }

        return itemPrice;
    }

    /// <summary>
    ///     Multiply the price by a randomised curve where n = 2, shift = 2
    /// </summary>
    /// <param name="existingPrice">price to alter</param>
    /// <param name="rangeValues">min and max to adjust price by</param>
    /// <returns>multiplied price</returns>
    protected double RandomiseOfferPrice(double existingPrice, MinMax<double> rangeValues)
    {
        // Multiply by 100 to get 2 decimal places of precision
        var multiplier = _randomUtil.GetBiasedRandomNumber(rangeValues.Min * 100, rangeValues.Max * 100, 2, 2);

        // return multiplier back to its original decimal place location
        return existingPrice * (multiplier / 100);
    }

    /// <summary>
    ///     Calculate the cost of a weapon preset by adding together the price of its mods + base price of default weapon preset
    /// </summary>
    /// <param name="weaponRootItem">base weapon</param>
    /// <param name="weaponWithChildren">weapon plus mods</param>
    /// <param name="existingPrice">price of existing base weapon</param>
    /// <returns>price of weapon in roubles</returns>
    protected double GetWeaponPresetPrice(Item weaponRootItem, List<Item> weaponWithChildren, double existingPrice)
    {
        // Get the default preset for this weapon
        var presetResult = GetWeaponPreset(weaponRootItem);
        if (presetResult.IsDefault)
        {
            return GetFleaPriceForItem(weaponRootItem.Template);
        }

        // Get mods on current gun not in default preset
        var newOrReplacedModsInPresetVsDefault = weaponWithChildren.Where(x => !presetResult.Preset.Items.Any(y => y.Template == x.Template));

        // Add up extra mods price
        var extraModsPrice = 0d;
        foreach (var mod in newOrReplacedModsInPresetVsDefault)
            // Use handbook or trader price, whatever is higher (dont use dynamic flea price as purchased item cannot be relisted)
        {
            extraModsPrice += GetHighestHandbookOrTraderPriceAsRouble(mod.Template).Value;
        }

        // Only deduct cost of replaced mods if there's replaced/new mods
        if (newOrReplacedModsInPresetVsDefault.Any())
        {
            // Add up cost of mods replaced
            var modsReplacedByNewMods = newOrReplacedModsInPresetVsDefault.Where(
                x =>
                    presetResult.Preset.Items.Any(y => y.SlotId == x.SlotId)
            );

            // Add up replaced mods price
            var replacedModsPrice = 0d;
            foreach (var replacedMod in modsReplacedByNewMods)
            {
                replacedModsPrice += GetHighestHandbookOrTraderPriceAsRouble(replacedMod.Template).Value;
            }

            // Subtract replaced mods total from extra mods total
            extraModsPrice -= replacedModsPrice;
        }

        // return extra mods price + base gun price
        return existingPrice + extraModsPrice;
    }

    /// <summary>
    ///     Get the highest price for an item that is stored in handbook or trader assorts
    /// </summary>
    /// <param name="itemTpl">Item to get highest price of</param>
    /// <returns>rouble cost</returns>
    protected double? GetHighestHandbookOrTraderPriceAsRouble(string itemTpl)
    {
        var price = GetStaticPriceForItem(itemTpl);
        var traderPrice = _traderHelper.GetHighestSellToTraderPrice(itemTpl);
        if (traderPrice > price)
        {
            price = traderPrice;
        }

        return price;
    }

    /// <summary>
    ///     Attempt to get the default preset for a weapon, failing that get the first preset in the array
    ///     (assumes default = has encyclopedia entry)
    /// </summary>
    /// <param name="presets">weapon presets to choose from</param>
    /// <returns>Default preset object</returns>
    protected WeaponPreset GetWeaponPreset(Item weapon)
    {
        var defaultPreset = _presetHelper.GetDefaultPreset(weapon.Template);
        if (defaultPreset is not null)
        {
            return new WeaponPreset
            {
                IsDefault = true,
                Preset = defaultPreset
            };
        }

        var nonDefaultPresets = _presetHelper.GetPresets(weapon.Template);

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                nonDefaultPresets.Count == 1
                    ? $"Item Id: {weapon.Template} has no default encyclopedia entry but only one preset: ({nonDefaultPresets[0].Name}), choosing preset: ({nonDefaultPresets[0].Name})"
                    : $"Item Id: {weapon.Template} has no default encyclopedia entry, choosing first preset({nonDefaultPresets[0].Name}) of {nonDefaultPresets.Count}"
            );
        }

        return new WeaponPreset
        {
            IsDefault = false,
            Preset = nonDefaultPresets[0]
        };
    }

    public record WeaponPreset
    {
        public bool IsDefault
        {
            get;
            set;
        }

        public Preset Preset
        {
            get;
            set;
        }
    }
}
