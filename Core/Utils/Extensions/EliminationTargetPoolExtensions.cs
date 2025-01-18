using Core.Models.Spt.Repeatable;

namespace Core.Utils.Extensions
{
    public static class EliminationTargetPoolExtensions
    {
        public static void Remove(this EliminationTargetPool pool, string key)
        {
            pool[key] = null;
        }
    }
}
