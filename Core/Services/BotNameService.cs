using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotNameService
{
    /// <summary>
    /// Clear out any entries in Name Set
    /// </summary>
    public void ClearNameCache() 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a unique bot nickname
    /// </summary>
    /// <param name="botJsonTemplate">bot JSON data from db</param>
    /// <param name="botGenerationDetails"></param>
    /// <param name="botRole">role of bot e.g. assault</param>
    /// <param name="uniqueRoles">Lowercase roles to always make unique</param>
    /// <param name="sessionId">OPTIONAL: profile session id</param>
    /// <returns>Nickname for bot</returns>
    public string GenerateUniqueBotNickname(
        BotType botJsonTemplate,
        BotGenerationDetails botGenerationDetails,
        string botRole,
        List<string> uniqueRoles = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add random PMC name to bots MainProfileNickname property
    /// </summary>
    /// <param name="bot">Bot to update</param>
    public void AddRandomPmcNameToBotMainProfileNicknameProperty(BotBase bot) 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a random PMC name from bear or usec bot jsons
    /// </summary>
    /// <returns>PMC name as string</returns>
    protected string GetRandomPMCName() 
    {
        throw new NotImplementedException();
    }
}
