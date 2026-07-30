using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class DataCallbacks(
    HttpResponseUtil httpResponseUtil,
    LocaleTable localeTable,
    GlobalTable globalTable,
    TemplateTable templateTable,
    SettingsTable settingsTable,
    HideoutTable hideoutTable,
    TraderController traderController,
    HideoutController hideoutController,
    LocaleService localeService
)
{
    /// <summary>
    ///     Handle client/settings
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetSettings(string url, EmptyRequestData _, MongoId sessionID)
    {
        var returns = httpResponseUtil.GetBody(settingsTable);
        return new ValueTask<string>(returns);
    }

    /// <summary>
    ///     Handle client/globals
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetGlobals(string url, EmptyRequestData _, MongoId sessionID)
    {
        var returns = httpResponseUtil.GetBody(globalTable);

        return new ValueTask<string>(returns);
    }

    /// <summary>
    ///     Handle client/items
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateItems(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetUnclearedBody(templateTable.Items));
    }

    /// <summary>
    ///     Handle client/handbook/templates
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateHandbook(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(templateTable.Handbook));
    }

    /// <summary>
    ///     Handle client/customization
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateSuits(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(templateTable.Customization));
    }

    /// <summary>
    ///     Handle client/account/customization
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTemplateCharacter(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(templateTable.Character));
    }

    /// <summary>
    ///     Handle client/hideout/settings
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetHideoutSettings(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(hideoutTable.Settings));
    }

    /// <summary>
    ///     Handle client/hideout/areas
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetHideoutAreas(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(hideoutTable.Areas));
    }

    /// <summary>
    ///     Handle client/hideout/production/recipes
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetHideoutProduction(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(hideoutTable.Production));
    }

    /// <summary>
    ///     Handle client/languages
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetLocalesLanguages(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(localeTable.Languages));
    }

    /// <summary>
    ///     Handle client/menu/locale
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetLocalesMenu(string url, EmptyRequestData _, MongoId sessionID)
    {
        var localeId = url.Replace("/client/menu/locale/", "");
        var result = localeTable.Menu?[localeId] ?? localeTable.Menu?.FirstOrDefault(m => m.Key == "en").Value;

        if (result == null)
        {
            throw new Exception($"Unable to determine locale for request with {localeId}");
        }

        return new ValueTask<string>(httpResponseUtil.GetBody(result));
    }

    /// <summary>
    ///     Handle client/locale
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetLocalesGlobal(string url, EmptyRequestData _, MongoId sessionID)
    {
        var localeId = url.Replace("/client/locale/", "");
        var locales = localeService.GetLocaleDb(localeId);

        return new ValueTask<string>(httpResponseUtil.GetUnclearedBody(locales));
    }

    /// <summary>
    ///     Handle client/hideout/qte/list
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetQteList(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetUnclearedBody(hideoutController.GetQteList(sessionID)));
    }

    /// <summary>
    ///     Handle client/items/prices/
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetItemPrices(string url, EmptyRequestData _, MongoId sessionID)
    {
        var traderId = url.Replace("/client/items/prices/", "");

        return new ValueTask<string>(httpResponseUtil.GetBody(traderController.GetItemPrices(sessionID, traderId)));
    }

    /// <summary>
    /// Handle /client/dialogue
    /// </summary>
    public ValueTask<string> GetDialogue(string url, GetClientDialogueRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetUnclearedBody(templateTable.Dialogue));
    }
}
