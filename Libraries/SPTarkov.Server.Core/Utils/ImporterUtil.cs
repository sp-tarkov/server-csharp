using System.Collections;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Database;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public sealed class ImporterUtil(ISptLogger<ImporterUtil> logger, FileUtil fileUtil, JsonUtil jsonUtil)
{
    private readonly FrozenSet<string> _directoriesToIgnore = ["./SPT_Data/database/locales/server", "./SPT_Data/database/locales/web"];
    private readonly FrozenSet<string> _filesToIgnore = ["bearsuits.json", "usecsuits.json", "archivedquests.json"];

    public async Task<T> LoadRecursiveAsync<T>(
        string filePath,
        Func<string, CancellationToken, Task>? onReadCallback = null,
        Func<string, object, CancellationToken, Task>? onObjectDeserialized = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await LoadRecursiveAsync(filePath, typeof(T), onReadCallback, onObjectDeserialized, cancellationToken);

        return (T)result;
    }

    /// <summary>
    ///     Load files into objects recursively (asynchronous)
    /// </summary>
    /// <param name="filePath">Path to folder with files</param>
    /// <param name="loadedType"></param>
    /// <param name="onReadCallback"></param>
    /// <param name="onObjectDeserialized"></param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the loading operation.
    /// </param>
    /// <returns>Task</returns>
    private async Task<object> LoadRecursiveAsync(
        string filePath,
        Type loadedType,
        Func<string, CancellationToken, Task>? onReadCallback = null,
        Func<string, object, CancellationToken, Task>? onObjectDeserialized = null,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tasks = new List<Task>();
        var dictionaryLock = new Lock();
        var result = Activator.CreateInstance(loadedType);

        // get all filepaths
        var files = fileUtil.GetFiles(filePath);
        var directories = fileUtil.GetDirectories(filePath);

        // Process files
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (
                fileUtil.GetFileExtension(file) != "json"
                || _filesToIgnore.Contains(fileUtil.GetFileNameAndExtension(file).ToLowerInvariant())
            )
            {
                continue;
            }

            tasks.Add(ProcessFileAsync(file, loadedType, onReadCallback, onObjectDeserialized, result, dictionaryLock, cancellationToken));
        }

        // Process directories
        foreach (var directory in directories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_directoriesToIgnore.Contains(directory))
            {
                continue;
            }

            tasks.Add(
                ProcessDirectoryAsync(
                    directory,
                    loadedType,
                    result,
                    onReadCallback,
                    onObjectDeserialized,
                    dictionaryLock,
                    cancellationToken
                )
            );
        }

        // Wait for all tasks to finish
        await Task.WhenAll(tasks);

        return result;
    }

    private async Task ProcessFileAsync(
        string file,
        Type loadedType,
        Func<string, CancellationToken, Task>? onReadCallback,
        Func<string, object, CancellationToken, Task>? onObjectDeserialized,
        object result,
        Lock dictionaryLock,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (onReadCallback != null)
            {
                await onReadCallback(file, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Get the set method to update the object
            var setMethod = GetSetMethod(
                fileUtil.StripExtension(file).ToLowerInvariant(),
                loadedType,
                out var propertyType,
                out var isDictionary
            );

            var fileDeserialized = await DeserializeFileAsync(file, propertyType, cancellationToken);

            if (onObjectDeserialized != null)
            {
                await onObjectDeserialized(file, fileDeserialized, cancellationToken);
            }

            lock (dictionaryLock)
            {
                setMethod.Invoke(result, isDictionary ? [fileUtil.StripExtension(file), fileDeserialized] : [fileDeserialized]);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ValidationErrorException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Critical($"Unable to deserialize or find properties on file '{file}'", ex);
            throw new Exception($"Unable to deserialize or find properties on file '{file}'", ex);
        }
    }

    private async Task ProcessDirectoryAsync(
        string directory,
        Type loadedType,
        object result,
        Func<string, CancellationToken, Task>? onReadCallback,
        Func<string, object, CancellationToken, Task>? onObjectDeserialized,
        Lock dictionaryLock,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var directoryName = directory.Split("/").Last().Replace("_", "");

            if (MongoId.IsValidMongoId(directoryName))
            {
                // For trader MongoId directories, we need to get the parent property. Get parent directory name to find the property
                var parentDirectory = directory.Substring(0, directory.LastIndexOf('/'));
                var parentName = parentDirectory.Split("/").Last().Replace("_", "");

                GetSetMethod(parentName, loadedType, out var matchedProperty, out _);

                var loadedData = await LoadRecursiveAsync(
                    $"{directory}/",
                    matchedProperty,
                    onReadCallback,
                    onObjectDeserialized,
                    cancellationToken
                );

                cancellationToken.ThrowIfCancellationRequested();

                lock (dictionaryLock)
                {
                    // Traders already have a dictionary, so we only need to handle this here
                    if (result is IDictionary dictionary)
                    {
                        dictionary[new MongoId(directoryName)] = loadedData;
                    }
                }
            }
            else
            {
                var setMethod = GetSetMethod(directoryName, loadedType, out var matchedProperty, out var isDictionary);

                var loadedData = await LoadRecursiveAsync(
                    $"{directory}/",
                    matchedProperty,
                    onReadCallback,
                    onObjectDeserialized,
                    cancellationToken
                );

                lock (dictionaryLock)
                {
                    setMethod.Invoke(result, isDictionary ? [directory, loadedData] : [loadedData]);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing directory '{directory}'", ex);
        }
    }

    private async Task<object> DeserializeFileAsync(string file, Type propertyType, CancellationToken cancellationToken = default)
    {
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(LazyLoad<>))
        {
            return CreateLazyLoadDeserialization(file, propertyType);
        }

        return await jsonUtil.DeserializeFromFileAsync(file, propertyType, cancellationToken);
    }

    private object CreateLazyLoadDeserialization(string file, Type propertyType)
    {
        var genericArgument = propertyType.GetGenericArguments()[0];

        var deserializeCall = Expression.Call(
            Expression.Constant(jsonUtil),
            "DeserializeFromFile",
            Type.EmptyTypes,
            Expression.Constant(file),
            Expression.Constant(genericArgument)
        );

        var typeAsExpression = Expression.TypeAs(deserializeCall, genericArgument);

        var expression = Expression.Lambda(typeof(Func<>).MakeGenericType(genericArgument), typeAsExpression);

        var expressionDelegate = expression.Compile();

        return Activator.CreateInstance(propertyType, expressionDelegate);
    }

    public MethodInfo GetSetMethod(string propertyName, Type type, out Type propertyType, out bool isDictionary)
    {
        MethodInfo? setMethod;
        isDictionary = false;

        if (TryGetDictionaryValueType(type, out var dictionaryValueType))
        {
            propertyType = dictionaryValueType;
            setMethod = type.GetMethod("Add") ?? throw new Exception($"Unable to find Add method for dictionary type '{type.Name}'");
            isDictionary = true;
        }
        else
        {
            var strippedPropertyName = fileUtil.StripExtension(propertyName);

            var matchedProperty =
                type.GetProperties()
                    .FirstOrDefault(prop =>
                        string.Equals(prop.Name.ToLowerInvariant(), strippedPropertyName.ToLowerInvariant(), StringComparison.Ordinal)
                    )
                ?? throw new Exception($"Unable to find property '{strippedPropertyName}' for type '{type.Name}'");
            propertyType = matchedProperty.PropertyType;
            setMethod =
                matchedProperty.GetSetMethod()
                ?? throw new Exception($"Unable to find setter for property '{matchedProperty.Name}' on type '{type.Name}'");
        }

        return setMethod;
    }

    private static bool TryGetDictionaryValueType(Type type, out Type? valueType)
    {
        var currentType = type;

        while (currentType is not null)
        {
            if (currentType.IsGenericType)
            {
                if (currentType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    valueType = currentType.GetGenericArguments()[1];
                    return true;
                }
            }

            currentType = currentType.BaseType;
        }

        valueType = null;
        return false;
    }
}
