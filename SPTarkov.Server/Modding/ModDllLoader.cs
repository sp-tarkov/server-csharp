using System.Runtime.Loader;
using System.Text.Json;
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
        // should contain a dll and Package.json
        // load both
        // if either is missing Throw Warning and skip

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
    ///     Check the provided directory path for a dll and package.json file, load into memory
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
        var packageJsonCount = 0;
        foreach (var file in new DirectoryInfo(path).GetFiles()) // Only search top level
        {
            if (string.Equals(file.Name, "package.json", StringComparison.OrdinalIgnoreCase))
            {
                packageJsonCount++;

                // Handle package.json
                var rawJson = File.ReadAllText(file.FullName);
                result.PackageJson = JsonSerializer.Deserialize<PackageJsonData>(rawJson);
                if (packageJsonCount > 1)
                {
                    throw new Exception($"More than one package.json file found in path: {path}");
                }

                continue;
            }

            if (string.Equals(file.Extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                assemblyCount++;
                result.Assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(file.FullName)));
            }
        }

        if (assemblyCount == 0 && packageJsonCount == 0)
        {
            throw new Exception($"No Assembly or package.json found in: {Path.GetFullPath(path)}");
        }

        if (packageJsonCount == 0)
        {
            throw new Exception($"No package.json found in path: {Path.GetFullPath(path)}");
        }

        if (assemblyCount == 0)
        {
            throw new Exception($"No Assemblies found in path: {Path.GetFullPath(path)}");
        }

        if (result.PackageJson?.Name == null || result.PackageJson?.Author == null ||
            result.PackageJson?.Version == null || result.PackageJson?.Licence == null ||
            result.PackageJson?.SptVersion == null)
        {
            throw new Exception($"The package.json file for: {path} is missing one of these properties: name, author, licence, version or sptVersion");
        }

        return result;
    }
}
