namespace SPTarkov.Common.Logger.Util;

internal static class LogFileRollMonitor
{
    private static CancellationTokenSource? _cts;
    private static Task? _monitorTask;
    private static readonly Lock _startupLock = new();

    public static void RegisterHandler()
    {
        lock (_startupLock)
        {
            if (_monitorTask == null)
            {
                _cts = new CancellationTokenSource();
                _monitorTask = Task.Factory.StartNew(MonitorLoop, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }
    }

    public static void UnregisterHandler()
    {
        lock (_startupLock)
        {
            _cts?.Cancel();
            _monitorTask = null;
            _cts = null;
        }
    }

    private static void MonitorLoop()
    {
        try
        {
            while (_cts is not null && !_cts.Token.IsCancellationRequested)
            {
                foreach (var metadata in LogFileCoordinator.GetAllFiles())
                {
                    var fileLock = LogFileCoordinator.GetFileLock(metadata.FileInfo.FullName);
                    lock (fileLock)
                    {
                        ValidateAndRollFile(metadata);
                    }
                }

                Thread.Sleep(5000);
            }
        }
        catch (OperationCanceledException) { }
    }

    private static void ValidateAndRollFile(FileMetadata metadata)
    {
        var config = metadata.Config;

        // MaxFileSizeMb == 0 means no max file size
        if (config.MaxFileSizeMb == 0)
        {
            return;
        }

        metadata.FileInfo.Refresh();
        var fileSizeMb = metadata.FileInfo.Length / 1024D / 1024D;

        if (fileSizeMb > config.MaxFileSizeMb)
        {
            RollFile(metadata);
        }
    }

    private static void RollFile(FileMetadata metadata)
    {
        var config = metadata.Config;
        var fileInfo = metadata.FileInfo;

        if (config.MaxRollingFiles <= 0)
        {
            // Just truncate if no rolling
            TruncateFile(fileInfo);
            return;
        }

        // Get the base filename without patterns (e.g., "log.txt" from "log-2024-01-15.txt")
        var unpatternedFileName = GetUnpatternedFileName(metadata);

        // Delete the oldest file if it exists
        var lastFile = $"{unpatternedFileName}.{config.MaxRollingFiles - 1}";

        if (File.Exists(lastFile))
        {
            File.Delete(lastFile);
        }

        // Shift all existing rolled files up by one
        for (var i = config.MaxRollingFiles - 1; i > 0; i--)
        {
            var oldFileIndex = i - 1;
            var oldFile = oldFileIndex == 0 ? fileInfo.FullName : $"{unpatternedFileName}.{oldFileIndex}";
            var newFile = $"{unpatternedFileName}.{i}";

            if (File.Exists(oldFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }

                File.Move(oldFile, newFile);
            }
        }
    }

    private static string GetUnpatternedFileName(FileMetadata metadata)
    {
        var config = metadata.Config;
        var fileName = Path.GetFileName(metadata.FileInfo.FullName);
        var wipedFileName = fileName;

        foreach (var pattern in metadata.Replacers.Keys)
        {
            wipedFileName = wipedFileName.Replace(pattern, "");
        }

        return Path.Combine(config.FilePath, wipedFileName);
    }

    private static void TruncateFile(FileInfo fileInfo)
    {
        try
        {
            using var stream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Write, FileShare.Read);
            stream.SetLength(0);
        }
        catch (IOException) { }
    }
}
