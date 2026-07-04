using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Exceptions;

namespace SPTarkov.Server.Modding;

public sealed class ModLoader(ISptLogger<ModLoader> logger, ModValidator modValidator)
{
    private List<SptMod> _loadedMods = [];
    private readonly List<AbstractPrepatch> _prepatches = [];

    private ModuleDefinition? _serverCoreModule;
    private MemoryStream? _serverCoreModuleStream;
    private MemoryStream? _serverCoreSymbolStream;
    private bool _serverCoreHasSymbols;
    private readonly List<PrepatchResultEntry> _prepatchResults = [];

    private const string ModPath = "./user/mods/";
    private const string PatcherPath = "./user/patchers/";
    private const string PatchedAssemblyName = "./SPTarkov.Server.Core.Patched.dll";

    /// <summary>
    ///     Initializes the mod loader container, and runs the entire mod loader process
    /// </summary>
    /// <returns>Active runtime mods</returns>
    public async Task<ModLoaderRunResult> RunModLoader(string[] args)
    {
        // The hosted copy already runs inside the prepatch context, so it must not patch or host again.
        var isHostedPatchedProcess =
            AssemblyLoadContext.GetLoadContext(typeof(ModLoader).Assembly)?.Name == PrepatchLoadContext.ContextName;

        await LoadMods(isHostedPatchedProcess);
        var loadedMods = modValidator.ValidateMods(_loadedMods);

        if (!isHostedPatchedProcess && _prepatches.Count > 0)
        {
            // Clean the console a bit
            ClearConsole();

            var patchedCore = await ApplyPrepatchesInMemory(loadedMods);
            if (patchedCore is not null)
            {
                await BootPatchedServerInMemory(patchedCore, args);
                return new ModLoaderRunResult(false, loadedMods);
            }
        }

        return new ModLoaderRunResult(true, loadedMods);
    }

    private async Task LoadMods(bool isPrepatchedProcess)
    {
        if (!await TryLoadServerCoreBytes())
        {
            return;
        }

        if (!Directory.Exists(ModPath))
        {
            Directory.CreateDirectory(ModPath);
        }

        if (!isPrepatchedProcess)
        {
            if (File.Exists(PatchedAssemblyName))
            {
                File.Delete(PatchedAssemblyName);
            }

            var patchedSymbolPath = Path.ChangeExtension(PatchedAssemblyName, ".pdb");

            if (File.Exists(patchedSymbolPath))
            {
                File.Delete(patchedSymbolPath);
            }
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

    /// <summary>
    ///     Applies all prepatches, writes the patched Core (and its pdb) to disk
    /// </summary>
    /// <returns>Patched assembly and it's symbols, or null if any prepatch failed.</returns>
    private async Task<PatchedCoreAssembly?> ApplyPrepatchesInMemory(IReadOnlyCollection<SptMod> validRuntimeMods)
    {
        var success = RunActivePrepatches(validRuntimeMods);

        try
        {
            if (!success)
            {
                return null;
            }

            using var patchedStream = new MemoryStream();
            using var symbolStream = new MemoryStream();

            var writerParameters = new WriterParameters();
            if (_serverCoreHasSymbols)
            {
                writerParameters.WriteSymbols = true;
                writerParameters.SymbolWriterProvider = new PortablePdbWriterProvider();
                writerParameters.SymbolStream = symbolStream;
            }

            _serverCoreModule!.Write(patchedStream, writerParameters);

            var assemblyBytes = patchedStream.ToArray();
            var symbolBytes = _serverCoreHasSymbols ? symbolStream.ToArray() : null;

            await File.WriteAllBytesAsync(PatchedAssemblyName, assemblyBytes);

            // Write PDB, this is important when debugging
            if (symbolBytes is not null)
            {
                await File.WriteAllBytesAsync(Path.ChangeExtension(PatchedAssemblyName, ".pdb"), symbolBytes);
            }

            return new PatchedCoreAssembly(assemblyBytes, symbolBytes);
        }
        finally
        {
            DisposeServerCoreModule();
            _serverCoreModuleStream?.Dispose();
            _serverCoreModuleStream = null;
            _serverCoreSymbolStream?.Dispose();
            _serverCoreSymbolStream = null;
        }
    }

    /// <summary>
    ///     Runs every active, valid prepatch against the loaded Core module in a deterministic order.
    /// </summary>
    /// <returns>True if all prepatches succeeded.</returns>
    private bool RunActivePrepatches(IReadOnlyCollection<SptMod> validRuntimeMods)
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

            _prepatchResults.Add(new PrepatchResultEntry { ModGuid = prepatch.ModGuid, Succeeded = succeeded });
        }

        return _prepatchResults.All(r => r.Succeeded);
    }

