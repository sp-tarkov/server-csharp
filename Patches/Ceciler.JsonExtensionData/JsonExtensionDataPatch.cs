using Ceciler.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Ceciler.JsonExtensionData;

public class JsonExtensionDataPatch : IPatcher
{
    private TypeReference? _dictionaryStringObjectReference;

    public void Patch(AssemblyDefinition assembly)
    {
        var sptReferenceType = assembly.MainModule.Types.First(t => t.FullName == "SPTarkov.Server.Core.Utils.Reference.StaticReferences");
        var propertyReferenceType = sptReferenceType.Properties.First(p => p.Name == "Reference");
        var fieldReferenceType = sptReferenceType.Fields.First(p => p.Name == "_reference");
        _dictionaryStringObjectReference = propertyReferenceType.PropertyType;

        // We need to steal from the constructor the IL line 2 (index 1)
        var createDictionaryReference = sptReferenceType.GetConstructors().First(c => c.Parameters.Count == 0).Body.Instructions[1];

        var processed = new HashSet<string>();
        foreach (var typeDefinition in assembly.MainModule.Types)
        {
            if (
                !typeDefinition.Namespace.Contains("SPTarkov.Server.Core.Models")
                || typeDefinition.IsInterface
                || typeDefinition.IsEnum
                || IsStaticClass(typeDefinition)
                || processed.Contains(typeDefinition.FullName)
                || typeDefinition.IsAbstract
                || typeDefinition.HasGenericParameters
            )
            {
                continue;
            }

            var propertyDefinition = new PropertyDefinition("ExtensionData", PropertyAttributes.None, _dictionaryStringObjectReference);
            propertyDefinition.CustomAttributes.Add(propertyReferenceType.CustomAttributes.First());

            // Add backing field
            var field = new FieldDefinition(
                "_extensionData",
                FieldAttributes.Private | FieldAttributes.InitOnly,
                _dictionaryStringObjectReference
            );
            field.CustomAttributes.Add(fieldReferenceType.CustomAttributes.First());
            typeDefinition.Fields.Add(field);

            // Add getter
            var get = new MethodDefinition(
                "get_ExtensionData",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                _dictionaryStringObjectReference
            );

            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            propertyDefinition.GetMethod = get;
            typeDefinition.Methods.Add(get);

            typeDefinition.Properties.Add(propertyDefinition);

            foreach (var methodDefinition in typeDefinition.GetConstructors().Where(c => !c.IsStatic))
            {
                var ilCtor = methodDefinition.Body.GetILProcessor();

                var loadArg = ilCtor.Create(OpCodes.Ldarg_0);
                var createObj = createDictionaryReference;
                var setField = ilCtor.Create(OpCodes.Stfld, field);
                var first = ilCtor.Body.Instructions.First();
                ilCtor.InsertBefore(first, loadArg);
                ilCtor.InsertAfter(loadArg, createObj);
                ilCtor.InsertAfter(createObj, setField);
            }

            processed.Add(typeDefinition.FullName);
        }

        var writerParams = new WriterParameters { WriteSymbols = true };
        assembly.Write(writerParams);
    }

    private bool IsStaticClass(TypeDefinition type)
    {
        return type.IsClass && type.IsAbstract && type.IsSealed;
    }

    public string Name
    {
        get { return "ExtensionData"; }
    }
}
