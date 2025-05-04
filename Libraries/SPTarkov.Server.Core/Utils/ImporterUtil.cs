using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class ImporterUtil
{
    protected readonly ConcurrentDictionary<Type, Delegate> lazyLoadDeserializationCache = [];
    protected FileUtil _fileUtil;
    protected JsonUtil _jsonUtil;
    protected ISptLogger<ImporterUtil> _logger;
    protected HashSet<string> directoriesToIgnore = ["./Assets/database/locales/server"];

    protected HashSet<string> filesToIgnore =
    [
        "bearsuits.json",
        "usecsuits.json",
        "archivedquests.json",
    ];

    public ImporterUtil(ISptLogger<ImporterUtil> logger, FileUtil fileUtil, JsonUtil jsonUtil)
    {
        _logger = logger;
        _fileUtil = fileUtil;
        _jsonUtil = jsonUtil;
    }

    public Task<T> LoadRecursiveAsync<T>(
        string filepath,
        Action<string>? onReadCallback = null,
        Action<string, object>? onObjectDeserialized = null
    )
    {
        return LoadRecursiveAsync(filepath, typeof(T), onReadCallback, onObjectDeserialized)
            .ContinueWith(res =>
            {
                return (T)res.Result;
            });
    }

    /// <summary>
    ///     Load files into objects recursively (asynchronous)
    /// </summary>
    /// <param name="filepath">Path to folder with files</param>
    /// <param name="loadedType"></param>
    /// <param name="onReadCallback"></param>
    /// <param name="onObjectDeserialized"></param>
    /// <returns>Task</returns>
    protected async Task<object> LoadRecursiveAsync(
        string filepath,
        Type loadedType,
        Action<string>? onReadCallback = null,
        Action<string, object>? onObjectDeserialized = null
    )
    {
        var tasks = new List<Task>();
        var dictionaryLock = new Lock();
        var result = Activator.CreateInstance(loadedType);

        // get all filepaths
        var files = _fileUtil.GetFiles(filepath);
        var directories = _fileUtil.GetDirectories(filepath);

        // Process files
        foreach (var file in files)
        {
            if (
                _fileUtil.GetFileExtension(file) != "json"
                || filesToIgnore.Contains(_fileUtil.GetFileNameAndExtension(file).ToLower())
            )
            {
                continue;
            }

            tasks.Add(
                ProcessFileAsync(
                    file,
                    loadedType,
                    onReadCallback,
                    onObjectDeserialized,
                    result,
                    dictionaryLock
                )
            );
        }

        // Process directories
        foreach (var directory in directories)
        {
            if (directoriesToIgnore.Contains(directory))
            {
                continue;
            }

            tasks.Add(ProcessDirectoryAsync(directory, loadedType, result, dictionaryLock));
        }

        // Wait for all tasks to finish
        await Task.WhenAll(tasks);

        return result;
    }

    private async Task ProcessFileAsync(
        string file,
        Type loadedType,
        Action<string>? onReadCallback,
        Action<string, object>? onObjectDeserialized,
        object result,
        Lock dictionaryLock
    )
    {
        try
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                onReadCallback?.Invoke(file);

                // Get the set method to update the object
                var setMethod = GetSetMethod(
                    _fileUtil.StripExtension(file).ToLower(),
                    loadedType,
                    out var propertyType,
                    out var isDictionary
                );

                var fileDeserialized = await DeserializeFileAsync(fs, file, propertyType);

                onObjectDeserialized?.Invoke(file, fileDeserialized);

                lock (dictionaryLock)
                {
                    setMethod.Invoke(
                        result,
                        isDictionary
                            ? [_fileUtil.StripExtension(file), fileDeserialized]
                            : new[] { fileDeserialized }
                    );
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to deserialize or find properties on file '{file}'", ex);
        }
    }

    private async Task ProcessDirectoryAsync(
        string directory,
        Type loadedType,
        object result,
        Lock dictionaryLock
    )
    {
        try
        {
            var setMethod = GetSetMethod(
                directory.Split("/").Last().Replace("_", ""),
                loadedType,
                out var matchedProperty,
                out var isDictionary
            );

            var loadedData = await LoadRecursiveAsync($"{directory}/", matchedProperty);

            lock (dictionaryLock)
            {
                setMethod.Invoke(
                    result,
                    isDictionary ? [directory, loadedData] : new[] { loadedData }
                );
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing directory '{directory}'", ex);
        }
    }

    private async Task<object> DeserializeFileAsync(FileStream fs, string file, Type propertyType)
    {
        if (
            propertyType.IsGenericType
            && propertyType.GetGenericTypeDefinition() == typeof(LazyLoad<>)
        )
        {
            return CreateLazyLoadDeserialization(file, propertyType);
        }

        return await Task.Run(() =>
        {
            return _jsonUtil.DeserializeFromFileStream(fs, propertyType);
        });
    }

    private object CreateLazyLoadDeserialization(string file, Type propertyType)
    {
        var genericArgument = propertyType.GetGenericArguments()[0];

        var deserializeCall = Expression.Call(
            Expression.Constant(_jsonUtil),
            "DeserializeFromFile",
            Type.EmptyTypes,
            Expression.Constant(file),
            Expression.Constant(genericArgument)
        );

        var typeAsExpression = Expression.TypeAs(deserializeCall, genericArgument);

        var expression = Expression.Lambda(
            typeof(Func<>).MakeGenericType(genericArgument),
            typeAsExpression
        );

        var expressionDelegate = expression.Compile();

        return Activator.CreateInstance(propertyType, expressionDelegate);
    }

    public MethodInfo GetSetMethod(
        string propertyName,
        Type type,
        out Type propertyType,
        out bool isDictionary
    )
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
                .FirstOrDefault(prop =>
                {
                    return string.Equals(
                        prop.Name.ToLower(),
                        _fileUtil.StripExtension(propertyName).ToLower(),
                        StringComparison.Ordinal
                    );
                });

            if (matchedProperty == null)
            {
                throw new Exception(
                    $"Unable to find property '{_fileUtil.StripExtension(propertyName)}' for type '{type.Name}'"
                );
            }

            propertyType = matchedProperty.PropertyType;
            setMethod = matchedProperty.GetSetMethod();
        }

        return setMethod;
    }
}
