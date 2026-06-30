using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Server;

[Injectable]
public class ModHelper(FileUtil fileUtil, JsonUtil jsonUtil)
{
    public string GetAbsolutePathToModFolder(Assembly modAssembly)
    {
        // The full path to the mod folder
        return Path.GetDirectoryName(modAssembly.Location);
    }

    public string GetAbsolutePathToModFolder()
    {
        return GetAbsolutePathToModFolder(Assembly.GetCallingAssembly());
    }

    public string GetRawFileData(string pathToFile, string fileName)
    {
        // Read the content of the config file as a string
        return fileUtil.ReadFile(Path.Combine(pathToFile, fileName));
    }

    public string GetRawModFileData(string pathToFile, string fileName)
    {
        var callingModFolder = GetAbsolutePathToModFolder(Assembly.GetCallingAssembly());
        return GetRawFileData(Path.Combine(callingModFolder, pathToFile), fileName);
    }

    public T GetJsonDataFromFile<T>(string pathToFile, string fileName)
    {
        // Read the content of the config file as a string
        var rawContent = fileUtil.ReadFile(Path.Combine(pathToFile, fileName));

        // Take the string above and deserialise it into a file with a type (defined between the diamond brackets)
        return jsonUtil.Deserialize<T>(rawContent);
    }

    public T GetJsonDataFromModFile<T>(string pathToFile, string fileName)
    {
        var callingModFolder = GetAbsolutePathToModFolder(Assembly.GetCallingAssembly());
        return GetJsonDataFromFile<T>(Path.Combine(callingModFolder, pathToFile), fileName);
    }
}
