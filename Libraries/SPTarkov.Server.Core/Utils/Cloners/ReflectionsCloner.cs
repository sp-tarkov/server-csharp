using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Json;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils.Cloners;

/// <summary>
///     Not in use at the moment
/// </summary>
/// <param name="logger"></param>
public class ReflectionsCloner(ISptLogger<ReflectionsCloner> logger) : ICloner
{
    private static readonly Dictionary<Type, MemberInfo[]> MemberInfoCache = new();
    private static readonly Dictionary<Type, MethodInfo> AddMethodInfoCache = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo> _itemPropertyInfoCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo> _listPropertyInfoCache = new();

    public T? Clone<T>(T? obj)
    {
        return (T?) Clone(obj, typeof(T)).Result;
    }

    public async Task<object?> Clone(object? obj, Type objectType)
    {
        // if its null, primitive, enum or string, just return the object
        if (obj == null)
        {
            return obj;
        }

        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            objectType = Nullable.GetUnderlyingType(objectType);
        }

        if (objectType.IsPrimitive || objectType.IsEnum || obj is string)
        {
            return obj;
        }

        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ListOrT<>))
        {
            return await HandleSpecialClones(obj, objectType);
        }

        var result = Activator.CreateInstance(objectType);

        if (obj is IList listToClone)
        {
            foreach (var toClone in listToClone)
            {
                var cloned = await Clone(toClone, toClone.GetType());
                (result as IList).Add(cloned);
            }
        }
        else if (obj is IDictionary dictionaryToClone)
        {
            foreach (DictionaryEntry entryToClone in dictionaryToClone)
            {
                var clonedKey = await Clone(entryToClone.Key, entryToClone.Key.GetType());
                var clonedValue = await Clone(entryToClone.Value, entryToClone.Value.GetType());
                (result as IDictionary).Add(clonedKey, clonedValue);
            }
        }
        else if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(HashSet<>))
        {
            if (!AddMethodInfoCache.TryGetValue(objectType, out var addMethodInfo))
            {
                addMethodInfo = objectType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                while (!AddMethodInfoCache.TryAdd(objectType, addMethodInfo))
                {
                    ;
                }
            }

            var toCloneEnumerable = (IEnumerable) obj;

            foreach (var toClone in toCloneEnumerable)
            {
                try
                {
                    var cloned = await Clone(toClone, toClone.GetType());
                    addMethodInfo.Invoke(result, [cloned]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
        else if (objectType.IsClass)
        {
            if (!MemberInfoCache.TryGetValue(objectType, out var memberInfos))
            {
                memberInfos = objectType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
                while (!MemberInfoCache.TryAdd(objectType, memberInfos))
                {
                    ;
                }
            }

            foreach (var member in memberInfos)
            {
                try
                {
                    switch (member)
                    {
                        case PropertyInfo propertyInfo:

                            var propertyValue = propertyInfo.GetValue(obj, null);
                            var propertyCloned = await Clone(propertyValue, propertyInfo.PropertyType);
                            propertyInfo.SetValue(result, propertyCloned, null);


                            break;
                        case FieldInfo fieldInfo:

                            var fieldValue = fieldInfo.GetValue(obj);
                            var fieldCloned = await Clone(fieldValue, fieldInfo.FieldType);
                            fieldInfo.SetValue(result, fieldCloned);


                            break;
                        case MemberInfo:
                            break;
                        default:
                            if (logger.IsLogEnabled(LogLevel.Debug))
                            {
                                logger.Debug($"Unknown member type {member.Name} {member.MemberType}");
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
        else
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Clone of type {objectType} is not supported");
            }
        }

        return result;
    }

    private async Task<object?> HandleSpecialClones(object obj, Type objectType)
    {
        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ListOrT<>))
        {
            var clone = Activator.CreateInstance(objectType, true);
            var type = objectType.GetGenericArguments()[0];
            if (!_itemPropertyInfoCache.TryGetValue(type, out var item))
            {
                item = objectType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
                while (!_itemPropertyInfoCache.TryAdd(type, item))
                {
                    ;
                }
            }

            if (!_listPropertyInfoCache.TryGetValue(type, out var list))
            {
                list = objectType.GetProperty("List", BindingFlags.Public | BindingFlags.Instance);
                while (!_listPropertyInfoCache.TryAdd(type, list))
                {
                    ;
                }
            }

            item.GetSetMethod(true).Invoke(clone, [await Clone(item.GetValue(obj), item.PropertyType)]);
            list.GetSetMethod(true).Invoke(clone, [await Clone(list.GetValue(obj), list.PropertyType)]);
            return clone;
        }

        return null;
    }
}
