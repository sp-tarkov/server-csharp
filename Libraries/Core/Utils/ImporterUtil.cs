using System.Linq.Expressions;
using System.Reflection;
using SptCommon.Annotations;
using Core.Models.Utils;
using Core.Utils.Json;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class ImporterUtil
{
    protected FileUtil _fileUtil;
    protected JsonUtil _jsonUtil;
    protected ISptLogger<ImporterUtil> _logger;
    protected HashSet<string> directoriesToIgnore = ["./Assets/database/locales/server"];

    protected HashSet<string> filesToIgnore = ["bearsuits.json", "usecsuits.json", "archivedquests.json"];

    public ImporterUtil(ISptLogger<ImporterUtil> logger, FileUtil fileUtil, JsonUtil jsonUtil)
    {
        _logger = logger;
        _fileUtil = fileUtil;
        _jsonUtil = jsonUtil;
    }

    public Task<T> LoadRecursiveAsync<T>(
        string filepath,
        Action<string, string>? onReadCallback = null,
        Action<string, object>? onObjectDeserialized = null
    )
    {
        return LoadRecursiveAsync(filepath, typeof(T), onReadCallback, onObjectDeserialized)
            .ContinueWith(res => (T)res.Result);
    }

    /**
     * Load files into js objects recursively (asynchronous)
     * @param filepath Path to folder with files
     * @returns Promise<T> return T type associated with this class
     */
    protected Task<object> LoadRecursiveAsync(
        string filepath,
        Type loadedType,
        Action<string, string>? onReadCallback = null,
        Action<string, object>? onObjectDeserialized = null
    )
    {
        var tasks = new List<Task>();
        var dictionaryLock = new object();
        var result = Activator.CreateInstance(loadedType);

        // get all filepaths
        var files = _fileUtil.GetFiles(filepath);
        var directories = _fileUtil.GetDirectories(filepath);

        // add file content to result
        foreach (var file in files)
        {
            if (_fileUtil.GetFileExtension(file) != "json") continue;
            if (filesToIgnore.Contains(_fileUtil.GetFileNameAndExtension(file).ToLower())) continue;
            tasks.Add(
                Task.Factory.StartNew(
                    () =>
                    {
                        var fileData = _fileUtil.ReadFile(file);
                        if (onReadCallback != null)
                            onReadCallback(file, fileData);

                        var setMethod = GetSetMethod(
                            _fileUtil.StripExtension(file).ToLower(),
                            loadedType,
                            out var propertyType,
                            out var isDictionary
                        );
                        try
                        {
                            object fileDeserialized = null;
                            if (propertyType.IsGenericType &&
                                propertyType.GetGenericTypeDefinition() == typeof(LazyLoad<>))
                            {
                                // This expression is create a generic type delegate for lazy loading a LazyLoad type
                                var expression = Expression.Lambda(
                                        // this is the expected type of the lambda which is a function of whatever generic type LazyLoad<> is
                                        typeof(Func<>).MakeGenericType(propertyType.GetGenericArguments()),
                                        // An expression block will have a return type and then will execute the expression
                                        Expression.Block(
                                            // this is the return type
                                            propertyType.GetGenericArguments()[0],
                                            // this is the expression
                                            // This expression casts the result of the Call expression as the generic argument type
                                            Expression.TypeAs(
                                                // this expression calls the json util Deserialize method
                                                Expression.Call(
                                                    Expression.Constant(_jsonUtil),
                                                    "Deserialize",
                                                    [],
                                                    [Expression.Constant(fileData), Expression.Constant(propertyType.GetGenericArguments()[0])]
                                                ),
                                                propertyType.GetGenericArguments()[0]
                                            )
                                        )
                                    )
                                    .Compile();
                                fileDeserialized = Activator.CreateInstance(propertyType, expression);
                            }
                            else
                            {
                                fileDeserialized = _jsonUtil.Deserialize(fileData, propertyType);
                            }

                            if (onObjectDeserialized != null)
                                onObjectDeserialized(file, fileDeserialized);

                            lock (dictionaryLock)
                            {
                                setMethod.Invoke(
                                    result,
                                    isDictionary
                                        ? [_fileUtil.StripExtension(file), fileDeserialized]
                                        : [fileDeserialized]
                                );
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Unable to deserialize or set properties for file '{file}'", e);
                        }
                    }
                )
            );
        }

        // deep tree search
        foreach (var directory in directories)
        {
            if (directoriesToIgnore.Contains(directory)) continue;
            tasks.Add(
                Task.Factory.StartNew(
                    () =>
                    {
                        var setMethod = GetSetMethod(
                            directory.Split("/").Last().Replace("_", ""),
                            loadedType,
                            out var matchedProperty,
                            out var isDictionary
                        );
                        var loadTask = LoadRecursiveAsync($"{directory}/", matchedProperty);
                        loadTask.Wait();
                        lock (dictionaryLock)
                        {
                            setMethod.Invoke(result, isDictionary ? [directory, loadTask.Result] : [loadTask.Result]);
                        }
                    }
                )
            );
        }

        // return the result of all async fetch
        return Task.WhenAll(tasks)
            .ContinueWith(
                (t) =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        var exceptionList = tasks.Where(t => t.IsFaulted || t.IsCanceled)
                            .Select(t => t.Exception!)
                            .ToList();
                        throw new Exception(
                            "Error processing one or more DatabaseFiles",
                            new AggregateException(exceptionList)
                        );
                    }
                }
            )
            .ContinueWith(
                t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                        throw t.Exception!;
                    return result;
                }
            );
    }

    public MethodInfo GetSetMethod(string propertyName, Type type, out Type propertyType, out bool isDictionary)
    {
        MethodInfo setMethod;
        isDictionary = false;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            propertyType = type.GetGenericArguments()[1];
            setMethod = type.GetMethod("Add");
            isDictionary = true;
        }
        else
        {
            var matchedProperty = type.GetProperties()
                .FirstOrDefault(
                    prop =>
                        string.Equals(
                            prop.Name.ToLower(),
                            _fileUtil.StripExtension(propertyName).ToLower(),
                            StringComparison.Ordinal
                        )
                );
            if (matchedProperty == null)
                throw new Exception(
                    $"Unable to find property '{_fileUtil.StripExtension(propertyName)}' for type '{type.Name}'"
                );
            propertyType = matchedProperty.PropertyType;
            setMethod = matchedProperty.GetSetMethod();
        }

        return setMethod;
    }
}
