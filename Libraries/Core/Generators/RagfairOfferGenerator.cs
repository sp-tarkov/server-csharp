using System.Runtime.InteropServices.JavaScript;
using Core.Helpers;
using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Ragfair;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Server;
using SptCommon.Extensions;

namespace Core.Generators;

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
    FenceService fenceService,
    ItemHelper itemHelper,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();
    protected TraderConfig traderConfig = configServer.GetConfig<TraderConfig>();
    protected BotConfig botConfig = configServer.GetConfig<BotConfig>();
    protected List<TplWithFleaPrice>? allowedFleaPriceItemsForBarter;

    /** Internal counter to ensure each offer created has a unique value for its intId property */
    protected int offerCounter = 0;
    
    /**
     * Create a flea offer and store it in the Ragfair server offers array
     * @param userID Owner of the offer
     * @param time Time offer is listed at
     * @param items Items in the offer
     * @param barterScheme Cost of item (currency or barter)
     * @param loyalLevel Loyalty level needed to buy item
     * @param sellInOnePiece Flags sellInOnePiece to be true
     * @returns Created flea offer
     */
    public RagfairOffer CreateAndAddFleaOffer(
        string userID,
        long time,
        List<Item> items,
        List<BarterScheme> barterScheme,
        int loyalLevel,
        bool sellInOnePiece = false
    )
    {
        var offer = CreateOffer(userID, time, items, barterScheme, loyalLevel, sellInOnePiece);
        ragfairOfferService.AddOffer(offer);

        return offer;
    }

    /**
     * Create an offer object ready to send to ragfairOfferService.addOffer()
     * @param userID Owner of the offer
     * @param time Time offer is listed at
     * @param items Items in the offer
     * @param barterScheme Cost of item (currency or barter)
     * @param loyalLevel Loyalty level needed to buy item
     * @param isPackOffer Is offer being created flaged as a pack
     * @returns IRagfairOffer
     */
    protected RagfairOffer CreateOffer(
        string userID,
        long time,
        List<Item> items,
        List<BarterScheme> barterScheme,
        int loyalLevel,
        bool isPackOffer = false
    )
    {
        var isTrader = ragfairServerHelper.IsTrader(userID);

        var offerRequirements = barterScheme.Select((barter) => {
            var offerRequirement = new OfferRequirement(){
                Template = barter.Template,
                Count = Math.Round((double) barter.Count, 2),
                OnlyFunctional = barter.OnlyFunctional ?? false,
            };

            // Dogtags define level and side
            if (barter.Level != null) {
                offerRequirement.Level = barter.Level;
                offerRequirement.Side = barter.Side;
            }

            return offerRequirement;
        }).ToList();

        // Clone to avoid modifying original array
        var itemsClone = cloner.Clone(items);
        var itemStackCount = itemsClone[0].Upd?.StackObjectsCount ?? 1;

        // Hydrate ammo boxes with cartridges + ensure only 1 item is present (ammo box)
        // On offer refresh dont re-add cartridges to ammo box that already has cartridges
        if (itemHelper.IsOfBaseclass(itemsClone[0].Template, BaseClasses.AMMO_BOX) && itemsClone.Count == 1) {
            itemHelper.AddCartridgesToAmmoBox(itemsClone, itemHelper.GetItem(items[0].Template).Value);
        }

        var roubleListingPrice = Math.Round((double) ConvertOfferRequirementsIntoRoubles(offerRequirements));
        var singleItemListingPrice = isPackOffer ? roubleListingPrice / itemStackCount : roubleListingPrice;

        var offer = new RagfairOffer() {
            Id= hashUtil.Generate(),
            InternalId= offerCounter,
            User= CreateUserDataForFleaOffer(userID, isTrader),
            Root= items[0].Id,
            Items= itemsClone,
            ItemsCost= Math.Round((double) handbookHelper.GetTemplatePrice(items[0].Template)), // Handbook price
            Requirements= offerRequirements,
            RequirementsCost= Math.Round((double) singleItemListingPrice),
            SummaryCost= roubleListingPrice,
            StartTime= time,
            EndTime= GetOfferEndTime(userID, time),
            LoyaltyLevel= loyalLevel,
            SellInOnePiece= isPackOffer,
            Locked= false,
        };

        offerCounter++;

        return offer;
    }

    /**
     * Create the user object stored inside each flea offer object
     * @param userID user creating the offer
     * @param isTrader Is the user creating the offer a trader
     * @returns IRagfairOfferUser
     */
    protected RagfairOfferUser CreateUserDataForFleaOffer(string userID, bool isTrader)
    {
        // Trader offer
        if (isTrader) {
            return new RagfairOfferUser(){
                Id = userID,
                MemberType = MemberCategory.Trader
            };
        }

        var isPlayerOffer = profileHelper.IsPlayer(userID);
        if (isPlayerOffer) {
            var playerProfile = profileHelper.GetPmcProfile(userID);
            return new RagfairOfferUser {
                Id= playerProfile.Id,
                MemberType= playerProfile.Info.MemberCategory,
                SelectedMemberCategory= playerProfile.Info.SelectedMemberCategory,
                Nickname= playerProfile.Info.Nickname,
                Rating= playerProfile.RagfairInfo.Rating ?? 0,
                IsRatingGrowing= playerProfile.RagfairInfo.IsRatingGrowing,
                Avatar= null,
                Aid= playerProfile.Aid,
            };
        }

        // Fake pmc offer
        return new RagfairOfferUser(){
            Id= userID,
            MemberType= MemberCategory.Default,
            Nickname= botHelper.GetPmcNicknameOfMaxLength(botConfig.BotNameLengthLimit),
            Rating= randomUtil.GetDouble(
                (double) ragfairConfig.Dynamic.Rating.Min,
                (double) ragfairConfig.Dynamic.Rating.Max
            ),
            IsRatingGrowing= randomUtil.GetBool(),
            Avatar= null,
            Aid= hashUtil.GenerateAccountId(),
        };
    }

    /**
     * Calculate the offer price that's listed on the flea listing
     * @param offerRequirements barter requirements for offer
     * @returns rouble cost of offer
     */
    protected int ConvertOfferRequirementsIntoRoubles(List<OfferRequirement> offerRequirements) {
        var roublePrice = 0;
        foreach (var requirement in offerRequirements) {
            roublePrice += (int) (paymentHelper.IsMoneyTpl(requirement.Template)
                ? Math.Round((double) CalculateRoublePrice((int) requirement.Count, requirement.Template))
                : ragfairPriceService.GetFleaPriceForItem(requirement.Template) * requirement.Count); // get flea price for barter offer items
        }

        return roublePrice;
    }

    /**
     * Get avatar url from trader table in db
     * @param isTrader Is user we're getting avatar for a trader
     * @param userId persons id to get avatar of
     * @returns url of avatar
     */
    protected string GetAvatarUrl(bool isTrader, string userId)
    {
        if (isTrader) {
            return databaseService.GetTrader(userId).Base.Avatar;
        }

        return "/files/trader/avatar/unknown.jpg";
    }

    /**
     * Convert a count of currency into roubles
     * @param currencyCount amount of currency to convert into roubles
     * @param currencyType Type of currency (euro/dollar/rouble)
     * @returns count of roubles
     */
    protected int CalculateRoublePrice(int currencyCount, string currencyType)
    {
        if (currencyType == Money.ROUBLES) {
            return currencyCount;
        }

        return handbookHelper.InRUB(currencyCount, currencyType);
    }

    /**
     * Check userId, if its a player, return their pmc _id, otherwise return userId parameter
     * @param userId Users Id to check
     * @returns Users Id
     */
    protected string GetTraderId(string userId)
    {
        if (profileHelper.IsPlayer(userId)) {
            return saveServer.GetProfile(userId).CharacterData.PmcData.Id;
        }

        return userId;
    }

    /**
     * Get a flea trading rating for the passed in user
     * @param userId User to get flea rating of
     * @returns Flea rating value
     */
    protected double? GetRating(string userId)
    {
        if (profileHelper.IsPlayer(userId)) {
            // Player offer
            return saveServer.GetProfile(userId).CharacterData?.PmcData?.RagfairInfo?.Rating;
        }

        if (ragfairServerHelper.IsTrader(userId)) {
            // Trader offer
            return 1;
        }

        // Generated pmc offer
        return randomUtil.GetDouble((double) ragfairConfig.Dynamic.Rating.Min, (double) ragfairConfig.Dynamic.Rating.Max);
    }

    /**
     * Is the offers user rating growing
     * @param userID user to check rating of
     * @returns true if its growing
     */
    protected bool GetRatingGrowing(string userID)
    {
        if (profileHelper.IsPlayer(userID))
        {
            // player offer
            return saveServer.GetProfile(userID).CharacterData?.PmcData?.RagfairInfo?.IsRatingGrowing ?? false;
        }

        if (ragfairServerHelper.IsTrader(userID)) {
            // trader offer
            return true;
        }

        // generated offer
        // 50/50 growing/falling
        return randomUtil.GetBool();
    }

    /**
     * Get number of section until offer should expire
     * @param userID Id of the offer owner
     * @param time Time the offer is posted
     * @returns number of seconds until offer expires
     */
    protected long GetOfferEndTime(string userID, long time)
    {
        if (profileHelper.IsPlayer(userID)) {
            // Player offer = current time + offerDurationTimeInHour;
            var offerDurationTimeHours = databaseService.GetGlobals().Configuration.RagFair.OfferDurationTimeInHour;
            return (long) (timeUtil.GetTimeStamp() + Math.Round((double) offerDurationTimeHours * TimeUtil.OneHourAsSeconds));
        }

        if (ragfairServerHelper.IsTrader(userID)) {
            // Trader offer
            return (long) databaseService.GetTrader(userID).Base.NextResupply;
        }

        // Generated fake-player offer
        return (long) Math.Round((double) (time + randomUtil.GetInt((int) ragfairConfig.Dynamic.EndTimeSeconds.Min, (int) ragfairConfig.Dynamic.EndTimeSeconds.Max)));
    }

    /**
     * Create multiple offers for items by using a unique list of items we've generated previously
     * @param expiredOffers optional, expired offers to regenerate
     */
    public void GenerateDynamicOffers(List<List<Item>>? expiredOffers = null)
    {
        var replacingExpiredOffers = (expiredOffers?.Count ?? 0) > 0;

        // get assort items from param if they exist, otherwise grab freshly generated assorts
        var assortItemsToProcess = replacingExpiredOffers
            ? expiredOffers ?? []
            : ragfairAssortGenerator.GetAssortItems();


        assortItemsToProcess.ForEach(assortItemWithChildren => CreateOffersFromAssort(assortItemWithChildren, replacingExpiredOffers, ragfairConfig.Dynamic));

    }

    /**
     * @param assortItemWithChildren Item with its children to process into offers
     * @param isExpiredOffer is an expired offer
     * @param config Ragfair dynamic config
     */
    protected void CreateOffersFromAssort(
        List<Item> assortItemWithChildren,
        bool isExpiredOffer,
        Dynamic config
    )
    {
        var itemToSellDetails = itemHelper.GetItem(assortItemWithChildren[0].Template);
        var isPreset = presetHelper.IsPreset(assortItemWithChildren[0].Upd.SptPresetId);

        // Only perform checks on newly generated items, skip expired items being refreshed
        if (!(isExpiredOffer || ragfairServerHelper.IsItemValidRagfairItem(itemToSellDetails))) {
            return;
        }

        // Armor presets can hold plates above the allowed flea level, remove if necessary
        if (isPreset && ragfairConfig.Dynamic.Blacklist.EnableBsgList) {
            RemoveBannedPlatesFromPreset(assortItemWithChildren, ragfairConfig.Dynamic.Blacklist.ArmorPlate);
        }

        // Get number of offers to create
        // Limit to 1 offer when processing expired - like-for-like replacement
        var offerCount = isExpiredOffer
            ? 1
            : randomUtil.GetInt((int) config.OfferItemCount.Min, (int) config.OfferItemCount.Max);

        /* TODO: ???????
        if (ProgramStatics.DEBUG && !ProgramStatics.COMPILED) {
            offerCount = 2;
        }
        */

        for (var index = 0; index < offerCount; index++) {
            // Clone the item so we don't have shared references and generate new item IDs
            var clonedAssort = cloner.Clone(assortItemWithChildren);
            itemHelper.ReparentItemAndChildren(clonedAssort[0], clonedAssort);

            // Clear unnecessary properties
            clonedAssort[0].ParentId = null;
            clonedAssort[0].SlotId = null;

            CreateSingleOfferForItem(hashUtil.Generate(), clonedAssort, isPreset, itemToSellDetails.Value);
        }
    }

    /**
     * iterate over an items chidren and look for plates above desired level and remove them
     * @param presetWithChildren preset to check for plates
     * @param plateSettings Settings
     * @returns True if plate removed
     */
    protected bool RemoveBannedPlatesFromPreset(
        List<Item> presetWithChildren,
        ArmorPlateBlacklistSettings plateSettings
    )
    {
        if (!itemHelper.ArmorItemCanHoldMods(presetWithChildren[0].Template)) {
            // Cant hold armor inserts, skip
            return false;
        }

        var plateSlots = presetWithChildren.Where((item) => itemHelper.GetRemovablePlateSlotIds().Contains(item.SlotId?.ToLower())). ToList();
        if (plateSlots.Count == 0) {
            // Has no plate slots e.g. "front_plate", exit
            return false;
        }

        var removedPlate = false;
        foreach (var plateSlot in plateSlots) {
            var plateDetails = itemHelper.GetItem(plateSlot.Template).Value;
            if (plateSettings.IgnoreSlots.Contains(plateSlot.SlotId.ToLower())) {
                continue;
            }

            var plateArmorLevel = plateDetails.Properties.ArmorClass ?? 0;
            if (plateArmorLevel > plateSettings.MaxProtectionLevel) {
                presetWithChildren.Splice(presetWithChildren.IndexOf(plateSlot), 1);
                removedPlate = true;
            }
        }

        return removedPlate;
    }

    /**
     * Create one flea offer for a specific item
     * @param sellerId Id of seller
     * @param itemWithChildren Item to create offer for
     * @param isPreset Is item a weapon preset
     * @param itemToSellDetails Raw db item details
     * @returns Item array
     */
    protected void CreateSingleOfferForItem(
        string sellerId,
        List<Item> itemWithChildren,
        bool isPreset,
        TemplateItem itemToSellDetails
    )
    {
        // Set stack size to random value
        itemWithChildren[0].Upd.StackObjectsCount = ragfairServerHelper.CalculateDynamicStackCount(
            itemWithChildren[0].Template,
            isPreset
        );

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
        if (itemHelper.ArmorItemCanHoldMods(itemWithChildren[0].Template)) {
            var armorConfig = ragfairConfig.Dynamic.Armor;

            var shouldRemovePlates = randomUtil.GetChance100(armorConfig.RemoveRemovablePlateChance);
            if (shouldRemovePlates && itemHelper.ArmorItemHasRemovablePlateSlots(itemWithChildren[0].Template)) {
                var offerItemPlatesToRemove = itemWithChildren.Where((item) =>
                    armorConfig.PlateSlotIdToRemovePool.Contains(item.SlotId?.ToLower())
                );

                foreach (var plateItem in offerItemPlatesToRemove) {
                    itemWithChildren.Splice(itemWithChildren.IndexOf(plateItem), 1);
                }
            }
        }

        List<BarterScheme> barterScheme;
        if (isPackOffer) {
            // Set pack size
            var stackSize = randomUtil.GetInt(
                ragfairConfig.Dynamic.Pack.ItemCountMin,
                ragfairConfig.Dynamic.Pack.ItemCountMax
            );
            itemWithChildren[0].Upd.StackObjectsCount = stackSize;

            // Don't randomise pack items
            barterScheme = CreateCurrencyBarterScheme(itemWithChildren, isPackOffer, stackSize);
        } else if (isBarterOffer) {
            // Apply randomised properties
            RandomiseOfferItemUpdProperties(sellerId, itemWithChildren, itemToSellDetails);
            barterScheme = CreateBarterBarterScheme(itemWithChildren, ragfairConfig.Dynamic.Barter);
            if (ragfairConfig.Dynamic.Barter.MakeSingleStackOnly) {
                itemWithChildren[0].Upd.StackObjectsCount = 1;
            }
        } else {
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
            isPackOffer // sellAsOnePiece - pack offer
        );
    }

    /**
     * Generate trader offers on flea using the traders assort data
     * @param traderID Trader to generate offers for
     */
    public void GenerateFleaOffersForTrader(string traderID)
    {
        // Purge
        ragfairOfferService.RemoveAllOffersByTrader(traderID);

        var time = timeUtil.GetTimeStamp();
        var trader = databaseService.GetTrader(traderID);
        var assortsClone = cloner.Clone(trader.Assort);

        // Trader assorts / assort items are missing
        if (assortsClone?.Items?.Count is null or 0) {
            logger.Error(
                localisationService.GetText(
                    "ragfair-no_trader_assorts_cant_generate_flea_offers",
                    trader.Base.Nickname
                )
            );
            return;
        }

        var blacklist = ragfairConfig.Dynamic.Blacklist;
        foreach (var item in assortsClone.Items) {
            // We only want to process 'base/root' items, no children
            if (item.SlotId != "hideout") {
                // skip mod items
                continue;
            }

            // Run blacklist check on trader offers
            if (blacklist.TraderItems) {
                var itemDetails = itemHelper.GetItem(item.Template);
                if (!itemDetails.Key) {
                    logger.Warning(localisationService.GetText("ragfair-tpl_not_a_valid_item", item.Template));
                    continue;
                }

                // Don't include items that BSG has blacklisted from flea
                if (blacklist.EnableBsgList && !(itemDetails.Value?.Properties?.CanSellOnRagfair ?? false)) {
                    continue;
                }
            }

            var isPreset = presetHelper.IsPreset(item.Id);
            var items = isPreset
                ? ragfairServerHelper.GetPresetItems(item)
                : [item, ..itemHelper.FindAndReturnChildrenByAssort(item.Id, assortsClone.Items)];

            if (!assortsClone.BarterScheme.TryGetValue(item.Id, out var barterScheme))
            {
                logger.Warning(
                    localisationService.GetText(
                        "ragfair-missing_barter_scheme",
                        new { itemId = item.Id, tpl = item.Template, name = trader.Base.Nickname }
                    )
                );
                continue;
            }

            var barterSchemeItems = assortsClone.BarterScheme[item.Id][0];
            var loyalLevel = assortsClone.LoyalLevelItems[item.Id];

            var offer = CreateAndAddFleaOffer(traderID, time, items, barterSchemeItems, loyalLevel, false);

            // Refresh complete, reset flag to false
            trader.Base.RefreshTraderRagfairOffers = false;
        }
    }

    /**
     * Get array of an item with its mods + condition properties (e.g durability)
     * Apply randomisation adjustments to condition if item base is found in ragfair.json/dynamic/condition
     * @param userID id of owner of item
     * @param itemWithMods Item and mods, get condition of first item (only first array item is modified)
     * @param itemDetails db details of first item
     */
    protected void RandomiseOfferItemUpdProperties(string userID, List<Item> itemWithMods, TemplateItem itemDetails)
    {
        // Add any missing properties to first item in array
        AddMissingConditions(itemWithMods[0]);

        if (!(profileHelper.IsPlayer(userID) || ragfairServerHelper.IsTrader(userID))) {
            var parentId = GetDynamicConditionIdForTpl(itemDetails.Id);
            if (string.IsNullOrEmpty(parentId)) {
                // No condition details found, don't proceed with modifying item conditions
                return;
            }

            // Roll random chance to randomise item condition
            if (randomUtil.GetChance100(ragfairConfig.Dynamic.Condition[parentId].ConditionChance * 100)) {
                RandomiseItemCondition(parentId, itemWithMods, itemDetails);
            }
        }
    }

    /**
     * Get the relevant condition id if item tpl matches in ragfair.json/condition
     * @param tpl Item to look for matching condition object
     * @returns condition id
     */
    protected string? GetDynamicConditionIdForTpl(string tpl)
    {
        // Get keys from condition config dictionary
        var configConditions = ragfairConfig.Dynamic.Condition.Keys;
        foreach (var baseClass in configConditions) {
            if (itemHelper.IsOfBaseclass(tpl, baseClass)) {
                return baseClass;
            }
        }

        return null;
    }

    /**
     * Alter an items condition based on its item base type
     * @param conditionSettingsId also the parentId of item being altered
     * @param itemWithMods Item to adjust condition details of
     * @param itemDetails db item details of first item in array
     */
    protected void RandomiseItemCondition(
        string conditionSettingsId,
        List<Item> itemWithMods,
        TemplateItem itemDetails
    )
    {
        var rootItem = itemWithMods[0];

        var itemConditionValues = ragfairConfig.Dynamic.Condition[conditionSettingsId];
        var maxMultiplier = randomUtil.GetDouble((double) itemConditionValues.Max.Min, (double) itemConditionValues.Max.Min);
        var currentMultiplier = randomUtil.GetDouble(
            (double) itemConditionValues.Current.Min,
            (double) itemConditionValues.Current.Max
        );

        // Randomise armor + plates + armor related things
        if (itemHelper.ArmorItemCanHoldMods(rootItem.Template) ||
            itemHelper.IsOfBaseclasses(rootItem.Template, [BaseClasses.ARMOR_PLATE, BaseClasses.ARMORED_EQUIPMENT])
        ) {
            RandomiseArmorDurabilityValues(itemWithMods, currentMultiplier, maxMultiplier);

            // Add hits to visor
            var visorMod = itemWithMods.FirstOrDefault((item) => item.ParentId == BaseClasses.ARMORED_EQUIPMENT && item.SlotId == "mod_equipment_000");
            if (randomUtil.GetChance100(25) && visorMod != null) {
                itemHelper.AddUpdObjectToItem(visorMod);

                visorMod.Upd.FaceShield = new UpdFaceShield() { Hits = randomUtil.GetInt(1, 3) };
            }

            return;
        }

        // Randomise Weapons
        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.WEAPON)) { 
            RandomiseWeaponDurability(itemWithMods[0], itemDetails, maxMultiplier, currentMultiplier);

            return;
        }

        if (rootItem.Upd?.MedKit != null) {
            // Randomize health
            var hpResource = Math.Round((double)rootItem.Upd.MedKit.HpResource * maxMultiplier);
            rootItem.Upd.MedKit.HpResource = hpResource == 0D ? 1D : hpResource;
            return;
        }

        if (rootItem.Upd?.Key != null && itemDetails.Properties.MaximumNumberOfUsage > 1) {
            // Randomize key uses
            rootItem.Upd.Key.NumberOfUsages = Math.Round((double) itemDetails.Properties.MaximumNumberOfUsage * (1 - maxMultiplier));
            return;
        }

        if (rootItem.Upd?.FoodDrink != null) {
            // randomize food/drink value
            var hpPercent = Math.Round((double)itemDetails.Properties.MaxResource * maxMultiplier);
            rootItem.Upd.FoodDrink.HpPercent = hpPercent == 0D ? 1D : hpPercent;

            return;
        }

        if (rootItem.Upd?.RepairKit != null) {
            // randomize repair kit (armor/weapon) uses
            var resource = Math.Round((double)itemDetails.Properties.MaxRepairResource * maxMultiplier);
            rootItem.Upd.RepairKit.Resource = resource == 0D ? 1D : resource;

            return;
        }

        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.FUEL)) {
            var totalCapacity = itemDetails.Properties.MaxResource;
            var remainingFuel = Math.Round((double) totalCapacity * maxMultiplier);
            rootItem.Upd.Resource = new UpdResource()
                { UnitsConsumed = totalCapacity - remainingFuel, Value = remainingFuel };
        }
    }

    /**
     * Adjust an items durability/maxDurability value
     * @param item item (weapon/armor) to Adjust
     * @param itemDbDetails Weapon details from db
     * @param maxMultiplier Value to multiply max durability by
     * @param currentMultiplier Value to multiply current durability by
     */
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

    /**
     * Randomise the durabiltiy values for an armors plates and soft inserts
     * @param armorWithMods Armor item with its child mods
     * @param currentMultiplier Chosen multipler to use for current durability value
     * @param maxMultiplier Chosen multipler to use for max durability value
     */
    protected void RandomiseArmorDurabilityValues(
        List<Item> armorWithMods,
        double currentMultiplier,
        double maxMultiplier
    )
    {
        foreach (var armorItem in armorWithMods) {
            var itemDbDetails = itemHelper.GetItem(armorItem.Template).Value;
            if (itemDbDetails.Properties.ArmorClass > 1) {
                itemHelper.AddUpdObjectToItem(armorItem);

                var baseMaxDurability = itemDbDetails.Properties.MaxDurability;
                var lowestMaxDurability = randomUtil.GetDouble(maxMultiplier, 1) * baseMaxDurability;
                var chosenMaxDurability = Math.Round(randomUtil.GetDouble((double) lowestMaxDurability,(double) baseMaxDurability));

                var lowestCurrentDurability = randomUtil.GetDouble(currentMultiplier, 1) * chosenMaxDurability;
                var chosenCurrentDurability = Math.Round(randomUtil.GetDouble(lowestCurrentDurability, chosenMaxDurability));

                armorItem.Upd.Repairable = new UpdRepairable() {
                    Durability = chosenCurrentDurability == 0D ? 1D : chosenCurrentDurability, // Never var value become 0
                    MaxDurability = chosenMaxDurability
                };
            }
        }
    }

    /**
     * Add missing conditions to an item if needed
     * Durabiltiy for repairable items
     * HpResource for medical items
     * @param item item to add conditions to
     */
    protected void AddMissingConditions(Item item) {
        var props = itemHelper.GetItem(item.Template).Value.Properties;
        var isRepairable = props.Durability != null;
        var isMedkit = props.MaxHpResource != null;
        var isKey = props.MaximumNumberOfUsage != null;
        var isConsumable = props.MaxResource > 1 && props.FoodUseTime != null;
        var isRepairKit = props.MaxRepairResource != null;

        if (isRepairable && props.Durability > 0) {
            item.Upd.Repairable = new UpdRepairable()
                { Durability = props.Durability, MaxDurability = props.Durability };

            return;
        }

        if (isMedkit && props.MaxHpResource > 0)
        {
            item.Upd.MedKit = new UpdMedKit() { HpResource = props.MaxHpResource };

            return;
        }

        if (isKey) {
            item.Upd.Key = new UpdKey(){ NumberOfUsages = 0 };

            return;
        }

        // Food/drink
        if (isConsumable) {
            item.Upd.FoodDrink = new UpdFoodDrink() { HpPercent = props.MaxResource };

            return;
        }

        if (isRepairKit) {
            item.Upd.RepairKit = new UpdRepairKit() { Resource = props.MaxRepairResource };
        }
    }

    /**
     * Create a barter-based barter scheme, if not possible, fall back to making barter scheme currency based
     * @param offerItems Items for sale in offer
     * @param barterConfig Barter config from ragfairConfig.Dynamic.barter
     * @returns Barter scheme
     */
    protected List<BarterScheme> CreateBarterBarterScheme(List<Item> offerItems, BarterDetails barterConfig)
    {
        // Get flea price of item being sold
        var priceOfOfferItem = ragfairPriceService.GetDynamicOfferPriceForOffer(
            offerItems,
            Money.ROUBLES,
            false
        );

        // Dont make items under a designated rouble value into barter offers
        if (priceOfOfferItem < barterConfig.MinRoubleCostToBecomeBarter) {
            return CreateCurrencyBarterScheme(offerItems, false);
        }

        // Get a randomised number of barter items to list offer for
        var barterItemCount = randomUtil.GetInt(barterConfig.ItemCountMin, barterConfig.ItemCountMax);

        // Get desired cost of individual item offer will be listed for e.g. offer = 15k, item count = 3, desired item cost = 5k
        var desiredItemCostRouble = Math.Round(priceOfOfferItem / barterItemCount);

        // Rouble amount to go above/below when looking for an item (Wiggle cost of item a little)
        var offerCostVarianceRoubles = (desiredItemCostRouble * barterConfig.PriceRangeVariancePercent) / 100;

        // Dict of items and their flea price (cached on first use)
        var itemFleaPrices = GetFleaPricesAsArray();

        // Filter possible barters to items that match the price range + not itself
        var itemsInsidePriceBounds = itemFleaPrices.Where(
            itemAndPrice =>
                itemAndPrice.Price >= desiredItemCostRouble - offerCostVarianceRoubles &&
                itemAndPrice.Price <= desiredItemCostRouble + offerCostVarianceRoubles &&
                itemAndPrice.Tpl != offerItems[0].Template // Don't allow the item being sold to be chosen
        ).ToList();

        // No items on flea have a matching price, fall back to currency
        if (itemsInsidePriceBounds.Count == 0) {
            return CreateCurrencyBarterScheme(offerItems, false);
        }

        // Choose random item from price-filtered flea items
        var randomItem = randomUtil.GetArrayValue(itemsInsidePriceBounds);

        return [new BarterScheme() { Count = barterItemCount, Template = randomItem.Tpl }];
    }

    /**
     * Get an array of flea prices + item tpl, cached in generator class inside `allowedFleaPriceItemsForBarter`
     * @returns array with tpl/price values
     */
    protected List<TplWithFleaPrice> GetFleaPricesAsArray()
    {
        // Generate if needed
        if (allowedFleaPriceItemsForBarter == null) {
            var fleaPrices = databaseService.GetPrices();

            // Only get prices for items that also exist in items.json
            var filteredFleaItems = fleaPrices
                .Select(kvTpl => new TplWithFleaPrice { Tpl = kvTpl.Key, Price = kvTpl.Value })
                .Where(item => itemHelper.GetItem(item.Tpl).Key);

            var itemTypeBlacklist = ragfairConfig.Dynamic.Barter.ItemTypeBlacklist;
            allowedFleaPriceItemsForBarter = filteredFleaItems.Where(item => !itemHelper.IsOfBaseclasses(item.Tpl, itemTypeBlacklist)).ToList();
        }

        return allowedFleaPriceItemsForBarter;
    }

    /**
     * Create a random currency-based barter scheme for an array of items
     * @param offerWithChildren Items on offer
     * @param isPackOffer Is the barter scheme being created for a pack offer
     * @param multipler What to multiply the resulting price by
     * @returns Barter scheme for offer
     */
    protected List<BarterScheme> CreateCurrencyBarterScheme(
        List<Item> offerWithChildren,
        bool isPackOffer,
        double multipler = 1
    )
    {
        var currency = ragfairServerHelper.GetDynamicOfferCurrency();
        var price = ragfairPriceService.GetDynamicOfferPriceForOffer(offerWithChildren, currency, isPackOffer) * multipler;

        return [new BarterScheme() { Count = price, Template = currency }];
    }
}

