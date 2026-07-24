using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using AsmResolver.DotNet;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Exceptions;

namespace SPTarkov.Server.Modding;

public sealed class ModLoader(ISptLogger<ModLoader> logger, ModValidator modValidator)
{
    private List<SptMod> _loadedMods = [];
    private readonly List<EnumPrepatch> _enumPrepatches = [];

    private ModuleDefinition? _serverCoreModule;
    private byte[]? _serverCoreSymbols;
    private readonly List<PrepatchResultEntry> _prepatchResults = [];

    private const string ModPath = "./user/mods/";
    private const string PatcherPath = "./user/patchers/";
    private const string PatchedAssemblyName = "./SPTarkov.Server.Core.Patched.dll";

    /// <summary>
    ///     Initializes the mod loader container, and runs the entire mod loader process
    /// </summary>
    /// <returns>Active runtime mods</returns>
    public async Task<ModLoaderRunResult> RunModLoader(string[] args, CancellationToken cancellationToken = default)
    {
        // The hosted copy already runs inside the prepatch context, so it must not patch or host again.
        var isHostedPatchedProcess =
            AssemblyLoadContext.GetLoadContext(typeof(ModLoader).Assembly)?.Name == PrepatchLoadContext.ContextName;

        if (!isHostedPatchedProcess)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Load all prepatches without loading metadata, preventing a stale copy of the patched assembly
            await LoadPrepatchesForPrepatchPass();

            if (_enumPrepatches.Count > 0)
            {
                // Clean the console a bit
                ClearConsole();

                var patchedCore = await ApplyPrepatchesInMemory(cancellationToken: cancellationToken);
                if (patchedCore is not null)
                {
                    await BootPatchedServerInMemory(patchedCore, args);
                    return new ModLoaderRunResult(false, []);
                }
            }
        }

        await LoadMods(isHostedPatchedProcess, cancellationToken);
        var loadedMods = modValidator.ValidateMods(_loadedMods);

