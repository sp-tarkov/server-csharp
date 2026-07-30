using System.Globalization;
using SPTarkov.Common.Models.Logging;
using ZLinq;

namespace SPTarkov.Common.Logger;

/// <summary>
/// This class handles booting up a task for each provider, managing the amount of rolled files and deleting the oldest ones if necessary
/// </summary>
internal sealed class LogFileRollMonitor : IAsyncDisposable
{
    private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);
    private readonly Lock _targetsLock = new();
    private readonly Dictionary<string, FileSptLoggerReference> _targets = [];

    private CancellationTokenSource? _cleanupCancellationTokenSource;
    private Task? _cleanupTask;

    public void RegisterTarget(string key, FileSptLoggerReference config)
    {
        lock (_targetsLock)
        {
            if (_targets.ContainsKey(key))
            {
                return;
            }

            _targets.Add(key, config);

            CleanupRolledFiles(config, DateTimeOffset.UtcNow);
            EnsureCleanupWorkerStarted();
        }
    }

    private void EnsureCleanupWorkerStarted()
    {
        if (_cleanupTask != null)
        {
            return;
        }

        _cleanupCancellationTokenSource = new CancellationTokenSource();

        _cleanupTask = Task.Factory.StartNew(
            async () => await CleanupWorkerAsync(_cleanupCancellationTokenSource.Token).ConfigureAwait(false),
            TaskCreationOptions.LongRunning
        );
    }

    private async Task CleanupWorkerAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_cleanupInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                List<FileSptLoggerReference> targets;

                lock (_targetsLock)
                {
                    targets = _targets.Values.ToList();
                }

                foreach (var target in targets)
                {
                    try
                    {
                        CleanupRolledFiles(target, DateTimeOffset.UtcNow);
                    }
                    catch { }
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private static void CleanupRolledFiles(FileSptLoggerReference config, DateTimeOffset timestamp)
    {
        if (config.MaxRollingFiles <= 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(config.FilePath) || string.IsNullOrEmpty(config.FilePattern))
        {
            return;
        }

        var directory = Path.GetFullPath(config.FilePath);

        if (!Directory.Exists(directory))
        {
            return;
        }

        var date = timestamp.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var currentFileName = config.FilePattern.Replace("%DATE%", date, StringComparison.OrdinalIgnoreCase);

        var name = Path.GetFileNameWithoutExtension(currentFileName);
        var extension = Path.GetExtension(currentFileName);
        var originalPath = Path.Combine(directory, currentFileName);

        var rolledFiles = Directory
            .EnumerateFiles(directory, $"{name}.*{extension}", SearchOption.TopDirectoryOnly)
            .AsValueEnumerable()
            .Select(path => new FileInfo(path))
            .Where(file => file.Exists)
            .Where(file => IsRolledFile(file.Name, name, extension))
            .ToList();

        if (rolledFiles.Count == 0)
        {
            return;
        }

        var files = rolledFiles;

        var originalFile = new FileInfo(originalPath);

        if (originalFile.Exists)
        {
            files.Add(originalFile);
        }

        files = files.OrderByDescending(file => file.LastWriteTimeUtc).ToList();

        if (files.Count <= config.MaxRollingFiles)
        {
            return;
        }

        foreach (var file in files.Skip(config.MaxRollingFiles))
        {
            try
            {
                file.Delete();
            }
            catch { }
        }
    }

    private static bool IsRolledFile(string fileName, string baseName, string extension)
    {
        if (!fileName.StartsWith($"{baseName}.", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var sequenceStart = baseName.Length + 1;
        var sequenceLength = fileName.Length - sequenceStart - extension.Length;

        if (sequenceLength <= 0)
        {
            return false;
        }

        var sequence = fileName.Substring(sequenceStart, sequenceLength);

        return int.TryParse(sequence, CultureInfo.InvariantCulture, out _);
    }

    public async ValueTask DisposeAsync()
    {
        _cleanupCancellationTokenSource?.Cancel();

        if (_cleanupTask != null)
        {
            try
            {
                await _cleanupTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
        }

        _cleanupCancellationTokenSource?.Dispose();
    }
}
