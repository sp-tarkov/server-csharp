using System.Reflection;

namespace Server;

public class ModDllLoader
{
    private const string ModPath = "./user/mods/";

    public static List<Assembly> LoadAllMods()
    {
        if (!Directory.Exists(ModPath))
            Directory.CreateDirectory(ModPath);
        var mods = new List<Assembly>();
        foreach (var file in Directory.GetFiles(ModPath, "*.dll", SearchOption.AllDirectories))
            try
            {
                mods.Add(Assembly.LoadFile(Path.GetFullPath(file)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        return mods;
    }
}
