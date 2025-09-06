using Microsoft.Extensions.Hosting;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using static SPTarkov.Server.Core.Extensions.StringExtensions;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class App(
    IServiceProvider serviceProvider,
    ISptLogger<App> logger,
    TimeUtil timeUtil,
    RandomUtil randomUtil,
    ServerLocalisationService serverLocalisationService,
    HttpServer httpServer,
    DatabaseService databaseService,
    IHostApplicationLifetime appLifeTime,
    IEnumerable<IOnLoad> onLoadComponents,
    IEnumerable<IOnUpdate> onUpdateComponents
)
{
    protected readonly Dictionary<string, long> _onUpdateLastRun = new();

    public async Task InitializeAsync()
    {
        ServiceLocator.SetServiceProvider(serviceProvider);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"OS: {Environment.OSVersion.Version} | {Environment.OSVersion.Platform}");
            logger.Debug($"Ran as admin: {Environment.IsPrivilegedProcess}");
            logger.Debug($"CPU cores: {Environment.ProcessorCount}");
            logger.Debug($"PATH: {(Environment.ProcessPath ?? "null returned").Encode(EncodeType.BASE64)}");
            logger.Debug($"Server: {ProgramStatics.SPT_VERSION()}");

            // _logger.Debug($"RAM: {(os.totalmem() / 1024 / 1024 / 1024).toFixed(2)}GB");

            if (ProgramStatics.BUILD_TIME() != 0)
            {
                logger.Debug($"Date: {ProgramStatics.BUILD_TIME()}");
            }

            logger.Debug($"Commit: {ProgramStatics.COMMIT()}");
        }

        // execute onLoad callbacks
        logger.Info(serverLocalisationService.GetText("executing_startup_callbacks"));
        foreach (var onLoad in onLoadComponents)
        {
            await onLoad.OnLoad();
        }

        // Discard here, as this task will run indefinitely
        _ = Task.Run(Update);

        logger.Success(serverLocalisationService.GetText("started_webserver_success", httpServer.ListeningUrl()));
        logger.Success(serverLocalisationService.GetText("websocket-started", httpServer.ListeningUrl().Replace("https://", "wss://")));

        logger.Success(GetRandomisedStartMessage());
    }

    protected string GetRandomisedStartMessage()
    {
        if (randomUtil.GetInt(1, 1000) > 999)
        {
            return serverLocalisationService.GetRandomTextThatMatchesPartialKey("server_start_meme_");
        }

        return serverLocalisationService.GetText("server_start_success");
    }

    protected async Task Update()
    {
        while (!appLifeTime.ApplicationStopping.IsCancellationRequested)
        {
            // If the server has failed to start, skip any update calls
            if (!databaseService.IsDatabaseValid())
            {
                await Task.Delay(5000, appLifeTime.ApplicationStopping);

                // Skip forward to the next loop
                continue;
            }

            foreach (var updateable in onUpdateComponents)
            {
                var updateableName = updateable.GetType().FullName;
                if (string.IsNullOrEmpty(updateableName))
                {
                    updateableName = $"{updateable.GetType().Namespace}.{updateable.GetType().Name}";
                }

                var lastRunTimeTimestamp = _onUpdateLastRun.GetValueOrDefault(updateableName, 0);
                var secondsSinceLastRun = timeUtil.GetTimeStamp() - lastRunTimeTimestamp;

                try
                {
                    if (await updateable.OnUpdate(secondsSinceLastRun))
                    {
                        _onUpdateLastRun[updateableName] = timeUtil.GetTimeStamp();
                    }
                }
                catch (Exception err)
                {
                    LogUpdateException(err, updateable);
                }
            }

            await Task.Delay(5000, appLifeTime.ApplicationStopping);
        }
    }

    protected void LogUpdateException(Exception err, IOnUpdate updateable)
    {
        logger.Error(serverLocalisationService.GetText("scheduled_event_failed_to_run", updateable.GetType().FullName));
        logger.Error(err.ToString());
    }
}
