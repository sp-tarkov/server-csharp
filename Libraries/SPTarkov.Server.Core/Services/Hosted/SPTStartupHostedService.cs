using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services.Hosted;

[Injectable(InjectionType.HostedService)]
public sealed class SPTStartupHostedService(
    IReadOnlyList<SptMod> loadedMods,
    IReadOnlyList<DependencyInjectionContainer> dependencyInjectionContainers,
    BundleLoader bundleLoader,
    TimeUtil timeUtil,
    RandomUtil randomUtil,
    ServerLocalisationService serverLocalisationService,
    HttpServer httpServer,
    ISptLogger<SPTStartupHostedService> logger,
    IServiceProvider serviceProvider,
    SystemInformationLogger systemInformationLogger,
    IEnumerable<IOnUpdate> onUpdateComponents
) : BackgroundService
{
    private readonly Dictionary<string, long> _onUpdateLastRun = [];

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (ProgramStatics.MODS())
            {
                foreach (var mod in loadedMods)
                {
                    if (File.Exists(Path.Join(Directory.GetCurrentDirectory(), mod.GetModPath(), "bundles.json")))
                    {
                        await bundleLoader.LoadBundlesAsync(mod, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            systemInformationLogger.LogSystemInformation();

            // execute OnLoad callbacks past PreLoad
            var PostPreloadComponents = dependencyInjectionContainers
                .Where(container => container.Type == typeof(IOnLoad))
                .Where(container => container.InjectableAttribute.TypePriority >= OnLoadOrder.GameCallbacks);

            foreach (var onLoadContainer in PostPreloadComponents)
            {
                var onLoadService = serviceProvider.GetRequiredService(onLoadContainer.ParentType);

                if (onLoadService is not IOnLoad onLoad)
                {
                    throw new InvalidOperationException($"Unable to resolve {onLoadContainer.ParentType.FullName} as {nameof(IOnLoad)}");
                }

                await onLoad.OnLoadAsync(cancellationToken).ConfigureAwait(false);
            }

            logger.Success(serverLocalisationService.GetText("started_webserver_success", httpServer.ListeningUrl()));
            logger.Success(serverLocalisationService.GetText("websocket-started", httpServer.ListeningUrl().Replace("https://", "wss://")));

            logger.Success(GetRandomisedStartMessage());

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            while (!cancellationToken.IsCancellationRequested)
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
                        if (await updateable.OnUpdateAsync(secondsSinceLastRun, cancellationToken).ConfigureAwait(false))
                        {
                            _onUpdateLastRun[updateableName] = timeUtil.GetTimeStamp();
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception err)
                    {
                        LogUpdateException(err, updateable);
                    }
                }

                await timer.WaitForNextTickAsync(cancellationToken);
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
