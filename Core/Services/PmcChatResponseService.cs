using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class PmcChatResponseService
{
    /**
     * For each PMC victim of the player, have a chance to send a message to the player, can be positive or negative
     * @param sessionId Session id
     * @param pmcVictims List of bots killed by player
     * @param pmcData Player profile
     */
    public void SendVictimResponse(string sessionId, List<Victim> pmcVictims, PmcData pmcData)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Not fully implemented yet, needs method of acquiring killers details after raid
     * @param sessionId Session id
     * @param pmcData Players profile
     * @param killer The bot who killed the player
     */
    public void SendKillerResponse(string sessionId, PmcData pmcData, Aggressor killer)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Choose a localised message to send the player (different if sender was killed or killed player)
     * @param isVictim Is the message coming from a bot killed by the player
     * @param pmcData Player profile
     * @param victimData OPTIMAL - details of the pmc killed
     * @returns Message from PMC to player
     */
    protected string? ChooseMessage(bool isVictim, PmcData pmcData, Victim? victimData = null)
    {
        throw new System.NotImplementedException();
    }

    /**
     * use map key to get a localised location name
     * e.g. factory4_day becomes "Factory"
     * @param locationKey location key to localise
     * @returns Localised location name
     */
    protected string GetLocationName(string locationKey)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Should capitalisation be stripped from the message response before sending
     * @param isVictim Was responder a victim of player
     * @returns true = should be stripped
     */
    protected bool StripCapitalisation(bool isVictim)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Should capitalisation be stripped from the message response before sending
     * @param isVictim Was responder a victim of player
     * @returns true = should be stripped
     */
    protected bool AllCaps(bool isVictim)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Should a suffix be appended to the end of the message being sent to player
     * @param isVictim Was responder a victim of player
     * @returns true = should be stripped
     */
    protected bool AppendSuffixToMessageEnd(bool isVictim)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Choose a type of response based on the weightings in pmc response config
     * @param isVictim Was responder killed by player
     * @returns Response type (positive/negative)
     */
    protected string ChooseResponseType(bool isVictim = true)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Get locale keys related to the type of response to send (victim/killer)
     * @param keyType Positive/negative
     * @param isVictim Was responder killed by player
     * @returns
     */
    protected List<string> GetResponseLocaleKeys(string keyType, bool isVictim = true)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Get all locale keys that start with `pmcresponse-suffix`
     * @returns list of keys
     */
    protected List<string> GetResponseSuffixLocaleKeys()
    {
        throw new System.NotImplementedException();
    }

    /**
     * Randomly draw a victim of the list and return their details
     * @param pmcVictims Possible victims to choose from
     * @returns IUserDialogInfo
     */
    protected UserDialogInfo ChooseRandomVictim(List<Victim> pmcVictims)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Convert a victim object into a IUserDialogInfo object
     * @param pmcVictim victim to convert
     * @returns IUserDialogInfo
     */
    protected UserDialogInfo GetVictimDetails(Victim pmcVictim)
    {
        throw new System.NotImplementedException();
    }
}
