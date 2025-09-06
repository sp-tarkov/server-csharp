namespace SPTarkov.Reflection.Patching;

/// <summary>
///     Cache of active patches for mod developers to use for compatibility reasons
/// </summary>
public static class ModPatchCache
{
    private static readonly List<AbstractPatch> _activePatches = [];

    // This class contains tighter access rules than we usually would implement in the project,
    // the reason for this is so that the data is a true representation of what's happening with patches without any external interference.

    // TODO: Mod GUID/Name associations to patches, need to think on that.
    // Required parameter on AbstractPatch ctor maybe?

    /// <summary>
    ///     Get all active patches
    /// </summary>
    /// <returns>
    /// List of active patches
    /// </returns>
    /// <remarks>
    ///     This should never be called before PreSptLoad is completed, otherwise could be empty.
    /// </remarks>
    public static IReadOnlyList<AbstractPatch> GetActivePatches()
    {
        // We're not exposing _activePatches so it cant be altered outside of this class. Do NOT implement this as a property.
        // Mod developers can still enable/disable these patches at will, this is fine, just don't allow external removal from the cache.
        return _activePatches.AsReadOnly();
    }

    /// <summary>
    ///     Get all actively patched target method names
    /// </summary>
    /// <returns>
    /// List of fully quantified method names; including namespace, type and method name
    /// </returns>
    /// <remarks>
    ///     This should never be called before PreSptLoad is completed, otherwise could be empty.
    /// </remarks>
    public static List<string> GetActivePatchedMethodNames()
    {
        var result = new List<string>();

        foreach (var patch in _activePatches)
        {
            // Fullname includes namespace
            var typeName = patch.TargetMethod?.DeclaringType?.FullName;
            var methodName = patch.TargetMethod?.Name;

            if (typeName != null && methodName != null)
            {
                result.Add($"{typeName}.{methodName}");
                continue;
            }

            result.Add($"{patch.HarmonyId}: Type or method is null for this patch.");
        }

        return result;
    }

    /// <summary>
    ///     Add a patch to the cache
    /// </summary>
    /// <param name="patch">Patch to add to cache</param>
    /// <remarks>
    ///     DO NOT PATCH THIS METHOD, IT IS INTERNAL FOR A REASON. YOU ARE ONLY HARMING OTHER MOD DEVELOPERS BY DOING SO.
    /// </remarks>
    internal static void AddPatch(AbstractPatch patch)
    {
        _activePatches.Add(patch);
    }

    /// <summary>
    ///     Remove a patch from the cache
    /// </summary>
    /// <param name="patch">Patch to remove</param>
    /// <returns>
    /// True if patch was removed
    /// </returns>
    /// <remarks>
    ///     DO NOT PATCH THIS METHOD, IT IS INTERNAL FOR A REASON. YOU ARE ONLY HARMING OTHER MOD DEVELOPERS BY DOING SO.
    /// </remarks>
    internal static bool RemovePatch(AbstractPatch patch)
    {
        return _activePatches.Remove(patch);
    }
}
