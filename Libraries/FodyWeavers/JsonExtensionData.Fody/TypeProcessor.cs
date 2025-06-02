using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace JsonExtensionData.Fody;

public partial class ModuleWeaver
{
    private TypeReference? _dictionaryStringObjectReference;
    private MethodReference? _jsonExtensionDataAttributeReference;
    public void ProcessType(TypeDefinition typeDefinition)
    {
        _dictionaryStringObjectReference ??= ModuleDefinition.ImportReference(typeof(Dictionary<string, object>));
        if (_jsonExtensionDataAttributeReference is null)
        {
            var jsonConstructorReference = ModuleDefinition.AssemblyResolver
                .Resolve(AssemblyNameReference.Parse("System.Text.Json")).MainModule
                .GetType("System.Text.Json.Serialization.JsonExtensionDataAttribute").Methods
                .First(m => m.IsConstructor && !m.HasParameters);
            _jsonExtensionDataAttributeReference = ModuleDefinition.ImportReference(jsonConstructorReference);
        }
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
