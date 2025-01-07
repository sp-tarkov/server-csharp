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

    public GetBodyResponseData<SettingsBase> GetSettings(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<Globals> GetGlobals(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetTemplateItems(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<HandbookBase> GetTemplateHandbook(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<Dictionary<string, CustomizationItem>> GetTemplateSuits(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<string>> GetTemplateCharacter(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

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

    public GetBodyResponseData<Dictionary<string, string>> GetLocalesLanguages(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<string> GetLocalesMenu(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetLocalesGlobal(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetQteList(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetItemPricesResponse> GetItemPrices(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}