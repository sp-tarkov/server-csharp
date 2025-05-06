using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Utils.Cloners;

[Injectable]
public class FastCloner : ICloner
{
    public T? Clone<T>(T? obj)
    {
        return global::FastCloner.FastCloner.DeepClone(obj);
    }
}
