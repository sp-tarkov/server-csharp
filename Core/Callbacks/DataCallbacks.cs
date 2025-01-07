using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Game;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.HttpResponse;
using Core.Models.Spt.Server;

namespace Core.Callbacks;

public class DataCallbacks
{
    public DataCallbacks()
    {
        
    }

    /// <summary>
    /// Handle client/settings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<SettingsBase> GetSettings(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/globals
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<Globals> GetGlobals(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/items
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetTemplateItems(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/handbook/templates
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<HandbookBase> GetTemplateHandbook(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/customization
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<Dictionary<string, CustomizationItem>> GetTemplateSuits(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/account/customization
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<string>> GetTemplateCharacter(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/hideout/settings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<HideoutSettingsBase> GetHideoutSettings(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<HideoutArea>> GetHideoutAreas(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<HideoutProductionData> GetHideoutProduction(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/languages
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<Dictionary<string, string>> GetLocalesLanguages(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/menu/locale
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<string> GetLocalesMenu(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/locale
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetLocalesGlobal(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/hideout/qte/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetQteList(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/items/prices/
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetItemPricesResponse> GetItemPrices(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}