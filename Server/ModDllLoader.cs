using System.Reflection;
using System.Text.Json;
using Core.Models.Spt.Mod;
using Core.Models.Utils;

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

    private static SptMod? LoadMod(string path)
    {
        var result = new SptMod();
        var asmCount = 0;
        var packCount = 0;
        foreach (var file in new DirectoryInfo(path).GetFiles()) // only search top level
        {
            if (file.Name.ToLower() == "package.json")
            {
                packCount++;

                // deal with package.json
                var jjson = File.ReadAllText(file.FullName);
                var json = JsonSerializer.Deserialize<PackageJsonData>(jjson);
                result.PackageJson = json;
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
                    throw new Exception($"More than one Assembly found in {path}");
                }
            }
        }

        if (asmCount == 0 && packCount == 0)
        {
            throw new Exception($"No Assembly or package.json found in {Path.GetFullPath(path)}");
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
            Console.WriteLine($"Loaded {result.PackageJson.Name} mod by {result.PackageJson.Author}");
        }

        return result;
    }
}
