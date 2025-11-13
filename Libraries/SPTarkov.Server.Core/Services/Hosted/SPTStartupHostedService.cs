using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using static SPTarkov.Server.Core.Extensions.StringExtensions;

namespace SPTarkov.Server.Core.Services.Hosted;

[Injectable(InjectionType.HostedService)]
public sealed class SPTStartupHostedService(
    IReadOnlyList<SptMod> loadedMods,
    BundleLoader bundleLoader,
    TimeUtil timeUtil,
    RandomUtil randomUtil,
    ServerLocalisationService serverLocalisationService,
    HttpServer httpServer,
    ISptLogger<SPTStartupHostedService> logger,
    IEnumerable<IOnLoad> onLoadComponents,
    IEnumerable<IOnUpdate> onUpdateComponents
) : BackgroundService
{
    private readonly Dictionary<string, long> _onUpdateLastRun = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (ProgramStatics.MODS())
            {
                foreach (var mod in loadedMods)
                {
                    if (mod.ModMetadata.IsBundleMod == true)
                    {
                        await bundleLoader.LoadBundlesAsync(mod).ConfigureAwait(false);
                    }
                }
            }

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                var totalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

                // Convert bytes to GB
                var totalMemoryGb = totalMemoryBytes / (1024.0 * 1024.0 * 1024.0);
                var pageFileGb = Environment.SystemPageSize / (1024.0 * 1024.0 * 1024.0);

                logger.Debug($"OS: {Environment.OSVersion.Version} | {Environment.OSVersion.Platform}");
                logger.Debug($"Pagefile: {pageFileGb:F2} GB");
                logger.Debug($"RAM: {totalMemoryGb:F2} GB");
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
                await onLoad.OnLoad(stoppingToken).ConfigureAwait(false);
            }

            logger.Success(serverLocalisationService.GetText("started_webserver_success", httpServer.ListeningUrl()));
            logger.Success(serverLocalisationService.GetText("websocket-started", httpServer.ListeningUrl().Replace("https://", "wss://")));

            logger.Success(GetRandomisedStartMessage());

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            while (!stoppingToken.IsCancellationRequested)
            {
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
                        if (await updateable.OnUpdate(stoppingToken, secondsSinceLastRun).ConfigureAwait(false))
                        {
                            _onUpdateLastRun[updateableName] = timeUtil.GetTimeStamp();
                        }
                    }
                    catch (Exception err)
                    {
                        LogUpdateException(err, updateable);
                    }
                }

                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            // Thrown when we stop gracefully, we don't need to care for this
            if (ex is OperationCanceledException)
            {
                logger.Info("Stopping server...");
                return;
            }

            logger.Critical("Critical exception, stopping server...", ex);
            throw;
        }
    }

    private string GetRandomisedStartMessage()
    {
        if (randomUtil.GetInt(1, 1000) > 999)
        {
            return serverLocalisationService.GetRandomTextThatMatchesPartialKey("server_start_meme_");
        }

        return serverLocalisationService.GetText("server_start_success");
    }

    private void LogUpdateException(Exception err, IOnUpdate updateable)
    {
        logger.Error(serverLocalisationService.GetText("scheduled_event_failed_to_run", updateable.GetType().FullName));
        logger.Error(err.ToString());
    }
}
