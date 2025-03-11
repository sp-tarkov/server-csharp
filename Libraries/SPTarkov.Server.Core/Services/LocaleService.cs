using System.Globalization;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocaleService(
    ISptLogger<LocaleService> _logger,
    DatabaseServer _databaseServer,
    ConfigServer _configServer
)
{
    protected LocaleConfig _localeConfig = _configServer.GetConfig<LocaleConfig>();

    /// <summary>
    ///  Get the eft globals db file based on the configured locale in config/locale.json, if not found, fall back to 'en'
    /// </summary>
    /// <returns> Dictionary </returns>
    public Dictionary<string, string> GetLocaleDb()
    {
        if (_databaseServer.GetTables().Locales.Global.TryGetValue(GetDesiredGameLocale(), out var desiredLocale))
        {
            return desiredLocale.Value;
        }

        _logger.Warning(
            $"Unable to find desired locale file using locale: {GetDesiredGameLocale()} from config/locale.json, falling back to 'en'"
        );

        return _databaseServer.GetTables().Locales.Global["en"].Value;
    }

    /// <summary>
    /// Gets the game locale key from the locale.json file,
    /// if value is 'system' get system locale
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
    /// Gets the game locale key from the locale.json file,
    /// if value is 'system' get system locale
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
    /// Get array of languages supported for localisation
    /// </summary>
    /// <returns> List of locales e.g. en/fr/cn </returns>
    public List<string> GetServerSupportedLocales()
    {
        return _localeConfig.ServerSupportedLocales;
    }

    /// <summary>
    /// Get array of languages supported for localisation
    /// </summary>
    /// <returns> Dictionary of locales e.g. en/fr/cn </returns>
    public Dictionary<string, string> GetLocaleFallbacks()
    {
        return _localeConfig.Fallbacks;
    }

    /// <summary>
    /// Get the full locale of the computer running the server lowercased e.g. en-gb / pt-pt
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
    /// Get the locale of the computer running the server
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
    /// This is in a function so we can overwrite it during testing
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
}
