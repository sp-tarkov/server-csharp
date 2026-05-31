using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using HarmonyLib.Tools;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger;
using SPTarkov.DI;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Hosted;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Helpers;
using SPTarkov.Server.Middleware;
using SPTarkov.Server.Modding;
using SPTarkov.Server.Web;

namespace SPTarkov.Server;

public static class Program
{
    internal static ILogger? _earlyLogger;

    public static async Task Main(string[] args)
    {
        // Initialize the program variables
        ProgramStatics.Initialize();

        var loggerFactory = SptLoggerProvider.Create(ProgramStatics.DEBUG());

        // Some users don't know how to create a shortcut...
        if (!IsRunFromInstallationFolder())
        {
            Console.WriteLine("You have not created a shortcut properly. Please hold alt when dragging to create a shortcut.");
            await Task.Delay(-1);
            return;
        }

        try
        {
            _earlyLogger = loggerFactory.CreateLogger("SPTarkov.Server.Core");

            await StartServer(loggerFactory, args);
        }
        catch (SocketException)
        {
            _earlyLogger!.LogCritical("You have multiple servers running or another process using port 6969");
            _earlyLogger!.LogInformation("Press any key to exit...");
            Console.ReadLine();
        }
        catch (Exception e)
        {
            if (
                e.Message.Contains(
                    "could not load file or assembly 'sptarkov.server.core, version=",
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                _earlyLogger!.LogCritical(
                    e,
                    "You may have installed a mod that needs a newer version of of SPT installed. Please try updating SPT"
                );

                Console.ReadLine();
                return;
            }

            if (e.Message.Contains("could not load file or assembly", StringComparison.InvariantCultureIgnoreCase))
            {
                _earlyLogger!.LogCritical(
                    e,
                    "You may have forgotten to install a requirement for one of your mods, please check the mod page again and install any requirements listed. Read the error message below CAREFULLY for the name of the mod you need to install"
                );

                Console.ReadLine();
                // Don't show below error message when it's a mod exception.
                return;
            }

            _earlyLogger!.LogCritical(
                e,
                "The server has unexpectedly stopped, reach out to #mod-questions-4-0 in our Discord server. Include a screenshot of this message and the surrounding error(s) above and below"
            );
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
        finally
        {
            loggerFactory.Provider.Dispose();
        }
    }

    public static async Task StartServer(SptEarlyLoggerFactory loggerFactory, string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var configuration = await ConfigLoader.Initialize(_earlyLogger!);
        var earlyServiceProvider = ProgramHelpers.CreateEarlySptProvider(loggerFactory, configuration);

        List<SptMod> loadedMods = [];
        var modLoader = earlyServiceProvider.GetRequiredService<ModLoader>();
        var runResult = await modLoader.RunModLoader(loggerFactory, args);
        if (!runResult.ShouldStartServer)
        {
            return;
        }

        loadedMods = runResult.ValidRuntimeMods;

        var cTSource = new CancellationTokenSource();
        var dbImporter = earlyServiceProvider.GetRequiredService<DatabaseImporter>();

        var shouldVerify = !ProgramStatics.DEBUG();
        if (shouldVerify)
        {
            await dbImporter.LoadHashesAsync(cTSource.Token);
        }

        var tables =
            await dbImporter.LoadDatabaseAsync(shouldVerify, cTSource.Token)
            ?? throw new NullReferenceException("Failed to import database tables.");

        // Create web builder and logger
        var builder = ProgramHelpers.CreateNewHostBuilder(loggerFactory, configuration, tables);
        builder.Host.UseSptLoggerWithoutProvider(loggerFactory.ServiceProvider);

        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });
        var diHandler = new DependencyInjectionHandler(builder.Services);

        // register SPT components
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(Program));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(PatchManager));

        if (ProgramStatics.MODS())
        {
            diHandler.AddInjectableTypesFromAssemblies(loadedMods.SelectMany(a => a.Assemblies));
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTStartupHostedService));
        }
        else
        {
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTStartupHostedService));
        }

        diHandler.InjectAll();

        builder.InitializeSptBlazor(loadedMods);

        builder.Services.AddSingleton(builder);
        builder.Services.AddSingleton<IReadOnlyList<SptMod>>(loadedMods);
        // Configure Kestrel options
        ConfigureKestrel(builder);

        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient(
            "Github",
            httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://api.github.com/");

                // These headers are _required_ by GitHub API
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("spt-csharp-server");
                httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            }
        );
        var app = builder.Build();

        // Configure Kestrel WS options and Handle fallback requests
        ConfigureWebApp(app);

        // Handle edge cases where reverse proxies might pass X-Forwarded-For, use this as the actual IP address
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = null,
        };
        forwardedHeadersOptions.KnownIPNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);

        await RunPreSptLoadCallbacks(app.Services);

        var httpConfig = app.Services.GetRequiredService<HttpConfig>();

        VerifyWebServerPortAvailable(httpConfig);

        await app.RunAsync($"https://{httpConfig.Ip}:{httpConfig.Port}");
    }

    private static void ConfigureWebApp(WebApplication app)
    {
        app.UseWebSockets(
            new WebSocketOptions
            {
                // Every minute a heartbeat is sent to keep the connection alive.
                KeepAliveInterval = TimeSpan.FromSeconds(60),
            }
        );

        app.UseMiddleware<SptLoggerMiddleware>();

        app.UseNoGCRegions();

        app.Use(
            async (context, next) =>
                await context.RequestServices.GetRequiredService<HttpServer>().HandleRequestAsync(context, next, context.RequestAborted)
        );

        app.UseSptBlazor();
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(
            (_, options) =>
            {
                var certHelper = options.ApplicationServices.GetRequiredService<CertificateHelper>();

                options.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                    httpsOptions.ServerCertificate = certHelper.LoadOrGenerateCertificate();
                    httpsOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                });
            }
        );
    }

    private static void VerifyWebServerPortAvailable(HttpConfig httpConfig)
    {
        // Probe the http ip and port to see if its being used, this method will throw an exception and crash
        // the server if the IP/Port combination is already in use
        TcpListener? listener = null;

        try
        {
            listener = new TcpListener(IPAddress.Parse(httpConfig.Ip), httpConfig.Port);
            listener.Start();
        }
        finally
        {
            listener?.Stop();
        }
    }

    private static async Task RunPreSptLoadCallbacks(IServiceProvider serviceProvider)
    {
        // This is necessary here so that mods can modify SPT configs pre-emptively before we startup the container
        // It will make HttpConfig modifiable for mods like Fika
        var injectableTypes = serviceProvider.GetRequiredService<IReadOnlyList<DependencyInjectionContainer>>();
        var cancellationToken = serviceProvider.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;

        var preSptLoadTypes = injectableTypes
            .Where(container => container.Type == typeof(IOnLoad))
            .Where(container => container.InjectableAttribute.TypePriority >= OnLoadOrder.Watermark)
            .Where(container => container.InjectableAttribute.TypePriority < OnLoadOrder.GameCallbacks)
            .OrderBy(container => container.InjectableAttribute.TypePriority);

        if (_earlyLogger!.IsEnabled(LogLevel.Information))
        {
            var executingCallbacksLog = serviceProvider
                .GetRequiredService<ServerLocalisationService>()
                .GetText("executing_startup_callbacks");
            _earlyLogger.LogInformation("{Message}", executingCallbacksLog);
        }

        foreach (var preSptLoadType in preSptLoadTypes)
        {
            var onLoadService = serviceProvider.GetRequiredService(preSptLoadType.ParentType);

            if (onLoadService is not IOnLoad onLoad)
            {
                continue;
            }

            await onLoad.OnLoad(cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsRunFromInstallationFolder()
    {
        var dirFiles = Directory.GetFiles(Directory.GetCurrentDirectory());

        // This file is guaranteed to exist if ran from the correct location, even if the game does not exist here.
        return dirFiles.Any(dirFile => dirFile.EndsWith("sptLogger.json") || dirFile.EndsWith("sptLogger.Development.json"));
    }
}
