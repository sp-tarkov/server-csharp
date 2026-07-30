namespace SPTarkov.Reflection.Patching;

public interface IRuntimePatch
{
    /// <summary>
    ///     Is this patch active?
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    ///     Is this patch managed by a patch manager?
    /// </summary>
    public bool IsManaged { get; }

    /// <summary>
    ///     Do you have ownership over this patch?
    /// </summary>
    public bool IsYourPatch { get; }

    /// <summary>
    ///     The harmony Id assigned to this patch, usually the name of the patch class unless overriden.
    /// </summary>
    public string HarmonyId { get; }

    /// <summary>
    ///     Apply patch to target
    /// <br/><br/>
    /// You cannot enable patches you have no ownership over
    /// </summary>
    public void Enable();

    /// <summary>
    ///     Remove applied patch from target
    /// <br/><br/>
    /// You cannot disable patches you have no ownership over
    /// </summary>
    public void Disable();
}
