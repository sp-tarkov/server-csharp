using SPTarkov.DI.Annotations;

namespace SPTarkov.DI.Extensions;

public static class DependencyInjectionExtensions
{
    public static int GetTypePriority<T>(this T type)
    {
        if (type is null)
        {
            return int.MaxValue;
        }

        var attribute = Attribute.GetCustomAttribute(type.GetType(), typeof(Injectable)) as Injectable;

        return attribute?.TypePriority ?? int.MaxValue;
    }
}
