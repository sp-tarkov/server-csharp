using Core.Utils;
using Core.Annotations;
using Core.Servers;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocalisationService
{
    private readonly ILogger _logger;
    private readonly RandomUtil _randomUtil;
    private readonly DatabaseServer _databaseServer;
    private readonly LocaleService _localeService;
    private readonly I18nService _i18nService;

    public LocalisationService(
        ILogger logger,
        RandomUtil randomUtil,
        DatabaseServer databaseServer,
        LocaleService localeService,
        JsonUtil jsonUtil
    )
    {
        _logger = logger;
        _randomUtil = randomUtil;
        _databaseServer = databaseServer;
        _localeService = localeService;
        _i18nService = new I18nService(
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
        return _i18nService.GetLocalised(key, args);
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
