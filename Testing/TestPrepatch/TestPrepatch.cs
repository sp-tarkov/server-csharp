using System.Reflection;
using Mono.Cecil;
using SPTarkov.Reflection.Patching;

namespace TestPrepatch;

public sealed class TestPrepatch : AbstractPrepatch
{
    private const string MetadataKey = "TestPrepatch";
    private const string MetadataValue = "Applied";

    public override string ModGuid
    {
        get { return "com.sp-tarkov.test-mod2"; }
    }

    public override bool IsActive
    {
        get { return true; }
    }

    public override void Patch(ModuleDefinition serverCoreModule)
    {
        if (serverCoreModule.Assembly.CustomAttributes.Any(IsTestPrepatchMarker))
        {
            return;
        }

        var constructor = typeof(AssemblyMetadataAttribute).GetConstructor([typeof(string), typeof(string)]);
        var metadataAttribute = new CustomAttribute(serverCoreModule.ImportReference(constructor));
        metadataAttribute.ConstructorArguments.Add(new CustomAttributeArgument(serverCoreModule.TypeSystem.String, MetadataKey));
        metadataAttribute.ConstructorArguments.Add(new CustomAttributeArgument(serverCoreModule.TypeSystem.String, MetadataValue));

        serverCoreModule.Assembly.CustomAttributes.Add(metadataAttribute);
    }

    private static bool IsTestPrepatchMarker(CustomAttribute attribute)
    {
        return attribute.AttributeType.FullName == typeof(AssemblyMetadataAttribute).FullName
            && attribute.ConstructorArguments.Count == 2
            && string.Equals(attribute.ConstructorArguments[0].Value as string, MetadataKey, StringComparison.Ordinal);
    }
}
