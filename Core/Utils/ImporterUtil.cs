using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class ImporterUtil
{
    private readonly FileUtil _fileUtil;

    public ImporterUtil(FileUtil fileUtil)
    {
        _fileUtil = fileUtil;
    }
    
    /**
     * Load files into js objects recursively (asynchronous)
     * @param filepath Path to folder with files
     * @returns Promise<T> return T type associated with this class
     */
    public async Task<object> LoadRecursiveAsync(
        string filepath,
        Type loadedType,
        Action<string, string>? onReadCallback = null,
        Action<string, object>? onObjectDeserialized = null
    ) {
        var result = Activator.CreateInstance(loadedType);

        // get all filepaths
        var files = _fileUtil.GetFiles(filepath);
        var directories = _fileUtil.GetDirectories(filepath);

        // add file content to result
        foreach (var file in files)
        {
            if (_fileUtil.GetFileExtension(file) != "json") continue;
            // const filename = this.vfs.stripExtension(file);
            // const filePathAndName = `${filepath}${file}`;
            var fileData = await File.ReadAllTextAsync(file);
            if (onReadCallback != null)
                onReadCallback(file, fileData);

            Type propertyType;
            MethodInfo setMethod;
            bool isDictionary = false;
            if (loadedType.IsGenericType && loadedType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                propertyType = loadedType.GetGenericArguments()[1];
                setMethod = loadedType.GetMethod("Add");
                isDictionary = true;
            }
            else
            {
                var matchedProperty = loadedType.GetProperties().FirstOrDefault(prop => prop.Name.ToLower() == Path.GetFileNameWithoutExtension(file).ToLower());
                if (matchedProperty == null)
                    throw new Exception($"Unable to find property '{Path.GetFileNameWithoutExtension(file)}' for type '{loadedType.Name}'");
                propertyType = matchedProperty.PropertyType;
                setMethod = matchedProperty.GetSetMethod();
            }
            try
            {
                var fileDeserialized = JsonSerializer.Deserialize(fileData, propertyType, new JsonSerializerOptions { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow });
                if (onObjectDeserialized != null)
                    onObjectDeserialized(file, fileDeserialized);

                setMethod.Invoke(result, isDictionary ? [Path.GetFileNameWithoutExtension(file), fileDeserialized] : [fileDeserialized]);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to deserialize or set properties for file '{file}'", e);
            }
        }

        // deep tree search
        foreach (var directory in directories)
        {
            var matchedProperty = loadedType.GetProperties().FirstOrDefault(prop => prop.Name.ToLower() == directory.Split("/").Last().Replace("_", "").ToLower());
            if (matchedProperty == null)
                throw new Exception($"Unable to find property '{directory}' for type '{loadedType.Name}'");
            matchedProperty.GetSetMethod().Invoke(result, [await LoadRecursiveAsync($"{directory}/", matchedProperty.PropertyType)]);
        }

        // return the result of all async fetch
        return result;
    }

    /**
     * Load files into js objects recursively (synchronous)
     * @param filepath Path to folder with files
     * @returns
     */
    public object LoadRecursive(
        string filepath,
        Type loadedType,
        Action<string, string>? onReadCallback = null,
        Action<string, object>? onObjectDeserialized = null
    )
    {
        var result = Activator.CreateInstance(loadedType);

        // get all filepaths
        var files = Directory.GetFiles(filepath);
        var directories = Directory.GetDirectories(filepath);

        foreach (var file in files)
        {
            if (Path.GetExtension(file) == "json") 
            {
                // const filename = this.vfs.stripExtension(file);
                // const filePathAndName = `${filepath}${file}`;
                var fileData = File.ReadAllText(file);
                onReadCallback(file, fileData);
                var matchedProperty = loadedType.GetProperties().FirstOrDefault(prop => prop.Name.ToLower() == Path.GetFileNameWithoutExtension(file).ToLower());
                if (matchedProperty == null)
                    throw new Exception($"Unable to find property '{Path.GetFileNameWithoutExtension(file)}' for type '{loadedType.Name}'");
                var propertyType = matchedProperty.PropertyType;
                var fileDeserialized = JsonSerializer.Deserialize(fileData, propertyType);
                onObjectDeserialized(file, fileDeserialized);

                matchedProperty.GetSetMethod().Invoke(result, [fileDeserialized]);
            }
        }

        // deep tree search
        foreach (var directory in directories)
        {
            var matchedProperty = loadedType.GetProperties().FirstOrDefault(prop => prop.Name.ToLower() == directory.ToLower());
            if (matchedProperty == null)
                throw new Exception($"Unable to find property '{directory}' for type '{loadedType.Name}'");
            matchedProperty.GetSetMethod().Invoke(result, [LoadRecursive($"{filepath}{directory}/", matchedProperty.PropertyType)]);
        }

        // return the result of all async fetch
        return result;
    }

    public async Task<object> LoadAsync(
        string filepath,
        Type loadedType,
        string strippablePath = "",
        Action<string, string>? onReadCallback = null,
        Action<string, object>? onObjectDeserialized = null
    )
    {
        var result = Activator.CreateInstance(loadedType);
        var promises = new List<Task<object>>();
        var filesToProcess = new Queue<string>(_fileUtil.GetFiles(filepath, true));

        while (filesToProcess.Count != 0) {
            var fileNode = filesToProcess.Dequeue();
            if (fileNode == null || _fileUtil.GetFileExtension(fileNode) != "json") {
                continue;
            }

            promises.Add(File.ReadAllTextAsync(fileNode).ContinueWith(fd =>
            {
                onReadCallback(fileNode, fd.Result);
                return JsonSerializer.Deserialize(fd.Result, typeof(object));
            }));
            /*
            this.vfs
                .readFileAsync(filePathAndName)
                .then(async (fileData) => {
                    onReadCallback(filePathAndName, fileData);
                    return this.jsonUtil.deserializeWithCacheCheckAsync<any>(fileData, filePathAndName);
                })
                .then(async (fileDeserialized) => {
                    onObjectDeserialized(filePathAndName, fileDeserialized);
                    const strippedFilePath = this.vfs.stripExtension(filePathAndName).replace(filepath, "");
                    this.placeObject(fileDeserialized, strippedFilePath, result, strippablePath);
                })
                .then(() => progressWriter.increment()),
        );*/
        }

        //await JSType.Promise<>.all(promises).catch((e) => console.error(e));

        return result;
    }

    /*
    protected placeObject<T>(fileDeserialized: any, strippedFilePath: string, result: T, strippablePath: string): void {
        const strippedFinalPath = strippedFilePath.replace(strippablePath, "");
        let temp = result;
        const propertiesToVisit = strippedFinalPath.split("/");
        for (let i = 0; i < propertiesToVisit.length; i++) {
            const property = propertiesToVisit[i];

            if (i === propertiesToVisit.length - 1) {
                temp[property] = fileDeserialized;
            } else {
                if (!temp[property]) {
                    temp[property] = {};
                }
                temp = temp[property];
            }
        }
    }*/
}