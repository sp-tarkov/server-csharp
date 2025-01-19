using System.Globalization;
using SptCommon.Annotations;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocaleService(
    ISptLogger<LocaleService> _logger,
    DatabaseServer _databaseServer,
    ConfigServer _configServer
)
{
    protected ISptLogger<LocaleService> _logger;
    protected DatabaseServer _databaseServer;
    protected ConfigServer _configServer;
    protected LocaleConfig _localeConfig = _configServer.GetConfig<LocaleConfig>();

    /**
 * Get the eft globals db file based on the configured locale in config/locale.json, if not found, fall back to 'en'
 * @returns dictionary
 */
    public Dictionary<string, string> GetLocaleDb()
    {
        var desiredLocale = _databaseServer.GetTables().Locales.Global[GetDesiredGameLocale()];
        if (desiredLocale != null) return desiredLocale;

        _logger.Warning(
            $"Unable to find desired locale file using locale: {GetDesiredGameLocale()} from config/locale.json, falling back to 'en'"
        );

        return _databaseServer.GetTables().Locales.Global["en"];
    }

    /**
     * Gets the game locale key from the locale.json file,
     * if value is 'system' get system locale
     * @returns locale e.g en/ge/cz/cn
     */
    public string GetDesiredGameLocale()
    {
        if (_localeConfig.GameLocale.ToLower() == "system") return GetPlatformForClientLocale();

        return _localeConfig.GameLocale.ToLower();
    }

    /**
     * Gets the game locale key from the locale.json file,
     * if value is 'system' get system locale
     * @returns locale e.g en/ge/cz/cn
     */
    public string GetDesiredServerLocale()
    {
        if (_localeConfig.ServerLocale.ToLower() == "system") return GetPlatformForServerLocale();

        return _localeConfig.ServerLocale.ToLower();
    }

    /**
     * Get array of languages supported for localisation
     * @returns array of locales e.g. en/fr/cn
     */
    public List<string> GetServerSupportedLocales()
    {
        return _localeConfig.ServerSupportedLocales;
    }

    /**
     * Get array of languages supported for localisation
     * @returns array of locales e.g. en/fr/cn
     */
    public Dictionary<string, string> GetLocaleFallbacks()
    {
        return _localeConfig.Fallbacks;
    }

    /**
     * Get the full locale of the computer running the server lowercased e.g. en-gb / pt-pt
     * @returns string
     */
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
                    return "zh-cn";

                return languageCode;
            }

            if (baseNameCode == "pt")
                // Handle edge case of pt
                return "pt-pt";

            _logger.Warning($"Unsupported system language found: {baseNameCode}, falling back to english");

            return "en";
        }

        return baseNameCode;
    }

    /**
     * Get the locale of the computer running the server
     * @returns langage part of locale e.g. 'en' part of 'en-US'
     */
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
        if (locales.Global.ContainsKey(baseNameCode)) return baseNameCode;

        var languageCode = platformLocale.Name.ToLower();
        if (locales.Global.ContainsKey(languageCode)) return languageCode;

        /*
        const regionCode = platformLocale.region?.toLocaleLowerCase();
        if (regionCode && locales.global[regionCode]) {
            return regionCode;
        }
        */

        // BSG map DE to GE some reason
        if (baseNameCode == "de") return "ge";

        _logger.Warning($"Unsupported system language found: {languageCode}, falling back to english");
        return "en";
    }

    /**
     * This is in a function so we can overwrite it during testing
     * @returns The current platform locale
     */
    protected CultureInfo GetPlatformLocale()
    {
        return CultureInfo.InstalledUICulture;
    }
}
