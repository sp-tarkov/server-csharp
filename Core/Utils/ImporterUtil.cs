using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Utils.Json.Converters;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class ImporterUtil
{
    private readonly FileUtil _fileUtil;
    private readonly JsonUtil _jsonUtil;

    private readonly HashSet<string> filesToIgnore = ["bearsuits.json", "usecsuits.json", "archivedquests.json"];
    
    public ImporterUtil(FileUtil fileUtil, JsonUtil jsonUtil)
    {
        _fileUtil = fileUtil;
        _jsonUtil = jsonUtil;
    }

    /**
     * Load files into js objects recursively (asynchronous)
     * @param filepath Path to folder with files
     * @returns Promise<T> return T type associated with this class
     */
    public Task<object> LoadRecursiveAsync(
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
            if (filesToIgnore.Contains(_fileUtil.GetFileName(file).ToLower())) continue;
            tasks.Add(
                Task.Factory.StartNew(() =>
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
                        var fileDeserialized = _jsonUtil.Deserialize(fileData, propertyType);
                        if (onObjectDeserialized != null)
                            onObjectDeserialized(file, fileDeserialized);

                        lock (dictionaryLock)
                        {
                            setMethod.Invoke(result,
                                isDictionary ? [_fileUtil.StripExtension(file), fileDeserialized] : [fileDeserialized]);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Unable to deserialize or set properties for file '{file}'", e);
                    }
                })
            );
        }

        // deep tree search
        foreach (var directory in directories)
        {
            tasks.Add(
                Task.Factory.StartNew(() =>
                {
                    var setMethod = GetSetMethod(directory.Split("/").Last().Replace("_", ""), loadedType, out var matchedProperty, out var isDictionary);
                    var loadTask = LoadRecursiveAsync($"{directory}/", matchedProperty);
                    loadTask.Wait();
                    lock (dictionaryLock)
                    {
                        setMethod.Invoke(result, isDictionary ? [directory, loadTask.Result] : [loadTask.Result]);
                    }
                })
            );
        }

        // return the result of all async fetch
        return Task.WhenAll(tasks).ContinueWith((t) =>
        {
            if (t.IsCanceled || t.IsFaulted)
                tasks.Where(t => t.IsFaulted || t.IsCanceled).ToList().ForEach(t => Console.WriteLine(t.Exception));
        }).ContinueWith(_ => result);
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
                    prop.Name.ToLower() == _fileUtil.StripExtension(propertyName).ToLower());
            if (matchedProperty == null)
                throw new Exception(
                    $"Unable to find property '{_fileUtil.StripExtension(propertyName)}' for type '{type.Name}'");
            propertyType = matchedProperty.PropertyType;
            setMethod = matchedProperty.GetSetMethod();
        }
        return setMethod;
    }
}
