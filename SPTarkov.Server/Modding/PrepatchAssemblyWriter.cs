using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;

namespace SPTarkov.Server.Modding;

internal static class PrepatchAssemblyWriter
{
    public static byte[] Write(this ModuleDefinition module)
    {
        using var stream = new MemoryStream();
        module.Write(stream, new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
        return stream.ToArray();
    }
}
