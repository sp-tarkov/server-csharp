using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class DataCallbacks(
    HttpResponseUtil _httpResponseUtil,
    DatabaseService _databaseService,
    TraderController _traderController,
    HideoutController _hideoutController,
    LocaleService _localeService
)
{
    /// <summary>
    ///     Handle client/settings
    /// </summary>
    /// <returns></returns>
    public string GetSettings(string url, EmptyRequestData _, string sessionID)
    {
        var returns = _httpResponseUtil.GetBody(_databaseService.GetSettings());
        return returns;
    }

    /// <summary>
    ///     Handle client/globals
    /// </summary>
    /// <returns></returns>
    public string GetGlobals(string url, EmptyRequestData _, string sessionID)
    {
        var globals = _databaseService.GetGlobals();
        var returns = _httpResponseUtil.GetBody(globals);

        return returns;
    }

    /// <summary>
    ///     Handle client/items
    /// </summary>
    /// <returns></returns>
    public string GetTemplateItems(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetUnclearedBody(_databaseService.GetItems());
    }

    /// <summary>
    ///     Handle client/handbook/templates
    /// </summary>
    /// <returns></returns>
    public string GetTemplateHandbook(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHandbook());
    }

    /// <summary>
    ///     Handle client/customization
    /// </summary>
    /// <returns></returns>
    public string GetTemplateSuits(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetTemplates().Customization);
    }

    /// <summary>
    ///     Handle client/account/customization
    /// </summary>
    /// <returns></returns>
    public string GetTemplateCharacter(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetTemplates().Character);
    }

    /// <summary>
    ///     Handle client/hideout/settings
    /// </summary>
    /// <returns></returns>
    public string GetHideoutSettings(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHideout().Settings);
    }

    /// <summary>
    ///     Handle client/hideout/areas
    /// </summary>
    /// <returns></returns>
    public string GetHideoutAreas(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHideout().Areas);
    }

    /// <summary>
    ///     Handle client/hideout/production/recipes
    /// </summary>
    /// <returns></returns>
    public string GetHideoutProduction(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetHideout().Production);
    }

    /// <summary>
    ///     Handle client/languages
    /// </summary>
    /// <returns></returns>
    public string GetLocalesLanguages(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetLocales().Languages);
    }

    /// <summary>
    ///     Handle client/menu/locale
    /// </summary>
    /// <returns></returns>
    public string GetLocalesMenu(string url, EmptyRequestData _, string sessionID)
    {
        var localeId = url.Replace("/client/menu/locale/", "");
        var locales = _databaseService.GetLocales();
        var result = locales.Menu?[localeId] ?? locales.Menu?.FirstOrDefault(m => m.Key == "en").Value;

        if (result == null)
        {
            throw new Exception($"Unable to determine locale for request with {localeId}");
        }

        return _httpResponseUtil.GetBody(result);
    }

    /// <summary>
    ///     Handle client/locale
    /// </summary>
    /// <returns></returns>
    public string GetLocalesGlobal(string url, EmptyRequestData _, string sessionID)
    {
        var localeId = url.Replace("/client/locale/", "");
        var locales = _localeService.GetLocaleDb(localeId);

        return _httpResponseUtil.GetUnclearedBody(locales);
    }

    /// <summary>
    ///     Handle client/hideout/qte/list
    /// </summary>
    /// <returns></returns>
    public string GetQteList(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetUnclearedBody(_hideoutController.GetQteList(sessionID));
    }

    /// <summary>
    ///     Handle client/items/prices/
    /// </summary>
    /// <returns></returns>
    public string GetItemPrices(string url, EmptyRequestData _, string sessionID)
    {
        var traderId = url.Replace("/client/items/prices/", "");

        return _httpResponseUtil.GetBody(_traderController.GetItemPrices(sessionID, traderId));
    }
}
