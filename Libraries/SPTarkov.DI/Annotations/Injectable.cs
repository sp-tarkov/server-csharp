namespace SPTarkov.DI.Annotations;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class Injectable(InjectionType injectionType = InjectionType.Scoped, Type? type = null, int typePriority = int.MaxValue) : Attribute
{
    public InjectionType InjectionType
    {
        get;
        set;
    } = injectionType;

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
