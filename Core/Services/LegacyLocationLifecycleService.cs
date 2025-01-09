using System.Text.Json.Serialization;
using Core.Models.Eft.Common;
using Core.Models.Eft.Match;

namespace Core.Services;

public class LegacyLocationLifecycleService
{
    /// <summary>
    /// Handle client/match/offline/end
    /// </summary>
    public void endOfflineRaid(EndOfflineRaidRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle when a player extracts using a car - Add rep to fence
    /// </summary>
    /// <param name="extractName">name of the extract used</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="sessionId">Session id</param>
    protected void handleCarExtract(string extractName, PmcData pmcData, string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the fence rep gain from using a car or coop extract
    /// </summary>
    /// <param name="pmcData">Profile</param>
    /// <param name="baseGain">amount gained for the first extract</param>
    /// <param name="extractCount">Number of times extract was taken</param>
    /// <returns>Fence standing after taking extract</returns>
    protected int getFenceStandingAfterExtract(PmcData pmcData, int baseGain, int extractCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Was extract by car
    /// </summary>
    /// <param name="extractName">name of extract</param>
    /// <returns>true if car extract</returns>
    protected bool extractWasViaCar(string extractName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Did player take a COOP extract
    /// </summary>
    /// <param name="extractName">Name of extract player took</param>
    /// <returns>True if coop extract</returns>
    protected bool extractWasViaCoop(string extractName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle when a player extracts using a coop extract - add rep to fence
    /// </summary>
    /// <param name="sessionId">Session/player id</param>
    /// <param name="pmcData">Profile</param>
    /// <param name="extractName">Name of extract taken</param>
    protected void handleCoopExtract(string sessionId, PmcData pmcData, string extractName)
    {
        throw new NotImplementedException();
    }

    protected void sendCoopTakenFenceMessage(string sessionId)
    {
        throw new NotImplementedException();
    }
}
