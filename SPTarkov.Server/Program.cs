using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Exceptions;
using SPTarkov.Server.Extensions;
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
        RegisterSatelliteLocalizations();

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

        using var startupCancellation = new StartupCancellation();

        try
        {
            _earlyLogger = loggerFactory.CreateLogger("SPTarkov.Server.Core");

            await StartServer(loggerFactory, args, startupCancellation);
        }
        catch (OperationCanceledException) when (startupCancellation.IsCancellationRequested)
        {
            if (_earlyLogger is not null && _earlyLogger.IsEnabled(LogLevel.Information))
            {
                _earlyLogger.LogInformation("Server startup was cancelled, shutting down.");
            }
        }
        catch (WebServerPortUnavailableException ex)
        {
            if (_earlyLogger is not null)
            {
                if (_earlyLogger.IsEnabled(LogLevel.Critical))
                {
                    _earlyLogger.LogCritical(
                        "Failed to start the web server on {Ip}:{Port}. Socket error: {SocketErrorCode} ({NativeErrorCode}).",
                        ex.Ip,
                        ex.Port,
                        ex.SocketErrorCode,
                        ex.NativeErrorCode
                    );
                }

                if (_earlyLogger.IsEnabled(LogLevel.Error))
                {
                    switch (ex.SocketErrorCode)
                    {
                        case SocketError.AddressAlreadyInUse:
                            _earlyLogger.LogError(
                                "Another SPT server may already be running, or another process is using port {Port}.",
                                ex.Port
                            );

                            _earlyLogger.LogError(
                                "Close the other process, or change the server port in your HTTP configuration, then restart SPT."
                            );

                            break;

                        case SocketError.AddressNotAvailable:
                            _earlyLogger.LogError("The configured IP address {Ip} is not available on this machine.", ex.Ip);

                            _earlyLogger.LogError("Check your HTTP configuration and use a IP address that exists on this system.");

                            break;

                        case SocketError.AccessDenied:
                            _earlyLogger.LogError("The server does not have permission to bind to {Ip}:{Port}.", ex.Ip, ex.Port);

                            _earlyLogger.LogError("Try running SPT as administrator, or use a different IP address and port.");

                            break;

                        default:
                            _earlyLogger.LogError("The web server could not bind to the configured endpoint {Ip}:{Port}.", ex.Ip, ex.Port);

                            break;
                    }
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
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
                "The server has unexpectedly stopped, reach out to the support channel in our Discord server. Include a screenshot of this message and the surrounding error(s) above and below"
            );
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
        finally
        {
            loggerFactory.Provider.Dispose();
        }
    }

    public static async Task StartServer(SptEarlyLoggerFactory loggerFactory, string[] args, StartupCancellation startupCancellation)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var configuration = await ConfigLoader.Initialize(_earlyLogger!, startupCancellation.Token);
        var earlyServiceProvider = ProgramHelpers.CreateEarlySptProvider(loggerFactory, configuration, ProgramStatics.MODS());

        List<SptMod> loadedMods = [];

        if (ProgramStatics.MODS())
        {
            var modLoader = earlyServiceProvider.GetRequiredService<ModLoader>();
            var runResult = await modLoader.RunModLoader(args, startupCancellation.Token);
            if (!runResult.ShouldStartServer)
            {
                return;
            }

            loadedMods = runResult.ValidRuntimeMods;
        }

        await StartServerAfterModLoading(loggerFactory, configuration, earlyServiceProvider, loadedMods, startupCancellation);
    }

    /// <summary>
    /// Split the loading logic, this method needs to stay seperated as otherwise things are forced into context too early which causes issues with mod loading pre-patching (In particular the SIC not loading)
    /// </summary>
    private static async Task StartServerAfterModLoading(
        SptEarlyLoggerFactory loggerFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration,
        IServiceProvider earlyServiceProvider,
        List<SptMod> loadedMods,
        StartupCancellation startupCancellation
    )
    {
        var cancellationToken = startupCancellation.Token;
        var dbImporter = earlyServiceProvider.GetRequiredService<DatabaseImporter>();

        var shouldVerify = !ProgramStatics.DEBUG();
        if (shouldVerify)
        {
            await dbImporter.LoadHashesAsync(cancellationToken);
        }

        var tables =
            await dbImporter.LoadDatabaseAsync(shouldVerify, cancellationToken)
            ?? throw new NullReferenceException("Failed to import database tables.");

        // Create web builder and logger
        var builder = ProgramHelpers.CreateNewHostBuilder(loggerFactory, configuration, tables);
        builder.Host.UseSptLoggerWithoutProvider(loggerFactory.ServiceProvider);

        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });
        await ProgramHelpers.RegisterSptServicesAsync(builder, loadedMods, ProgramStatics.MODS(), cancellationToken);

        // Configure Kestrel options
        ConfigureKestrel(builder);

        var app = builder.Build();

        // Link startup token to the host's lifetime
        startupCancellation.LinkTo(app.Services.GetRequiredService<IHostApplicationLifetime>());

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

        await app.Services.RunPreSptLoadCallbacks(_earlyLogger!, cancellationToken);

        var httpConfig = app.Services.GetRequiredService<HttpConfig>();

        cancellationToken.ThrowIfCancellationRequested();

        httpConfig.VerifyWebServerPortAvailable();

        await app.RunAsync($"https://{httpConfig.Ip}:{httpConfig.Port}");
    }

    private static void ConfigureWebApp(WebApplication app)
    {
        app.UseWebSockets();

        app.UseMiddleware<SptLoggerMiddleware>();

        // Docker health endpoint
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", version = ProgramStatics.SPT_VERSION().ToString() }))
            .AllowAnonymous();

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

    private static bool IsRunFromInstallationFolder()
    {
        var dirFiles = Directory.GetFiles(Directory.GetCurrentDirectory());

        // This file is guaranteed to exist if ran from the correct location, even if the game does not exist here.
        return dirFiles.Any(dirFile => dirFile.EndsWith("sptLogger.json") || dirFile.EndsWith("sptLogger.Development.json"));
    }

    /// <summary>
    /// This method makes sure that the satellite assemblies that other libraries create get moved out of the root directory to reduce clutter
    /// </summary>
    private static void RegisterSatelliteLocalizations()
    {
        AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
            ResolveSatelliteAssembly(AppContext.BaseDirectory, assemblyName, context.LoadFromAssemblyPath);
    }

    public static Assembly? ResolveSatelliteAssembly(string baseDirectory, AssemblyName assemblyName, Func<string, Assembly> loadFromPath)
    {
        if (
            assemblyName.Name is not { } name
            || !name.EndsWith(".resources", StringComparison.Ordinal)
            || assemblyName.CultureName is not { Length: > 0 } culture
        )
        {
            return null;
        }

        var path = Path.Combine(baseDirectory, "SPT_Data", "dotnet", culture, $"{name}.dll");
        return File.Exists(path) ? loadFromPath(path) : null;
    }
}
