namespace Core.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public class Injectable(InjectionType injectionType = InjectionType.Scoped, Type? type = null) : Attribute
{
    public InjectionType InjectionType { get; set; } = injectionType;
    public Type? InjectableTypeOverride { get; set; } = type;
}

public enum InjectionType
{
    Singleton,
    Transient,
    Scoped
}