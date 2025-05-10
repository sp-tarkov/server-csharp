using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models;

public record Prapor() : ITrader
{
    public string Name { get; } = "Prapor";
    public string Id { get; } = Traders.PRAPOR;
}

public record Therapist() : ITrader
{
    public string Name { get; } = "Therapist";
    public string Id { get; } = Traders.THERAPIST;
}

public record Fence() : ITrader
{
    public string Name { get; } = "Fence";
    public string Id { get; } = Traders.FENCE;
}

public record Skier() : ITrader
{
    public string Name { get; } = "Skier";
    public string Id { get; } = Traders.SKIER;
}

public record Peacekeeper() : ITrader
{
    public string Name { get; } = "Peacekeeper";
    public string Id { get; } = Traders.PEACEKEEPER;
}

public record Mechanic() : ITrader
{
    public string Name { get; } = "Mechanic";
    public string Id { get; } = Traders.MECHANIC;
}

public record Ragman() : ITrader
{
    public string Name { get; } = "Ragman";
    public string Id { get; } = Traders.RAGMAN;
}

public record Jaeger() : ITrader
{
    public string Name { get; } = "Jaeger";
    public string Id { get; } = Traders.JAEGER;
}

public record LighthouseKeeper() : ITrader
{
    public string Name { get; } = "LighthouseKeeper";
    public string Id { get; } = Traders.LIGHTHOUSEKEEPER;
}

public record Btr() : ITrader
{
    public string Name { get; } = "Btr";
    public string Id { get; } = Traders.BTR;
}

public record Ref() : ITrader
{
    public string Name { get; } = "Ref";
    public string Id { get; } = Traders.REF;
}
