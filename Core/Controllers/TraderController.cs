using Core.Models.Eft.Common.Tables;

namespace Core.Controllers;

public class TraderController
{
	/// <summary>
	/// Runs when onLoad event is fired
	/// Iterate over traders, ensure a pristine copy of their assorts is stored in traderAssortService
	/// Store timestamp of next assort refresh in nextResupply property of traders .base object
	/// </summary>
	public void Load()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Runs when onUpdate is fired
	/// If current time is > nextResupply(expire) time of trader, refresh traders assorts and
	/// Fence is handled slightly differently
	/// </summary>
	/// <returns></returns>
	public bool Update()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Handle client/trading/api/traderSettings
	/// </summary>
	/// <param name="sessionId">session id</param>
	/// <returns>Return a list of all traders</returns>
	public List<TraderBase> GetAllTraders(string sessionId)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Order traders by their traderId (Ttid)
	/// </summary>
	/// <param name="traderA">First trader to compare</param>
	/// <param name="traderB">Second trader to compare</param>
	/// <returns>1,-1 or 0</returns>
	private int SortByTraderId(
		TraderBase traderA, 
		TraderBase traderB)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Handle client/trading/api/getTrader
	/// </summary>
	/// <param name="sessionId"></param>
	/// <param name="traderId"></param>
	/// <returns></returns>
	public TraderBase GetTrader(
		string sessionId,
		string traderId)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Handle client/trading/api/getTraderAssort
	/// </summary>
	/// <param name="sessionId"></param>
	/// <param name="traderId"></param>
	/// <returns></returns>
	public TraderAssort GetAssort(
		string sessionId,
		string traderId)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Handle client/items/prices/TRADERID
	/// </summary>
	/// <returns></returns>
	public GetItemPricesResponse GetItemPrices()
	{
		throw new NotImplementedException();
	}
}