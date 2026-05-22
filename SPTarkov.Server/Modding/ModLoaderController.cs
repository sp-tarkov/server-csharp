using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;

namespace SPTarkov.Server.Modding;

public class ModLoaderController(ISptLogger<ModLoaderController> logger, ModValidator modValidator)
{
    private const string ModPath = "./user/mods/";
    private const string PatcherPath = "./user/patchers/";

    public List<SptMod> ValidRuntimeMods
    {
        get { return modValidator.ValidateMods(_loadedMods); }
    }

    private List<SptMod> _loadedMods = [];
    private ModuleDefinition? _serverCoreModule;

    public void LoadMods()
    {
        if (!TryLoadServerCoreBytes())
        {
            return;
        }

        LoadAllPatchers();
        LoadAllMods();
    }

    // We need control and coupling between pre-patchers and the associated primary assembly,
    // handle that here and any state needed.
    // This will control the process of both patchers and the mods themselves

    private void LoadAllPatchers()
    {
        var files = Directory.GetFiles(PatcherPath, "*.dll");
        foreach (var file in files)
        {
            LoadPatcher(file);
        }
    }

    private void LoadPatcher(string path) { }

    private void LoadAllMods()
    {
        if (!Directory.Exists(ModPath))
        {
            Directory.CreateDirectory(ModPath);
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
                Console.WriteLine(e.Message);
            }
        }

        _loadedMods = _loadedMods.OrderBy(m => m.ModMetadata.ModGuid, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    ///     Check the provided directory path for a dll, load into memory
    /// </summary>
    /// <param name="path">Directory path that contains mod files</param>
    /// <returns>SptMod</returns>
    private static SptMod LoadMod(string path)
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
            throw new Exception($"No Assemblies found in path: {Path.GetFullPath(path)}");
        }

        SptMod result = new()
        {
            Directory = path,
            Assemblies = assemblyList,
            ModMetadata = LoadModMetadata(assemblyList, path),
        };

        if (result.ModMetadata.HasPatcher) { }

        if (
            string.IsNullOrEmpty(result.ModMetadata.ModGuid)
            || string.IsNullOrEmpty(result.ModMetadata.Name)
            || string.IsNullOrEmpty(result.ModMetadata.Author)
            || string.IsNullOrEmpty(result.ModMetadata.License)
        )
        {
            throw new Exception(
                $"The mod metadata for: {Path.GetFullPath(path)} is missing one of these properties: ModGuid, Name, Author, or License"
            );
        }

        return result;
    }

    /// <summary>
    /// Finds and returns the mod metadata for this mod
    /// </summary>
    /// <param name="assemblies">All mod assemblies</param>
    /// <param name="path">Path of the mod directory</param>
    /// <returns>Mod metadata</returns>
    /// <exception cref="Exception">Thrown if duplicate metadata implementations are found</exception>
    private static AbstractModMetadata LoadModMetadata(IEnumerable<Assembly> assemblies, string path)
    {
        AbstractModMetadata? result = null;

        foreach (var allAsmModules in assemblies.Select(a => a.Modules))
        {
            foreach (var module in allAsmModules)
            {
                var modMetadata = module.GetTypes().SingleOrDefault(t => typeof(AbstractModMetadata).IsAssignableFrom(t));

                if (result != null && modMetadata != null)
                {
                    throw new Exception($"Duplicate mod metadata found for mod at path: {Path.GetFullPath(path)}");
                }

                if (modMetadata != null)
                {
                    try
                    {
                        result = (AbstractModMetadata)Activator.CreateInstance(modMetadata)!;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to load mod metadata for: {Path.GetFullPath(path)} \n{ex}");
                    }
                }
            }
        }

        if (result == null)
        {
            throw new Exception($"Failed to load mod metadata for: {Path.GetFullPath(path)} \ndid you override `AbstractModMetadata`?");
        }

        return result;
    }

    private bool TryLoadServerCoreBytes()
    {
        try
        {
            using var fs = new FileStream(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "SPTarkov.Server.Core.dll"),
                FileMode.Open
            );

            _serverCoreModule = ModuleDefinition.ReadModule(fs);
        }
        catch (Exception e)
        {
            logger.Critical("Critical error occured while loading the server core dll for the pre-patcher: ", e);
            return false;
        }

        return true;
    }
}
