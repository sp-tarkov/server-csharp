using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;

namespace Core.Controllers;

public class WishlistController
{
	/// <summary>
	/// Handle AddToWishList
	/// </summary>
	/// <param name="pmcData"></param>
	/// <param name="request"></param>
	/// <param name="sessionId"></param>
	/// <returns></returns>
	public ItemEventRouterResponse AddToWishList(
		PmcData pmcData,
		AddItemToWishlistRequest request,
		string sessionId)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Handle RemoveFromWishList event
	/// </summary>
	/// <param name="pmcData"></param>
	/// <param name="request"></param>
	/// <param name="sessionId"></param>
	/// <returns></returns>
	public ItemEventRouterResponse RemoveFromWishList(
		PmcData pmcData,
		RemoveFromWishlistRequest request,
		string sessionId)
	{
		throw new NotImplementedException();
	}
	
	/// <summary>
	/// Handle changeWishlistItemCategory event
	/// </summary>
	/// <param name="pmcData"></param>
	/// <param name="request"></param>
	/// <param name="sessionId"></param>
	/// <returns></returns>
	public ItemEventRouterResponse ChangeWishListItemCategory(
		PmcData pmcData,
		ChangeWishlistItemCategoryRequest request,
		string sessionId)
	{
		throw new NotImplementedException();
	}
}