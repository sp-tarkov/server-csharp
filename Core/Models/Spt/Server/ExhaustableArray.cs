// using System.Collections.Generic;
//
// namespace Types.Models.Spt.Server;
//
// public class ExhaustableArray<T>
// {
//     private List<T> pool;
//
//     public ExhaustableArray(List<T> itemPool, RandomUtil randomUtil, ICloner cloner)
//     {
//         this.pool = cloner.Clone(itemPool);
//     }
//
//     public T GetRandomValue()
//     {
//         if (pool == null || pool.Count == 0)
//         {
//             return default;
//         }
//
//         int index = randomUtil.GetInt(0, pool.Count - 1);
//         T toReturn = cloner.Clone(pool[index]);
//         pool.RemoveAt(index);
//         return toReturn;
//     }
//
//     public T GetFirstValue()
//     {
//         if (pool == null || pool.Count == 0)
//         {
//             return default;
//         }
//
//         T toReturn = cloner.Clone(pool[0]);
//         pool.RemoveAt(0);
//         return toReturn;
//     }
//
//     public bool HasValues()
//     {
//         return pool != null && pool.Count > 0;
//     }
// }

// TODO: Convert this to C# properly