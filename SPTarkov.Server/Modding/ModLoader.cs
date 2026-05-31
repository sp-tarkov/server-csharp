using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services.Hosted;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Helpers;

namespace SPTarkov.Server.Modding;

public sealed class ModLoader(ISptLogger<ModLoader> logger, ModValidator modValidator, JsonUtil jsonUtil, FileUtil fileUtil)
{
    private List<SptMod> _loadedMods = [];
    private readonly List<AbstractPrepatch> _prepatches = [];

    private ModuleDefinition? _serverCoreModule;
    private MemoryStream? _serverCoreModuleStream;
    private readonly List<PrepatchResultEntry> _prepatchResults = [];

    private const string ModPath = "./user/mods/";
    private const string PatcherPath = "./user/patchers/";
    private const string PatchedAssemblyName = "./SPTarkov.Server.Core.Patched.dll";
    private const string PrepatchStagePath = "./user/cache/prepatcher/server";
    private const string PrepatchedArg = "--prepatched";

    /// <summary>
    ///     Initializes the mod loader container, and runs the entire mod loader process
    /// </summary>
    /// <returns>Active runtime mods</returns>
    public async Task<ModLoaderRunResult> RunModLoader(SptEarlyLoggerFactory loggerFactory, string[] args)
    {
        // Clean the console a bit
        var isPrepatchedProcess = args.Contains(PrepatchedArg, StringComparer.OrdinalIgnoreCase);
        if (isPrepatchedProcess)
        {
            ClearConsole();
            await LogPrepatches();
        }

        await LoadMods();
        var loadedMods = modValidator.ValidateMods(_loadedMods);

        if (!isPrepatchedProcess && _prepatches.Count > 0)
        {
            if (await ApplyPrepatches(loadedMods))
            {
                await StartPrepatchedServerProcess(loggerFactory, args);
                return new ModLoaderRunResult(false, loadedMods);
            }
        }

        return new ModLoaderRunResult(true, loadedMods);
    }

