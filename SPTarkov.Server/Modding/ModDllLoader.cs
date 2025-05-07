using System.Reflection;
using System.Runtime.Loader;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Modding;

public class ModDllLoader
{
    private const string ModPath = "./user/mods/";

    public static List<SptMod> LoadAllMods()
    {
        if (!Directory.Exists(ModPath))
        {
            Directory.CreateDirectory(ModPath);
        }

        var mods = new List<SptMod>();

        if (!ProgramStatics.MODS())
        {
            return mods;
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
                mods.Add(LoadMod(modDirectory));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        return mods;
    }

    /// <summary>
    ///     Check the provided directory path for a dll, load into memory
    /// </summary>
    /// <param name="path">Directory path that contains mod files</param>
    /// <returns>SptMod</returns>
    private static SptMod LoadMod(string path)
    {
        var result = new SptMod
        {
            Directory = path,
            Assemblies = []
        };
        var assemblyCount = 0;
        foreach (var file in new DirectoryInfo(path).GetFiles()) // Only search top level
        {
            if (string.Equals(file.Extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                assemblyCount++;
                result.Assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(file.FullName)));
            }
        }

        if (assemblyCount == 0)
        {
            throw new Exception($"No Assemblies found in path: {Path.GetFullPath(path)}");
        }

        result.ModMetadata = LoadModMetadata(result.Assemblies, path);

        if (result.ModMetadata == null)
        {
            throw new Exception($"Failed to load mod metadata for: {Path.GetFullPath(path)} \ndid you override `AbstractModMetadata`?");
        }

        if (result.ModMetadata?.Name == null || result.ModMetadata?.Author == null ||
            result.ModMetadata?.Version == null || result.ModMetadata?.Licence == null ||
            result.ModMetadata?.SptVersion == null)
        {
            throw new Exception($"The mod metadata for: {Path.GetFullPath(path)} is missing one of these properties: name, author, licence, version or sptVersion");
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
    private static AbstractModMetadata? LoadModMetadata(List<Assembly> assemblies, string path)
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
                    result = (AbstractModMetadata)Activator.CreateInstance(modMetadata)!;
                }
            }
        }

        return result;
    }
}
