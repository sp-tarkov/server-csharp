using Ceciler.Interfaces;
using Mono.Cecil;

namespace Ceciler.Virtualizer;

public class VirtualizerPatch : IPatcher
{
    public void Patch(AssemblyDefinition assembly)
    {
        foreach (var typeDefinition in assembly.MainModule.Types)
        {
            foreach (var typeDefinitionMethod in typeDefinition.Methods)
            {
                if (typeDefinitionMethod == null)
                {
                    continue;
                }

                if (typeDefinitionMethod.IsConstructor)
                {
                    continue;
                }


                if (typeDefinitionMethod.IsFinal && typeDefinitionMethod.IsVirtual)
                {
                    typeDefinitionMethod.IsFinal = false;
                    continue;
                }

                if (typeDefinitionMethod.IsFinal)
                {
                    continue;
                }

                if (typeDefinitionMethod.IsVirtual)
                {
                    continue;
                }

                if (typeDefinitionMethod.IsStatic)
                {
                    continue;
                }

                if (typeDefinitionMethod.IsPrivate)
                {
                    continue;
                }

                if (MethodIsSerializationCallback(typeDefinitionMethod))
                {
                    continue;
                }

                typeDefinitionMethod.IsVirtual = true;
                typeDefinitionMethod.IsNewSlot = true;
            }
        }

#if DEBUG
        var writerParams = new WriterParameters()
        {
            WriteSymbols = true
        };
        assembly.Write(writerParams);
#else
        assembly.Write();
#endif

    }

    static bool MethodIsSerializationCallback(MethodDefinition method)
    {
        return ContainsAttribute(method.CustomAttributes, "OnSerializingAttribute")
               || ContainsAttribute(method.CustomAttributes, "OnSerializedAttribute")
               || ContainsAttribute(method.CustomAttributes, "OnDeserializingAttribute")
               || ContainsAttribute(method.CustomAttributes, "OnDeserializedAttribute");
    }

    public static bool ContainsAttribute(IEnumerable<CustomAttribute> attributes, string attributeName) =>
        attributes.Any(attribute => attribute.Constructor.DeclaringType.Name == attributeName);

    public string Name
    {
        get { return "Virtualizer"; }
    }
}
