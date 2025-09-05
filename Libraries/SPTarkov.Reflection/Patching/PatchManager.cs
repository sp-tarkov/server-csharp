using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Reflection.Patching;

/// <summary>
///     A manager for your patches. You MUST set the PatcherName property BEFORE enabling patches. This is used to identify your harmony instance.
/// </summary>
/// <remarks>
///     A known limitation is that exceptions and logging are only sent to the console and are not color coded. There is no disk logging here.
/// </remarks>
[Injectable]
public class PatchManager
{
    /// <summary>
    ///     Patcher name to be assigned to the harmony instance this manager controls. MUST be set prior to patching
    /// </summary>
    public string? PatcherName { get; set; }

    /// <summary>
    ///     Should the manager find and enable patches on its own?
    /// </summary>
    public bool AutoPatch { get; set; }

    private Harmony? _harmony;
    private readonly List<AbstractPatch> _patches = [];

    /// <summary>
    ///     Adds a single patch
    /// </summary>
    /// <param name="patch">Patch to add</param>
    /// <exception cref="PatchException"> Thrown if autopatch is enabled. You cannot add patches during auto patching. </exception>
    public void AddPatch(AbstractPatch patch)
    {
        if (AutoPatch)
        {
            throw new PatchException("You cannot manually add patches when using auto patching");
        }

        _patches.Add(patch);
    }

    /// <summary>
    ///     Adds a list of patches
    /// </summary>
    /// <param name="patchList">List of patches to add</param>
    /// <exception cref="PatchException"> Thrown if autopatch is enabled. You cannot add patches during auto patching. </exception>
    public void AddPatches(List<AbstractPatch> patchList)
    {
        if (AutoPatch)
        {
            throw new PatchException("You cannot manually add patches when using auto patching");
        }

        _patches.AddRange(patchList);
    }

    /// <summary>
    ///     Retrieves a list of types from the given assembly that inherit from <see cref="AbstractPatch"/>, <br/>
    /// excluding those marked with <see cref="IgnoreAutoPatchAttribute"/> and, in non-debug builds, <br/>
    /// excluding those marked with <see cref="DebugPatchAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan for patch types.</param>
    /// <returns>
    /// A list of types that inherit from <see cref="AbstractPatch"/> and meet the filtering criteria.
    /// </returns>
    private List<Type> GetPatches(Assembly assembly)
    {
        List<Type> patches = [];

        var baseType = typeof(AbstractPatch);
        var ignoreAttrType = typeof(IgnoreAutoPatchAttribute);

        foreach (var type in assembly.GetTypes())
        {
            if (type.BaseType != baseType)
            {
                continue;
            }

            if (type.IsDefined(ignoreAttrType, inherit: false))
            {
                continue;
            }

            // Assembly was not built in debug and this is a debug patch, skip it.
            if (!IsAssemblyDebugBuild(assembly) && type.IsDefined(typeof(DebugPatchAttribute), inherit: false))
            {
                continue;
            }

            patches.Add(type);
        }

        return patches;
    }

    /// <summary>
    ///     Enables all patches, if <see cref="AutoPatch"/> is enabled it will find them automatically
    /// </summary>
    /// <exception cref="PatchException">
    ///     Thrown if PatcherName was not set, or there are no patches found during auto patching, or there are no patches added manually.
    /// </exception>
    public void EnablePatches()
    {
        if (PatcherName is null)
        {
            throw new PatchException("You cannot enable patches without setting a PatcherName.");
        }

        _harmony ??= new Harmony(PatcherName);

        if (AutoPatch)
        {
            var patches = GetPatches(Assembly.GetCallingAssembly());

            if (patches.Count == 0)
            {
                throw new PatchException("Could not find any patches defined in the assembly during auto patching");
            }

            var successfulPatches = 0;
            foreach (var type in patches)
            {
                try
                {
                    ((AbstractPatch)Activator.CreateInstance(type)).Enable(_harmony);
                    successfulPatches++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to init [{type.Name}]: {ex.Message}");
                }
            }

            Console.WriteLine($"Enabled {successfulPatches} patches");
            return;
        }

        if (_patches.Count == 0)
        {
            throw new PatchException("No patches have been added to enable. You must add them with AddPatches()");
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _patches.Count; i++)
        {
            try
            {
                _patches[i].Enable(_harmony);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to init [{_patches[i].GetType().Name}]: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     Disables all patches, if <see cref="AutoPatch"/> is enabled it will find them automatically
    /// </summary>
    /// <exception cref="PatchException">
    ///     Thrown if there are no enabled patches, or no patches are found during auto patch disabling, or there were no patches added manually to disable.
    /// </exception>
    public void DisablePatches()
    {
        if (_harmony is null)
        {
            throw new PatchException("You cannot disable without first enabling patches. _harmony is null");
        }

        if (AutoPatch)
        {
            var patches = GetPatches(Assembly.GetCallingAssembly());

            if (patches.Count == 0)
            {
                throw new PatchException("Could not find any patches defined in the assembly during auto patching");
            }

            var disabledPatches = 0;
            foreach (var type in patches)
            {
                try
                {
                    ((AbstractPatch)Activator.CreateInstance(type)).Disable(_harmony);
                    disabledPatches++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to disable [{type.Name}]: {ex.Message}");
                }
            }

            Console.WriteLine($"Disabled {disabledPatches} patches");
            return;
        }

        if (_patches.Count == 0)
        {
            throw new PatchException("There were no patches to disable");
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _patches.Count; i++)
        {
            try
            {
                _patches[i].Disable(_harmony);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to disable [{_patches[i].GetType().Name}]: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     Enables a single patch
    /// </summary>
    /// <param name="patch"></param>
    /// <exception cref="PatchException">
    ///     Thrown if PatcherName was not set
    /// </exception>
    public void EnablePatch(AbstractPatch patch)
    {
        if (PatcherName is null || _harmony is null)
        {
            throw new PatchException("You cannot enable patches without setting a PatcherName.");
        }

        patch.Enable(_harmony);
    }

    /// <summary>
    ///     Disables a single patch
    /// </summary>
    /// <param name="patch"></param>
    public void DisablePatch(AbstractPatch patch)
    {
        if (!patch.IsActive)
        {
            Console.WriteLine($"Cannot disable patch: {patch.HarmonyId} because it is not active");
            return;
        }

        if (_harmony is null)
        {
            throw new PatchException("You cannot disable without first enabling patches. _harmony is null");
        }

        patch.Disable(_harmony);
    }

    /// <summary>
    ///     Check if an assembly is built in debug mode
    /// </summary>
    /// <param name="assembly">Assembly to check</param>
    /// <returns>True if debug mode</returns>
    private bool IsAssemblyDebugBuild(Assembly assembly)
    {
        var debugAttr = assembly.GetCustomAttribute<DebuggableAttribute>();

        return debugAttr != null && debugAttr.IsJITOptimizerDisabled;
    }
}
