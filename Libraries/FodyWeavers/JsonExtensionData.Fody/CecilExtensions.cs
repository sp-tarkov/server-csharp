using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace JsonExtensionData.Fody;

public static class CecilExtensions
{
    public static List<TypeDefinition> GetAllClasses(this ModuleDefinition moduleDefinition)
    {
        var definitions = new List<TypeDefinition>();
        //First is always module so we will skip that;
        GetTypes(moduleDefinition.Types.Skip(1), definitions);
        return definitions;
    }

    static void GetTypes(IEnumerable<TypeDefinition> typeDefinitions, List<TypeDefinition> definitions)
    {
        foreach (var typeDefinition in typeDefinitions)
        {
            GetTypes(typeDefinition.NestedTypes, definitions);

            if (typeDefinition.IsInterface)
            {
                continue;
            }

            if (typeDefinition.IsEnum)
            {
                continue;
            }

            definitions.Add(typeDefinition);
        }
    }
}