        return new ModLoaderRunResult(true, loadedMods);
    }

    private async Task LoadPrepatchesForPrepatchPass()
    {
        if (!await TryLoadServerCoreBytes())
        {
            return;
        }

        if (File.Exists(PatchedAssemblyName))
        {
            File.Delete(PatchedAssemblyName);
        }

        var patchedSymbolPath = Path.ChangeExtension(PatchedAssemblyName, ".pdb");

        if (File.Exists(patchedSymbolPath))
        {
            File.Delete(patchedSymbolPath);
        }

        if (!Directory.Exists(PatcherPath))
        {
            return;
        }

        foreach (var patcherDirectory in Directory.GetDirectories(PatcherPath))
        {
            LoadEnumPrepatch(Path.GetFileName(patcherDirectory), patcherDirectory);
        }
    }

    private async Task LoadMods(bool isPrepatchedProcess, CancellationToken cancellationToken = default)
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
            cancellationToken.ThrowIfCancellationRequested();

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
    private async Task<PatchedCoreAssembly?> ApplyPrepatchesInMemory(
        IReadOnlyCollection<SptMod>? validRuntimeMods = null,
        CancellationToken cancellationToken = default
    )
    {
        var success = RunEnumPrepatches(validRuntimeMods);

        try
        {
            if (!success)
            {
                return null;
            }

            var assemblyBytes = _serverCoreModule!.Write();
            var symbolBytes = _serverCoreSymbols;

            await File.WriteAllBytesAsync(PatchedAssemblyName, assemblyBytes, cancellationToken);

            // Write PDB, this is important when debugging
            if (symbolBytes is not null)
            {
                await File.WriteAllBytesAsync(Path.ChangeExtension(PatchedAssemblyName, ".pdb"), symbolBytes, cancellationToken);
            }

            return new PatchedCoreAssembly(assemblyBytes, symbolBytes);
        }
        finally
        {
            _serverCoreModule = null;
            _serverCoreSymbols = null;
        }
    }

    /// <summary>
    ///     Runs every valid enum prepatch against the loaded Core module in a deterministic order.
    /// </summary>
    /// <returns>True if all prepatches succeeded.</returns>
    private bool RunEnumPrepatches(IReadOnlyCollection<SptMod>? validRuntimeMods)
    {
        if (_serverCoreModule is null)
        {
            throw new ModLoaderException("Server core module was not loaded, unable to apply prepatches.");
        }

        var validModGuids = validRuntimeMods?.Select(mod => mod.ModMetadata.ModGuid).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activePrepatches = _enumPrepatches
            .Where(prepatch => validModGuids is null || validModGuids.Contains(prepatch.ModGuid))
            .OrderBy(prepatch => prepatch.ModGuid, StringComparer.OrdinalIgnoreCase)
            .ThenBy(prepatch => prepatch.DefinitionPath, StringComparer.OrdinalIgnoreCase);

        foreach (var prepatch in activePrepatches)
        {
            logger.Info($"Applying enum prepatch definitions: {prepatch.DefinitionPath}");
            var succeeded = false;
            try
            {
                EnumPatcher.Patch(_serverCoreModule, prepatch.Entries);
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
            LoadEnumPrepatch(result.ModMetadata.ModGuid, Path.Combine(PatcherPath, result.ModMetadata.ModGuid));
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
    private IModMetadata LoadModMetadata(IEnumerable<Assembly> assemblies, string path)
    {
        IModMetadata? result = null;

        foreach (var allAsmModules in assemblies.Select(a => a.Modules))
        {
            foreach (var module in allAsmModules)
            {
                var modMetadata = module.GetTypes().SingleOrDefault(t => typeof(IModMetadata).IsAssignableFrom(t));

                if (result != null && modMetadata != null)
                {
                    throw new ModLoaderException($"Duplicate mod metadata found for mod at path: {Path.GetFullPath(path)}");
                }

                if (modMetadata != null)
                {
                    try
                    {
                        result = (IModMetadata)Activator.CreateInstance(modMetadata)!;
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
            throw new ModLoaderException($"Failed to load mod metadata for: {Path.GetFullPath(path)} \ndid you implement `IModMetadata`?");
        }

        return result;
    }

    private void LoadEnumPrepatch(string modGuid, string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ModLoaderException(
                $"Failed to locate patcher directory for mod: `{modGuid}`. Expected directory: `{Path.GetFullPath(path)}`"
            );
        }

        var definitionFiles = Directory
            .EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly)
            .Where(file => string.Equals(Path.GetExtension(file), ".json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (definitionFiles.Length == 0)
        {
            throw new ModLoaderException(
                $"Failed to locate an enum prepatch JSON file for mod: `{modGuid}`. If you did not intend to ship a prepatcher, disable `HasPrepatcher` in your IModMetadata implementation."
            );
        }

        if (definitionFiles.Length > 1)
        {
            throw new ModLoaderException(
                $"Found multiple enum prepatch JSON files for mod: `{modGuid}`. Expected exactly one file in: `{Path.GetFullPath(path)}`"
            );
        }

        var definitionPath = definitionFiles[0];
        List<EnumEntryDefinition>? entries;

        try
        {
            using var definitionStream = File.OpenRead(definitionPath);
            entries = JsonSerializer.Deserialize<List<EnumEntryDefinition>>(definitionStream);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            throw new ModLoaderException($"Failed to load enum prepatch definitions from: `{Path.GetFullPath(definitionPath)}`", exception);
        }

        if (entries is null || entries.Count == 0)
        {
            throw new ModLoaderException($"Enum prepatch file contains no definitions: `{Path.GetFullPath(definitionPath)}`");
        }

        _enumPrepatches.Add(new EnumPrepatch(modGuid, Path.GetFullPath(definitionPath), entries));
    }

    private async Task<bool> TryLoadServerCoreBytes()
    {
        try
        {
            var serverCorePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "SPTarkov.Server.Core.dll");
            var symbolPath = Path.ChangeExtension(serverCorePath, ".pdb");

            _serverCoreModule = ModuleDefinition.FromBytes(await File.ReadAllBytesAsync(serverCorePath));

            // Existing metadata tokens are preserved, so the original PDB remains valid for the patched assembly.
            if (File.Exists(symbolPath))
            {
                _serverCoreSymbols = await File.ReadAllBytesAsync(symbolPath);
            }
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

internal sealed record EnumPrepatch(string ModGuid, string DefinitionPath, IReadOnlyList<EnumEntryDefinition> Entries);
