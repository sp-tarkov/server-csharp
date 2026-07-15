using System.Reflection;
using HarmonyLib;

namespace SPTarkov.Reflection.Patching;

/// <summary>
///     Harmony patch wrapper class. See mod example 6.1 for usage.
/// </summary>
/// <remarks>
///     A known limitation is that exceptions and logging are only sent to the console and are not color coded. There is no disk logging here.
/// </remarks>
public abstract class AbstractPatch : IRuntimePatch
{
    /// <summary>
    ///     Method this patch targets
    /// </summary>
    public MethodBase? TargetMethod { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsManaged { get; private set; }
    public bool IsYourPatch
    {
        get { return _ownersAssembly != null && ReferenceEquals(_ownersAssembly, Assembly.GetCallingAssembly()); }
    }
    public string HarmonyId
    {
        get { return _harmony?.Id ?? "Harmony Id is null for this patch"; }
    }

    private Harmony? _harmony;
    private readonly Assembly? _ownersAssembly;

    private readonly List<HarmonyMethod> _prefixList;
    private readonly List<HarmonyMethod> _postfixList;
    private readonly List<HarmonyMethod> _transpilerList;
    private readonly List<HarmonyMethod> _finalizerList;
    private readonly List<HarmonyMethod> _ilManipulatorList;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">name of the harmony instance</param>
    protected AbstractPatch(string? name = null)
    {
        _ownersAssembly = Assembly.GetCallingAssembly();
        _harmony = new Harmony(name ?? GetType().Name);
        _prefixList = GetPatchMethods(typeof(PatchPrefixAttribute));
        _postfixList = GetPatchMethods(typeof(PatchPostfixAttribute));
        _transpilerList = GetPatchMethods(typeof(PatchTranspilerAttribute));
        _finalizerList = GetPatchMethods(typeof(PatchFinalizerAttribute));
        _ilManipulatorList = GetPatchMethods(typeof(PatchIlManipulatorAttribute));

        if (
            _prefixList.Count == 0
            && _postfixList.Count == 0
            && _transpilerList.Count == 0
            && _finalizerList.Count == 0
            && _ilManipulatorList.Count == 0
        )
        {
            throw new PatchException($"{GetType().Name}: At least one of the patch methods must be specified");
        }
    }

    /// <summary>
    /// Get original method
    /// </summary>
    /// <returns>Method</returns>
    protected abstract MethodBase? GetTargetMethod();

    /// <summary>
    /// Get HarmonyMethod from string
    /// </summary>
    /// <param name="attributeType">Attribute type</param>
    /// <returns>Method</returns>
    private List<HarmonyMethod> GetPatchMethods(Type attributeType)
    {
        var T = GetType();
        var methods = new List<HarmonyMethod>();

        foreach (var method in T.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            if (method.GetCustomAttribute(attributeType) != null)
            {
                methods.Add(new HarmonyMethod(method));
            }
        }

        return methods;
    }

    public void Enable()
    {
        // Already active
        if (IsActive)
        {
            return;
        }

        var caller = Assembly.GetCallingAssembly();
        // No ownership over this patch
        if (!ReferenceEquals(_ownersAssembly, caller))
        {
            return;
        }

        TargetMethod = GetTargetMethod();

        if (TargetMethod == null)
        {
            throw new PatchException($"{GetType().Name}: TargetMethod is null");
        }

        try
        {
            foreach (var prefix in _prefixList)
            {
                _harmony!.Patch(TargetMethod, prefix: prefix);
            }

            foreach (var postfix in _postfixList)
            {
                _harmony!.Patch(TargetMethod, postfix: postfix);
            }

            foreach (var transpiler in _transpilerList)
            {
                _harmony!.Patch(TargetMethod, transpiler: transpiler);
            }

            foreach (var finalizer in _finalizerList)
            {
                _harmony!.Patch(TargetMethod, finalizer: finalizer);
            }

            foreach (var ilmanipulator in _ilManipulatorList)
            {
                _harmony!.Patch(TargetMethod, ilmanipulator: ilmanipulator);
            }

            IsActive = true;
        }
        catch (Exception ex)
        {
            throw new PatchException(
                $"{GetType().Name}: Failed to patch {TargetMethod.DeclaringType?.FullName}.{TargetMethod.Name} - {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    ///     Internal use only, called from the patch manager.
    /// </summary>
    /// <param name="harmony">Harmony instance of the patch manager</param>
    internal void Enable(Harmony harmony)
    {
        if (!ReferenceEquals(_harmony, harmony))
        {
            // Override the initial harmony instance with the PatchManagers instance
            _harmony = harmony;
        }

        IsManaged = true;
        Enable();
    }

    public void Disable()
    {
        // Nothing to disable
        if (!IsActive)
        {
            return;
        }

        var caller = Assembly.GetCallingAssembly();
        // No ownership over this patch
        if (!ReferenceEquals(_ownersAssembly, caller))
        {
            return;
        }

        var target = GetTargetMethod();

        if (target == null)
        {
            throw new PatchException($"{GetType().Name}: TargetMethod is null");
        }

        try
        {
            // Using null forgiving operator here because we want to throw if _harmony is null, but want the compiler to shut up about it.
            _harmony!.Unpatch(target, HarmonyPatchType.All, _harmony.Id);
        }
        catch (Exception ex)
        {
            throw new PatchException($"{GetType().Name}:", ex);
        }

        IsActive = false;
    }

    /// <summary>
    ///     Internal use only, called from the patch manager.
    /// </summary>
    /// <param name="harmony">Harmony instance of the patch manager</param>
    internal void Disable(Harmony harmony)
    {
        //  Attempting to disable a patch that is not managed by the patch manager
        if (harmony is null || !ReferenceEquals(_harmony, harmony))
        {
            throw new PatchException(
                $"Patch: {GetType().Name} is attempting to be disabled internally while not managed by the patch manager."
            );
        }

        Disable();

        // This patch is no longer considered managed.
        IsManaged = false;
    }
}
