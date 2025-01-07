using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class App
{
    protected Dictionary<string, long> _onUpdateLastRun;
    protected CoreConfig _coreConfig;
    
    private ILogger _logger;
    private TimeUtil _timeUtil;
    private LocalisationService _localisationService;
    private ConfigServer _configServer;
    private EncodingUtil _encodingUtil;
    private HttpServer _httpServer;
    private DatabaseService _databaseService;
    private IEnumerable<OnLoad> _onLoad;
    private IEnumerable<OnUpdate> _onUpdate;
    
    public App(
        ILogger logger,
        TimeUtil timeUtil,
        LocalisationService localisationService,
        ConfigServer configServer,
        EncodingUtil encodingUtil,
        HttpServer httpServer,
        DatabaseService databaseService,
        IEnumerable<OnLoad> onLoadComponents,
        IEnumerable<OnUpdate> onUpdateComponents
    ) {
        _logger = logger;
        _timeUtil = timeUtil;
        _localisationService = localisationService;
        _configServer = configServer;
        _encodingUtil = encodingUtil;
        _httpServer = httpServer;
        _databaseService = databaseService;
        _onLoad = onLoadComponents;
        _onUpdate = onUpdateComponents;
        
        _coreConfig = configServer.GetConfig<CoreConfig>(ConfigTypes.CORE);
    }
}