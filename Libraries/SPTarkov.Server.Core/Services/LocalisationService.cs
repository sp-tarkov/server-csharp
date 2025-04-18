using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

/// <summary>
/// Handles translating server text into different langauges
/// </summary>
[Injectable(InjectionType.Singleton)]
public class LocalisationService
{
    protected DatabaseServer _databaseServer;

    protected I18nService _i18nService;
    protected LocaleService _localeService;
    protected ISptLogger<LocalisationService> _logger;
    protected RandomUtil _randomUtil;

    // TODO: turn into primary ctor
    public LocalisationService(
        ISptLogger<LocalisationService> logger,
        RandomUtil randomUtil,
        DatabaseServer databaseServer,
        LocaleService localeService,
        JsonUtil jsonUtil,
        FileUtil fileUtil
    )
    {
        _logger = logger;
        _randomUtil = randomUtil;
        _databaseServer = databaseServer;
        _localeService = localeService;
        _i18nService = new I18nService(
            fileUtil,
            jsonUtil,
            localeService.GetServerSupportedLocales().ToHashSet(),
            localeService.GetLocaleFallbacks(),
            "en",
            "./Assets/database/locales/server",
            localeService
        );
        _i18nService.SetLocaleByKey(localeService.GetDesiredServerLocale());
    }
    /// <summary>
    /// Get a localised value using the passed in key
    /// </summary>
    /// <param name="key"> Key to look up locale for </param>
    /// <param name="args"> optional arguments </param>
    /// <returns> Localised string </returns>
    public string GetText(string key, object? args = null)
    {
        return args is null
            ? _i18nService.GetLocalisedValue(key)
            : _i18nService.GetLocalised(key, args);
    }

    /// <summary>
    /// Get a localised value using the passed in key
    /// </summary>
    /// <param name="key"> Key to look up locale for </param>
    /// <param name="value"> Value to localize </param>
    /// <returns> Localised string </returns>
    public string GetText<T>(string key, T value) where T : IConvertible?
    {
        return _i18nService.GetLocalised(key, value);
    }

    /// <summary>
    /// Get all locale keys
    /// </summary>
    /// <returns> Generic collection of keys </returns>
    public ICollection<string> GetKeys()
    {
        return _i18nService.GetLocalisedKeys();
    }

    /// <summary>
    /// From the provided partial key, find all keys that start with text and choose a random match
    /// </summary>
    /// <param name="partialKey"> Key to match locale keys on </param>
    /// <returns> Locale text </returns>
    public string GetRandomTextThatMatchesPartialKey(string partialKey)
    {
        var values = _localeService.GetLocaleKeysThatStartsWithValue(partialKey);
        var chosenKey = _randomUtil.GetArrayValue(values);

        return GetText(chosenKey);
    }
}
