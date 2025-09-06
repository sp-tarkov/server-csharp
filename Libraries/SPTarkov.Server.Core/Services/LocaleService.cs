using System.Globalization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocaleService(ISptLogger<LocaleService> logger, DatabaseServer databaseServer, ConfigServer configServer)
{
    protected readonly LocaleConfig LocaleConfig = configServer.GetConfig<LocaleConfig>();
    private string _chosenServerLocale = string.Empty;
    private string _chosenClientLocale = string.Empty;

    /// <summary>
    ///     Get the eft globals db file based on the configured locale in config/locale.json, if not found, fall back to 'en'
    /// </summary>
    /// <returns> Dictionary of locales for desired language - en/fr/cn </returns>
    public Dictionary<string, string> GetLocaleDb(string? language = null)
    {
        var languageToUse = string.IsNullOrEmpty(language) ? GetDesiredGameLocale() : language;

        // if it can't get locales for language provided, default to en
        if (TryGetLocaleDb(languageToUse, out var localeToReturn) || TryGetLocaleDb("en", out localeToReturn))
        {
            return localeToReturn;
        }

        throw new Exception($"unable to get locales from either {languageToUse} or en");
    }

    /// <summary>
    ///     Attempts to retrieve the locale database for the specified language key
    /// </summary>
    /// <param name="languageKey">The language key for which the locale database should be retrieved.</param>
    /// <param name="localeToReturn">The resulting locale database as a dictionary, or null if the operation fails.</param>
    /// <returns>True if the locale database was successfully retrieved, otherwise false.</returns>
    protected bool TryGetLocaleDb(string languageKey, out Dictionary<string, string>? localeToReturn)
    {
        localeToReturn = null;
        if (!databaseServer.GetTables().Locales.Global.TryGetValue(languageKey, out var keyedLocales))
        {
            return false;
        }

        localeToReturn = keyedLocales.Value;

        return true;
    }

    /// <summary>
    ///     Gets the game locale key from the locale.json file,
    ///     if value is 'system' get system-configured locale
    /// </summary>
    /// <returns> Locale e.g en/ge/cz/cn </returns>
    public string GetDesiredGameLocale()
    {
        if (string.IsNullOrEmpty(_chosenClientLocale))
        {
            _chosenClientLocale = string.Equals(LocaleConfig.GameLocale, "system", StringComparison.OrdinalIgnoreCase)
                ? GetPlatformForClientLocale()
                : LocaleConfig.GameLocale.ToLowerInvariant(); // Use custom locale value
        }

        return _chosenClientLocale;
    }

    /// <summary>
    ///     Gets the game locale key from the locale.json file,
    ///     if value is 'system' get system locale
    /// </summary>
    /// <returns> Locale e.g en/ge/cz/cn </returns>
    public string GetDesiredServerLocale()
    {
        if (string.IsNullOrEmpty(_chosenServerLocale))
        {
            _chosenServerLocale = string.Equals(LocaleConfig.ServerLocale, "system", StringComparison.OrdinalIgnoreCase)
                ? GetPlatformForServerLocale()
                : LocaleConfig.ServerLocale.ToLowerInvariant(); // Use custom locale value
        }

        return _chosenServerLocale;
    }

    /// <summary>
    ///     Get array of languages supported for localisation
    /// </summary>
    /// <returns> List of locales e.g. en/fr/cn </returns>
    public HashSet<string> GetServerSupportedLocales()
    {
        return LocaleConfig.ServerSupportedLocales;
    }

    /// <summary>
    ///     Get array of languages supported for localisation
    /// </summary>
    /// <returns> Dictionary of locales e.g. en/fr/cn </returns>
    public Dictionary<string, string> GetLocaleFallbacks()
    {
        return LocaleConfig.Fallbacks;
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
            logger.Warning("System language not found, falling back to english");
            return "en";
        }

        var baseNameCode = platformLocale.TwoLetterISOLanguageName.ToLowerInvariant();
        if (LocaleConfig.ServerSupportedLocales.Contains(baseNameCode))
        {
            // Found a matching locale
            return baseNameCode;
        }

        // Check if base language (e.g. CN / EN / DE) exists
        var languageCode = platformLocale.Name.ToLowerInvariant();
        if (LocaleConfig.ServerSupportedLocales.Contains(languageCode))
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

        logger.Debug(
            $"Unsupported system language found: {baseNameCode}, langCode: {languageCode} falling back to english for server locale"
        );

        return "en";
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
            logger.Warning("System language not found, falling back to english");
            return "en";
        }

        var locales = databaseServer.GetTables().Locales;
        var baseNameCode = platformLocale.TwoLetterISOLanguageName.ToLowerInvariant();
        if (locales.Global.ContainsKey(baseNameCode))
        {
            return baseNameCode;
        }

        var languageCode = platformLocale.Name.ToLowerInvariant();
        if (locales.Global.ContainsKey(languageCode))
        {
            return languageCode;
        }

        // language code wasn't found, if it's over 2 characters
        // we can try taking first 2 characters and see if we have a locale that matches
        if (languageCode.Length > 2)
        {
            // Take first 2 characters and see if that exists
            if (locales.Global.ContainsKey(languageCode[..1]))
            {
                return languageCode;
            }
        }

        // BSG map DE to GE some reason
        if (baseNameCode == "de")
        {
            return "ge";
        }

        if (baseNameCode == "zh")
        // Handle edge case of zh
        {
            return "cn";
        }

        logger.Debug(
            $"Unsupported system language found: {languageCode} baseLocale: {baseNameCode}, falling back to english for client locale"
        );
        return "en";
    }

    /// <summary>
    ///     Get the current machines locale data
    /// </summary>
    /// <returns> The current platform locale </returns>
    protected static CultureInfo GetPlatformLocale()
    {
        return CultureInfo.InstalledUICulture;
    }
}
