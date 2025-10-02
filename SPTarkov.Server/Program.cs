using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.Reflection.Patching;
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
using SPTarkov.Server.Web;

namespace SPTarkov.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Some users don't know how to create a shortcut...
        if (!IsRunFromInstallationFolder())
        {
            Console.WriteLine("You have not created a shortcut properly. Please hold alt when dragging to create a shortcut.");
            await Task.Delay(-1);
            return;
        }

        // Initialize the program variables
        ProgramStatics.Initialize();

        // Create web builder and logger
        var builder = CreateNewHostBuilder();

        var diHandler = new DependencyInjectionHandler(builder.Services);
        // register SPT components
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(Program));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(App));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(PatchManager));

        List<SptMod> loadedMods = [];
        if (ProgramStatics.MODS())
        {
            // Search for mod dlls
            loadedMods = ModDllLoader.LoadAllMods();
            // validate and sort mods, this will also discard any mods that are invalid
            var validatedLoadedMods = ValidateMods(loadedMods);

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

        // In case of exceptions we snatch a Server logger
        var serverExceptionLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Server");
        // We need any logger instance to use as a finalizer when the app closes
        var loggerFinalizer = app.Services.GetRequiredService<ISptLogger<App>>();
        try
        {
            // Handle edge cases where reverse proxies might pass X-Forwarded-For, use this as the actual IP address
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                ForwardLimit = null,
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            SetConsoleOutputMode();

            await app.Services.GetRequiredService<SptServerStartupService>().Startup();

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

        app.UseMiddleware<SptLoggerMiddleware>();

        app.Use(
            async (HttpContext context, RequestDelegate next) =>
            {
                await context.RequestServices.GetRequiredService<HttpServer>().HandleRequest(context, next);
            }
        );

        app.UseSptBlazor();
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(
            (_, options) =>
            {
                // This method is not expected to be async so we need to wait for the Task instead of using await keyword
                options.ApplicationServices.GetRequiredService<OnWebAppBuildModLoader>().OnLoad().Wait();
                var httpConfig = options.ApplicationServices.GetRequiredService<ConfigServer>().GetConfig<HttpConfig>();

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
                            opts.ServerCertificate = certHelper.LoadOrGenerateCertificatePfx();
                            opts.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                        });
                    }
                );
            }
        );
    }

    private static WebApplicationBuilder CreateNewHostBuilder()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = "./SPT_Data/wwwroot" });
        builder.Logging.ClearProviders();
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Host.UseSptLogger();

        return builder;
    }

    private static List<SptMod> ValidateMods(IEnumerable<SptMod> mods)
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
            .BuildServiceProvider();
        var modValidator = provider.GetRequiredService<ModValidator>();
        return modValidator.ValidateMods(mods);
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
        return dirFiles.Any(dirFile => dirFile.EndsWith("sptLogger.json") || dirFile.EndsWith("sptLogger.Development.json"));
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}
