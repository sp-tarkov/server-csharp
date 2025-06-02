using System.Runtime;
using System.Runtime.InteropServices;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;
using SPTarkov.Server.Logger;
using SPTarkov.Server.Modding;

namespace SPTarkov.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
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

            diHandler.AddInjectableTypesFromAssemblies(sortedLoadedMods.SelectMany(a => a.Assemblies));
        }
        diHandler.InjectAll();

        builder.Services.AddSingleton(builder);
        builder.Services.AddSingleton<IReadOnlyList<SptMod>>(loadedMods);
        var serviceProvider = builder.Services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Server");
        // Load bundles for bundle mods
        if (ProgramStatics.MODS())
        {
            var bundleLoader = serviceProvider.GetService<BundleLoader>();
            foreach (var mod in loadedMods)
            {
                if (mod.ModMetadata?.IsBundleMod == true)
                {
                    // Convert to relative path
                    string relativeModPath = Path.GetRelativePath(
                        Directory.GetCurrentDirectory(),
                        mod.Directory
                    ).Replace('\\', '/');

                    bundleLoader.AddBundles(relativeModPath);
                }
            }
        }
        try
        {
            SetConsoleOutputMode();

            // Get the Built app and run it
            var app = serviceProvider.GetService<App>();

            if (app != null)
            {
                await app.InitializeAsync();

                // Run garbage collection now the server is ready to start
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

                await app.StartAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogCritical(ex, "Critical exception, stopping server...");
        }
        finally
        {
            serviceProvider.GetService<SptLogger<object>>()?.DumpAndStop();
        }
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
        var provider = builder.Services
            .AddScoped(typeof(ISptLogger<ModValidator>), typeof(SptLogger<ModValidator>))
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

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}
