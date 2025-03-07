using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class CustomLocaleService(
    ISptLogger<CustomLocaleService> logger
)
{
    protected Dictionary<string, Dictionary<string, string>> customServerLocales = new();
    protected Dictionary<string, Dictionary<string, string>> customClientLocales = new();


    /// <summary>
    /// Path should link to a folder containing every locale that should be added to the server locales
    /// e.g. en.json for english, fr.json for french
    /// Inside each JSON should be a Dictionary of the locale key and localised text
    /// </summary>
    /// <param name="locale">en/fr/de</param>
    /// <param name="localeKey">locale key to store values against</param>
    /// <param name="localeValue">Localised string to store</param>
    public void AddServerLocales(string locale, string localeKey, string localeValue)
    {
        AddToDictionary(locale, localeKey, localeValue, customServerLocales);
    }

    /// <summary>
    /// Path should link to a folder containing every locale that should be added to the game locales
    /// e.g. en.json for english, fr.json for french
    /// Inside each JSON should be a Dictionary of the locale key and localised text
    /// </summary>
    /// <param name="locale">en/fr/de</param>
    /// <param name="localeKey">locale key to store values against</param>
    /// <param name="localeValue">Localised string to store</param>
    public void AddGameLocales(string locale, string localeKey, string localeValue)
    {
        AddToDictionary(locale, localeKey, localeValue, customClientLocales);
    }

    protected void AddToDictionary(string locale, string localeKey, string localeValue,
        Dictionary<string, Dictionary<string, string>> dictionaryToAddTo)
    {
        dictionaryToAddTo.TryAdd(locale, new Dictionary<string, string>());
        if (!dictionaryToAddTo.TryGetValue(locale, out var localeDictToAddTo))
        {
            logger.Error($"Unable to get custom locale dictionary keyed by: {locale}");

            return;
        }

        if (!localeDictToAddTo.TryAdd(localeKey, localeValue))
        {
            logger.Error($"Unable to add: {localeKey} {localeValue} to custom locale dictionary: {locale}");
        }
    }

    public string? GetServerValue(string locale, string localeKey)
    {
        return GetValueFromDictionary(locale, localeKey, customServerLocales);
    }

    public string? GetClientValue(string locale, string localeKey)
    {
        return GetValueFromDictionary(locale, localeKey, customClientLocales);
    }

    protected string? GetValueFromDictionary(string locale, string localeKey,
        Dictionary<string, Dictionary<string, string>> dictionaryToGetFrom)
    {
        return dictionaryToGetFrom.TryGetValue(locale, out var localeDictToGetFrom)
            ? localeDictToGetFrom.GetValueOrDefault(localeKey) // Locale exists, look up value or return null
            : null; // No locale (e.g. en/fr/de) at all
    }
}
