using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class App(
    IServiceProvider _serviceProvider,
    ISptLogger<App> _logger,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    EncodingUtil _encodingUtil,
    HttpServer _httpServer,
    DatabaseService _databaseService,
    IHostApplicationLifetime _appLifeTime,
    IEnumerable<IOnLoad> _onLoadComponents,
    IEnumerable<IOnUpdate> _onUpdateComponents,
    HttpServerHelper _httpServerHelper
)
{
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    protected Dictionary<string, long> _onUpdateLastRun = new();

    public async Task InitializeAsync()
    {
        ServiceLocator.SetServiceProvider(_serviceProvider);

        // execute onLoad callbacks
        _logger.Info(_localisationService.GetText("executing_startup_callbacks"));

        var isAlreadyRunning = _httpServerHelper.IsAlreadyRunning();
        if (isAlreadyRunning)
        {
            _logger.Critical(_localisationService.GetText("webserver_already_running"));
            await Task.Delay(Timeout.Infinite);
        }

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

        foreach (var onLoad in _onLoadComponents)
        {
            await onLoad.OnLoad();
        }

        // Discard here, as this task will run indefinitely
        _ = Task.Run(Update);
    }

    public async Task StartAsync()
    {
        if(!_httpServer.IsStarted())
        {
            _logger.Success(_localisationService.GetText("started_webserver_success", _httpServer.ListeningUrl()));
            _logger.Success(_localisationService.GetText("websocket-started", _httpServer.ListeningUrl().Replace("https://", "wss://")));
        }

        _logger.Success(GetRandomisedStartMessage());

       await _httpServer.StartAsync();
    }

    protected string GetRandomisedStartMessage()
    {
        if (_randomUtil.GetInt(1, 1000) > 999)
        {
            return _localisationService.GetRandomTextThatMatchesPartialKey("server_start_meme_");
        }

        return _localisationService.GetText("server_start_success");
    }

    protected async Task Update()
    {
        while (!_appLifeTime.ApplicationStopping.IsCancellationRequested)
        {
            // If the server has failed to start, skip any update calls
            if (!_httpServer.IsStarted() || !_databaseService.IsDatabaseValid())
            {
                await Task.Delay(5000, _appLifeTime.ApplicationStopping);

                // Skip forward to the next loop
                continue;
            }

            foreach (var updateable in _onUpdateComponents)
            {
                var updateableName = updateable.GetType().FullName;
                if (string.IsNullOrEmpty(updateableName))
                {
                    updateableName = $"{updateable.GetType().Namespace}.{updateable.GetType().Name}";
                }

                if (!_onUpdateLastRun.TryGetValue(updateableName, out var lastRunTimeTimestamp))
                {
                    lastRunTimeTimestamp = 0;
                }

                var secondsSinceLastRun = _timeUtil.GetTimeStamp() - lastRunTimeTimestamp;

                try
                {
                    await updateable.OnUpdate(secondsSinceLastRun);
                }
                catch (Exception err)
                {
                    LogUpdateException(err, updateable);
                }

                // Set last run after try catch, so if an exception is caused the task is seen as failed.
                _onUpdateLastRun[updateableName] = _timeUtil.GetTimeStamp();
            }

            await Task.Delay(5000, _appLifeTime.ApplicationStopping);
        }
    }

    protected void LogUpdateException(Exception err, IOnUpdate updateable)
    {
        _logger.Error(_localisationService.GetText("scheduled_event_failed_to_run", updateable.GetType().FullName));
        _logger.Error(err.ToString());
    }
}
