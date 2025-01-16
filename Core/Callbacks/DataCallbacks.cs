using Core.Annotations;
using Core.Controllers;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Game;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.HttpResponse;
using Core.Models.Spt.Server;
using Core.Services;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(DataCallbacks))]
public class DataCallbacks
{
    protected HttpResponseUtil _httpResponseUtil;
    protected TimeUtil _timeUtil;
    protected TraderHelper _traderHelper;
    protected DatabaseService _databaseService;
    protected TraderController _traderController;
    protected HideoutController _hideoutController;
    
    public DataCallbacks
    (
        HttpResponseUtil httpResponseUtil,
        TimeUtil timeUtil,
        TraderHelper traderHelper,
        DatabaseService databaseService,
        TraderController traderController,
        HideoutController hideoutController
    )
    {
        _httpResponseUtil = httpResponseUtil;
        _timeUtil = timeUtil;
        _traderHelper = traderHelper;
        _databaseService = databaseService;
        _traderController = traderController;
        _hideoutController = hideoutController;
    }

    /// <summary>
    /// Handle client/settings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetSettings(string url, EmptyRequestData info, string sessionID)
    {
        var returns = _httpResponseUtil.GetBody(_databaseService.GetSettings());
        return returns;
    }

    /// <summary>
    /// Handle client/globals
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetGlobals(string url, EmptyRequestData info, string sessionID)
    {
        var globals = _databaseService.GetGlobals();
        globals.Time = _timeUtil.GetTimeStamp() / 1000;
        var returns = _httpResponseUtil.GetBody(globals);

        return returns;
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
        return _httpResponseUtil.GetUnclearedBody(_databaseService.GetItems());
    }

    /// <summary>
    /// Handle client/handbook/templates
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetTemplateHandbook(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHandbook());
    }

    /// <summary>
    /// Handle client/customization
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetTemplateSuits(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetTemplates().Customization);
    }

    /// <summary>
    /// Handle client/account/customization
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetTemplateCharacter(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetTemplates().Character);
    }

    /// <summary>
    /// Handle client/hideout/settings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetHideoutSettings(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHideout().Settings);
    }

    /// <summary>
    /// Handle client/hideout/areas
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetHideoutAreas(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHideout().Areas);
    }

    /// <summary>
    /// Handle client/hideout/production/recipes
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetHideoutProduction(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHideout().Production);
    }

    /// <summary>
    /// Handle client/languages
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetLocalesLanguages(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetLocales().Languages);
    }

    /// <summary>
    /// Handle client/menu/locale
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetLocalesMenu(string url, EmptyRequestData info, string sessionID)
    {
        var localeId = url.Replace("/client/menu/locale/", "");
        var locales = _databaseService.GetLocales();
        var result = locales.Menu[localeId];

        if (result == null)
            result = locales.Menu.FirstOrDefault(m => m.Key == "en").Value;

        if (result == null)
            throw new Exception($"Unable to determine locale for request with {localeId}");
        
        return _httpResponseUtil.GetBody(result);
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
        var localeId = url.Replace("/client/locale/", "");
        var locales = _databaseService.GetLocales();
        var result = locales.Global[localeId];
        
        if (result == null)
            result = locales.Global.FirstOrDefault(m => m.Key == "en").Value;

        return _httpResponseUtil.GetUnclearedBody(result);
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
         return _httpResponseUtil.GetUnclearedBody(_hideoutController.GetQteList(sessionID));
    }

    /// <summary>
    /// Handle client/items/prices/
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetItemPrices(string url, EmptyRequestData info, string sessionID)
    {
        var traderId = url.Replace("/client/items/prices/", "");
        
        return _httpResponseUtil.GetBody(_traderController.GetItemPrices(sessionID, traderId));
    }
}
