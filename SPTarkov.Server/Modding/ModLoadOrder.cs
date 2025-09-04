using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Modding;

public class ModLoadOrder(ICloner cloner)
{
    protected readonly Dictionary<string, AbstractModMetadata> LoadOrder = new();
    protected Dictionary<string, AbstractModMetadata> Mods = new();
    protected Dictionary<string, AbstractModMetadata> ModsAvailable = new();

    public Dictionary<string, AbstractModMetadata> SetModList(Dictionary<string, AbstractModMetadata> mods)
    {
        this.Mods = mods;
        ModsAvailable = cloner.Clone(this.Mods);
        LoadOrder.Clear();

        var visited = new HashSet<string>();

        // invert loadBefore into loadAfter on specified mods
        foreach (var (modGuid, modConfig) in ModsAvailable)
        {
            if ((modConfig.LoadBefore ?? []).Count > 0)
            {
                InvertLoadBefore(modGuid);
            }
        }

        foreach (var modGuid in ModsAvailable.Keys)
        {
            GetLoadOrderRecursive(modGuid, visited);
        }

        return LoadOrder;
    }

    public List<string> GetLoadOrder()
    {
        return [.. LoadOrder.Keys];
    }

    public HashSet<string> GetModsOnLoadBefore(string modGuid)
    {
        if (!Mods.TryGetValue(modGuid, out var config))
        {
            throw new Exception($"The mod: {modGuid} does not exist!");
        }

        var loadBefore = new HashSet<string>(config.LoadBefore);
        foreach (var loadBeforeModGuid in loadBefore)
        {
            if (!Mods.ContainsKey(loadBeforeModGuid))
            {
                loadBefore.Remove(loadBeforeModGuid);
            }
        }

        return loadBefore;
    }

    protected void InvertLoadBefore(string modGuid)
    {
        var loadBefore = GetModsOnLoadBefore(modGuid);

        foreach (var loadBeforeMod in loadBefore)
        {
            var loadBeforeModConfig = ModsAvailable[loadBeforeMod];

            loadBeforeModConfig.LoadAfter ??= [];
            loadBeforeModConfig.LoadAfter.Add(modGuid);

            ModsAvailable.Add(loadBeforeMod, loadBeforeModConfig);
        }
    }

    protected void GetLoadOrderRecursive(string modGuid, HashSet<string> visited)
    {
        // Validate package
        if (LoadOrder.ContainsKey(modGuid))
        {
            return;
        }

        if (visited.Contains(modGuid))
        {
            // Additional info to help debug
            throw new Exception($"Cyclic dependency detected for mod: {modGuid}!");
        }

        // Check dependencies
        if (!ModsAvailable.TryGetValue(modGuid, out var config))
        {
            throw new Exception("modloader-error_parsing_mod_load_order");
        }

        config.LoadAfter ??= [];
        config.ModDependencies ??= [];

        var dependencies = new HashSet<string>(config.ModDependencies.Keys);

        foreach (var modAfterGuid in config.LoadAfter)
        {
            if (ModsAvailable.TryGetValue(modAfterGuid, out var value))
            {
                if (value?.LoadAfter?.Contains(modGuid) ?? false)
                {
                    throw new Exception("modloader-load_order_conflict");
                }

                dependencies.Add(modAfterGuid);
            }
        }

        visited.Add(modGuid);

        foreach (var nextModGuid in dependencies)
        {
            GetLoadOrderRecursive(nextModGuid, visited);
        }

        visited.Remove(modGuid);
        LoadOrder.Add(modGuid, config);
    }
}
