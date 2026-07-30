using System.Net;
using System.Net.Sockets;
using SPTarkov.DI;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Exceptions;

namespace SPTarkov.Server.Extensions;

public static class ProgramExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public async Task RunPreSptLoadCallbacks(ILogger logger, CancellationToken cancellationToken = default)
        {
            // This is necessary here so that mods can modify SPT configs pre-emptively before we startup the container
            // It will make HttpConfig modifiable for mods like Fika
            var injectableTypes = serviceProvider.GetRequiredService<IReadOnlyList<DependencyInjectionContainer>>();

            var preSptLoadTypes = injectableTypes
                .Where(container => container.Type == typeof(IOnLoad))
                .Where(container => container.InjectableAttribute.TypePriority >= OnLoadOrder.Watermark)
                .Where(container => container.InjectableAttribute.TypePriority < OnLoadOrder.GameCallbacks);

            if (logger.IsEnabled(LogLevel.Information))
            {
                var executingCallbacksLog = serviceProvider
                    .GetRequiredService<ServerLocalisationService>()
                    .GetText("executing_startup_callbacks");
                logger.LogInformation("{Message}", executingCallbacksLog);
            }

            foreach (var preSptLoadType in preSptLoadTypes)
            {
                var onLoadService = serviceProvider.GetRequiredService(preSptLoadType.ParentType);

                if (onLoadService is not IOnLoad onLoad)
                {
                    continue;
                }

                await onLoad.OnLoadAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    extension(HttpConfig httpConfig)
    {
        /// <summary>
        /// Verifies that the configured web server IP address and port are available for binding.
        /// </summary>
        /// <remarks>
        /// This opens a temporary TCP listener using the configured <see cref="HttpConfig.Ip"/>
        /// and <see cref="HttpConfig.Port"/> values.
        ///
        /// If the address is invalid, or if the address/port combination is already in use,
        /// this method will throw and server startup should be aborted.
        ///
        /// The listener is always stopped before this method returns.
        /// </remarks>
        public void VerifyWebServerPortAvailable()
        {
            TcpListener? listener = null;

            try
            {
                listener = new TcpListener(IPAddress.Parse(httpConfig.Ip), httpConfig.Port);
                listener.Start();
            }
            catch (SocketException ex)
            {
                throw new WebServerPortUnavailableException(httpConfig, ex);
            }
            finally
            {
                listener?.Stop();
            }
        }
    }
}
