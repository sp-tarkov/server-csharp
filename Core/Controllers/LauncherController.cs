using Core.Models.Eft.Launcher;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Mod;

namespace Core.Controllers;

public class LauncherController
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ConnectResponse Connect()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get descriptive text for each of the profile edtions a player can choose, keyed by profile.json profile type
    /// e.g. "Edge Of Darkness"
    /// </summary>
    /// <returns>Dictionary of profile types with related descriptive text</returns>
    private Dictionary<string, string> GetProfileDescriptions()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Info Find(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string Login(LoginRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string Register(RegisterData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private string CreateAccount(RegisterData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string GenerateProfileId()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <param name="counter"></param>
    /// <returns></returns>
    private string FormatId(
        long timeStamp,
        int counter)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string ChangeUsername(ChangeRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string ChangePassword(ChangeRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle launcher requesting profile be wiped
    /// </summary>
    /// <param name="info">RegisterData</param>
    /// <returns>Session id</returns>
    public string Wipe(RegisterData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetCompatibleTarkovVersion()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the mods the server has currently loaded
    /// </summary>
    /// <returns>Dictionary of mod name and mod details</returns>
    public Dictionary<string, PackageJsonData> GetLoadedServerMods() // TODO: We no longer use a package.json
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the mods a profile has ever loaded into game with
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <returns>List of mod details</returns>
    public List<ModDetails> GetServerModsProfileUsed(string sessionId)
    {
        throw new NotImplementedException();
    }
}