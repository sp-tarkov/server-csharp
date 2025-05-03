using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class HealthController(
    ISptLogger<HealthController> _logger,
    EventOutputHolder _eventOutputHolder,
    ItemHelper _itemHelper,
    PaymentService _paymentService,
    InventoryHelper _inventoryHelper,
    LocalisationService _localisationService,
    HttpResponseUtil _httpResponseUtil,
    HealthHelper _healthHelper,
    ICloner _cloner)
{
    /// <summary>
    ///     When healing in menu
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Healing request</param>
    /// <param name="sessionID">Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidHeal(
        PmcData pmcData,
        OffraidHealRequestData request,
        string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        // Update medkit used (hpresource)
        var healingItemToUse = pmcData.Inventory.Items.FirstOrDefault(item =>
        {
            return item.Id == request.Item;
        });
        if (healingItemToUse is null)
        {
            var errorMessage = _localisationService.GetText("health-healing_item_not_found", request.Item);
            _logger.Error(errorMessage);

            return _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
        }

        // Ensure item has a upd object
        _itemHelper.AddUpdObjectToItem(healingItemToUse);

        if (healingItemToUse.Upd.MedKit is not null)
        {
            healingItemToUse.Upd.MedKit.HpResource -= request.Count;
        }
        else
        {
            // Get max healing from db
            var maxHp = _itemHelper.GetItem(healingItemToUse.Template).Value.Properties.MaxHpResource;
            healingItemToUse.Upd.MedKit = new UpdMedKit
            {
                HpResource = maxHp - request.Count
            }; // Subtract amout used from max
            // request.count appears to take into account healing effects removed, e.g. bleeds
            // Salewa heals limb for 20 and fixes light bleed = (20+45 = 65)
        }

        // Resource in medkit is spent, delete it
        if (healingItemToUse.Upd.MedKit.HpResource <= 0)
        {
            _inventoryHelper.RemoveItem(pmcData, request.Item, sessionID, output);
        }

        var healingItemDbDetails = _itemHelper.GetItem(healingItemToUse.Template);

        var healItemEffectDetails = healingItemDbDetails.Value.Properties.EffectsDamage;
        var bodyPartToHeal = pmcData.Health.BodyParts.GetValueOrDefault(request.Part);
        if (bodyPartToHeal is null)
        {
            _logger.Warning($"Player: {sessionID} Tried to heal a non-existent body part: {request.Part}");

            return output;
        }

        // Get initial heal amount
        var amountToHealLimb = request.Count;

        // Check if healing item removes negative effects
        var itemRemovesEffects = healingItemDbDetails.Value.Properties.EffectsDamage.Count > 0;
        if (itemRemovesEffects && bodyPartToHeal.Effects is not null)
        {
            // Can remove effects and limb has effects to remove
            foreach (var effectKvP in bodyPartToHeal.Effects)
            {
                // Check enum has effectType
                if (!Enum.TryParse<DamageEffectType>(effectKvP.Key, out var effect))
                // Enum doesnt contain this key
                {
                    continue;
                }

                // Check if healing item removes the effect on limb
                if (!healItemEffectDetails.TryGetValue(effect, out var matchingEffectFromHealingItem))
                // Healing item doesn't have matching effect, it doesn't remove the effect
                {
                    continue;
                }

                // Adjust limb heal amount based on if it's fixing an effect (request.count is TOTAL cost of hp resource on heal item, NOT amount to heal limb)
                amountToHealLimb -= (int) (matchingEffectFromHealingItem.Cost ?? 0);
                bodyPartToHeal.Effects.Remove(effectKvP.Key);
            }
        }

        // Adjust body part hp value
        bodyPartToHeal.Health.Current += amountToHealLimb;

        // Ensure we've not healed beyond the limbs max hp
        if (bodyPartToHeal.Health.Current > bodyPartToHeal.Health.Maximum)
        {
            bodyPartToHeal.Health.Current = bodyPartToHeal.Health.Maximum;
        }

        return output;
    }

    /// <summary>
    ///     Handle Eat event
    ///     Consume food/water outside of a raid
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Eat request</param>
    /// <param name="sessionID">Session id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidEat(
        PmcData pmcData,
        OffraidEatRequestData request,
        string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);
        var resourceLeft = 0d;

        var itemToConsume = pmcData.Inventory.Items.FirstOrDefault(item =>
        {
            return item.Id == request.Item;
        });
        if (itemToConsume is null)
        // Item not found, very bad
        {
            return _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("health-unable_to_find_item_to_consume", request.Item)
            );
        }

        var consumedItemMaxResource = _itemHelper.GetItem(itemToConsume.Template).Value.Properties.MaxResource;
        if (consumedItemMaxResource > 1)
        {
            // Ensure item has a upd object
            _itemHelper.AddUpdObjectToItem(itemToConsume);

            if (itemToConsume.Upd.FoodDrink is null)
            {
                itemToConsume.Upd.FoodDrink = new UpdFoodDrink
                {
                    HpPercent = consumedItemMaxResource - request.Count
                };
            }
            else
            {
                itemToConsume.Upd.FoodDrink.HpPercent -= request.Count;
            }

            resourceLeft = itemToConsume.Upd.FoodDrink.HpPercent.Value;
        }

        // Remove item from inventory if resource has dropped below threshold
        if (consumedItemMaxResource == 1 || resourceLeft < 1)
        {
            _inventoryHelper.RemoveItem(pmcData, request.Item, sessionID, output);
        }

        // Check what effect eating item has and handle
        var foodItemDbDetails = _itemHelper.GetItem(itemToConsume.Template).Value;
        var foodItemEffectDetails = foodItemDbDetails.Properties.EffectsHealth;
        var foodIsSingleUse = foodItemDbDetails.Properties.MaxResource == 1;

        foreach (var (key, effectProps) in foodItemEffectDetails)
        {
            switch (key)
            {
                case HealthFactor.Hydration:
                    ApplyEdibleEffect(pmcData.Health.Hydration, effectProps, foodIsSingleUse, request);
                    break;
                case HealthFactor.Energy:
                    ApplyEdibleEffect(pmcData.Health.Energy, effectProps, foodIsSingleUse, request);
                    break;

                default:
                    _logger.Warning($"Unhandled effect after consuming: {itemToConsume.Template}, {key}");
                    break;
            }
        }

        return output;
    }

    /// <summary>
    ///     Apply effects to profile from consumable used
    /// </summary>
    /// <param name="bodyValue">Hydration/Energy</param>
    /// <param name="consumptionDetails">Properties of consumed item</param>
    /// <param name="foodIsSingleUse">Single use item</param>
    /// <param name="request">Client request</param>
    protected void ApplyEdibleEffect(CurrentMinMax bodyValue, EffectsHealthProps consumptionDetails, bool foodIsSingleUse,
        OffraidEatRequestData request)
    {
        if (foodIsSingleUse)
        // Apply whole value from passed in parameter
        {
            bodyValue.Current += consumptionDetails.Value;
        }
        else
        {
            bodyValue.Current += request.Count;
        }

        // Ensure current never goes over max
        if (bodyValue.Current > bodyValue.Maximum)
        {
            bodyValue.Current = bodyValue.Maximum;

            return;
        }

        // Same as above but for the lower bound
        if (bodyValue.Current < 0)
        {
            bodyValue.Current = 0;
        }
    }

    /// <summary>
    ///     Handle RestoreHealth event
    ///     Occurs on post-raid healing page
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <param name="healthTreatmentRequest">Request data from client</param>
    /// <param name="sessionID">Session id</param>
    /// <returns></returns>
    public ItemEventRouterResponse HealthTreatment(
        PmcData pmcData,
        HealthTreatmentRequestData healthTreatmentRequest,
        string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);
        var payMoneyRequest = new ProcessBuyTradeRequestData
        {
            Action = healthTreatmentRequest.Action,
            TransactionId = Traders.THERAPIST,
            SchemeItems = healthTreatmentRequest.Items,
            Type = "",
            ItemId = "",
            Count = 0,
            SchemeId = 0
        };

        _paymentService.PayMoney(pmcData, payMoneyRequest, sessionID, output);
        if (output.Warnings.Count > 0)
        {
            return output;
        }

        foreach (var bodyPartKvP in healthTreatmentRequest.Difference.BodyParts.GetAllPropsAsDict())
        {
            // Get body part from request + from pmc profile
            var partRequest = (BodyPartEffects) bodyPartKvP.Value;
            var profilePart = pmcData.Health.BodyParts[bodyPartKvP.Key];

            // Bodypart healing is chosen when part request hp is above 0
            if (partRequest.Health > 0)
            // Heal bodypart
            {
                profilePart.Health.Current = profilePart.Health.Maximum;
            }

            // Check for effects to remove
            if (partRequest.Effects?.Count > 0)
            {
                // Found some, loop over them and remove from pmc profile
                foreach (var effect in partRequest.Effects)
                {
                    pmcData.Health.BodyParts[bodyPartKvP.Key].Effects.Remove(effect);
                }

                // Remove empty effect object
                if (pmcData.Health.BodyParts[bodyPartKvP.Key].Effects.Count == 0)
                {
                    pmcData.Health.BodyParts[bodyPartKvP.Key].Effects = null;
                }
            }
        }

        // Inform client of new post-raid, post-therapist heal values
        output.ProfileChanges[sessionID].Health = _cloner.Clone(pmcData.Health);

        return output;
    }

    /// <summary>
    ///     applies skills from hideout workout.
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Request data</param>
    /// <param name="sessionId">session id</param>
    public void ApplyWorkoutChanges(
        PmcData? pmcData,
        WorkoutData request,
        string sessionId)
    {
        pmcData.Skills.Common = request.Skills.Common;
    }
}
