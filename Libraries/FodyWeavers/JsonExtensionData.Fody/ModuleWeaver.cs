using System.Collections.Generic;
using Fody;
using Mono.Cecil;

namespace JsonExtensionData.Fody;

public partial class ModuleWeaver : BaseModuleWeaver
{
    List<TypeDefinition> allClasses;

    public override void Execute()
    {
        allClasses = ModuleDefinition.GetAllClasses();
        ReadConfig();
        ProcessAssembly();
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        return [];
    }

    public override bool ShouldCleanReference => true;
}
