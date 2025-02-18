using Core.DI;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Server;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class App
{
    protected readonly RandomUtil _randomUtil;
    protected ConfigServer _configServer;
    protected CoreConfig _coreConfig;
    protected DatabaseService _databaseService;
    protected EncodingUtil _encodingUtil;
    protected HttpServer _httpServer;
    protected LocalisationService _localisationService;

    protected ISptLogger<App> _logger;
    protected IEnumerable<IOnLoad> _onLoad;
    protected IEnumerable<IOnUpdate> _onUpdate;
    protected Dictionary<string, long> _onUpdateLastRun = new();
    protected Timer _timer;
    protected TimeUtil _timeUtil;

    public App(
        ISptLogger<App> logger,
        TimeUtil timeUtil,
        RandomUtil randomUtil,
        LocalisationService localisationService,
        ConfigServer configServer,
        EncodingUtil encodingUtil,
        HttpServer httpServer,
        DatabaseService databaseService,
        IEnumerable<IOnLoad> onLoadComponents,
        IEnumerable<IOnUpdate> onUpdateComponents
    )
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _randomUtil = randomUtil;
        _localisationService = localisationService;
        _configServer = configServer;
        _encodingUtil = encodingUtil;
        _httpServer = httpServer;
        _databaseService = databaseService;
        _onLoad = onLoadComponents;
        _onUpdate = onUpdateComponents;

        _coreConfig = configServer.GetConfig<CoreConfig>();
    }

    public async Task Run()
    {
        // execute onLoad callbacks
        _logger.Info(_localisationService.GetText("executing_startup_callbacks"));

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"OS: {Environment.OSVersion.Version} | {Environment.OSVersion.Platform}");
            _logger.Debug($"Ran as admin: {Environment.IsPrivilegedProcess}");
            _logger.Debug($"CPU cores: {Environment.ProcessorCount}");
            _logger.Debug($"PATH: {_encodingUtil.ToBase64(Environment.ProcessPath ?? "null returned")}");
            _logger.Debug($"Server: {ProgramStatics.SPT_VERSION() ?? _coreConfig.SptVersion}");

            // _logger.Debug($"RAM: {(os.totalmem() / 1024 / 1024 / 1024).toFixed(2)}GB");

            if (ProgramStatics.BUILD_TIME() is not null)
            {
                _logger.Debug($"Date: {ProgramStatics.BUILD_TIME()}");
            }

            if (ProgramStatics.COMMIT() is not null)
            {
                _logger.Debug($"Commit: {ProgramStatics.COMMIT()}");
            }
        }

        foreach (var onLoad in _onLoad)
        {
            await onLoad.OnLoad();
        }

        _timer = new Timer(_ => Update(_onUpdate), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(5000));

        if (_httpServer.IsStarted())
        {
            _logger.Success(_localisationService.GetText("started_webserver_success", _httpServer.ListeningUrl()));
            _logger.Success(_localisationService.GetText("websocket-started", _httpServer.ListeningUrl().Replace("https://", "wss://")));
        }

        _logger.Success(GetRandomisedStartMessage());
    }

    protected string GetRandomisedStartMessage()
    {
        if (_randomUtil.GetInt(1, 1000) > 999)
        {
            return _localisationService.GetRandomTextThatMatchesPartialKey("server_start_meme_");
        }

        return _localisationService.GetText("server_start_success");
    }

    protected void Update(IEnumerable<IOnUpdate> onUpdateComponents)
    {
        try
        {
            // If the server has failed to start, skip any update calls
            if (!_httpServer.IsStarted() || !_databaseService.IsDatabaseValid())
            {
                return;
            }

            foreach (var updateable in onUpdateComponents)
            {
                var success = false;
                if (!_onUpdateLastRun.TryGetValue(updateable.GetRoute(), out var lastRunTimeTimestamp))
                {
                    lastRunTimeTimestamp = 0;
                }

                var secondsSinceLastRun = _timeUtil.GetTimeStamp() - lastRunTimeTimestamp;

                try
                {
                    success = updateable.OnUpdate(secondsSinceLastRun);
                }
                catch (Exception err)
                {
                    LogUpdateException(err, updateable);
                }

                if (success)
                {
                    _onUpdateLastRun[updateable.GetRoute()] = _timeUtil.GetTimeStamp();
                }
                else
                {
                    /* temporary for debug */
                    const int warnTime = 20 * 60;

                    if (secondsSinceLastRun % warnTime == 0)
                    {
                        if (_logger.IsLogEnabled(LogLevel.Debug))
                        {
                            _logger.Debug(_localisationService.GetText("route_onupdate_no_response", updateable.GetRoute()));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    protected void LogUpdateException(Exception err, IOnUpdate updateable)
    {
        _logger.Error(_localisationService.GetText("scheduled_event_failed_to_run", updateable.GetRoute()));
        _logger.Error(err.ToString());
    }
}
