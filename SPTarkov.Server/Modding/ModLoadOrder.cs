using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Modding;

public class ModLoadOrder(ICloner cloner)
{
    protected Dictionary<string, PackageJsonData> loadOrder = new();
    protected Dictionary<string, PackageJsonData> mods = new();
    protected Dictionary<string, PackageJsonData> modsAvailable = new();

    public Dictionary<string, PackageJsonData> SetModList(Dictionary<string, PackageJsonData> mods)
    {
        this.mods = mods;
        modsAvailable = cloner.Clone(this.mods);
        loadOrder = new Dictionary<string, PackageJsonData>();

        var visited = new HashSet<string>();

        // invert loadBefore into loadAfter on specified mods
        foreach (var (modName, modConfig) in modsAvailable)
        {
            if ((modConfig.LoadBefore ?? []).Count > 0)
            {
                InvertLoadBefore(modName);
            }
        }

        foreach (var modName in modsAvailable.Keys)
        {
            GetLoadOrderRecursive(modName, visited);
        }

        return loadOrder;
    }

    public List<string> GetLoadOrder()
    {
        return [..loadOrder.Keys];
    }

    public HashSet<string> GetModsOnLoadBefore(string mod)
    {
        if (!mods.ContainsKey(mod))
        {
            throw new Exception($"The mod {mod} does not exist!");
        }

        var config = mods[mod];

        var loadBefore = new HashSet<string>(config.LoadBefore);

        foreach (var loadBeforeMod in loadBefore)
        {
            if (!mods.ContainsKey(loadBeforeMod))
            {
                loadBefore.Remove(loadBeforeMod);
            }
        }

        return loadBefore;
    }

    protected void InvertLoadBefore(string mod)
    {
        var loadBefore = GetModsOnLoadBefore(mod);

        foreach (var loadBeforeMod in loadBefore)
        {
            var loadBeforeModConfig = modsAvailable[loadBeforeMod];

            loadBeforeModConfig.LoadAfter ??= [];
            loadBeforeModConfig.LoadAfter.Add(mod);

            modsAvailable.Add(loadBeforeMod, loadBeforeModConfig);
        }
    }

    protected void GetLoadOrderRecursive(string mod, HashSet<string> visited)
    {
        // Validate package
        if (loadOrder.ContainsKey(mod))
        {
            return;
        }

        if (visited.Contains(mod))
        {
            // Additional info to help debug
            throw new Exception($"Cyclic dependency detected for mod {mod}!");
        }

        // Check dependencies
        if (!modsAvailable.ContainsKey(mod))
        {
            throw new Exception("modloader-error_parsing_mod_load_order");
        }

        var config = modsAvailable[mod];

        config.LoadAfter ??= [];
        config.ModDependencies ??= [];

        var dependencies = new HashSet<string>(config.ModDependencies.Keys);

        foreach (var modAfter in config.LoadAfter)
        {
            if (modsAvailable.ContainsKey(modAfter))
            {
                if (modsAvailable[modAfter]?.LoadAfter?.Contains(mod) ?? false)
                {
                    throw new Exception("modloader-load_order_conflict");
                }

                dependencies.Add(modAfter);
            }
        }

        visited.Add(mod);

        foreach (var nextMod in dependencies)
        {
            GetLoadOrderRecursive(nextMod, visited);
        }

        visited.Remove(mod);
        loadOrder.Add(mod, config);
    }
}
