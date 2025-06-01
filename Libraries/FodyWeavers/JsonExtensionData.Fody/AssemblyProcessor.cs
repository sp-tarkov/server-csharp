using System.Linq;
using Mono.Cecil;

namespace JsonExtensionData.Fody;

public partial class ModuleWeaver
{
    public void ProcessAssembly()
    {
        foreach (var type in allClasses)
        {
            if (!ShouldInclude(type))
            {
                continue;
            }

            if (ShouldIncludeType(type))
            {
                ProcessType(type);
            }
        }
    }

    public bool ShouldIncludeType(TypeDefinition type)
    {
        return IncludeNamespacesRegex.Any(r => r.IsMatch(type.Namespace));
    }

    static bool ShouldInclude(TypeDefinition type) => !type.IsSealed;
}
