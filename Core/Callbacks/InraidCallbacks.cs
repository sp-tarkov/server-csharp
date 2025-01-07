using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.InRaid;

namespace Core.Callbacks;

public class InraidCallbacks
{
    public InraidCallbacks()
    {
        
    }

    public NullResponseData RegisterPlayer(string url, RegisterPlayerRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData SaveProgress(string url, ScavSaveRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetRaidMenuSettings()
    {
        throw new NotImplementedException();
    }

    public string GetTraitorScavHostileChance(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetBossConvertSettings(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}