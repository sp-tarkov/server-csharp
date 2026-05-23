using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Hosted;
using SPTarkov.Server.Core.Utils;
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
            loggerFactory?.Provider.Dispose();
        }
    }

    public static async Task StartServer(SptEarlyLoggerFactory loggerFactory, string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var configuration = await ConfigLoader.Initialize(_earlyLogger!);

        // Create web builder and logger
        var builder = CreateNewHostBuilder(loggerFactory, configuration);
        builder.Host.UseSptLoggerWithoutProvider(loggerFactory.ServiceProvider);

#if DEBUG
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });
#endif
        var diHandler = new DependencyInjectionHandler(builder.Services);
        // register SPT components
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(Program));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTStartupHostedService));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(PatchManager));

        List<SptMod> loadedMods = [];
        if (ProgramStatics.MODS())
        {
            // Search for mod dlls
            loadedMods = ModDllLoader.LoadAllMods();
            // validate and sort mods, this will also discard any mods that are invalid
            var validatedLoadedMods = ValidateMods(loggerFactory, loadedMods, configuration);

            // update the loadedMods list with our validated mods
            loadedMods = validatedLoadedMods;

            diHandler.AddInjectableTypesFromAssemblies(validatedLoadedMods.SelectMany(a => a.Assemblies));
        }
        diHandler.InjectAll();

        builder.InitializeSptBlazor(loadedMods);

        builder.Services.AddSingleton(builder);
        builder.Services.AddSingleton<IReadOnlyList<SptMod>>(loadedMods);
        // Configure Kestrel options
        ConfigureKestrel(builder);

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

        await app.RunAsync();
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

        app.Use(async (context, next) => await context.RequestServices.GetRequiredService<HttpServer>().HandleRequest(context, next));

        app.UseSptBlazor();
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(
            (_, options) =>
            {
                // This method is not expected to be async so we need to wait for the Task instead of using await keyword
                options.ApplicationServices.GetRequiredService<OnWebAppBuildModLoader>().OnLoad().Wait();
                var httpConfig = options.ApplicationServices.GetRequiredService<HttpConfig>();

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

                var certHelper = options.ApplicationServices.GetRequiredService<CertificateHelper>();
                options.Listen(
                    IPAddress.Parse(httpConfig.Ip),
                    httpConfig.Port,
                    listenOptions =>
                    {
                        listenOptions.UseHttps(opts =>
                        {
                            opts.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                            opts.ServerCertificate = certHelper.LoadOrGenerateCertificate();
                            opts.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                        });
                    }
                );
            }
        );
    }

    private static WebApplicationBuilder CreateNewHostBuilder(
        SptEarlyLoggerFactory earlyFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration
    )
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = "./SPT_Data/wwwroot" });
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(earlyFactory.Provider);
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

        foreach (var configEntry in configuration)
        {
            builder.Services.AddSingleton(configEntry.Key, configEntry.Value);
        }

        return builder;
    }

    private static List<SptMod> ValidateMods(
        SptEarlyLoggerFactory loggerFactory,
        IEnumerable<SptMod> mods,
        IReadOnlyDictionary<Type, BaseConfig> configuration
    )
    {
        if (!ProgramStatics.MODS())
        {
            return [];
        }

        // We need the SPT dependencies for the ModValidator, but mods are loaded before the web application
        // So we create a disposable web application that we will throw away after getting the mods to load
        var builder = CreateNewHostBuilder(loggerFactory, configuration);
        // register SPT components
        var diHandler = new DependencyInjectionHandler(builder.Services);
        diHandler.AddInjectableTypesFromAssembly(typeof(Program).Assembly);
        diHandler.AddInjectableTypesFromAssembly(typeof(SPTStartupHostedService).Assembly);
        diHandler.InjectAll();
        // register the mod validator components
        var provider = builder
            .Services.AddScoped<ISemVer, SemanticVersioningSemVer>()
            .AddSingleton<ModValidator>()
            .AddSptLoggerWithoutProvider(loggerFactory.ServiceProvider)
            .BuildServiceProvider();
        var modValidator = provider.GetRequiredService<ModValidator>();
        return modValidator.ValidateMods(mods);
    }

    private static bool IsRunFromInstallationFolder()
    {
        var dirFiles = Directory.GetFiles(Directory.GetCurrentDirectory());

        // This file is guaranteed to exist if ran from the correct location, even if the game does not exist here.
        return dirFiles.Any(dirFile => dirFile.EndsWith("sptLogger.json") || dirFile.EndsWith("sptLogger.Development.json"));
    }
}
