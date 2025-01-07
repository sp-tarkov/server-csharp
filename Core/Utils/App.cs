using Core.Annotations;
using Core.DI;
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
    
    public async Task Load()
    {
        // execute onLoad callbacks
        _logger.Info(_localisationService.GetText("executing_startup_callbacks"));

        /*
        _logger.Debug($"OS: {os.arch()} | {os.version()} | {process.platform}");
        _logger.Debug($"CPU: {os.cpus()[0]?.model} cores: {os.cpus().length}");
        _logger.Debug($"RAM: {(os.totalmem() / 1024 / 1024 / 1024).toFixed(2)}GB");
        _logger.Debug($"PATH: {this.encodingUtil.toBase64(process.argv[0])}");
        _logger.Debug($"PATH: {this.encodingUtil.toBase64(process.execPath)}");
        _logger.Debug($"Server: {ProgramStatics.SPT_VERSION || this.coreConfig.sptVersion}");

        const nodeVersion = process.version.replace(/^v/, "");
        if (ProgramStatics.EXPECTED_NODE && nodeVersion !== ProgramStatics.EXPECTED_NODE) {
            this.logger.error(
                "Node version mismatch. Required: ${ProgramStatics.EXPECTED_NODE} | Current: ${nodeVersion}",
            );
            process.exit(1);
        }
        _logger.Debug("Node: ${nodeVersion}");

        if (ProgramStatics.BUILD_TIME) {
            _logger.Debug("Date: ${ProgramStatics.BUILD_TIME}");
        }

        if (ProgramStatics.COMMIT) {
            _logger.Debug("Commit: ${ProgramStatics.COMMIT}");
        }
        */
        foreach (var onLoad in _onLoad)
        {
            await onLoad.OnLoad();
        }

        var timer = new Timer(_ =>
        {
            update(_onUpdate);
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(5000));
    }

    protected async Task update(IEnumerable<OnUpdate> onUpdateComponents)
    {
        // If the server has failed to start, skip any update calls
        if (!_httpServer.IsStarted() || !_databaseService.IsDatabaseValid()) {
            return;
        }

        foreach (var updateable in onUpdateComponents)
        {
            var success = false;
            if (!_onUpdateLastRun.TryGetValue(updateable.GetRoute(), out var lastRunTimeTimestamp))
                lastRunTimeTimestamp = 0;
            var secondsSinceLastRun = _timeUtil.GetTimeStamp() - lastRunTimeTimestamp;

            try {
                success = await updateable.OnUpdate(secondsSinceLastRun);
            } catch (Exception err) {
                LogUpdateException(err, updateable);
            }

            if (success) {
                _onUpdateLastRun[updateable.GetRoute()] = _timeUtil.GetTimeStamp();
            } else {
                /* temporary for debug */
                var warnTime = 20 * 60;

                if (secondsSinceLastRun % warnTime == 0) {
                    _logger.Debug(_localisationService.GetText("route_onupdate_no_response", updateable.GetRoute()));
                }
            }
        }
    }

    protected void LogUpdateException(Exception err, OnUpdate updateable) {
        _logger.Error(_localisationService.GetText("scheduled_event_failed_to_run", updateable.GetRoute()));
        _logger.Error(err.ToString());
    }
}