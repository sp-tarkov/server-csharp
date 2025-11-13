using SPTarkov.Common.Logger.Handlers.File;
using SPTarkov.Common.Logger.Util;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger.Handlers;

internal sealed class FileLogHandler(IEnumerable<IFilePatternReplacer> replacers) : BaseLogHandler
{
    // To be more efficient and avoid creating extra strings we will cache file patterns to the current processed pattern
    // That way we dont need to process them twice and generate extra garbage
    // _cacheFileNames[config.FilePath][config.FilePattern] will give you the current file pattern
    private readonly Dictionary<string, Dictionary<string, string>> _cachedFileNames = new();

    // This section needs to be fully locked as it is a double dictionary lookup
    private readonly Lock _cachedFileNamesLocks = new();

    private readonly Dictionary<string, IFilePatternReplacer> _replacers = replacers.ToDictionary(kv => kv.Pattern, kv => kv);
    public override LoggerType LoggerType
    {
        get { return LoggerType.File; }
    }

    public override void Log(SptLogMessage message, BaseSptLoggerReference reference)
    {
        var config = (reference as FileSptLoggerReference)!;

        if (string.IsNullOrEmpty(config.FilePath) || string.IsNullOrEmpty(config.FilePattern))
        {
            throw new Exception("FilePath and FilePattern are required to use FileLogger");
        }

        var targetFile = GetParsedTargetFile(config);

        var fileLock = LogFileCoordinator.GetFileLock(targetFile);

        lock (fileLock)
        {
            if (!Directory.Exists(config.FilePath))
            {
                Directory.CreateDirectory(config.FilePath);
            }

            // The AppendAllText will create the file as long as the directory exists
            System.IO.File.AppendAllText(targetFile, FormatMessage(message.Message + "\n", message, reference));

            LogFileCoordinator.GetOrCreateMetadata(targetFile, config, _replacers);
        }
    }

    private string GetParsedTargetFile(FileSptLoggerReference? config)
    {
        lock (_cachedFileNamesLocks)
        {
            if (!_cachedFileNames.TryGetValue(config.FilePath, out var cachedFileNames))
            {
                cachedFileNames = new Dictionary<string, string>();
                _cachedFileNames.Add(config.FilePath, cachedFileNames);
            }

            if (!cachedFileNames.TryGetValue(config.FilePattern, out var cachedFile))
            {
                cachedFile = $"{config.FilePath}{ProcessPattern(config)}";
                cachedFileNames.Add(config.FilePattern, cachedFile);
            }

            return cachedFile;
        }
    }

    private string ProcessPattern(FileSptLoggerReference? configFilePattern)
    {
        var finalFile = configFilePattern.FilePattern;
        foreach (var filePatternReplacer in _replacers)
        {
            if (finalFile.Contains(filePatternReplacer.Key))
            {
                finalFile = filePatternReplacer.Value.ReplacePattern(configFilePattern, finalFile);
            }
        }

        return finalFile;
    }
}
