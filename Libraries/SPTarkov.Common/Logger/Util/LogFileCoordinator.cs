using System.Collections.Concurrent;
using SPTarkov.Common.Logger.Handlers.File;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger.Util;

internal static class LogFileCoordinator
{
    private static readonly ConcurrentDictionary<string, Lock> _fileLocks = new();
    private static readonly ConcurrentDictionary<string, FileMetadata> _fileMetadata = new();

    public static Lock GetFileLock(string filePath)
    {
        return _fileLocks.GetOrAdd(filePath, _ => new Lock());
    }

    public static FileMetadata GetOrCreateMetadata(
        string filePath,
        FileSptLoggerReference config,
        Dictionary<string, IFilePatternReplacer> replacers
    )
    {
        return _fileMetadata.GetOrAdd(filePath, _ => new FileMetadata(filePath, config, replacers));
    }

    public static IEnumerable<FileMetadata> GetAllFiles()
    {
        return _fileMetadata.Values;
    }
}

internal sealed record FileMetadata(string path, FileSptLoggerReference config, Dictionary<string, IFilePatternReplacer> replacers)
{
    public FileInfo FileInfo { get; } = new FileInfo(path);
    public FileSptLoggerReference Config { get; } = config;
    public Dictionary<string, IFilePatternReplacer> Replacers { get; } = replacers;
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}
