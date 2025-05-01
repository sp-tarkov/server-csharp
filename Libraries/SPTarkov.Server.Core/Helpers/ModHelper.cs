using System.Reflection;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class ModHelper
{
    private readonly FileUtil _fileUtil;
    private readonly JsonUtil _jsonUtil;

    public ModHelper(
        FileUtil fileUtil,
        JsonUtil jsonUtil)
    {
        _fileUtil = fileUtil;
        _jsonUtil = jsonUtil;
    }

    public string GetAbsolutePathToModFolder(Assembly modAssembly)
    {
        // The full path to the mod folder
        return Path.GetDirectoryName(modAssembly.Location);
    }

    public string GetRawFileData(string pathToFile, string fileName)
    {
        // Read the content of the config file as a string
        return _fileUtil.ReadFile(Path.Combine(pathToFile, fileName));
    }

    public T GetJsonDataFromFile<T>(string pathToFile, string fileName)
    {
        // Read the content of the config file as a string
        var rawContent = _fileUtil.ReadFile(Path.Combine(pathToFile, fileName));

        // Take the string above and deserialise it into a file with a type (defined between the diamond brackets)
        return _jsonUtil.Deserialize<T>(rawContent);
    }
}
