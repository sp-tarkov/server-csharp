using Microsoft.Extensions.Hosting;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using static SPTarkov.Server.Core.Extensions.StringExtensions;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class App(
    IServiceProvider _serviceProvider,
    ISptLogger<App> _logger,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    ServerLocalisationService _serverLocalisationService,
    ConfigServer _configServer,
    HttpServer _httpServer,
    DatabaseService _databaseService,
    IHostApplicationLifetime _appLifeTime,
    IEnumerable<IOnLoad> _onLoadComponents,
    IEnumerable<IOnUpdate> _onUpdateComponents
)
{
    protected readonly CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    protected readonly Dictionary<string, long> _onUpdateLastRun = new();

    public async Task InitializeAsync()
    {
        ServiceLocator.SetServiceProvider(_serviceProvider);

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"OS: {Environment.OSVersion.Version} | {Environment.OSVersion.Platform}");
            _logger.Debug($"Ran as admin: {Environment.IsPrivilegedProcess}");
            _logger.Debug($"CPU cores: {Environment.ProcessorCount}");
            _logger.Debug($"PATH: {(Environment.ProcessPath ?? "null returned").Encode(EncodeType.BASE64)}");
            _logger.Debug($"Server: {ProgramStatics.SPT_VERSION() ?? _coreConfig.SptVersion}");

            // _logger.Debug($"RAM: {(os.totalmem() / 1024 / 1024 / 1024).toFixed(2)}GB");

            if (ProgramStatics.BUILD_TIME() != 0)
            {
                _logger.Debug($"Date: {ProgramStatics.BUILD_TIME()}");
            }

            _logger.Debug($"Commit: {ProgramStatics.COMMIT()}");
        }

        // execute onLoad callbacks
        _logger.Info(_serverLocalisationService.GetText("executing_startup_callbacks"));
        foreach (var onLoad in _onLoadComponents)
        {
            await onLoad.OnLoad();
        }

        // Discard here, as this task will run indefinitely
        _ = Task.Run(Update);

        _logger.Success(_serverLocalisationService.GetText("started_webserver_success", _httpServer.ListeningUrl()));
        _logger.Success(_serverLocalisationService.GetText("websocket-started", _httpServer.ListeningUrl().Replace("https://", "wss://")));

        _logger.Success(GetRandomisedStartMessage());
    }

    protected string GetRandomisedStartMessage()
    {
        if (_randomUtil.GetInt(1, 1000) > 999)
        {
            return _serverLocalisationService.GetRandomTextThatMatchesPartialKey("server_start_meme_");
        }

        return _serverLocalisationService.GetText("server_start_success");
    }

    protected async Task Update()
    {
        while (!_appLifeTime.ApplicationStopping.IsCancellationRequested)
        {
            // If the server has failed to start, skip any update calls
            if (!_databaseService.IsDatabaseValid())
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

                var lastRunTimeTimestamp = _onUpdateLastRun.GetValueOrDefault(updateableName, 0);
                var secondsSinceLastRun = _timeUtil.GetTimeStamp() - lastRunTimeTimestamp;

                try
                {
                    if (await updateable.OnUpdate(secondsSinceLastRun))
                    {
                        _onUpdateLastRun[updateableName] = _timeUtil.GetTimeStamp();
                    }
                }
                catch (Exception err)
                {
                    LogUpdateException(err, updateable);
                }
            }

            await Task.Delay(5000, _appLifeTime.ApplicationStopping);
        }
    }

    protected void LogUpdateException(Exception err, IOnUpdate updateable)
    {
        _logger.Error(_serverLocalisationService.GetText("scheduled_event_failed_to_run", updateable.GetType().FullName));
        _logger.Error(err.ToString());
    }
}
