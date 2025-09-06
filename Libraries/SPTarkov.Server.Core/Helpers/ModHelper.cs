using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class ModHelper(FileUtil fileUtil, JsonUtil jsonUtil)
{
    public string GetAbsolutePathToModFolder(Assembly modAssembly)
    {
        // The full path to the mod folder
        return Path.GetDirectoryName(modAssembly.Location);
    }

    public string GetRawFileData(string pathToFile, string fileName)
    {
        // Read the content of the config file as a string
        return fileUtil.ReadFile(Path.Combine(pathToFile, fileName));
    }

    public T GetJsonDataFromFile<T>(string pathToFile, string fileName)
    {
        // Read the content of the config file as a string
        var rawContent = fileUtil.ReadFile(Path.Combine(pathToFile, fileName));

        // Take the string above and deserialise it into a file with a type (defined between the diamond brackets)
        return jsonUtil.Deserialize<T>(rawContent);
    }
}
