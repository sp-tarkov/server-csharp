using Ceciler.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Ceciler.JsonExtensionData;

public class JsonExtensionDataPatch : IPatcher
{
    private TypeReference? _dictionaryStringObjectReference;

    private MethodReference? _dictionaryStringObjectCtorReference;
    private MethodReference? _jsonExtensionDataAttributeReference;

    private MethodReference? _jsonIgnoreAttributeReference;

    public void Patch(AssemblyDefinition assembly)
    {
        _dictionaryStringObjectReference ??= assembly.MainModule.ImportReference(typeof(Dictionary<string, object>));
        _dictionaryStringObjectCtorReference ??= assembly.MainModule.ImportReference(
            typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes)
        );

        if (_jsonExtensionDataAttributeReference is null)
        {
            var jsonConstructorReference = assembly
                .MainModule.AssemblyResolver.Resolve(AssemblyNameReference.Parse("System.Text.Json"))
                .MainModule.GetType("System.Text.Json.Serialization.JsonExtensionDataAttribute")
                .Methods.First(m => m.IsConstructor && !m.HasParameters);

            _jsonExtensionDataAttributeReference = assembly.MainModule.ImportReference(jsonConstructorReference);
        }

        if (_jsonIgnoreAttributeReference is null)
        {
            var jsonIgnoreConstructorReference = assembly
                .MainModule.AssemblyResolver.Resolve(AssemblyNameReference.Parse("System.Text.Json"))
                .MainModule.GetType("System.Text.Json.Serialization.JsonIgnoreAttribute")
                .Methods.First(m => m.IsConstructor && !m.HasParameters);

            _jsonIgnoreAttributeReference = assembly.MainModule.ImportReference(jsonIgnoreConstructorReference);
        }
        var isExternalInitType = assembly.MainModule.ImportReference(typeof(System.Runtime.CompilerServices.IsExternalInit));

        var compilerGenerated = assembly.MainModule.ImportReference(
            assembly.MainModule.ImportReference(
                typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes)
            )
        );

        var nullableAttrType = assembly.MainModule.ImportReference(typeof(System.Runtime.CompilerServices.NullableAttribute));
        var attrCtor = assembly.MainModule.ImportReference(
            nullableAttrType
                .Resolve()
                .Methods.First(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "Byte")
        );
        var attr = new CustomAttribute(attrCtor);
        attr.ConstructorArguments.Add(new CustomAttributeArgument(assembly.MainModule.TypeSystem.Byte, (byte)1));

        var processed = new HashSet<string>();
        foreach (var typeDefinition in assembly.MainModule.Types)
        {
            if (
                !typeDefinition.Namespace.Contains("SPTarkov.Server.Core.Models")
                || typeDefinition.IsInterface
                || typeDefinition.IsEnum
                || IsStaticClass(typeDefinition)
                || processed.Contains(typeDefinition.FullName)
            )
            {
                continue;
            }

            var propertyDefinition = new PropertyDefinition("ExtensionData", PropertyAttributes.None, _dictionaryStringObjectReference);
            propertyDefinition.CustomAttributes.Add(new CustomAttribute(_jsonExtensionDataAttributeReference));
            propertyDefinition.CustomAttributes.Add(attr);

            // Add backing field
            var field = new FieldDefinition(
                "<ExtensionData>k__BackingField",
                FieldAttributes.Private | FieldAttributes.InitOnly,
                _dictionaryStringObjectReference
            );
            field.CustomAttributes.Add(new CustomAttribute(compilerGenerated));
            field.CustomAttributes.Add(attr);
            typeDefinition.Fields.Add(field);

            // Add getter
            var get = new MethodDefinition(
                "get_ExtensionData",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                _dictionaryStringObjectReference
            );

            get.CustomAttributes.Add(new CustomAttribute(compilerGenerated));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            propertyDefinition.GetMethod = get;

            typeDefinition.Methods.Add(get);

            // Add setter
            var set = new MethodDefinition(
                "set_ExtensionData",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                assembly.MainModule.TypeSystem.Void
            );

            var returnType = set.ReturnType;
            var modifiedReturnType = new RequiredModifierType(isExternalInitType, returnType);
            set.MethodReturnType.ReturnType = modifiedReturnType;
            set.CustomAttributes.Add(new CustomAttribute(compilerGenerated));
            set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, _dictionaryStringObjectReference));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
            set.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            propertyDefinition.SetMethod = set;

            typeDefinition.Methods.Add(set);
            typeDefinition.Properties.Add(propertyDefinition);

            foreach (var methodDefinition in typeDefinition.GetConstructors().Where(c => !c.IsStatic))
            {
                var ilCtor = methodDefinition.Body.GetILProcessor();

                var loadArg = ilCtor.Create(OpCodes.Ldarg_0);
                var createObj = ilCtor.Create(OpCodes.Newobj, _dictionaryStringObjectCtorReference);
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
        get { return "Virtualizer"; }
    }
}
