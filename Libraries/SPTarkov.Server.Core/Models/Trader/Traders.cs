using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models;

[Injectable]
public record Prapor() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Prapor";
    public string Id { get; } = Traders.PRAPOR;
}

[Injectable]
public record Therapist() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Therapist";
    public string Id { get; } = Traders.THERAPIST;
}

[Injectable]
public record Fence() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Fence";
    public string Id { get; } = Traders.FENCE;
}

[Injectable]
public record Skier() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Skier";
    public string Id { get; } = Traders.SKIER;
}

[Injectable]
public record Peacekeeper() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Peacekeeper";
    public string Id { get; } = Traders.PEACEKEEPER;
}

[Injectable]
public record Mechanic() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Mechanic";
    public string Id { get; } = Traders.MECHANIC;
}

[Injectable]
public record Ragman() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Ragman";
    public string Id { get; } = Traders.RAGMAN;
}

[Injectable]
public record Jaeger() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Jaeger";
    public string Id { get; } = Traders.JAEGER;
}

[Injectable]
public record LighthouseKeeper() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "LighthouseKeeper";
    public string Id { get; } = Traders.LIGHTHOUSEKEEPER;
}

[Injectable]
public record Btr() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Btr";
    public string Id { get; } = Traders.BTR;
}

[Injectable]
public record Ref() : ITrader
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public string Name { get; } = "Ref";
    public string Id { get; } = Traders.REF;
}
