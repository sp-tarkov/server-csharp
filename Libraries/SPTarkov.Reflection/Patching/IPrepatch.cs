using Mono.Cecil;

namespace SPTarkov.Reflection.Patching;

public interface IPrepatch
{
    /// <summary>
    ///     Guid of the mod this prepatch belongs to
    /// </summary>
    public string ModGuid { get; }

    /// <summary>
    ///     Is this patch active?
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    ///     prepatch method called by the mod loader
    /// </summary>
    void Patch();
}
