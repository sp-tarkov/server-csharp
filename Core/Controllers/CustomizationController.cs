using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;

namespace Core.Controllers;

[Injectable]
public class CustomizationController
{
    /// <summary>
    /// Get purchasable clothing items from trader that match players side (usec/bear)
    /// </summary>
    /// <param name="traderId">trader to look up clothing for</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>Suit array</returns>
    public Suit GetTraderSuits(string traderId, string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle CustomizationBuy event
    /// Purchase/unlock a clothing item from a trader
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="buyClothingRequest">Request object</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse BuyClothing(
        PmcData pmcData,
        BuyClothingRequestData buyClothingRequest,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update output object and player profile with purchase details
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemsToPayForClothingWith">Clothing purchased</param>
    /// <param name="output">Client response</param>
    private void PayForClothingItems(
        string sessionId,
        PmcData pmcData,
        List<PaymentItemForClothing> itemsToPayForClothingWith,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update output object and player profile with purchase details for single piece of clothing
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="paymentItemDetails">Payment details</param>
    /// <param name="output">Client response</param>
    private void PayForClothingItem(
        string sessionId,
        PmcData pmcData,
        PaymentItemForClothing paymentItemDetails,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private List<Suit> GetAllTraderSuits(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/hideout/customization/offer/list
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public HideoutCustomisation GetHideoutCustomisation(
        string sessionId,
        EmptyRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/customization/storage
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public List<CustomisationStorage> GetCustomisationStorage(
        string sessionId,
        EmptyRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    private string GetGameEdition(SptProfile profile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle CustomizationSet event
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <param name="pmcData"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SetClothing(
        string sessionId,
        CustomizationSetRequest request,
        PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Applies a purchased suit to the players doll
    /// </summary>
    /// <param name="customization">Suit to apply to profile</param>
    /// <param name="pmcData">Profile to update</param>
    private void ApplyClothingItemToProfile(
        CustomizationSetOption customization,
        PmcData pmcData)
    {
        throw new NotImplementedException();
    }
}