    private async Task LoadMods()
    {
        if (!await TryLoadServerCoreBytes())
        {
            return;
        }

        if (!Directory.Exists(ModPath))
        {
            Directory.CreateDirectory(ModPath);
        }

        // Delete the old patched assembly
        if (File.Exists(PatchedAssemblyName))
        {
            File.Delete(PatchedAssemblyName);
        }

        // foreach directory in /user/mods/
        // treat this as the MOD
        // should contain a dll
        // if dll is missing Throw Warning and skip
        var modDirectories = Directory.GetDirectories(ModPath);

        // Load mods found in dir
        foreach (var modDirectory in modDirectories)
        {
            try
            {
                _loadedMods.Add(LoadMod(modDirectory));
            }
            catch (Exception e)
            {
                logger.Critical($"Exception occured while loading a mod at path: {modDirectory}", e);
            }
        }

        _loadedMods = _loadedMods.OrderBy(m => m.ModMetadata.ModGuid, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private async Task LogPrepatches()
    {
        var text = await fileUtil.ReadFileAsync(Path.Combine(Path.GetFullPath(PrepatchStagePath), "prepatch-result.json"));
        var results = jsonUtil.Deserialize<List<PrepatchResultEntry>>(text);

        logger.Info($"ModLoader: Applied {results?.Count ?? 0} prepatches");
        foreach (var result in results ?? [])
        {
            logger.Info($"ModLoader: Applied prepatch from mod: {result.ModGuid}");
        }
    }

    /// <summary>
    ///     Applies all prepatches to the server core
    /// </summary>
    /// <param name="validRuntimeMods">Validated runtime mods</param>
    /// <returns>True if all patches succeeded</returns>
    private async Task<bool> ApplyPrepatches(IReadOnlyCollection<SptMod> validRuntimeMods)
    {
        if (_serverCoreModule is null)
        {
            throw new ModLoaderException("Server core module was not loaded, unable to apply prepatches.");
        }

        var validModGuids = validRuntimeMods.Select(mod => mod.ModMetadata.ModGuid).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activePrepatches = _prepatches
            .Where(prepatch => prepatch.IsActive && validModGuids.Contains(prepatch.ModGuid))
            .OrderBy(prepatch => prepatch.ModGuid, StringComparer.OrdinalIgnoreCase)
            .ThenBy(prepatch => prepatch.GetType().FullName, StringComparer.OrdinalIgnoreCase);

        foreach (var prepatch in activePrepatches)
        {
            logger.Info($"Applying prepatch: {prepatch.GetType().FullName}");
            var succeeded = false;
            try
            {
                prepatch.Patch();
                succeeded = true;
            }
            catch (Exception e)
            {
                logger.Critical($"Critical error occured while applying a prepatch from mod: {prepatch.ModGuid}", e);
            }

            var result = new PrepatchResultEntry { ModGuid = prepatch.ModGuid, Succeeded = succeeded };
            _prepatchResults.Add(result);
        }

        try
        {
            _serverCoreModule.Write(PatchedAssemblyName);
        }
        finally
        {
            _serverCoreModule.Dispose();
            _serverCoreModule = null;
            await _serverCoreModuleStream!.DisposeAsync();
            _serverCoreModuleStream = null;
        }

        return _prepatchResults.All(r => r.Succeeded);
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
    ///     Starts the patched server as a new process. This one is destroyed in release and held open in debug so IDE's don't die.
    /// </summary>
    private async Task StartPrepatchedServerProcess(SptEarlyLoggerFactory loggerFactory, string[] args)
    {
        var sourceDirectory = AppContext.BaseDirectory;
        var stageDirectory = Path.GetFullPath(PrepatchStagePath);

        CopyApplicationToCache(sourceDirectory, stageDirectory);
        await WriteResultLog();

        File.Copy(Path.GetFullPath(PatchedAssemblyName), Path.Combine(stageDirectory, "SPTarkov.Server.Core.dll"), overwrite: true);

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
        // Dispose of logging, we dont need to keep it alive anymore for this program
        loggerFactory.Provider.Dispose();
        loggerFactory.Dispose();
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

    private async Task WriteResultLog()
    {
        var text = jsonUtil.Serialize(_prepatchResults);
        await fileUtil.WriteFileAsync(Path.Combine(Path.GetFullPath(PrepatchStagePath), "prepatch-result.json"), text);
    }

    /// <summary>
    ///     Check the provided directory path for a dll, load into memory
    /// </summary>
    /// <param name="path">Directory path that contains mod files</param>
    /// <returns>SptMod</returns>
    private SptMod LoadMod(string path)
    {
        List<Assembly> assemblyList = [];
        foreach (var file in new DirectoryInfo(path).GetFiles()) // Only search top level
        {
            if (string.Equals(file.Extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                assemblyList.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(file.FullName)));
            }
        }

        if (assemblyList.Count == 0)
        {
            throw new ModLoaderException($"No Assemblies found in path: {Path.GetFullPath(path)}");
        }

        SptMod result = new()
        {
            Directory = path,
            Assemblies = assemblyList,
            ModMetadata = LoadModMetadata(assemblyList, path),
        };

        if (
            string.IsNullOrEmpty(result.ModMetadata.ModGuid)
            || string.IsNullOrEmpty(result.ModMetadata.Name)
            || string.IsNullOrEmpty(result.ModMetadata.Author)
            || string.IsNullOrEmpty(result.ModMetadata.License)
        )
        {
            throw new ModLoaderException(
                $"The mod metadata for: {Path.GetFullPath(path)} is missing one of these properties: ModGuid, Name, Author, or License"
            );
        }

        if (result.ModMetadata.HasPrepatcher)
        {
            LoadModPatchers(result, Path.Combine(PatcherPath, result.ModMetadata.ModGuid));
        }

        return result;
    }

    /// <summary>
    /// Finds and returns the mod metadata for this mod
    /// </summary>
    /// <param name="mod">mod</param>
    /// <param name="assemblies">All mod assemblies</param>
    /// <param name="path">Path of the mod directory</param>
    /// <returns>Mod metadata</returns>
    /// <exception cref="ModLoaderException">Thrown if duplicate metadata implementations are found</exception>
    private AbstractModMetadata LoadModMetadata(IEnumerable<Assembly> assemblies, string path)
    {
        AbstractModMetadata? result = null;

        foreach (var allAsmModules in assemblies.Select(a => a.Modules))
        {
            foreach (var module in allAsmModules)
            {
                var modMetadata = module.GetTypes().SingleOrDefault(t => typeof(AbstractModMetadata).IsAssignableFrom(t));

                if (result != null && modMetadata != null)
                {
                    throw new ModLoaderException($"Duplicate mod metadata found for mod at path: {Path.GetFullPath(path)}");
                }

                if (modMetadata != null)
                {
                    try
                    {
                        result = (AbstractModMetadata)Activator.CreateInstance(modMetadata)!;
                    }
                    catch (Exception ex)
                    {
                        throw new ModLoaderException($"Failed to load mod metadata for: {Path.GetFullPath(path)} \n{ex}");
                    }
                }
            }
        }

        if (result == null)
        {
            throw new ModLoaderException(
                $"Failed to load mod metadata for: {Path.GetFullPath(path)} \ndid you override `AbstractModMetadata`?"
            );
        }

        return result;
    }

    private void LoadModPatchers(SptMod mod, string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ModLoaderException(
                $"Failed to locate patcher directory for mod: `{mod.ModMetadata.ModGuid}`. Expected directory: `{Path.GetFullPath(path)}`"
            );
        }

        var patcherPath = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (patcherPath is null)
        {
            throw new ModLoaderException(
                $"Failed to locate a patcher for mod: `{mod.ModMetadata.ModGuid}`. If you did not intend to ship a patcher. Disable `HasPatcher` in AbstractModMetadata."
            );
        }

        mod.PatcherAssembly = Assembly.LoadFrom(patcherPath);

        var prepatchTypes = mod
            .PatcherAssembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(AbstractPrepatch).IsAssignableFrom(type));
        if (!prepatchTypes.Any())
        {
            throw new ModLoaderException($"Patcher at path: `{patcherPath}` has no patcher entry point(s) of type `AbstractPrepatch`");
        }

        foreach (var prepatchType in prepatchTypes)
        {
            _prepatches.Add((AbstractPrepatch)Activator.CreateInstance(prepatchType, args: [_serverCoreModule])!);
        }
    }

    private async Task<bool> TryLoadServerCoreBytes()
    {
        try
        {
            var serverCorePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "SPTarkov.Server.Core.dll");

            // Don't dispose the stream, keep it open, it will cause cecil to have a stroke if it's disposed of
            _serverCoreModuleStream = new MemoryStream(await File.ReadAllBytesAsync(serverCorePath), writable: false);
            _serverCoreModule = ModuleDefinition.ReadModule(
                _serverCoreModuleStream,
                new ReaderParameters { ReadingMode = ReadingMode.Immediate, InMemory = true }
            );
        }
        catch (Exception e)
        {
            logger.Critical("Critical error occured while loading the server core dll for the pre-patcher: ", e);
            return false;
        }

        return true;
    }
}

public sealed record ModLoaderRunResult(bool ShouldStartServer, List<SptMod> ValidRuntimeMods);
