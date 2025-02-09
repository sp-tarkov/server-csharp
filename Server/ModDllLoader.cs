using System.Reflection;
using System.Text.Json;
using Core.Models.Spt.Mod;

namespace Server;

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

        ValidateModDependencies(mods);

        // Sort by mods LoadBefore/LoadAfter collections
        SortMods(mods);

        return mods;
    }

    /// <summary>
    /// Ensure all mods have their dependencies
    /// </summary>
    /// <param name="mods">Mods to check dependencies of</param>
    private static void ValidateModDependencies(List<SptMod> mods)
    {
        foreach (var sptMod in mods)
        {
            if (sptMod.PackageJson?.Dependencies?.Count > 0)
            {
                // Has deps, validate they exist
                foreach (var dependency in sptMod.PackageJson.Dependencies
                             .Where(dependency => !mods.Exists(x => x.PackageJson.Name.ToLower() == dependency.Key)))
                {
                    // TODO: also check version passes semver check
                    throw new Exception($"Mod: {sptMod.PackageJson.Name} is unable to load as it cannot find another mod it needs: {dependency.Key} version: {dependency.Value}");
                }
            }
        }
    }

    private static void SortMods(List<SptMod> mods)
    {
        // TODO: implement
        Console.WriteLine($"NOT IMPLEMENTED: SortMods");
    }

    /// <summary>
    /// Check the provided directory path for a dll and .json file, load into memory
    /// </summary>
    /// <param name="path">Directory path that contains mod files</param>
    /// <returns>SptMod</returns>
    private static SptMod LoadMod(string path)
    {
        var result = new SptMod();
        var asmCount = 0;
        var packCount = 0;
        foreach (var file in new DirectoryInfo(path).GetFiles()) // only search top level
        {
            if (file.Name.ToLower() == "package.json")
            {
                packCount++;

                // Handle package.json
                var rawJson = File.ReadAllText(file.FullName);
                result.PackageJson = JsonSerializer.Deserialize<PackageJsonData>(rawJson);
                if (packCount > 1)
                {
                    throw new Exception($"More than one package.json file found in path: {path}");
                }
            }

            if (file.Extension.ToLower() == ".dll")
            {
                asmCount++;

                result.Assembly = Assembly.LoadFile(Path.GetFullPath(file.FullName));
                if (asmCount > 1)
                {
                    throw new Exception($"More than one Assembly found in: {path}");
                }
            }
        }

        if (asmCount == 0 && packCount == 0)
        {
            throw new Exception($"No Assembly or package.json found in: {Path.GetFullPath(path)}");
        }

        if (packCount == 0)
        {
            throw new Exception($"No package.json found in path: {Path.GetFullPath(path)}");
        }

        if (asmCount == 0)
        {
            throw new Exception($"No Assemblies found in path: {Path.GetFullPath(path)}");
        }

        if (result.Assembly is not null && result.PackageJson is not null)
        {
            Console.WriteLine($"Loaded: {result.PackageJson.Name} Version: {result.PackageJson.Version} by: {result.PackageJson.Author}");
        }

        return result;
    }
}
