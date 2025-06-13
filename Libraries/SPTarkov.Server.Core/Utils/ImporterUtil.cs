using System.Linq.Expressions;
using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class ImporterUtil(ISptLogger<ImporterUtil> _logger, FileUtil _fileUtil, JsonUtil _jsonUtil)
{
    protected HashSet<string> directoriesToIgnore = ["./Assets/database/locales/server"];

    protected HashSet<string> filesToIgnore = ["bearsuits.json", "usecsuits.json", "archivedquests.json"];

    public async Task<T> LoadRecursiveAsync<T>(
        string filePath,
        Func<string, Task>? onReadCallback = null,
        Func<string, object, Task>? onObjectDeserialized = null
    )
    {
        var result = await LoadRecursiveAsync(filePath, typeof(T), onReadCallback, onObjectDeserialized);

        return (T) result;
    }

    /// <summary>
    ///     Load files into objects recursively (asynchronous)
    /// </summary>
    /// <param name="filePath">Path to folder with files</param>
    /// <param name="loadedType"></param>
    /// <param name="onReadCallback"></param>
    /// <param name="onObjectDeserialized"></param>
    /// <returns>Task</returns>
    protected async Task<object> LoadRecursiveAsync(
        string filePath,
        Type loadedType,
        Func<string, Task>? onReadCallback = null,
        Func<string, object, Task>? onObjectDeserialized = null
    )
    {
        var tasks = new List<Task>();
        var dictionaryLock = new Lock();
        var result = Activator.CreateInstance(loadedType);

        // get all filepaths
        var files = _fileUtil.GetFiles(filePath);
        var directories = _fileUtil.GetDirectories(filePath);

        // Process files
        foreach (var file in files)
        {
            if (_fileUtil.GetFileExtension(file) != "json" || filesToIgnore.Contains(_fileUtil.GetFileNameAndExtension(file).ToLower()))
            {
                continue;
            }

            tasks.Add(ProcessFileAsync(file, loadedType, onReadCallback, onObjectDeserialized, result, dictionaryLock));
        }

        // Process directories
        foreach (var directory in directories)
        {
            if (directoriesToIgnore.Contains(directory))
            {
                continue;
            }

            tasks.Add(ProcessDirectoryAsync(directory, loadedType, result, onReadCallback, onObjectDeserialized, dictionaryLock));
        }

        // Wait for all tasks to finish
        await Task.WhenAll(tasks);

        return result;
    }

    private async Task ProcessFileAsync(
        string file,
        Type loadedType,
        Func<string, Task>? onReadCallback,
        Func<string, object, Task>? onObjectDeserialized,
        object result,
        Lock dictionaryLock
    )
    {
        try
        {
            if (onReadCallback != null)
            {
                await onReadCallback(file);
            }

            // Get the set method to update the object
            var setMethod = GetSetMethod(
                _fileUtil.StripExtension(file).ToLower(),
                loadedType,
                out var propertyType,
                out var isDictionary
            );

            var fileDeserialized = await DeserializeFileAsync(file, propertyType);

            if (onObjectDeserialized != null)
            {
                await onObjectDeserialized(file, fileDeserialized);
            }

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
        catch (Exception ex)
        {
            throw new Exception($"Unable to deserialize or find properties on file '{file}'", ex);
        }
    }

    private async Task ProcessDirectoryAsync(
        string directory,
        Type loadedType,
        object result,
        Func<string, Task>? onReadCallback,
        Func<string, object, Task>? onObjectDeserialized,
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

            var loadedData = await LoadRecursiveAsync($"{directory}/", matchedProperty, onReadCallback, onObjectDeserialized);

            lock (dictionaryLock)
            {
                setMethod.Invoke(result, isDictionary ? [directory, loadedData] : new[] { loadedData });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing directory '{directory}'", ex);
        }
    }

    private async Task<object> DeserializeFileAsync(string file, Type propertyType)
    {
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(LazyLoad<>))
        {
            return CreateLazyLoadDeserialization(file, propertyType);
        }

        return await _jsonUtil.DeserializeFromFileAsync(file, propertyType);
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
                .FirstOrDefault(prop =>
                    string.Equals(
                        prop.Name.ToLower(),
                        _fileUtil.StripExtension(propertyName).ToLower(),
                        StringComparison.Ordinal
                    )
                );

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
