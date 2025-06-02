using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace JsonExtensionData.Fody;

public partial class ModuleWeaver
{
    private TypeReference? _dictionaryObjectReference;
    private GenericInstanceType? _dictionaryGenericNullableReference;
    private TypeReference? _nullableTypeReference;
    private MethodReference? _jsonExtensionDataAttributeReference;
    private MethodReference? _jsonIgnoreAttributeReference;
    public void ProcessType(TypeDefinition typeDefinition)
    {
        _nullableTypeReference ??= ModuleDefinition.ImportReference(typeof(Nullable<>));

        if (_dictionaryGenericNullableReference == null)
        {
            _dictionaryObjectReference ??= ModuleDefinition.ImportReference(typeof(Dictionary<string, object>));
            _dictionaryGenericNullableReference = new GenericInstanceType(_nullableTypeReference);
            _dictionaryGenericNullableReference.GenericArguments.Add(_dictionaryObjectReference);
        }
        if (_jsonExtensionDataAttributeReference is null)
        {
            var jsonConstructorReference = ModuleDefinition.AssemblyResolver
                .Resolve(AssemblyNameReference.Parse("System.Text.Json")).MainModule
                .GetType("System.Text.Json.Serialization.JsonExtensionDataAttribute").Methods
                .First(m => m.IsConstructor && !m.HasParameters);
            _jsonExtensionDataAttributeReference = ModuleDefinition.ImportReference(jsonConstructorReference);
        }
        if (_jsonIgnoreAttributeReference is null)
        {
            var jsonIgnoreConstructorReference = ModuleDefinition.AssemblyResolver
                .Resolve(AssemblyNameReference.Parse("System.Text.Json")).MainModule
                .GetType("System.Text.Json.Serialization.JsonIgnoreAttribute").Methods
                .First(m => m.IsConstructor && !m.HasParameters);
            _jsonIgnoreAttributeReference = ModuleDefinition.ImportReference(jsonIgnoreConstructorReference);
        }
        var propertyDefinition = new PropertyDefinition("ExtensionData", PropertyAttributes.None, _dictionaryGenericNullableReference);
        propertyDefinition.CustomAttributes.Add(new CustomAttribute(_jsonExtensionDataAttributeReference));

        // Add backing field
        var field = new FieldDefinition("_extensionData",
            FieldAttributes.Private,
            _dictionaryGenericNullableReference);
        field.CustomAttributes.Add(new CustomAttribute(_jsonIgnoreAttributeReference));
        typeDefinition.Fields.Add(field);

        // Add getter
        var get = new MethodDefinition("get_ExtensionData",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            _dictionaryGenericNullableReference);
        get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
        get.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        propertyDefinition.GetMethod = get;
        typeDefinition.Methods.Add(get);

        // Add setter
        var set = new MethodDefinition("set_ExtensionData",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            ModuleDefinition.TypeSystem.Void);
        set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, _dictionaryGenericNullableReference));

        set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        set.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
        set.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        propertyDefinition.SetMethod = set;
        typeDefinition.Methods.Add(set);
        typeDefinition.Properties.Add(propertyDefinition);
    }
}
