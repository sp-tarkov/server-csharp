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
    public ValueTask<string> GetSettings(string url, EmptyRequestData _, string sessionID)
    {
        var returns = _httpResponseUtil.GetBody(_databaseService.GetSettings());
        return new ValueTask<string>(returns);
    }

    /// <summary>
    ///     Handle client/globals
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetGlobals(string url, EmptyRequestData _, string sessionID)
    {
        var globals = _databaseService.GetGlobals();
        var returns = _httpResponseUtil.GetBody(globals);

        return new ValueTask<string>(returns);
    }

    /// <summary>
    ///     Handle client/items
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateItems(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetUnclearedBody(_databaseService.GetItems()));
    }

    /// <summary>
    ///     Handle client/handbook/templates
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateHandbook(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_databaseService.GetHandbook()));
    }

    /// <summary>
    ///     Handle client/customization
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateSuits(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_databaseService.GetTemplates().Customization));
    }

    /// <summary>
    ///     Handle client/account/customization
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateCharacter(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_databaseService.GetTemplates().Character));
    }

    /// <summary>
    ///     Handle client/hideout/settings
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetHideoutSettings(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_databaseService.GetHideout().Settings));
    }

    /// <summary>
    ///     Handle client/hideout/areas
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetHideoutAreas(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_databaseService.GetHideout().Areas));
    }

    /// <summary>
    ///     Handle client/hideout/production/recipes
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetHideoutProduction(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_databaseService.GetHideout().Production));
    }

    /// <summary>
    ///     Handle client/languages
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetLocalesLanguages(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_databaseService.GetLocales().Languages));
    }

    /// <summary>
    ///     Handle client/menu/locale
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetLocalesMenu(string url, EmptyRequestData _, string sessionID)
    {
        var localeId = url.Replace("/client/menu/locale/", "");
        var locales = _databaseService.GetLocales();
        var result = locales.Menu?[localeId] ?? locales.Menu?.FirstOrDefault(m => m.Key == "en").Value;

        if (result == null)
        {
            throw new Exception($"Unable to determine locale for request with {localeId}");
        }

        return new ValueTask<string>(_httpResponseUtil.GetBody(result));
    }

    /// <summary>
    ///     Handle client/locale
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetLocalesGlobal(string url, EmptyRequestData _, string sessionID)
    {
        var localeId = url.Replace("/client/locale/", "");
        var locales = _localeService.GetLocaleDb(localeId);

        return new ValueTask<string>(_httpResponseUtil.GetUnclearedBody(locales));
    }

    /// <summary>
    ///     Handle client/hideout/qte/list
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetQteList(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetUnclearedBody(_hideoutController.GetQteList(sessionID)));
    }

    /// <summary>
    ///     Handle client/items/prices/
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetItemPrices(string url, EmptyRequestData _, string sessionID)
    {
        var traderId = url.Replace("/client/items/prices/", "");

        return new ValueTask<string>(_httpResponseUtil.GetBody(_traderController.GetItemPrices(sessionID, traderId)));
    }
}
