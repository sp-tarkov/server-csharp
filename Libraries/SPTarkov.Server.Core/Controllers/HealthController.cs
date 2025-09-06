using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
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
    ISptLogger<HealthController> logger,
    EventOutputHolder eventOutputHolder,
    ItemHelper itemHelper,
    PaymentService paymentService,
    InventoryHelper inventoryHelper,
    ServerLocalisationService serverLocalisationService,
    HttpResponseUtil httpResponseUtil,
    ICloner cloner
)
{
    /// <summary>
    ///     When healing in menu
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Healing request</param>
    /// <param name="sessionID">Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidHeal(PmcData pmcData, OffraidHealRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        // Update medkit used (hpresource)
        var healingItemToUse = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == request.Item);
        if (healingItemToUse is null)
        {
            var errorMessage = serverLocalisationService.GetText("health-healing_item_not_found", request.Item);
            logger.Error(errorMessage);

            return httpResponseUtil.AppendErrorToOutput(output, errorMessage);
        }

        // Ensure item has an upd object
        healingItemToUse.AddUpd();

        if (healingItemToUse.Upd.MedKit is not null)
        {
            healingItemToUse.Upd.MedKit.HpResource -= request.Count;
        }
        else
        {
            // Get max healing from db
            var maxHp = itemHelper.GetItem(healingItemToUse.Template).Value.Properties.MaxHpResource;
            healingItemToUse.Upd.MedKit = new UpdMedKit { HpResource = maxHp - request.Count }; // Subtract amout used from max
            // request.count appears to take into account healing effects removed, e.g. bleeds
            // Salewa heals limb for 20 and fixes light bleed = (20+45 = 65)
        }

        // Resource in medkit is spent, delete it
        if (healingItemToUse.Upd.MedKit.HpResource <= 0)
        {
            inventoryHelper.RemoveItem(pmcData, request.Item, sessionID, output);
        }

        var healingItemDbDetails = itemHelper.GetItem(healingItemToUse.Template);

        var healItemEffectDetails = healingItemDbDetails.Value.Properties.EffectsDamage;
        var bodyPartToHeal = pmcData.Health.BodyParts.GetValueOrDefault(request.Part);
        if (bodyPartToHeal is null)
        {
            logger.Warning($"Player: {sessionID} Tried to heal a non-existent body part: {request.Part}");

            return output;
        }

        // Get initial heal amount
        var amountToHealLimb = request.Count;

        // Check if healing item removes negative effects
        var itemRemovesEffects = healingItemDbDetails.Value.Properties.EffectsDamage.Count > 0;
        if (itemRemovesEffects && bodyPartToHeal.Effects is not null)
        {
            // Can remove effects and limb has effects to remove
            foreach (var (effectId, _) in bodyPartToHeal.Effects)
            {
                // Check enum has effectType
                if (!Enum.TryParse<DamageEffectType>(effectId, out var effect))
                // Enum doesn't contain this key
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
                amountToHealLimb -= (int)(matchingEffectFromHealingItem.Cost ?? 0);
                bodyPartToHeal.Effects.Remove(effectId);
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
    ///     Consume food/water outside a raid
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Eat request</param>
    /// <param name="sessionID">Session id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidEat(PmcData pmcData, OffraidEatRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);
        var resourceLeft = 0d;

        var itemToConsume = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == request.Item);
        if (itemToConsume is null)
        {
            // Item not found, very bad
            return httpResponseUtil.AppendErrorToOutput(
                output,
                serverLocalisationService.GetText("health-unable_to_find_item_to_consume", request.Item)
            );
        }

        var foodItemDbDetails = itemHelper.GetItem(itemToConsume.Template).Value;
        var consumedItemMaxResource = foodItemDbDetails.Properties.MaxResource;
        if (consumedItemMaxResource > 1)
        {
            // Ensure item has an upd object
            itemToConsume.AddUpd();

            if (itemToConsume.Upd.FoodDrink is null)
            {
                itemToConsume.Upd.FoodDrink = new UpdFoodDrink { HpPercent = consumedItemMaxResource - request.Count };
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
            inventoryHelper.RemoveItem(pmcData, request.Item, sessionID, output);
        }

        // Check what effect eating item has and handle

        var foodItemEffectDetails = foodItemDbDetails.Properties.EffectsHealth;
        var foodIsSingleUse = foodItemDbDetails.Properties.MaxResource == 1;

        foreach (var (key, effectProperties) in foodItemEffectDetails)
        {
            switch (key)
            {
                case HealthFactor.Hydration:
                    ApplyEdibleEffect(pmcData.Health.Hydration, effectProperties, foodIsSingleUse, request);
                    break;
                case HealthFactor.Energy:
                    ApplyEdibleEffect(pmcData.Health.Energy, effectProperties, foodIsSingleUse, request);
                    break;

                default:
                    logger.Warning($"Unhandled effect after consuming: {itemToConsume.Template}, {key}");
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
    protected void ApplyEdibleEffect(
        CurrentMinMax bodyValue,
        EffectsHealthProperties consumptionDetails,
        bool foodIsSingleUse,
        OffraidEatRequestData request
    )
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
    public ItemEventRouterResponse HealthTreatment(PmcData pmcData, HealthTreatmentRequestData healthTreatmentRequest, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);
        var payMoneyRequest = new ProcessBuyTradeRequestData
        {
            Action = healthTreatmentRequest.Action,
            TransactionId = Traders.THERAPIST,
            SchemeItems = healthTreatmentRequest.Items,
            Type = string.Empty,
            ItemId = MongoId.Empty(),
            Count = 0,
            SchemeId = 0,
        };

        paymentService.PayMoney(pmcData, payMoneyRequest, sessionID, output);
        if (output.Warnings.Count > 0)
        {
            return output;
        }

        foreach (var (key, partValues) in healthTreatmentRequest.Difference?.BodyParts)
        {
            // Get body part from request + from pmc profile
            if (!pmcData.Health.BodyParts.TryGetValue(key, out var profilePart))
            {
                // Profile somehow doesn't have part therapist health, skip
                continue;
            }

            // Update hp value when health value is above 0, indicating healing was performed
            if (partValues.Health > 0)
            {
                profilePart.Health.Current = profilePart.Health.Maximum;
            }

            // Check for effects to remove
            if (partValues.Effects?.Count > 0)
            {
                // Found effects that have been healed by therapist
                // key e.g. "LightBleeding"
                foreach (var effectKey in partValues.Effects)
                {
                    profilePart.Effects.Remove(effectKey);
                }

                // Remove empty effect object to match what live data shows
                if (profilePart.Effects.Count == 0)
                {
                    profilePart.Effects = null;
                }
            }
        }

        // Inform client of new post-raid, post-therapist heal values
        output.ProfileChanges[sessionID].Health = cloner.Clone(pmcData.Health);

        return output;
    }

    /// <summary>
    ///     applies skills from hideout workout.
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Request data</param>
    /// <param name="sessionId">session id</param>
    public void ApplyWorkoutChanges(PmcData? pmcData, WorkoutData request, MongoId sessionId)
    {
        pmcData.Skills.Common = request.Skills.Common;
    }
}
