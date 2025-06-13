using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils.Logger.Handlers.File;

namespace SPTarkov.Server.Core.Utils.Logger.Handlers;

[Injectable(InjectionType.Singleton)]
public class FileLogHandler(IEnumerable<IFilePatternReplacer> replacers) : BaseLogHandler, IOnLoad
{
    private const int FileSystemPollMonitorTimeMs = 5000;

    // To be more efficient and avoid creating extra strings we will cache file patterns to the current processed pattern
    // That way we dont need to process them twice and generate extra garbage
    // _cacheFileNames[config.FilePath][config.FilePattern] will give you the current file pattern
    private readonly Dictionary<string, Dictionary<string, string>> _cachedFileNames = new();
    // This section needs to be fully locked as it is a double dictionary lookup
    private readonly Lock _cachedFileNamesLocks = new();

    private readonly Dictionary<string, Dictionary<string, string>> _cachedWipedPatterns = new();

    private readonly Dictionary<string, IFilePatternReplacer> _replacers = replacers.ToDictionary(kv => kv.Pattern, kv => kv);

    private readonly ConcurrentDictionary<string, Lock> _fileLocks = new();
    private readonly ConcurrentDictionary<string, FileInfo> _fileInfos = new();
    private readonly ConcurrentDictionary<string, FileSptLoggerReference> _fileConfigs = new();
    public override LoggerType LoggerType => LoggerType.File;

    public override void Log(SptLogMessage message, BaseSptLoggerReference reference)
    {
        var config = (reference as FileSptLoggerReference)!;

        if (string.IsNullOrEmpty(config.FilePath) || string.IsNullOrEmpty(config.FilePattern))
        {
            throw new Exception("FilePath and FilePattern are required to use FileLogger");
        }

        var targetFile = GetParsedTargetFile(config);

        if (!_fileLocks.TryGetValue(targetFile, out var lockObject))
        {
            lockObject = new Lock();
            while (!_fileLocks.TryAdd(targetFile, lockObject)) ;
        }
        lock (lockObject)
        {
            if (!Directory.Exists(config.FilePath))
            {
                Directory.CreateDirectory(config.FilePath);
            }

            // The AppendAllText will create the file as long as the directory exists
            System.IO.File.AppendAllText(targetFile, FormatMessage(message.Message + "\n", message, reference));

            if (!_fileInfos.TryGetValue(targetFile, out _))
            {
                var fileInfo = new FileInfo(targetFile);
                while (!_fileInfos.TryAdd(targetFile, fileInfo)) ;
            }

            if (!_fileConfigs.TryGetValue(targetFile, out _))
            {
                while (!_fileConfigs.TryAdd(targetFile, config)) ;
            }
        }
    }

    protected string GetParsedTargetFile(FileSptLoggerReference? config)
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

    protected string ProcessPattern(FileSptLoggerReference? configFilePattern)
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

    public Task OnLoad()
    {
        Task.Factory.StartNew(FileSystemWatcherMonitor, TaskCreationOptions.LongRunning);
        return Task.CompletedTask;
    }

    protected void FileSystemWatcherMonitor()
    {
        while (true)
        {
            if (!_fileInfos.IsEmpty)
            {
                foreach (var fileInfosKvp in _fileInfos)
                {
                    if (!_fileLocks.TryGetValue(fileInfosKvp.Key, out var fileLock))
                    {
                        continue;
                    }

                    lock (fileLock)
                    {
                        ValidateAndRollFile(fileInfosKvp.Key, fileInfosKvp.Value);
                    }
                }
            }

            Thread.Sleep(FileSystemPollMonitorTimeMs);
        }
    }

    protected void ValidateAndRollFile(string key, FileInfo fileInfo)
    {
        if (!_fileConfigs.TryGetValue(key, out var fileConfig))
        {
            return;
        }

        // MaxFileSizeMb == 0 means no max file size
        if (fileConfig.MaxFileSizeMb == 0)
        {
            return;
        }

        fileInfo.Refresh();
        if (fileInfo.Length / 1024D / 1024D > fileConfig.MaxFileSizeMb)
        {
            RollFile(fileConfig, fileInfo);
        }
    }

    protected void RollFile(FileSptLoggerReference fileConfig, FileInfo fileInfo)
    {
        if (fileConfig.MaxRollingFiles > 0)
        {
            var unpatternedFileName = GetWipedPattern(fileConfig);
            var lastFile = $"{unpatternedFileName}.{fileConfig.MaxRollingFiles - 1}";
            if (System.IO.File.Exists(lastFile))
            {
                System.IO.File.Delete(lastFile);
            }

            for (var i = fileConfig.MaxRollingFiles - 1; i > 0; i--)
            {
                var oldReference = i - 1;
                var oldFile = oldReference == 0 ? fileInfo.FullName : $"{unpatternedFileName}.{i - 1}";
                var newFile = $"{unpatternedFileName}.{i}";
                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Copy(oldFile, newFile, true);
                }
            }
        }

        var stream = System.IO.File.Open(fileInfo.FullName, FileMode.Open);
        stream.SetLength(0);
        stream.Close();
    }

    protected string GetWipedPattern(FileSptLoggerReference fileConfig)
    {
        if (!_cachedWipedPatterns.TryGetValue(fileConfig.FilePath, out var wipePatterns))
        {
            wipePatterns = new Dictionary<string, string>();
            _cachedWipedPatterns.Add(fileConfig.FilePath, wipePatterns);
        }

        if (!wipePatterns.TryGetValue(fileConfig.FilePattern, out var wipedPattern))
        {
            wipedPattern = $"{fileConfig.FilePath}{WipePattern(fileConfig.FilePattern)}";
            wipePatterns.Add(fileConfig.FilePattern, wipedPattern);
        }

        return wipedPattern;
    }

    protected string WipePattern(string fileConfigFilePattern)
    {
        var finalUnpatternedFilename = fileConfigFilePattern;
        foreach (var replacersKey in _replacers.Keys)
        {
            finalUnpatternedFilename = finalUnpatternedFilename.Replace(replacersKey, "");
        }
        return finalUnpatternedFilename;
    }
}