    private void DisposeServerCoreModule()
    {
        _serverCoreModule?.Dispose();
        _serverCoreModule = null;
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
    ///     Hosts the server in-process with the patched Core in an isolated load context
    ///     The hosted copy detects the context and runs the normal startup without re-patching.
    /// </summary>
    private async Task BootPatchedServerInMemory(PatchedCoreAssembly patchedCore, string[] args)
    {
        var hostAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var context = new PrepatchLoadContext(hostAssemblyPath, patchedCore.Assembly, patchedCore.Symbols);
        var hostedServer = context.LoadFromAssemblyPath(hostAssemblyPath);

        var entryPoint =
            hostedServer.GetType("SPTarkov.Server.Program")?.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)
            ?? throw new ModLoaderException("Unable to locate the hosted server entry point for in-memory prepatching.");

        await (Task)entryPoint.Invoke(null, [args])!;
    }

    /// <summary>
    ///     Check the provided directory path for a dll, load into memory
    /// </summary>
    /// <param name="path">Directory path that contains mod files</param>
    /// <returns>SptMod</returns>
    private SptMod LoadMod(string path)
    {
        // Load mods into whichever context this loader runs in, so under in-memory prepatching they bind to the patched Core.
        var loadContext = AssemblyLoadContext.GetLoadContext(typeof(ModLoader).Assembly) ?? AssemblyLoadContext.Default;

        List<Assembly> assemblyList = [];
        foreach (var file in new DirectoryInfo(path).GetFiles()) // Only search top level
        {
            if (string.Equals(file.Extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                assemblyList.Add(loadContext.LoadFromAssemblyPath(Path.GetFullPath(file.FullName)));
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

        var patcherPath =
            Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault() ?? throw new ModLoaderException(
                $"Failed to locate a patcher for mod: `{mod.ModMetadata.ModGuid}`. If you did not intend to ship a patcher. Disable `HasPatcher` in AbstractModMetadata."
            );

        // Load into the loader's own context so the patcher's AbstractPrepatch matches ours
        var loadContext = AssemblyLoadContext.GetLoadContext(typeof(ModLoader).Assembly) ?? AssemblyLoadContext.Default;
        mod.PatcherAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath(patcherPath));

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
            var symbolPath = Path.ChangeExtension(serverCorePath, ".pdb");

            // Don't dispose the streams, keep them open, it will cause cecil to have a stroke if they're disposed of
            _serverCoreModuleStream = new MemoryStream(await File.ReadAllBytesAsync(serverCorePath), writable: false);

            var readerParameters = new ReaderParameters { ReadingMode = ReadingMode.Immediate, InMemory = true };

            // Read the symbols so the patched Core can emit a matching pdb and stay breakpointable
            if (File.Exists(symbolPath))
            {
                _serverCoreSymbolStream = new MemoryStream(await File.ReadAllBytesAsync(symbolPath), writable: false);
                readerParameters.ReadSymbols = true;
                readerParameters.SymbolReaderProvider = new PortablePdbReaderProvider();
                readerParameters.SymbolStream = _serverCoreSymbolStream;
                _serverCoreHasSymbols = true;
            }

            _serverCoreModule = ModuleDefinition.ReadModule(_serverCoreModuleStream, readerParameters);
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

public sealed record PatchedCoreAssembly(byte[] Assembly, byte[]? Symbols);
