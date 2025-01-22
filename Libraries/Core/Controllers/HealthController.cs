using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Health;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Common.Tables;
using Core.Helpers;
using Core.Models.Utils;
using Core.Routers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Controllers;

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
    ICloner cloner)
{
    /// <summary>
    /// When healing in menu
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Healing request</param>
    /// <param name="sessionId">Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidHeal(
        PmcData pmcData,
        OffraidHealRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Eat event
    /// Consume food/water outside of a raid
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Eat request</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidEat(
        PmcData pmcData,
        OffraidEatRequestData request,
        string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);
        var resourceLeft = 0d;

        var itemToConsume = pmcData.Inventory.Items.FirstOrDefault((item) => item.Id == request.Item);
        if (itemToConsume is null)
        {
            // Item not found, very bad
            return _httpResponseUtil.AppendErrorToOutput(output, _localisationService.GetText("health-unable_to_find_item_to_consume", request.Item));
        }

        var consumedItemMaxResource = _itemHelper.GetItem(itemToConsume.Template).Value.Properties.MaxResource;
        if (consumedItemMaxResource > 1)
        {
            // Ensure item has a upd object
            _itemHelper.AddUpdObjectToItem(itemToConsume);

            if (itemToConsume.Upd.FoodDrink is null)
            {
                itemToConsume.Upd.FoodDrink = new UpdFoodDrink{ HpPercent = consumedItemMaxResource - request.Count };
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

        foreach (var (key, effectProps) in foodItemEffectDetails) {
            switch (key)
            {
                case "Hydration":
                    ApplyEdibleEffect(pmcData.Health.Hydration, effectProps, foodIsSingleUse, request);
                    break;
                case "Energy":
                    ApplyEdibleEffect(pmcData.Health.Energy, effectProps, foodIsSingleUse, request);
                    break;

                default:
                    _logger.Warning($"Unhandled effect after consuming: ${ itemToConsume.Template}, ${key}");
                    break;
            }
        }
        return output;
    }

    protected void ApplyEdibleEffect(CurrentMax bodyValue, EffectsHealthProps consumptionDetails, bool foodIsSingleUse, OffraidEatRequestData request)
    {
        if (foodIsSingleUse)
        {
            // Apply whole value from passed in parameter
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
    /// Handle RestoreHealth event
    /// Occurs on post-raid healing page
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <param name="request">Request data from client</param>
    /// <param name="sessionId">Session id</param>
    /// <returns></returns>
    public ItemEventRouterResponse HealthTreatment(
        PmcData pmcData,
        HealthTreatmentRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// applies skills from hideout workout.
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="info">Request data</param>
    /// <param name="sessionId">session id</param>
    public void ApplyWorkoutChanges(
        PmcData? pmcData,
        WorkoutData info,
        string sessionId)
    {
        throw new NotImplementedException();
    }
}
