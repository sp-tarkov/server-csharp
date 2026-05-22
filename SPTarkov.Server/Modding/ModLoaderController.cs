using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Spt.Mod;

namespace SPTarkov.Server.Modding;

public class ModLoaderController(ISptLogger<ModLoaderController> logger, ModValidator modValidator)
{
    public const string PatchedAssemblyName = "./SPTarkov.Server.Core.Patched.dll";

    public List<SptMod> ValidRuntimeMods
    {
        get { return modValidator.ValidateMods(_loadedMods); }
    }

    public bool HasPatchers
    {
        get { return _prepatches.Count > 0; }
    }

    private List<SptMod> _loadedMods = [];
    private readonly List<AbstractPrepatch> _prepatches = [];

    private ModuleDefinition? _serverCoreModule;
    private MemoryStream? _serverCoreModuleStream;

    private const string ModPath = "./user/mods/";
    private const string PatcherPath = "./user/patchers/";

    public async Task LoadMods()
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

    public void ApplyPrepatches(IReadOnlyCollection<SptMod> validRuntimeMods)
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
            prepatch.Patch(_serverCoreModule);
        }

        try
        {
            _serverCoreModule.Write(PatchedAssemblyName);
        }
        finally
        {
            _serverCoreModule.Dispose();
            _serverCoreModule = null;
            _serverCoreModuleStream?.Dispose();
            _serverCoreModuleStream = null;
        }
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

        SptMod result = new() { Directory = path, Assemblies = assemblyList };
        LoadModMetadata(result, assemblyList, path);

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
    private void LoadModMetadata(SptMod mod, IEnumerable<Assembly> assemblies, string path)
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

        mod.ModMetadata = result;
        if (result.HasPatcher)
        {
            LoadModPatchers(mod, Path.Combine(PatcherPath, result.ModGuid));
        }
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
            _prepatches.Add((AbstractPrepatch)Activator.CreateInstance(prepatchType)!);
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
