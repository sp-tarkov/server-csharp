using System.Globalization;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocaleService(
    ISptLogger<LocaleService> _logger,
    DatabaseServer _databaseServer,
    ConfigServer _configServer
)
{
    // we have to LazyLoad the data from the database and then combine it with the custom data before returning it
    protected LocaleConfig _localeConfig = _configServer.GetConfig<LocaleConfig>();
    protected Dictionary<string, Dictionary<string, string>> customClientLocales = new();

    /// <summary>
    ///     Get the eft globals db file based on the configured locale in config/locale.json, if not found, fall back to 'en'
    ///     This will contain Custom locales added by mods
    /// </summary>
    /// <returns> Dictionary of locales for desired language - en/fr/cn </returns>
    public Dictionary<string, string> GetLocaleDb(string? language = null)
    {
        var languageToUse = string.IsNullOrEmpty(language) ? GetDesiredGameLocale() : language;
        Dictionary<string, string>? localeToReturn;

        // if it can't get locales for language provided, default to en
        if (TryGetLocaleDbWithCustomLocales(languageToUse, out localeToReturn) ||
            TryGetLocaleDbWithCustomLocales("en", out localeToReturn))
        {
            // TODO: need to see if this needs to be cloned
            return RemovePraporTestMessage(localeToReturn);
        }

        throw new Exception($"unable to get locales from either {languageToUse} or en");
    }

    /// <summary>
    ///     Attempts to retrieve the locale database for the specified language key, including custom locales if available.
    /// </summary>
    /// <param name="languageKey">The language key for which the locale database should be retrieved.</param>
    /// <param name="localeToReturn">The resulting locale database as a dictionary, or null if the operation fails.</param>
    /// <returns>True if the locale database was successfully retrieved, otherwise false.</returns>
    private bool TryGetLocaleDbWithCustomLocales(string languageKey, out Dictionary<string, string>? localeToReturn)
    {
        localeToReturn = null;
        if (!_databaseServer.GetTables().Locales.Global.TryGetValue(languageKey, out var keyedLocales))
        {
            return false;
        }

        localeToReturn = keyedLocales.Value;

        if (customClientLocales.TryGetValue(languageKey, out var customClientLocale))
        {
            // there were custom locales for this language
            localeToReturn = CombineDbWithCustomLocales(localeToReturn, customClientLocale);
        }

        return true;
    }


    /// <summary>
    ///     Combines the provided database locales with custom locales, ensuring that all entries are merged into a single dictionary.
    ///     Custom locale entries will overwrite existing keys from the database locales if conflicts occur.
    /// </summary>
    /// <param name="dbLocales">The dictionary containing locale entries from the database.</param>
    /// <param name="customLocales">The dictionary containing custom locale entries to be merged.</param>
    /// <returns>A dictionary representing the merged result of database and custom locales.</returns>
    private Dictionary<string, string> CombineDbWithCustomLocales(Dictionary<string, string> dbLocales, Dictionary<string, string> customLocales)
    {
        try
        {
            return dbLocales
                .Concat(customLocales)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    group => group.Key,
                    group => group.Last().Value
                );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    ///     Gets the game locale key from the locale.json file,
    ///     if value is 'system' get system locale
    /// </summary>
    /// <returns> Locale e.g en/ge/cz/cn </returns>
    public string GetDesiredGameLocale()
    {
        if (_localeConfig.GameLocale.ToLower() == "system")
        {
            return GetPlatformForClientLocale();
        }

        return _localeConfig.GameLocale.ToLower();
    }

    /// <summary>
    ///     Gets the game locale key from the locale.json file,
    ///     if value is 'system' get system locale
    /// </summary>
    /// <returns> Locale e.g en/ge/cz/cn </returns>
    public string GetDesiredServerLocale()
    {
        if (_localeConfig.ServerLocale.ToLower() == "system")
        {
            return GetPlatformForServerLocale();
        }

        return _localeConfig.ServerLocale.ToLower();
    }

    /// <summary>
    ///     Get array of languages supported for localisation
    /// </summary>
    /// <returns> List of locales e.g. en/fr/cn </returns>
    public List<string> GetServerSupportedLocales()
    {
        return _localeConfig.ServerSupportedLocales;
    }

    /// <summary>
    ///     Get array of languages supported for localisation
    /// </summary>
    /// <returns> Dictionary of locales e.g. en/fr/cn </returns>
    public Dictionary<string, string> GetLocaleFallbacks()
    {
        return _localeConfig.Fallbacks;
    }

    /// <summary>
    ///     Get the full locale of the computer running the server lowercased e.g. en-gb / pt-pt
    /// </summary>
    /// <returns> System locale as String </returns>
    public string GetPlatformForServerLocale()
    {
        var platformLocale = GetPlatformLocale();
        if (platformLocale == null)
        {
            _logger.Warning("System language could not be found, falling back to english");
            return "en";
        }

        var baseNameCode = platformLocale.TwoLetterISOLanguageName.ToLower();
        if (!_localeConfig.ServerSupportedLocales.Contains(baseNameCode))
        {
            // Check if base language (e.g. CN / EN / DE) exists
            var languageCode = platformLocale.Name.ToLower();
            if (_localeConfig.ServerSupportedLocales.Contains(languageCode))
            {
                if (baseNameCode == "zh")
                    // Handle edge case of zh
                {
                    return "zh-cn";
                }

                return languageCode;
            }

            if (baseNameCode == "pt")
                // Handle edge case of pt
            {
                return "pt-pt";
            }

            _logger.Warning($"Unsupported system language found: {baseNameCode}, falling back to english");

            return "en";
        }

        return baseNameCode;
    }

    /// <summary>
    ///     Get the locale of the computer running the server
    /// </summary>
    /// <returns> Language part of locale e.g. 'en' part of 'en-US' </returns>
    protected string GetPlatformForClientLocale()
    {
        var platformLocale = GetPlatformLocale();
        if (platformLocale == null)
        {
            _logger.Warning("System language could not be found, falling back to english");
            return "en";
        }

        var locales = _databaseServer.GetTables().Locales;
        var baseNameCode = platformLocale.TwoLetterISOLanguageName.ToLower();
        if (locales.Global.ContainsKey(baseNameCode))
        {
            return baseNameCode;
        }

        var languageCode = platformLocale.Name.ToLower();
        if (locales.Global.ContainsKey(languageCode))
        {
            return languageCode;
        }

        //
        // const regionCode = platformLocale.region?.toLocaleLowerCase();
        // if (regionCode && locales.global[regionCode]) {
        //     return regionCode;
        // }

        // BSG map DE to GE some reason
        if (baseNameCode == "de")
        {
            return "ge";
        }

        _logger.Warning($"Unsupported system language found: {languageCode}, falling back to english");
        return "en";
    }

    /// <summary>
    ///     This is in a function so we can overwrite it during testing
    /// </summary>
    /// <returns> The current platform locale </returns>
    protected CultureInfo GetPlatformLocale()
    {
        return CultureInfo.InstalledUICulture;
    }

    public List<string> GetLocaleKeysThatStartsWithValue(string partialKey)
    {
        return GetLocaleDb().Keys.Where(x => x.StartsWith(partialKey)).ToList();
    }

    public void AddCustomClientLocale(string locale, string localeKey, string localeValue)
    {
        AddToDictionary(locale, localeKey, localeValue, customClientLocales);
    }

    private void AddToDictionary(string locale, string localeKey, string localeValue,
        Dictionary<string, Dictionary<string, string>> dictionaryToAddTo)
    {
        dictionaryToAddTo.TryAdd(locale, new Dictionary<string, string>());
        if (!dictionaryToAddTo.TryGetValue(locale, out var localeDictToAddTo))
        {
            _logger.Error($"Unable to get custom locale dictionary keyed by: {locale}");

            return;
        }

        if (!localeDictToAddTo.TryAdd(localeKey, localeValue))
        {
            localeDictToAddTo[localeKey] = localeValue;
        }
    }

    /// <summary>
    ///     Blank out the "test" mail message from prapor
    /// </summary>
    protected Dictionary<string, string> RemovePraporTestMessage(Dictionary<string, string> dbLocales)
    {
        dbLocales["61687e2c3e526901fa76baf9"] = "";
        return dbLocales;
    }
}
