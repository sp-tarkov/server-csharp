using System.Reflection;
using Mono.Cecil;

namespace SPTarkov.Reflection.Patching;

public abstract class AbstractPrepatch : IPrepatch
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public abstract string ModGuid { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public abstract bool IsActive { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="serverCoreModule"><inheritdoc/></param>
    public abstract void Patch(ModuleDefinition serverCoreModule);
}
