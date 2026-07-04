using System.Reflection;
using System.Runtime.Loader;

namespace SPTarkov.Server.Modding;

/// <summary>
/// This context hosts the server with an in-memory prepatched Core as well as it's symbols so they can be loaded in memory
/// </summary>
public sealed class PrepatchLoadContext(string hostAssemblyPath, byte[] patchedCore, byte[]? patchedSymbols)
    : AssemblyLoadContext(ContextName, isCollectible: false)
{
    // Identifies the hosting context by name; type checks fail here because each hosted copy has its own PrepatchLoadContext type.
    public const string ContextName = "SPT.PrepatchHost";

    private const string CoreAssemblyName = "SPTarkov.Server.Core";

    private readonly AssemblyDependencyResolver _resolver = new(hostAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Intercept Core so everything in this context binds to the patched copy, with symbols so it stays debuggable.
        if (assemblyName.Name == CoreAssemblyName)
        {
            using var assemblyStream = new MemoryStream(patchedCore);
            if (patchedSymbols is null)
            {
                return LoadFromStream(assemblyStream);
            }

            using var symbolStream = new MemoryStream(patchedSymbols);
            return LoadFromStream(assemblyStream, symbolStream);
        }

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is not null ? LoadFromAssemblyPath(path) : null;
    }
}
