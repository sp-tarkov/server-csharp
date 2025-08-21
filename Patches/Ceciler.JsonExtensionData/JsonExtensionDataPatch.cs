using Ceciler.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Ceciler.JsonExtensionData;

public class JsonExtensionDataPatch : IPatcher
{
    private TypeReference? _dictionaryStringObjectReference;


    private MethodReference? _jsonExtensionDataAttributeReference;


    private MethodReference? _jsonIgnoreAttributeReference;


    public void Patch(AssemblyDefinition assembly)
    {
        _dictionaryStringObjectReference ??= assembly.MainModule.ImportReference(typeof(Dictionary<string, object>));

        if (_jsonExtensionDataAttributeReference is null)
        {
            var jsonConstructorReference = assembly.MainModule.AssemblyResolver
                .Resolve(AssemblyNameReference.Parse("System.Text.Json")).MainModule
                .GetType("System.Text.Json.Serialization.JsonExtensionDataAttribute").Methods
                .First(m => m.IsConstructor && !m.HasParameters);

            _jsonExtensionDataAttributeReference = assembly.MainModule.ImportReference(jsonConstructorReference);
        }

        if (_jsonIgnoreAttributeReference is null)
        {
            var jsonIgnoreConstructorReference = assembly.MainModule.AssemblyResolver
                .Resolve(AssemblyNameReference.Parse("System.Text.Json")).MainModule
                .GetType("System.Text.Json.Serialization.JsonIgnoreAttribute").Methods
                .First(m => m.IsConstructor && !m.HasParameters);

            _jsonIgnoreAttributeReference = assembly.MainModule.ImportReference(jsonIgnoreConstructorReference);
        }

        var processed = new HashSet<string>();
        foreach (var typeDefinition in assembly.MainModule.Types)
        {
            if (!typeDefinition.Namespace.Contains("SPTarkov.Server.Core.Models") ||
                typeDefinition.IsInterface ||
                typeDefinition.IsEnum ||
                processed.Contains(typeDefinition.FullName))
            {
                continue;
            }

            var propertyDefinition = new PropertyDefinition("ExtensionData", PropertyAttributes.None, _dictionaryStringObjectReference);
            propertyDefinition.CustomAttributes.Add(new CustomAttribute(_jsonExtensionDataAttributeReference));

            // Add backing field
            var field = new FieldDefinition("_extensionData",
                FieldAttributes.Private,
                _dictionaryStringObjectReference);

            field.CustomAttributes.Add(new CustomAttribute(_jsonIgnoreAttributeReference));
            typeDefinition.Fields.Add(field);

            // Add getter
            var get = new MethodDefinition("get_ExtensionData",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                _dictionaryStringObjectReference);

            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            propertyDefinition.GetMethod = get;

            typeDefinition.Methods.Add(get);

            // Add setter
            var set = new MethodDefinition("set_ExtensionData",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                assembly.MainModule.TypeSystem.Void);

            set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, _dictionaryStringObjectReference));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            propertyDefinition.SetMethod = set;

            typeDefinition.Methods.Add(set);
            typeDefinition.Properties.Add(propertyDefinition);

            processed.Add(typeDefinition.FullName);
        }
#if DEBUG
        var writerParams = new WriterParameters() { WriteSymbols = true };
        assembly.Write(writerParams);
#else
        assembly.Write();
#endif
    }

    public string Name
    {
        get { return "Virtualizer"; }
    }
}
