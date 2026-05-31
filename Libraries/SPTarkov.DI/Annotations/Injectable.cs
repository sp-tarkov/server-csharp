using JetBrains.Annotations;

namespace SPTarkov.DI.Annotations;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[MeansImplicitUse]
public class Injectable(InjectionType injectionType = InjectionType.Transient, int typePriority = int.MaxValue) : Attribute
{
    public InjectionType InjectionType { get; init; } = injectionType;

    public int TypePriority { get; init; } = typePriority;
}

public enum InjectionType
{
    HostedService,
    Singleton,
    Transient,
    Scoped,
}
