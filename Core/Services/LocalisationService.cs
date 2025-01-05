using Types.Annotations;
using Types.Servers;
using Types.Utils;
using ILogger = Types.Models.Utils.ILogger;

namespace Types.Services;

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
        LocaleService localeService
    )
    {
        _logger = logger;
        _randomUtil = randomUtil;
        _databaseServer = databaseServer;
        _localeService = localeService;
        _i18nService = new I18nService();
        
        File.ReadAllText()
    }

    public string GetText(string key, object? args = null)
    {
        throw new NotImplementedException();
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