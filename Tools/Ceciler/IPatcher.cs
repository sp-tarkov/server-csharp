using Mono.Cecil;

namespace Ceciler;

public interface IPatcher
{
    void Patch(AssemblyDefinition assembly);
    string Name { get; }
}
