using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;

namespace Core.Controllers;

public class RepairController
{
	/// <summary>
	/// Handle TraderRepair event
	/// Repair with trader
	/// </summary>
	/// <param name="sessionId">session id</param>
	/// <param name="body">endpoint request data</param>
	/// <param name="pmcData">player profile</param>
	/// <returns>item event router action</returns>
	public ItemEventRouterResponse TraderRepair(
		string sessionId,
		TraderRepairActionDataRequest body,
		PmcData pmcData)
	{
		throw new NotImplementedException();
	}
	
	/// <summary>
	/// Handle Repair event
	/// Repair with repair kit
	/// </summary>
	/// <param name="sessionId">session id</param>
	/// <param name="body">endpoint request data</param>
	/// <param name="pmcData">player profile</param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public ItemEventRouterResponse RepairWithKit(
		string sessionId,
		RepairActionDataRequest body,
		PmcData pmcData)
	{
		throw new NotImplementedException();
	}
}