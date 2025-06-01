using System.Collections.Generic;
using System.Text.Json.Serialization;

using Mono.Cecil;
using BindingFlags = System.Reflection.BindingFlags;

namespace JsonExtensionData.Fody;

public partial class ModuleWeaver
{
    private TypeReference? _dictionaryStringObjectReference;
    private MethodReference? _jsonExtensionDataAttributeReference;
    public void ProcessType(TypeDefinition typeDefinition)
    {
        _dictionaryStringObjectReference ??= ModuleDefinition.ImportReference(typeof(Dictionary<string, object>));
        _jsonExtensionDataAttributeReference ??= ModuleDefinition.ImportReference(typeof(JsonExtensionDataAttribute).GetConstructor(BindingFlags.Public | BindingFlags.Instance, []));
        var propertyDefinition = new PropertyDefinition("ExtensionData", PropertyAttributes.None, _dictionaryStringObjectReference);
        propertyDefinition.CustomAttributes.Add(new CustomAttribute(_jsonExtensionDataAttributeReference));
        var get = new MethodDefinition("get_ExtensionData",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            _dictionaryStringObjectReference);
        propertyDefinition.GetMethod = get;
        typeDefinition.Methods.Add(get);
        typeDefinition.Properties.Add(propertyDefinition);
    }
}
