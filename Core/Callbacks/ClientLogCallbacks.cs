using Core.Models.Eft.HttpResponse;
using Core.Models.Spt.Logging;

namespace Core.Callbacks;

public class ClientLogCallbacks
{
    public ClientLogCallbacks()
    {
        
    }

    /// <summary>
    /// Handle /singleplayer/log
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData ClientLog(string url, ClientLogRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle /singleplayer/release
    /// </summary>
    /// <returns></returns>
    public string ReleaseNotes()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle /singleplayer/enableBSGlogging
    /// </summary>
    /// <returns></returns>
    public string BsgLogging()
    {
        throw new NotImplementedException();
    }
}