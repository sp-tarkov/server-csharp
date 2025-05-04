using System.Collections.Concurrent;
using System.Text;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Utils.Logger.Handlers;

[Injectable(InjectionType.Singleton)]
public class FileLogHandler : BaseLogHandler
{
    private static ConcurrentDictionary<string, object> _fileLocks = new();

    public override LoggerType LoggerType
    {
        get { return LoggerType.File; }
    }

    public override void Log(SptLogMessage message, BaseSptLoggerReference reference)
    {
        var config = reference as FileSptLoggerReference;

        if (!_fileLocks.TryGetValue(config.FilePath, out var lockObject))
        {
            lockObject = new object();
            while (!_fileLocks.TryAdd(config.FilePath, lockObject))
            {
                ;
            }
        }

        lock (lockObject)
        {
            if (!Directory.Exists(Path.GetDirectoryName(config.FilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(config.FilePath));
            }

            // The AppendAllText will create the file as long as the directory exists
            File.AppendAllText(
                config.FilePath,
                FormatMessage(message.Message + "\n", message, reference)
            );
        }
    }
}
