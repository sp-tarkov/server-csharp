using Core.Utils;
using Core.Annotations;
using Core.Models.Utils;
using Core.Servers;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocalisationService
{
    protected ISptLogger<LocalisationService> _logger;
    protected RandomUtil _randomUtil;
    protected DatabaseServer _databaseServer;
    protected LocaleService _localeService;
    protected I18nService _i18nService;
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
            localeService.GetServerSupportedLocales(),
            localeService.GetLocaleFallbacks(),
            "en",
            "./Assets/database/locales/server"
        );
        _i18nService.SetLocale(localeService.GetDesiredServerLocale());
    }

    public string GetText(string key, object? args = null)
    {
        return args is null
            ? _i18nService.GetLocalised(key)
            : _i18nService.GetLocalised(key, args);
    }

    public string GetText<T>(string key, T value) where T :IConvertible
    {
        return _i18nService.GetLocalised(key, value);
    }

    public ICollection<string> GetKeys()
    {
        throw new NotImplementedException();
    }

    public string GetRandomTextThatMatchesPartialKey(string partialKey)
    {
        throw new NotImplementedException();
    }
}
