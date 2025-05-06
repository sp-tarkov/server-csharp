using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class UtilityHelper
{
    public List<T> ArrayIntersect<T>(List<T> a, List<T> b)
    {
        //a.Intersect(x => b.Contains(x)).ToList();
        // gives error Delegate type could not be infered

        return a.Where(x => b.Contains(x)).ToList();
    }
}
