using Mono.Cecil;
using Mono.Cecil.Rocks;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Exceptions;

namespace SPTarkov.Server.Modding;

internal static class EnumPatcher
{
    public static void Patch(ModuleDefinition module, IReadOnlyCollection<EnumEntryDefinition> entries)
    {
        foreach (var entry in entries)
        {
            ApplyEntry(module, entry);
        }
    }

    private static void ApplyEntry(ModuleDefinition module, EnumEntryDefinition entry)
    {
        if (string.IsNullOrWhiteSpace(entry.EnumType))
        {
            throw new ModLoaderException("An enum prepatch entry has no enumType.");
        }

        if (string.IsNullOrWhiteSpace(entry.ConstantName))
        {
            throw new ModLoaderException($"An enum prepatch entry for `{entry.EnumType}` has no constantName.");
        }

        var cecilTypeName = entry.EnumType.Replace('+', '/');
        var enumType = module.GetAllTypes().FirstOrDefault(type => string.Equals(type.FullName, cecilTypeName, StringComparison.Ordinal));

        if (enumType is null || !enumType.IsEnum)
        {
            throw new ModLoaderException($"Could not find enum type `{entry.EnumType}` in SPTarkov.Server.Core.dll.");
        }

        if (enumType.Fields.Any(field => string.Equals(field.Name, entry.ConstantName, StringComparison.Ordinal)))
        {
            throw new ModLoaderException($"Enum `{entry.EnumType}` already contains an entry named `{entry.ConstantName}`.");
        }

        if (enumType.Fields.Any(field => field.HasConstant && Convert.ToDecimal(field.Constant) == entry.ConstantValue))
        {
            throw new ModLoaderException($"Enum `{entry.EnumType}` already contains the value {entry.ConstantValue}.");
        }

        enumType.Fields.Add(
            new FieldDefinition(
                entry.ConstantName,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                enumType
            )
            {
                Constant = ConvertConstant(enumType, entry),
            }
        );
    }

    private static object ConvertConstant(TypeDefinition enumType, EnumEntryDefinition entry)
    {
        var underlyingType = enumType.Fields.First(field => field.Name == "value__").FieldType.MetadataType;

        try
        {
            return underlyingType switch
            {
                MetadataType.SByte => checked((sbyte)entry.ConstantValue),
                MetadataType.Byte => checked((byte)entry.ConstantValue),
                MetadataType.Int16 => checked((short)entry.ConstantValue),
                MetadataType.UInt16 => checked((ushort)entry.ConstantValue),
                MetadataType.Int32 => checked((int)entry.ConstantValue),
                MetadataType.UInt32 => checked((uint)entry.ConstantValue),
                MetadataType.Int64 => entry.ConstantValue,
                MetadataType.UInt64 => checked((ulong)entry.ConstantValue),
                _ => throw new ModLoaderException($"Enum `{entry.EnumType}` has an unsupported underlying type `{underlyingType}`."),
            };
        }
        catch (OverflowException exception)
        {
            throw new ModLoaderException(
                $"Value {entry.ConstantValue} does not fit enum `{entry.EnumType}` underlying type `{underlyingType}`.",
                exception
            );
        }
    }
}
