using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models;

[Injectable]
public record Prapor : ITrader
{


    public string Name { get; } = "Prapor";
    public MongoId Id { get; } = Traders.PRAPOR;
}

[Injectable]
public record Therapist : ITrader
{


    public string Name { get; } = "Therapist";
    public MongoId Id { get; } = Traders.THERAPIST;
}

[Injectable]
public record Fence : ITrader
{


    public string Name { get; } = "Fence";
    public MongoId Id { get; } = Traders.FENCE;
}

[Injectable]
public record Skier : ITrader
{


    public string Name { get; } = "Skier";
    public MongoId Id { get; } = Traders.SKIER;
}

[Injectable]
public record Peacekeeper : ITrader
{


    public string Name { get; } = "Peacekeeper";
    public MongoId Id { get; } = Traders.PEACEKEEPER;
}

[Injectable]
public record Mechanic : ITrader
{


    public string Name { get; } = "Mechanic";
    public MongoId Id { get; } = Traders.MECHANIC;
}

[Injectable]
public record Ragman : ITrader
{


    public string Name { get; } = "Ragman";
    public MongoId Id { get; } = Traders.RAGMAN;
}

[Injectable]
public record Jaeger : ITrader
{


    public string Name { get; } = "Jaeger";
    public MongoId Id { get; } = Traders.JAEGER;
}

[Injectable]
public record LighthouseKeeper : ITrader
{


    public string Name { get; } = "LighthouseKeeper";
    public MongoId Id { get; } = Traders.LIGHTHOUSEKEEPER;
}

[Injectable]
public record Btr : ITrader
{


    public string Name { get; } = "Btr";
    public MongoId Id { get; } = Traders.BTR;
}

[Injectable]
public record Ref : ITrader
{


    public string Name { get; } = "Ref";
    public MongoId Id { get; } = Traders.REF;
}
