using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

/// <summary>
///     Provides enum prepatch data needed to apply custom enum constants
/// </summary>
public sealed record EnumEntryDefinition
{
    /// <summary>
    ///     Type of enum, full namespace must be provided.  E.g. `EFT.EBuffId` <br/><br/>
    /// When targeting nested types they should be denoted with `+`  E.g. `EFT.InventoryLogic.Weapon+EFireMode`
    /// </summary>
    [JsonPropertyName("enumType")]
    public required string EnumType { get; set; }

    /// <summary>
    ///     The name to give your new constant.
    /// This will be validated and pre-patcher will provide feedback if this name is already taken.
    /// </summary>
    [JsonPropertyName("constantName")]
    public required string ConstantName { get; set; }

    /// <summary>
    ///     Value to give your new constant.
    /// This will be validated and pre-patcher will provide feedback if this constant is already taken.
    /// This value is a long, however it will be converted to the proper type on application. E.g. int, sbyte, etc
    /// </summary>
    [JsonPropertyName("constantValue")]
    public required long ConstantValue { get; set; }

    /// <summary>
    ///     CLIENT ONLY <br/><br/>
    /// Denotes the string to give the `JsonEnumName` attribute constructor should one exist.
    /// E.g. `EFT.EBuffId`
    /// </summary>
    [JsonPropertyName("jsonEnumName")]
    public string? JsonEnumName { get; set; }
}

public sealed class ClientEnumDefinitions
{
    public IReadOnlyDictionary<string, List<EnumEntryDefinition>> Entries
    {
        get { return _entries; }
    }

    private readonly Dictionary<string, List<EnumEntryDefinition>> _entries = [];

    /// <summary>
    ///     Adds a singular definition
    /// </summary>
    /// <param name="modGuid">Mod guid that is adding this definition</param>
    /// <param name="definition">Definition to add</param>
    public void Add(string modGuid, EnumEntryDefinition definition)
    {
        if (!_entries.TryGetValue(modGuid, out var value))
        {
            _entries[modGuid] = [definition];
            return;
        }

        value.Add(definition);
    }

    /// <summary>
    ///     Adds multiple definitions
    /// </summary>
    /// <param name="modGuid">Mod guid that is adding these definitions</param>
    /// <param name="definitions">Definitions to add</param>
    public void AddRange(string modGuid, IEnumerable<EnumEntryDefinition> definitions)
    {
        if (!_entries.TryGetValue(modGuid, out var value))
        {
            _entries[modGuid] = [.. definitions];
            return;
        }

        value.AddRange(definitions);
    }
}
