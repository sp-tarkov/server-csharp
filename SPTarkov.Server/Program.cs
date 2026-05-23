using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
    private const string PrepatchedArg = "--prepatched";

    internal static ILogger? _earlyLogger;
    private static ModLoaderController? _modLoaderController;

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

        // Init mod loader
        ModLoaderController? modLoaderController = null;
        if (ProgramStatics.MODS())
        {
            modLoaderController = InitModLoader(loggerFactory, configuration);
        }

        // Clean the console a bit
        var isPrepatchedProcess = args.Contains(PrepatchedArg, StringComparer.OrdinalIgnoreCase);
        if (ProgramStatics.MODS() && isPrepatchedProcess && modLoaderController != null)
        {
            ClearConsole();
            await modLoaderController.LogPrepatches();
        }

        List<SptMod> loadedMods = [];
        if (ProgramStatics.MODS() && modLoaderController != null)
        {
            await modLoaderController.LoadMods();
            loadedMods = modLoaderController.ValidRuntimeMods;

            if (!isPrepatchedProcess && modLoaderController.HasPatchers && await modLoaderController.ApplyPrepatches(loadedMods))
            {
                await StartPrepatchedServerProcess(args, modLoaderController);
                return;
            }
        }

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
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(PatchManager));

        if (ProgramStatics.MODS() && modLoaderController != null)
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
        SptEarlyLoggerFactory loggerFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration
    )
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = "./SPT_Data/wwwroot" });
        builder.Logging.ClearProviders().AddProvider(loggerFactory.Provider);
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

        foreach (var configEntry in configuration)
        {
            builder.Services.AddSingleton(configEntry.Key, configEntry.Value);
        }

        return builder;
    }

    /// <summary>
    ///     Starts the patched server as a new process. This one is destroyed in release and held open in debug so IDE's don't die.
    /// </summary>
    private static async Task StartPrepatchedServerProcess(string[] args, ModLoaderController modLoaderController)
    {
        var sourceDirectory = AppContext.BaseDirectory;
        var stageDirectory = Path.GetFullPath(ModLoaderController.PrepatchStagePath);

        CopyApplicationToCache(sourceDirectory, stageDirectory);
        await modLoaderController.WriteResultLog();

        File.Copy(
            Path.GetFullPath(ModLoaderController.PatchedAssemblyName),
            Path.Combine(stageDirectory, "SPTarkov.Server.Core.dll"),
            overwrite: true
        );

        var startInfo = CreatePrepatchedProcessStartInfo(stageDirectory);

        foreach (var arg in args.Where(arg => !string.Equals(arg, PrepatchedArg, StringComparison.OrdinalIgnoreCase)))
        {
            startInfo.ArgumentList.Add(arg);
        }
        startInfo.ArgumentList.Add(PrepatchedArg);

        var prepatchedProcess = Process.Start(startInfo);
        if (prepatchedProcess is null)
        {
            throw new ModLoaderException($"Failed to start prepatched server process: {startInfo.FileName}");
        }

        // Needed for IDE development so the console doesn't just cease to exist when the process relaunches,
        // in a normal environment it just reattaches to the old console, but this behavior doesn't work in Rider/VS
#if DEBUG
        await prepatchedProcess.WaitForExitAsync();
        Environment.ExitCode = prepatchedProcess.ExitCode;
#endif
    }

    private static ProcessStartInfo CreatePrepatchedProcessStartInfo(string stageDirectory)
    {
        var processPath = Environment.ProcessPath;
        var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;

        if (IsDotnetHost(processPath) && !string.IsNullOrEmpty(entryAssemblyPath))
        {
            var stagedAssemblyPath = Path.Combine(stageDirectory, Path.GetFileName(entryAssemblyPath));
            var startInfo = CreateProcessStartInfo(processPath!);
            startInfo.ArgumentList.Add(stagedAssemblyPath);
            return startInfo;
        }

        var executableName = Path.GetFileName(processPath);
        if (string.IsNullOrEmpty(executableName))
        {
            executableName = OperatingSystem.IsWindows() ? "SPT.Server.exe" : "SPT.Server";
        }

        return CreateProcessStartInfo(Path.Combine(stageDirectory, executableName));
    }

    private static bool IsDotnetHost(string? processPath)
    {
        if (string.IsNullOrEmpty(processPath))
        {
            return false;
        }

        return string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase);
    }

    private static ProcessStartInfo CreateProcessStartInfo(string executablePath)
    {
        return new ProcessStartInfo(executablePath) { WorkingDirectory = Directory.GetCurrentDirectory(), UseShellExecute = false };
    }

    private static void ClearConsole()
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        Console.Clear();
    }

    /// <summary>
    ///     Copies the patched application to a cache
    /// </summary>
    /// <param name="sourceDirectory">Source dir</param>
    /// <param name="cacheDirectory"></param>
    private static void CopyApplicationToCache(string sourceDirectory, string cacheDirectory)
    {
        if (Directory.Exists(cacheDirectory))
        {
            Directory.Delete(cacheDirectory, recursive: true);
        }

        Directory.CreateDirectory(cacheDirectory);

        foreach (var file in Directory.GetFiles(sourceDirectory))
        {
            File.Copy(file, Path.Combine(cacheDirectory, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDirectory))
        {
            var directoryName = Path.GetFileName(directory);
            if (
                string.Equals(directoryName, "user", StringComparison.OrdinalIgnoreCase)
                || string.Equals(directoryName, "SPT_Data", StringComparison.OrdinalIgnoreCase)
            )
            {
                continue;
            }

            CopyDirectory(directory, Path.Combine(cacheDirectory, directoryName));
        }
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var file in Directory.GetFiles(sourceDirectory))
        {
            File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDirectory))
        {
            CopyDirectory(directory, Path.Combine(targetDirectory, Path.GetFileName(directory)));
        }
    }

    private static ModLoaderController InitModLoader(
        SptEarlyLoggerFactory loggerFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration
    )
    {
        // We need the SPT dependencies for the ModValidator, but mods are loaded before the web application
        // So we create a disposable web application that we will throw away after getting the mods to load
        var builder = CreateNewHostBuilder(loggerFactory, configuration);
        // register SPT components
        var diHandler = new DependencyInjectionHandler(builder.Services);
        diHandler.AddInjectableTypesFromAssembly(typeof(Program).Assembly);
        diHandler.AddInjectableTypesFromAssembly(typeof(SPTStartupHostedService).Assembly);
        diHandler.InjectAll();

        // register the mod loader components
        var provider = builder
            .Services.AddScoped<ISemVer, SemanticVersioningSemVer>()
            .AddSingleton<ModLoaderController>()
            .AddSingleton<ModValidator>()
            .AddSptLoggerWithoutProvider(loggerFactory.ServiceProvider)
            .BuildServiceProvider();

        return provider.GetRequiredService<ModLoaderController>();
    }

    private static bool IsRunFromInstallationFolder()
    {
        var dirFiles = Directory.GetFiles(Directory.GetCurrentDirectory());

        // This file is guaranteed to exist if ran from the correct location, even if the game does not exist here.
        return dirFiles.Any(dirFile => dirFile.EndsWith("sptLogger.json") || dirFile.EndsWith("sptLogger.Development.json"));
    }
}
