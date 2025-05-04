﻿using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Customization;

public record CustomizationSetRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("customizations")]
    public List<CustomizationSetOption>? Customizations { get; set; }
}

public record CustomizationSetOption
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
