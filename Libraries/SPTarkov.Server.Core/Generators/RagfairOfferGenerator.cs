using System.Diagnostics;
using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Ragfair;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class RagfairOfferGenerator(
    ISptLogger<RagfairOfferGenerator> logger,
    HashUtil hashUtil,
    RandomUtil randomUtil,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    RagfairServerHelper ragfairServerHelper,
    ProfileHelper profileHelper,
    HandbookHelper handbookHelper,
    BotHelper botHelper,
    SaveServer saveServer,
    PresetHelper presetHelper,
    RagfairAssortGenerator ragfairAssortGenerator,
    RagfairOfferService ragfairOfferService,
    RagfairPriceService ragfairPriceService,
    LocalisationService localisationService,
    PaymentHelper paymentHelper,
    ItemHelper itemHelper,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected List<TplWithFleaPrice>? allowedFleaPriceItemsForBarter;
    protected BotConfig botConfig = configServer.GetConfig<BotConfig>();

    /// Internal counter to ensure each offer created has a unique value for its intId property
    protected int offerCounter;

    protected RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();

    /// <summary>
    ///     Create a flea offer and store it in the Ragfair server offers array
    /// </summary>
    /// <param name="userId">Owner of the offer</param>
    /// <param name="time">Time offer is listed at</param>
    /// <param name="items">Items in the offer</param>
    /// <param name="barterScheme">Cost of item (currency or barter)</param>
    /// <param name="loyalLevel">Loyalty level needed to buy item</param>
    /// <param name="quantity">Amount of item being listed</param>
    /// <param name="sellInOnePiece">Flags sellInOnePiece to be true</param>
    /// <returns>RagfairOffer</returns>
    public RagfairOffer CreateAndAddFleaOffer(
        string userId,
        long time,
        List<Item> items,
        List<BarterScheme> barterScheme,
        int loyalLevel,
        int quantity,
        bool sellInOnePiece = false
    )
    {
        var offer = CreateOffer(userId, time, items, barterScheme, loyalLevel, quantity, sellInOnePiece);
        ragfairOfferService.AddOffer(offer);

        return offer;
    }

    /// <summary>
    ///     Create an offer object ready to send to ragfairOfferService.addOffer()
    /// </summary>
    /// <param name="userId">Owner of the offer</param>
    /// <param name="time">Timestamp offer is listed at</param>
    /// <param name="items">Items in the offer</param>
    /// <param name="barterScheme">Cost of item (currency or barter)</param>
    /// <param name="loyalLevel">Loyalty level needed to buy item</param>
    /// <param name="quantity">Amount of item being listed</param>
    /// <param name="isPackOffer">Is offer being created flagged as a pack</param>
    /// <returns>RagfairOffer</returns>
    protected RagfairOffer CreateOffer(
        string userId,
        long time,
        List<Item> items,
        List<BarterScheme> barterScheme,
        int loyalLevel,
        int quantity,
        bool isPackOffer = false
    )
    {
        var offerRequirements = barterScheme.Select(barter =>
                {
                    var offerRequirement = new OfferRequirement
                    {
                        Template = barter.Template,
                        Count = Math.Round(barter.Count.Value, 2),
                        OnlyFunctional = barter.OnlyFunctional ?? false
                    };

                    // Dogtags define level and side
                    if (barter.Level != null)
                    {
                        offerRequirement.Level = barter.Level;
                        offerRequirement.Side = barter.Side;
                    }

                    return offerRequirement;
                }
            )
            .ToList();

        // Clone to avoid modifying original array
        var itemsClone = cloner.Clone(items);
        var rootItem = itemsClone.FirstOrDefault();

        // Hydrate ammo boxes with cartridges + ensure only 1 item is present (ammo box)
        // On offer refresh don't re-add cartridges to ammo box that already has cartridges
        if (itemHelper.IsOfBaseclass(itemsClone[0].Template, BaseClasses.AMMO_BOX) && itemsClone.Count == 1)
        {
            itemHelper.AddCartridgesToAmmoBox(itemsClone, itemHelper.GetItem(rootItem.Template).Value);
        }

        var roubleListingPrice = Math.Round(ConvertOfferRequirementsIntoRoubles(offerRequirements));
        var singleItemListingPrice = isPackOffer ? roubleListingPrice / quantity : roubleListingPrice;

        var offer = new RagfairOffer
        {
            Id = hashUtil.Generate(),
            InternalId = offerCounter,
            User = CreateUserDataForFleaOffer(userId, ragfairServerHelper.IsTrader(userId)),
            Root = rootItem.Id,
            Items = itemsClone,
            ItemsCost = Math.Round(handbookHelper.GetTemplatePrice(rootItem.Template)), // Handbook price
            Requirements = offerRequirements,
            RequirementsCost = Math.Round(singleItemListingPrice),
            SummaryCost = roubleListingPrice,
            StartTime = time,
            EndTime = GetOfferEndTime(userId, time),
            LoyaltyLevel = loyalLevel,
            SellInOnePiece = isPackOffer,
            Locked = false,
            Quantity = quantity
        };

        offerCounter++;

        return offer;
    }

    /// <summary>
    ///     Create the user object stored inside each flea offer object
    /// </summary>
    /// <param name="userId">User creating the offer</param>
    /// <param name="isTrader">Is the user creating the offer a trader</param>
    /// <returns>RagfairOfferUser</returns>
    protected RagfairOfferUser CreateUserDataForFleaOffer(string userId, bool isTrader)
    {
        // Trader offer
        if (isTrader)
        {
            return new RagfairOfferUser
            {
                Id = userId,
                MemberType = MemberCategory.Trader
            };
        }

        var isPlayerOffer = profileHelper.IsPlayer(userId);
        if (isPlayerOffer)
        {
            var playerProfile = profileHelper.GetPmcProfile(userId);
            return new RagfairOfferUser
            {
                Id = playerProfile.Id,
                MemberType = playerProfile.Info.MemberCategory,
                SelectedMemberCategory = playerProfile.Info.SelectedMemberCategory,
                Nickname = playerProfile.Info.Nickname,
                Rating = playerProfile.RagfairInfo.Rating ?? 0,
                IsRatingGrowing = playerProfile.RagfairInfo.IsRatingGrowing,
                Avatar = null,
                Aid = playerProfile.Aid
            };
        }

        // 'Fake' pmc offer
        return new RagfairOfferUser
        {
            Id = userId,
            MemberType = MemberCategory.Default,
            Nickname = botHelper.GetPmcNicknameOfMaxLength(botConfig.BotNameLengthLimit),
            Rating = randomUtil.GetDouble(
                ragfairConfig.Dynamic.Rating.Min,
                ragfairConfig.Dynamic.Rating.Max
            ),
            IsRatingGrowing = randomUtil.GetBool(),
            Avatar = null,
            Aid = hashUtil.GenerateAccountId()
        };
    }

    /// <summary>
    ///     Calculate the offer price that's listed on the flea listing
    /// </summary>
    /// <param name="offerRequirements"> barter requirements for offer </param>
    /// <returns> rouble cost of offer </returns>
    protected double ConvertOfferRequirementsIntoRoubles(IEnumerable<OfferRequirement> offerRequirements)
    {
        var roublePrice = 0d;
        foreach (var requirement in offerRequirements)
        {
            roublePrice += paymentHelper.IsMoneyTpl(requirement.Template)
                ? Math.Round(CalculateRoublePrice(requirement.Count.Value, requirement.Template))
                : ragfairPriceService.GetFleaPriceForItem(requirement.Template) * requirement.Count.Value; // Get flea price for barter offer items
        }

        return roublePrice;
    }

    /// <summary>
    ///     Get avatar url from trader table in db
    /// </summary>
    /// <param name="isTrader"> Is user we're getting avatar for a trader </param>
    /// <param name="userId"> Persons id to get avatar of </param>
    /// <returns> Url of avatar as String </returns>
    protected string GetAvatarUrl(bool isTrader, string userId)
    {
        if (isTrader)
        {
            return databaseService.GetTrader(userId).Base.Avatar;
        }

        return "/files/trader/avatar/unknown.jpg";
    }

    /// <summary>
    ///     Convert a count of currency into roubles
    /// </summary>
    /// <param name="currencyCount"> Amount of currency to convert into roubles </param>
    /// <param name="currencyType"> Type of currency (euro/dollar/rouble) </param>
    /// <returns> Count of roubles </returns>
    protected double CalculateRoublePrice(double currencyCount, string currencyType)
    {
        if (currencyType == Money.ROUBLES)
        {
            return currencyCount;
        }

        return handbookHelper.InRUB(currencyCount, currencyType);
    }

    /// <summary>
    ///     Check userId, if it's a player, return their pmc _id, otherwise return userId parameter
    /// </summary>
    /// <param name="userId"> Users ID to check </param>
    /// <returns> Users ID </returns>
    protected string GetTraderId(string userId)
    {
        if (profileHelper.IsPlayer(userId))
        {
            return saveServer.GetProfile(userId).CharacterData.PmcData.Id;
        }

        return userId;
    }

    /// <summary>
    ///     Get a flea trading rating for the passed in user
    /// </summary>
    /// <param name="userId"> User to get flea rating of </param>
    /// <returns> Flea rating value </returns>
    protected double? GetRating(string userId)
    {
        // Player offer
        if (profileHelper.IsPlayer(userId))
        {
            return saveServer.GetProfile(userId).CharacterData?.PmcData?.RagfairInfo?.Rating;
        }

        // Trader offer
        if (ragfairServerHelper.IsTrader(userId))
        {
            return 1;
        }

        // Generated pmc offer
        return randomUtil.GetDouble(ragfairConfig.Dynamic.Rating.Min, ragfairConfig.Dynamic.Rating.Max);
    }

    /// <summary>
    ///     Is the offers user rating growing
    /// </summary>
    /// <param name="userID"> User to check rating of</param>
    /// <returns> True if growing </returns>
    protected bool GetRatingGrowing(string userID)
    {
        if (profileHelper.IsPlayer(userID))
            // player offer
        {
            return saveServer.GetProfile(userID).CharacterData?.PmcData?.RagfairInfo?.IsRatingGrowing ?? false;
        }

        if (ragfairServerHelper.IsTrader(userID))
            // trader offer
        {
            return true;
        }

        // generated offer
        // 50/50 growing/falling
        return randomUtil.GetBool();
    }

    /// <summary>
    ///     Get number of section until offer should expire
    /// </summary>
    /// <param name="userID"> ID of the offer owner </param>
    /// <param name="time"> Time the offer is posted in seconds </param>
    /// <returns> Number of seconds until offer expires </returns>
    protected long GetOfferEndTime(string userID, long time)
    {
        if (profileHelper.IsPlayer(userID))
        {
            // Player offer = current time + offerDurationTimeInHour;
            var offerDurationTimeHours = databaseService.GetGlobals().Configuration.RagFair.OfferDurationTimeInHour;
            return (long) (timeUtil.GetTimeStamp() + Math.Round((double) offerDurationTimeHours * TimeUtil.OneHourAsSeconds));
        }

        if (ragfairServerHelper.IsTrader(userID))
            // Trader offer
        {
            return (long) databaseService.GetTrader(userID).Base.NextResupply;
        }

        // Generated fake-player offer
        return (long) Math.Round(
            time + randomUtil.GetDouble(ragfairConfig.Dynamic.EndTimeSeconds.Min, ragfairConfig.Dynamic.EndTimeSeconds.Max)
        );
    }

    /// <summary>
    ///     Create multiple offers for items by using a unique list of items we've generated previously
    /// </summary>
    /// <param name="expiredOffers"> Optional, expired offers to regenerate </param>
    public void GenerateDynamicOffers(List<List<Item>>? expiredOffers = null)
    {
        var replacingExpiredOffers = (expiredOffers?.Count ?? 0) > 0;

        var stopwatch = Stopwatch.StartNew();
        // get assort items from param if they exist, otherwise grab freshly generated assorts
        var assortItemsToProcess = replacingExpiredOffers
            ? expiredOffers ?? []
            : ragfairAssortGenerator.GetAssortItems();
        stopwatch.Stop();
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Took {stopwatch.ElapsedMilliseconds}ms to GetRagfairAssorts - {assortItemsToProcess.Count} items");
        }

        stopwatch.Restart();
        var tasks = new List<Task>();
        foreach (var assortItem in assortItemsToProcess)
        {
            tasks.Add(
                Task.Factory.StartNew(() =>
                    {
                        CreateOffersFromAssort(assortItem, replacingExpiredOffers, ragfairConfig.Dynamic);
                    }
                )
            );
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Took {stopwatch.ElapsedMilliseconds}ms to CreateOffersFromAssort");
        }
    }

    /// <summary>
    ///     Generates offers from an item and it's children on the flea market
    /// </summary>
    /// <param name="assortItemWithChildren"> Item with its children to process into offers </param>
    /// <param name="isExpiredOffer"> Is an expired offer </param>
    /// <param name="config"> Ragfair dynamic config </param>
    protected void CreateOffersFromAssort(
        List<Item> assortItemWithChildren,
        bool isExpiredOffer,
        Dynamic config
    )
    {
        var itemToSellDetails = itemHelper.GetItem(assortItemWithChildren[0].Template);
        var isPreset = presetHelper.IsPreset(assortItemWithChildren[0].Upd.SptPresetId);

        // Only perform checks on newly generated items, skip expired items being refreshed
        if (!(isExpiredOffer || ragfairServerHelper.IsItemValidRagfairItem(itemToSellDetails)))
        {
            return;
        }

        // Armor presets can hold plates above the allowed flea level, remove if necessary
        if (isPreset && ragfairConfig.Dynamic.Blacklist.EnableBsgList)
        {
            RemoveBannedPlatesFromPreset(assortItemWithChildren, ragfairConfig.Dynamic.Blacklist.ArmorPlate);
        }

        // Get number of offers to create
        // Limit to 1 offer when processing expired - like-for-like replacement
        var offerCount = isExpiredOffer
            ? 1
            : randomUtil.GetDouble(config.OfferItemCount.Min, config.OfferItemCount.Max);

        /* // TODO: ???????
        if (ProgramStatics.DEBUG && !ProgramStatics.COMPILED) {
            offerCount = 2;
        }
        */

        for (var index = 0; index < offerCount; index++)
        {
            // Clone the item so we don't have shared references and generate new item IDs
            var clonedAssort = cloner.Clone(assortItemWithChildren);
            itemHelper.ReparentItemAndChildren(clonedAssort[0], clonedAssort);

            // Clear unnecessary properties
            clonedAssort[0].ParentId = null;
            clonedAssort[0].SlotId = null;

            CreateSingleOfferForItem(hashUtil.Generate(), clonedAssort, isPreset, itemToSellDetails.Value);
        }
    }

    /// <summary>
    ///     Iterate over an items children and look for plates above desired level and remove them
    /// </summary>
    /// <param name="presetWithChildren"> Preset to check for plates </param>
    /// <param name="plateSettings"> Settings </param>
    /// <returns> True if plates removed </returns>
    protected bool RemoveBannedPlatesFromPreset(
        List<Item> presetWithChildren,
        ArmorPlateBlacklistSettings plateSettings
    )
    {
        if (!itemHelper.ArmorItemCanHoldMods(presetWithChildren[0].Template))
            // Cant hold armor inserts, skip
        {
            return false;
        }

        var plateSlots = presetWithChildren.Where(item => itemHelper.GetRemovablePlateSlotIds().Contains(item.SlotId?.ToLower())).ToList();
        if (plateSlots.Count == 0)
            // Has no plate slots e.g. "front_plate", exit
        {
            return false;
        }

        var removedPlate = false;
        foreach (var plateSlot in plateSlots)
        {
            var plateDetails = itemHelper.GetItem(plateSlot.Template).Value;
            if (plateSettings.IgnoreSlots.Contains(plateSlot.SlotId.ToLower()))
            {
                continue;
            }

            var plateArmorLevel = plateDetails.Properties.ArmorClass ?? 0;
            if (plateArmorLevel > plateSettings.MaxProtectionLevel)
            {
                presetWithChildren.Splice(presetWithChildren.IndexOf(plateSlot), 1);
                removedPlate = true;
            }
        }

        return removedPlate;
    }

    /// <summary>
    ///     Create one flea offer for a specific item
    /// </summary>
    /// <param name="sellerId"> ID of seller</param>
    /// <param name="itemWithChildren"> Item to create offer for </param>
    /// <param name="isPreset"> Is item a weapon preset</param>
    /// <param name="itemToSellDetails"> Raw DB item details </param>
    protected void CreateSingleOfferForItem(
        string sellerId,
        List<Item> itemWithChildren,
        bool isPreset,
        TemplateItem itemToSellDetails
    )
    {
        // Get randomised amount to list on flea
        var desiredStackSize = ragfairServerHelper.CalculateDynamicStackCount(
            itemWithChildren[0].Template,
            isPreset
        );

        // Reset stack count to 1 from whatever it was prior
        itemWithChildren[0].Upd.StackObjectsCount = 1;

        var isBarterOffer = randomUtil.GetChance100(ragfairConfig.Dynamic.Barter.ChancePercent);
        var isPackOffer =
            randomUtil.GetChance100(ragfairConfig.Dynamic.Pack.ChancePercent) &&
            !isBarterOffer &&
            itemWithChildren.Count == 1 &&
            itemHelper.IsOfBaseclasses(
                itemWithChildren[0].Template,
                ragfairConfig.Dynamic.Pack.ItemTypeWhitelist
            );

        // Remove removable plates if % check passes
        if (itemHelper.ArmorItemCanHoldMods(itemWithChildren[0].Template))
        {
            var armorConfig = ragfairConfig.Dynamic.Armor;

            var shouldRemovePlates = randomUtil.GetChance100(armorConfig.RemoveRemovablePlateChance);
            if (shouldRemovePlates && itemHelper.ArmorItemHasRemovablePlateSlots(itemWithChildren[0].Template))
            {
                var offerItemPlatesToRemove = itemWithChildren.Where(item =>
                    armorConfig.PlateSlotIdToRemovePool.Contains(item.SlotId?.ToLower())
                );

                // Latest first, to ensure we don't move later items off by 1 each time we remove an item below it
                var indexesToRemove = offerItemPlatesToRemove.Select(plateItem => itemWithChildren.IndexOf(plateItem))
                    .ToHashSet();
                foreach (var index in indexesToRemove.OrderByDescending(x => x))
                {
                    itemWithChildren.RemoveAt(index);
                }
            }
        }

        List<BarterScheme> barterScheme;
        if (isPackOffer)
        {
            // Set pack size
            desiredStackSize = randomUtil.GetInt(
                ragfairConfig.Dynamic.Pack.ItemCountMin,
                ragfairConfig.Dynamic.Pack.ItemCountMax
            );

            // Don't randomise pack items
            barterScheme = CreateCurrencyBarterScheme(itemWithChildren, isPackOffer, desiredStackSize);
        }
        else if (isBarterOffer)
        {
            // Apply randomised properties
            RandomiseOfferItemUpdProperties(sellerId, itemWithChildren, itemToSellDetails);
            barterScheme = CreateBarterBarterScheme(itemWithChildren, ragfairConfig.Dynamic.Barter);
            if (ragfairConfig.Dynamic.Barter.MakeSingleStackOnly)
            {
                itemWithChildren[0].Upd.StackObjectsCount = 1;
            }
        }
        else
        {
            // Apply randomised properties
            RandomiseOfferItemUpdProperties(sellerId, itemWithChildren, itemToSellDetails);
            barterScheme = CreateCurrencyBarterScheme(itemWithChildren, isPackOffer);
        }

        var offer = CreateAndAddFleaOffer(
            sellerId,
            timeUtil.GetTimeStamp(),
            itemWithChildren,
            barterScheme,
            1,
            desiredStackSize,
            isPackOffer // sellAsOnePiece - pack offer
        );
    }

    /// <summary>
    ///     Generate trader offers on flea using the traders assort data
    /// </summary>
    /// <param name="traderID"> Trader to generate offers for </param>
    public void GenerateFleaOffersForTrader(string traderID)
    {
        // Purge
        ragfairOfferService.RemoveAllOffersByTrader(traderID);

        var time = timeUtil.GetTimeStamp();
        var trader = databaseService.GetTrader(traderID);
        var assortsClone = cloner.Clone(trader.Assort);

        // Trader assorts / assort items are missing
        if (assortsClone?.Items?.Count is null or 0)
        {
            logger.Error(
                localisationService.GetText(
                    "ragfair-no_trader_assorts_cant_generate_flea_offers",
                    trader.Base.Nickname
                )
            );
            return;
        }

        var blacklist = ragfairConfig.Dynamic.Blacklist;
        var childAssortItems = assortsClone.Items
            .Where(x => !string.Equals(x.ParentId, "hideout", StringComparison.Ordinal)).ToList();
        foreach (var item in assortsClone.Items)
        {
            // We only want to process 'base/root' items, no children
            if (item.SlotId != "hideout")
                // skip mod items
            {
                continue;
            }

            // Run blacklist check on trader offers
            if (blacklist.TraderItems)
            {
                var itemDetails = itemHelper.GetItem(item.Template);
                if (!itemDetails.Key)
                {
                    logger.Warning(localisationService.GetText("ragfair-tpl_not_a_valid_item", item.Template));
                    continue;
                }

                // Don't include items that BSG has blacklisted from flea
                if (blacklist.EnableBsgList && !(itemDetails.Value?.Properties?.CanSellOnRagfair ?? false))
                {
                    continue;
                }
            }

            var isPreset = presetHelper.IsPreset(item.Id);
            var items = isPreset
                ? ragfairServerHelper.GetPresetItems(item)
                : [item, ..itemHelper.FindAndReturnChildrenByAssort(item.Id, childAssortItems)];

            if (!assortsClone.BarterScheme.TryGetValue(item.Id, out var barterScheme))
            {
                logger.Warning(
                    localisationService.GetText(
                        "ragfair-missing_barter_scheme",
                        new
                        {
                            itemId = item.Id,
                            tpl = item.Template,
                            name = trader.Base.Nickname
                        }
                    )
                );
                continue;
            }

            var barterSchemeItems = barterScheme[0];
            var loyalLevel = assortsClone.LoyalLevelItems[item.Id];

            var offer = CreateAndAddFleaOffer(traderID, time, items, barterSchemeItems, loyalLevel, (int?) item.Upd.StackObjectsCount ?? 1);

            // Refresh complete, reset flag to false
            trader.Base.RefreshTraderRagfairOffers = false;
        }
    }

    /// <summary>
    ///     Get array of an item with its mods + condition properties (e.g. durability) <br />
    ///     Apply randomisation adjustments to condition if item base is found in ragfair.json/dynamic/condition
    /// </summary>
    /// <param name="userID"> ID of owner of item </param>
    /// <param name="itemWithMods"> Item and mods, get condition of first item (only first array item is modified) </param>
    /// <param name="itemDetails"> DB details of first item</param>
    protected void RandomiseOfferItemUpdProperties(string userID, List<Item> itemWithMods, TemplateItem itemDetails)
    {
        // Add any missing properties to first item in array
        AddMissingConditions(itemWithMods[0]);

        if (!(profileHelper.IsPlayer(userID) || ragfairServerHelper.IsTrader(userID)))
        {
            var parentId = GetDynamicConditionIdForTpl(itemDetails.Id);
            if (string.IsNullOrEmpty(parentId))
                // No condition details found, don't proceed with modifying item conditions
            {
                return;
            }

            // Roll random chance to randomise item condition
            if (randomUtil.GetChance100(ragfairConfig.Dynamic.Condition[parentId].ConditionChance * 100))
            {
                RandomiseItemCondition(parentId, itemWithMods, itemDetails);
            }
        }
    }

    /// <summary>
    ///     Get the relevant condition id if item tpl matches in ragfair.json/condition
    /// </summary>
    /// <param name="tpl"> Item to look for matching condition object</param>
    /// <returns> Condition ID </returns>
    protected string? GetDynamicConditionIdForTpl(string tpl)
    {
        // Get keys from condition config dictionary
        var configConditions = ragfairConfig.Dynamic.Condition.Keys;
        foreach (var baseClass in configConditions)
        {
            if (itemHelper.IsOfBaseclass(tpl, baseClass))
            {
                return baseClass;
            }
        }

        return null;
    }

    /// <summary>
    ///     Alter an items condition based on its item base type
    /// </summary>
    /// <param name="conditionSettingsId"> Also the parentID of item being altered </param>
    /// <param name="itemWithMods"> Item to adjust condition details of </param>
    /// <param name="itemDetails"> DB Item details of first item in list </param>
    protected void RandomiseItemCondition(
        string conditionSettingsId,
        List<Item> itemWithMods,
        TemplateItem itemDetails
    )
    {
        var rootItem = itemWithMods[0];

        var itemConditionValues = ragfairConfig.Dynamic.Condition[conditionSettingsId];
        var maxMultiplier = randomUtil.GetDouble(itemConditionValues.Max.Min, itemConditionValues.Max.Min);
        var currentMultiplier = randomUtil.GetDouble(
            itemConditionValues.Current.Min,
            itemConditionValues.Current.Max
        );

        // Randomise armor + plates + armor related things
        if (itemHelper.ArmorItemCanHoldMods(rootItem.Template) ||
            itemHelper.IsOfBaseclasses(rootItem.Template, [BaseClasses.ARMOR_PLATE, BaseClasses.ARMORED_EQUIPMENT])
           )
        {
            RandomiseArmorDurabilityValues(itemWithMods, currentMultiplier, maxMultiplier);

            // Add hits to visor
            var visorMod = itemWithMods.FirstOrDefault(item => item.ParentId == BaseClasses.ARMORED_EQUIPMENT && item.SlotId == "mod_equipment_000");
            if (randomUtil.GetChance100(25) && visorMod != null)
            {
                itemHelper.AddUpdObjectToItem(visorMod);

                visorMod.Upd.FaceShield = new UpdFaceShield
                {
                    Hits = randomUtil.GetInt(1, 3)
                };
            }

            return;
        }

        // Randomise Weapons
        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.WEAPON))
        {
            RandomiseWeaponDurability(itemWithMods[0], itemDetails, maxMultiplier, currentMultiplier);

            return;
        }

        if (rootItem.Upd?.MedKit != null)
        {
            // Randomize health
            var hpResource = Math.Round((double) rootItem.Upd.MedKit.HpResource * maxMultiplier);
            rootItem.Upd.MedKit.HpResource = hpResource == 0D ? 1D : hpResource;
            return;
        }

        if (rootItem.Upd?.Key != null && itemDetails.Properties.MaximumNumberOfUsage > 1)
        {
            // Randomize key uses
            rootItem.Upd.Key.NumberOfUsages = (int?) Math.Round(itemDetails.Properties.MaximumNumberOfUsage.Value * (1 - maxMultiplier));
            return;
        }

        if (rootItem.Upd?.FoodDrink != null)
        {
            // randomize food/drink value
            var hpPercent = Math.Round((double) itemDetails.Properties.MaxResource * maxMultiplier);
            rootItem.Upd.FoodDrink.HpPercent = hpPercent == 0D ? 1D : hpPercent;

            return;
        }

        if (rootItem.Upd?.RepairKit != null)
        {
            // randomize repair kit (armor/weapon) uses
            var resource = Math.Round((double) itemDetails.Properties.MaxRepairResource * maxMultiplier);
            rootItem.Upd.RepairKit.Resource = resource == 0D ? 1D : resource;

            return;
        }

        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.FUEL))
        {
            var totalCapacity = itemDetails.Properties.MaxResource;
            var remainingFuel = Math.Round((double) totalCapacity * maxMultiplier);
            rootItem.Upd.Resource = new UpdResource
            {
                UnitsConsumed = totalCapacity - remainingFuel,
                Value = remainingFuel
            };
        }
    }

    /// <summary>
    ///     Adjust an items durability/maxDurability value
    /// </summary>
    /// <param name="item"> Item (weapon/armor) to adjust </param>
    /// <param name="itemDbDetails"> Item details from DB </param>
    /// <param name="maxMultiplier"> Value to multiply max durability by </param>
    /// <param name="currentMultiplier"> Value to multiply current durability by </param>
    protected void RandomiseWeaponDurability(
        Item item,
        TemplateItem itemDbDetails,
        double maxMultiplier,
        double currentMultiplier
    )
    {
        // Max
        var baseMaxDurability = itemDbDetails.Properties.MaxDurability;
        var lowestMaxDurability = randomUtil.GetDouble(maxMultiplier, 1) * baseMaxDurability;
        var chosenMaxDurability = Math.Round(randomUtil.GetDouble((double) lowestMaxDurability, (double) baseMaxDurability));

        // Current
        var lowestCurrentDurability = randomUtil.GetDouble(currentMultiplier, 1) * chosenMaxDurability;
        var chosenCurrentDurability = Math.Round(randomUtil.GetDouble(lowestCurrentDurability, chosenMaxDurability));

        item.Upd.Repairable.Durability = chosenCurrentDurability == 0 ? 1D : chosenCurrentDurability; // Never var value become 0
        item.Upd.Repairable.MaxDurability = chosenMaxDurability;
    }

    /// <summary>
    ///     Randomise the durability values for an armors plates and soft inserts
    /// </summary>
    /// <param name="armorWithMods"> Armor item with its child mods </param>
    /// <param name="currentMultiplier"> Chosen multiplier to use for current durability value </param>
    /// <param name="maxMultiplier"> Chosen multiplier to use for max durability value </param>
    protected void RandomiseArmorDurabilityValues(
        List<Item> armorWithMods,
        double currentMultiplier,
        double maxMultiplier
    )
    {
        foreach (var armorItem in armorWithMods)
        {
            var itemDbDetails = itemHelper.GetItem(armorItem.Template).Value;
            if (itemDbDetails.Properties.ArmorClass > 1)
            {
                itemHelper.AddUpdObjectToItem(armorItem);

                var baseMaxDurability = itemDbDetails.Properties.MaxDurability;
                var lowestMaxDurability = randomUtil.GetDouble(maxMultiplier, 1) * baseMaxDurability;
                var chosenMaxDurability = Math.Round(randomUtil.GetDouble((double) lowestMaxDurability, (double) baseMaxDurability));

                var lowestCurrentDurability = randomUtil.GetDouble(currentMultiplier, 1) * chosenMaxDurability;
                var chosenCurrentDurability = Math.Round(randomUtil.GetDouble(lowestCurrentDurability, chosenMaxDurability));

                armorItem.Upd.Repairable = new UpdRepairable
                {
                    Durability = chosenCurrentDurability == 0D ? 1D : chosenCurrentDurability, // Never var value become 0
                    MaxDurability = chosenMaxDurability
                };
            }
        }
    }

    /// <summary>
    ///     Add missing conditions to an item if needed. <br />
    ///     Durabiltiy for repairable items. <br />
    ///     HpResource for medical items.
    /// </summary>
    /// <param name="item"> Item to add conditions to </param>
    protected void AddMissingConditions(Item item)
    {
        var props = itemHelper.GetItem(item.Template).Value.Properties;
        var isRepairable = props.Durability != null;
        var isMedkit = props.MaxHpResource != null;
        var isKey = props.MaximumNumberOfUsage != null;
        var isConsumable = props.MaxResource > 1 && props.FoodUseTime != null;
        var isRepairKit = props.MaxRepairResource != null;

        if (isRepairable && props.Durability > 0)
        {
            item.Upd.Repairable = new UpdRepairable
            {
                Durability = props.Durability,
                MaxDurability = props.Durability
            };

            return;
        }

        if (isMedkit && props.MaxHpResource > 0)
        {
            item.Upd.MedKit = new UpdMedKit
            {
                HpResource = props.MaxHpResource
            };

            return;
        }

        if (isKey)
        {
            item.Upd.Key = new UpdKey
            {
                NumberOfUsages = 0
            };

            return;
        }

        // Food/drink
        if (isConsumable)
        {
            item.Upd.FoodDrink = new UpdFoodDrink
            {
                HpPercent = props.MaxResource
            };

            return;
        }

        if (isRepairKit)
        {
            item.Upd.RepairKit = new UpdRepairKit
            {
                Resource = props.MaxRepairResource
            };
        }
    }

    /// <summary>
    ///     Create a barter-based barter scheme, if not possible, fall back to making barter scheme currency based
    /// </summary>
    /// <param name="offerItems"> Items for sale in offer </param>
    /// <param name="barterConfig"> Barter config from ragfairConfig.Dynamic.barter </param>
    /// <returns> Barter scheme </returns>
    protected List<BarterScheme> CreateBarterBarterScheme(List<Item> offerItems, BarterDetails barterConfig)
    {
        // Get flea price of item being sold
        var priceOfOfferItem = ragfairPriceService.GetDynamicOfferPriceForOffer(
            offerItems,
            Money.ROUBLES,
            false
        );

        // Don't make items under a designated rouble value into barter offers
        if (priceOfOfferItem < barterConfig.MinRoubleCostToBecomeBarter)
        {
            return CreateCurrencyBarterScheme(offerItems, false);
        }

        // Get a randomised number of barter items to list offer for
        var barterItemCount = randomUtil.GetInt(barterConfig.ItemCountMin, barterConfig.ItemCountMax);

        // Get desired cost of individual item offer will be listed for e.g. offer = 15k, item count = 3, desired item cost = 5k
        var desiredItemCostRouble = Math.Round(priceOfOfferItem / barterItemCount);

        // Rouble amount to go above/below when looking for an item (Wiggle cost of item a little)
        var offerCostVarianceRoubles = desiredItemCostRouble * barterConfig.PriceRangeVariancePercent / 100;

        // Dict of items and their flea price (cached on first use)
        var itemFleaPrices = GetFleaPricesAsArray();

        // Filter possible barters to items that match the price range + not itself
        var min = desiredItemCostRouble - offerCostVarianceRoubles;
        var max = desiredItemCostRouble + offerCostVarianceRoubles;
        var itemsInsidePriceBounds = itemFleaPrices.Where(itemAndPrice =>
                itemAndPrice.Price >= min &&
                itemAndPrice.Price <= max &&
                !string.Equals(itemAndPrice.Tpl, offerItems[0].Template,
                    StringComparison.OrdinalIgnoreCase) // Don't allow the item being sold to be chosen
        );


        // No items on flea have a matching price, fall back to currency
        if (!itemsInsidePriceBounds.Any())
        {
            return CreateCurrencyBarterScheme(offerItems, false);
        }

        // Choose random item from price-filtered flea items
        var randomItem = randomUtil.GetArrayValue(itemsInsidePriceBounds.ToList());

        return
        [
            new BarterScheme
            {
                Count = barterItemCount,
                Template = randomItem.Tpl
            }
        ];
    }

    /// <summary>
    ///     Get an array of flea prices + item tpl, cached in generator class inside `allowedFleaPriceItemsForBarter`
    /// </summary>
    /// <returns> List with tpl/price values </returns>
    protected List<TplWithFleaPrice> GetFleaPricesAsArray()
    {
        // Generate if needed
        if (allowedFleaPriceItemsForBarter == null)
        {
            var fleaPrices = databaseService.GetPrices();

            // Only get prices for items that also exist in items.json
            var filteredFleaItems = fleaPrices
                .Select(kvTpl => new TplWithFleaPrice
                    {
                        Tpl = kvTpl.Key,
                        Price = kvTpl.Value
                    }
                )
                .Where(item => itemHelper.GetItem(item.Tpl).Key);

            var itemTypeBlacklist = ragfairConfig.Dynamic.Barter.ItemTypeBlacklist;
            allowedFleaPriceItemsForBarter = filteredFleaItems.Where(item => !itemHelper.IsOfBaseclasses(item.Tpl, itemTypeBlacklist)).ToList();
        }

        return allowedFleaPriceItemsForBarter;
    }

    /// <summary>
    ///     Create a random currency-based barter scheme for an array of items
    /// </summary>
    /// <param name="offerWithChildren"> Items on offer </param>
    /// <param name="isPackOffer"> Is the barter scheme being created for a pack offer </param>
    /// <param name="multiplier"> What to multiply the resulting price by </param>
    /// <returns> Barter scheme for offer </returns>
    protected List<BarterScheme> CreateCurrencyBarterScheme(
        List<Item> offerWithChildren,
        bool isPackOffer,
        double multiplier = 1
    )
    {
        var currency = ragfairServerHelper.GetDynamicOfferCurrency();
        var price = ragfairPriceService.GetDynamicOfferPriceForOffer(offerWithChildren, currency, isPackOffer) * multiplier;

        return
        [
            new BarterScheme
            {
                Count = price,
                Template = currency
            }
        ];
    }
}
