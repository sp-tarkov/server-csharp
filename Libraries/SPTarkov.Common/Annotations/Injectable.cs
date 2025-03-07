namespace SPTarkov.Common.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class Injectable(InjectionType injectionType = InjectionType.Scoped, Type? type = null, int typePriority = int.MaxValue) : Attribute
{
    public InjectionType InjectionType
    {
        get;
        set;
    } = injectionType;

    public Type? InjectableTypeOverride
    {
        get;
        set;
    } = type;

    public int TypePriority
    {
        get;
        set;
    } = typePriority;
}

public enum InjectionType
{
    Singleton,
    Transient,
    Scoped
}
