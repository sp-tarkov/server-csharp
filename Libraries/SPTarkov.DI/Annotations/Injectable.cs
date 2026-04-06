using JetBrains.Annotations;

namespace SPTarkov.DI.Annotations;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[MeansImplicitUse]
public class Injectable(InjectionType injectionType = InjectionType.Transient, Type? typeOverride = null, int typePriority = int.MaxValue)
    : Attribute
{
    public InjectionType InjectionType { get; init; } = injectionType;

    public int TypePriority { get; init; } = typePriority;

    public Type? TypeOverride { get; init; } = typeOverride;
}

public enum InjectionType
{
    HostedService,
    Singleton,
    Transient,
    Scoped,
}
