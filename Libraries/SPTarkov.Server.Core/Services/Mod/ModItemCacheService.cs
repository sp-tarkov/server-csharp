using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services.Mod;

[Injectable(InjectionType.Singleton)]
public class ModItemCacheService(ISptLogger<ModItemCacheService> logger, IReadOnlyList<SptMod> loadedMods)
{
    private readonly Dictionary<string, HashSet<MongoId>> _cachedItems = [];

    /// <summary>
    ///     Get all mod items for a provided mod by GUID
    /// </summary>
    /// <param name="guid">Guid of the mod to get the items for</param>
    /// <returns>Hashset of mod items</returns>
    public IReadOnlySet<MongoId> GetCachedItemIdsFromMod(string guid)
    {
        return _cachedItems.TryGetValue(guid, out var modItems) ? modItems : [];
    }

    /// <summary>
    ///     Get all items added by all mods. Key is mods guid and value is the items it has added
    /// </summary>
    /// <returns>All loaded mod items</returns>
    public IReadOnlyDictionary<string, IReadOnlySet<MongoId>> GetAllCachedModItemIds()
    {
        return _cachedItems.ToDictionary<KeyValuePair<string, HashSet<MongoId>>, string, IReadOnlySet<MongoId>>(
            modItem => modItem.Key,
            modItem => modItem.Value
        );
    }

    /// <summary>
    ///     Adds a mod item to the cache, internal use only.
    /// </summary>
    /// <param name="caller">Callers assembly</param>
    /// <param name="modId">Item id added to database</param>
    internal void AddModItem(Assembly caller, MongoId modId)
    {
        var mod = GetModFromAssembly(caller);
        if (mod is null)
        {
            logger.Error(
                $"Could not find mod reference for assembly: {caller.GetName()} when adding item tpl: {modId.ToString()} to cache"
            );
            return;
        }

        var guid = mod.ModMetadata.ModGuid;
        if (!_cachedItems.TryGetValue(guid, out _))
        {
            _cachedItems.Add(guid, []);
        }

        _cachedItems[guid].Add(modId);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Mod: {guid} added item: {modId.ToString()} to database");
        }
    }

    /// <summary>
    ///     Get the SptMod object for the callers assembly
    /// </summary>
    /// <param name="caller">Assembly adding the item id</param>
    /// <returns>SptMod of the assembly</returns>
    private SptMod? GetModFromAssembly(Assembly caller)
    {
        foreach (var mod in loadedMods)
        {
            if (mod.Assemblies.Any(modAssembly => ReferenceEquals(caller, modAssembly)))
            {
                return mod;
            }
        }

        return null;
    }
}
