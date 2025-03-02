using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Services;

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
        CustomLocaleService customLocaleService,
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
            customLocaleService,
            localeService.GetServerSupportedLocales().ToHashSet(),
            localeService.GetLocaleFallbacks(),
            "en",
            "./Assets/database/locales/server"
        );
        _i18nService.SetLocaleByKey(localeService.GetDesiredServerLocale());
    }

    public string GetText(string key, object? args = null)
    {
        return args is null
            ? _i18nService.GetLocalisedValue(key)
            : _i18nService.GetLocalised(key, args);
    }

    public string GetText<T>(string key, T value) where T : IConvertible?
    {
        return _i18nService.GetLocalised(key, value);
    }

    public ICollection<string> GetKeys()
    {
        return _i18nService.GetLocalisedKeys();
    }

    public string GetRandomTextThatMatchesPartialKey(string partialKey)
    {
        var values = _localeService.GetLocaleKeysThatStartsWithValue(partialKey);
        var chosenKey = _randomUtil.GetArrayValue(values);

        return GetText(chosenKey);
    }
}
