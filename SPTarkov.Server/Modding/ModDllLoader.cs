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

        if (
            string.IsNullOrEmpty(result.ModMetadata.ModGuid)
            || string.IsNullOrEmpty(result.ModMetadata.Name)
            || string.IsNullOrEmpty(result.ModMetadata.Author)
            || string.IsNullOrEmpty(result.ModMetadata.Version)
            || string.IsNullOrEmpty(result.ModMetadata.License)
            || string.IsNullOrEmpty(result.ModMetadata.SptVersion)
        )
        {
            throw new Exception(
                $"The mod metadata for: {Path.GetFullPath(path)} is missing one of these properties: ModGuid, Name, Author, License, Version or SptVersion"
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
                    result = (AbstractModMetadata)Activator.CreateInstance(modMetadata)!;
                }
            }
        }

        if (result == null)
        {
            throw new Exception($"Failed to load mod metadata for: {Path.GetFullPath(path)} \ndid you override `AbstractModMetadata`?");
        }

        return result;
    }
}
