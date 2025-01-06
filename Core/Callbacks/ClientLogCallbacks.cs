using Core.Models.Eft.HttpResponse;
using Core.Models.Spt.Logging;

namespace Core.Callbacks;

public class ClientLogCallbacks
{
    public ClientLogCallbacks()
    {
        
    }

    public NullResponseData ClientLog(string url, ClientLogRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string ReleaseNotes()
    {
        throw new NotImplementedException();
    }

    public string BsgLogging()
    {
        throw new NotImplementedException();
    }
}