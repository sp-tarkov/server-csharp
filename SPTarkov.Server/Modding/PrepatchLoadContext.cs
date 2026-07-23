using System.Reflection;
using System.Runtime.Loader;

namespace SPTarkov.Server.Modding;

/// <summary>
/// Binds the hosted server to an in-memory prepatched Core while sharing process-wide framework assemblies.
/// </summary>
public sealed class PrepatchLoadContext(string hostAssemblyPath, byte[] patchedCore, byte[]? patchedSymbols)
    : AssemblyLoadContext(ContextName, isCollectible: false)
{
    public const string ContextName = "SPT.PrepatchHost";

    private const string CoreAssemblyName = "SPTarkov.Server.Core";

    private static readonly string[] _sharedAssemblyPrefixes =
    [
        "Microsoft.",
        "System.",
        "MudBlazor",
        "ZLogger",
        "0Harmony",
        "MonoMod.",
        "Mono.Cecil",
    ];

    private readonly AssemblyDependencyResolver _resolver = new(hostAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
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

        if (ShouldShareFromDefaultContext(assemblyName.Name))
        {
            var sharedAssembly = Default.Assemblies.FirstOrDefault(assembly =>
                AssemblyName.ReferenceMatchesDefinition(assemblyName, assembly.GetName())
            );

            if (sharedAssembly is not null)
            {
                return sharedAssembly;
            }

            var sharedPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (sharedPath is not null)
            {
                return Default.LoadFromAssemblyPath(sharedPath);
            }
        }

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is not null ? LoadFromAssemblyPath(path) : null;
    }

    private static bool ShouldShareFromDefaultContext(string? assemblyName)
    {
        return assemblyName is not null
            && _sharedAssemblyPrefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
