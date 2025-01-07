using Core.Models.Eft.Common;
using Core.Models.Eft.Launcher;

namespace Core.Callbacks;

public class LauncherCallbacks
{
    public LauncherCallbacks()
    {
        
    }
    
    public string Connect()
    {
        throw new NotImplementedException();
    }

    public string Login(string url, LoginRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string Register(string url, RegisterData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string Get(string url, LoginRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string ChangeUsername(string url, ChangeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string ChangePassword(string url, ChangeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string Wipe(string url, RegisterData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetServerVersion()
    {
        throw new NotImplementedException();
    }

    public string Ping(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string RemoveProfile(string url, RemoveProfileData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetCompatibleTarkovVersion()
    {
        throw new NotImplementedException();
    }

    public string GetLoadedServerMods()
    {
        throw new NotImplementedException();
    }

    public string GetServerModsProfileUsed(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}