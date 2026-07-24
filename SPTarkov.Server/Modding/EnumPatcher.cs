using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
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

        var enumType = module.GetAllTypes().FirstOrDefault(type => string.Equals(type.FullName, entry.EnumType, StringComparison.Ordinal));

        if (enumType is null || !enumType.IsEnum)
        {
            throw new ModLoaderException($"Could not find enum type `{entry.EnumType}` in SPTarkov.Server.Core.dll.");
        }

        if (enumType.Fields.Any(field => string.Equals(field.Name, entry.ConstantName, StringComparison.Ordinal)))
        {
            throw new ModLoaderException($"Enum `{entry.EnumType}` already contains an entry named `{entry.ConstantName}`.");
        }

        if (enumType.Fields.Any(field => field.Constant?.InterpretData() is { } value && Convert.ToDecimal(value) == entry.ConstantValue))
        {
            throw new ModLoaderException($"Enum `{entry.EnumType}` already contains the value {entry.ConstantValue}.");
        }

        var field = new FieldDefinition(
            entry.ConstantName,
            FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
            enumType.ToTypeSignature()
        );
        field.Constant = ConvertConstant(enumType, entry);
        enumType.Fields.Add(field);
        module.TokenAllocator.AssignNextAvailableToken(field);
    }

    private static Constant ConvertConstant(TypeDefinition enumType, EnumEntryDefinition entry)
    {
        var underlyingType =
            enumType.GetEnumUnderlyingType()?.ElementType
            ?? throw new ModLoaderException($"Enum `{entry.EnumType}` has no underlying type.");

        try
        {
            return underlyingType switch
            {
                ElementType.I1 => Constant.FromValue(checked((sbyte)entry.ConstantValue)),
                ElementType.U1 => Constant.FromValue(checked((byte)entry.ConstantValue)),
                ElementType.I2 => Constant.FromValue(checked((short)entry.ConstantValue)),
                ElementType.U2 => Constant.FromValue(checked((ushort)entry.ConstantValue)),
                ElementType.I4 => Constant.FromValue(checked((int)entry.ConstantValue)),
                ElementType.U4 => Constant.FromValue(checked((uint)entry.ConstantValue)),
                ElementType.I8 => Constant.FromValue(entry.ConstantValue),
                ElementType.U8 => Constant.FromValue(checked((ulong)entry.ConstantValue)),
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
