using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;
using SPTarkov.Server.Logger;
using SPTarkov.Server.Modding;
using SPTarkov.Server.Services;

namespace SPTarkov.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Some users don't know how to create a shortcut...
        if (!IsRunFromInstallationFolder())
        {
            Console.WriteLine(
                "You have not created a shortcut properly. Please hold alt when dragging to create a shortcut."
            );
            await Task.Delay(-1);
            return;
        }

        // Initialize the program variables
        ProgramStatics.Initialize();

        // Create web builder and logger
        var builder = CreateNewHostBuilder(args);

        var diHandler = new DependencyInjectionHandler(builder.Services);
        // register SPT components
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(Program));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(App));

        List<SptMod> loadedMods = [];
        if (ProgramStatics.MODS())
        {
            // Search for mod dlls
            loadedMods = ModDllLoader.LoadAllMods();
            // validate and sort mods, this will also discard any mods that are invalid
            var sortedLoadedMods = ValidateMods(loadedMods);

            // update the loadedMods list with our validated sorted mods
            loadedMods = sortedLoadedMods;

            diHandler.AddInjectableTypesFromAssemblies(
                sortedLoadedMods.SelectMany(a => a.Assemblies)
            );
        }
        diHandler.InjectAll();

        builder.Services.AddSingleton(builder);
        builder.Services.AddSingleton<IReadOnlyList<SptMod>>(loadedMods);
        builder.Services.AddHostedService<SptServerBackgroundService>();
        // Configure Kestrel options
        ConfigureKestrel(builder);

        var app = builder.Build();

        // Configure Kestrel WS options and Handle fallback requests
        ConfigureWebApp(app);

        // In case of exceptions we snatch a Server logger
        var serverExceptionLogger = app
            .Services.GetService<ILoggerFactory>()!
            .CreateLogger("Server");
        // We need any logger instance to use as a finalizer when the app closes
        var loggerFinalizer = app.Services.GetService<ISptLogger<App>>()!;
        try
        {
            SetConsoleOutputMode();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            serverExceptionLogger.LogCritical(ex, "Critical exception, stopping server...");
        }
        finally
        {
            loggerFinalizer.DumpAndStop();
        }
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
        app.Use(
            async (HttpContext context, RequestDelegate _) =>
            {
                await context.RequestServices.GetService<HttpServer>()!.HandleRequest(context);
            }
        );
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(
            (_, options) =>
            {
                // This method is not expected to be async so we need to wait for the Task instead of using await keyword
                options.ApplicationServices.GetService<OnWebAppBuildModLoader>()!.OnLoad().Wait();
                var httpConfig = options.ApplicationServices.GetService<ConfigServer>()?.GetConfig<HttpConfig>()!;
                var certHelper = options.ApplicationServices.GetService<CertificateHelper>()!;
                options.Listen(
                    IPAddress.Parse(httpConfig.Ip),
                    httpConfig.Port,
                    listenOptions =>
                    {
                        listenOptions.UseHttps(opts =>
                        {
                            opts.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                            opts.ServerCertificate = certHelper.LoadOrGenerateCertificatePfx();
                            opts.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                        });
                    }
                );
            }
        );
    }

    private static WebApplicationBuilder CreateNewHostBuilder(string[]? args = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Host.UseSptLogger();

        return builder;
    }

    private static List<SptMod> ValidateMods(List<SptMod> mods)
    {
        if (!ProgramStatics.MODS())
        {
            return [];
        }

        // We need the SPT dependencies for the ModValidator, but mods are loaded before the web application
        // So we create a disposable web application that we will throw away after getting the mods to load
        var builder = CreateNewHostBuilder();
        // register SPT components
        var diHandler = new DependencyInjectionHandler(builder.Services);
        diHandler.AddInjectableTypesFromAssembly(typeof(Program).Assembly);
        diHandler.AddInjectableTypesFromAssembly(typeof(App).Assembly);
        diHandler.InjectAll();
        // register the mod validator components
        var provider = builder
            .Services.AddScoped(typeof(ISptLogger<ModValidator>), typeof(SptLogger<ModValidator>))
            .AddScoped(typeof(ISemVer), typeof(SemanticVersioningSemVer))
            .AddSingleton<ModValidator>()
            .AddSingleton<ModLoadOrder>()
            .BuildServiceProvider();
        var modValidator = provider.GetRequiredService<ModValidator>();
        return modValidator.ValidateAndSort(mods);
    }

    private static void SetConsoleOutputMode()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        const int stdOutputHandle = -11;
        const uint enableVirtualTerminalProcessing = 0x0004;

        var handle = GetStdHandle(stdOutputHandle);

        if (!GetConsoleMode(handle, out var consoleMode))
        {
            throw new Exception("Unable to get console mode");
        }

        consoleMode |= enableVirtualTerminalProcessing;

        if (!SetConsoleMode(handle, consoleMode))
        {
            throw new Exception("Unable to set console mode");
        }
    }

    private static bool IsRunFromInstallationFolder()
    {
        var dirFiles = Directory.GetFiles(Directory.GetCurrentDirectory());

        // This file is guaranteed to exist if ran from the correct location, even if the game does not exist here.
        return dirFiles.Any(dirFile =>
            dirFile.EndsWith("sptLogger.json") || dirFile.EndsWith("sptLogger.Development.json")
        );
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}
