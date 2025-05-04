using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils.Logger;

[Injectable(InjectableTypeOverride = typeof(ISptLogger<>), TypePriority = int.MinValue)]
public class SptLogger<T> : ISptLogger<T>, IDisposable
{
    private string _category;
    private readonly SptLoggerQueueManager _loggerQueueManager;

    private const string ConfigurationPath = "./sptLogger.json";
    private const string ConfigurationPathDev = "./sptLogger.Development.json";
    private SptLoggerConfiguration _config;

    ~SptLogger()
    {
        _loggerQueueManager.DumpAndStop();
    }

    public SptLogger(FileUtil fileUtil, JsonUtil jsonUtil, SptLoggerQueueManager loggerQueueManager)
    {
        _category = typeof(T).FullName;
        _loggerQueueManager = loggerQueueManager;

        if (ProgramStatics.DEBUG())
        {
            LoadConfig(fileUtil, jsonUtil, ConfigurationPathDev);
        }
        else
        {
            LoadConfig(fileUtil, jsonUtil, ConfigurationPath);
        }

        if (_config == null)
        {
            throw new Exception(
                "The configuration path was loaded but it contained invalid or incorrect configuration."
            );
        }

        _loggerQueueManager.Initialize(_config);
    }

    private void LoadConfig(FileUtil fileUtil, JsonUtil jsonUtil, string sptloggerDevelopmentJson)
    {
        if (fileUtil.FileExists(sptloggerDevelopmentJson))
        {
            _config = jsonUtil.DeserializeFromFile<SptLoggerConfiguration>(
                sptloggerDevelopmentJson
            );
        }
        else
        {
            throw new Exception($"Unable to find SPTLogger file '{sptloggerDevelopmentJson}'");
        }
    }

    public void OverrideCategory(string category)
    {
        _category = category;
    }

    public void LogWithColor(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Info,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                textColor,
                backgroundColor
            )
        );
    }

    public void Success(string data, Exception? ex = null)
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Info,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Green
            )
        );
    }

    public void Error(string data, Exception? ex = null)
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Error,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Red
            )
        );
    }

    public void Warning(string data, Exception? ex = null)
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Warn,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Yellow
            )
        );
    }

    public void Info(string data, Exception? ex = null)
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Info,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex
            )
        );
    }

    public void Debug(string data, Exception? ex = null)
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Debug,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Gray
            )
        );
    }

    public void Critical(string data, Exception? ex = null)
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Fatal,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Black,
                LogBackgroundColor.Red
            )
        );
    }

    public void Log(
        LogLevel level,
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        _loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                level,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                textColor,
                backgroundColor
            )
        );
    }

    public bool IsLogEnabled(LogLevel level)
    {
        return _config.Loggers.Any(l =>
        {
            return l.LogLevel.CanLog(level);
        });
    }

    public void DumpAndStop()
    {
        _loggerQueueManager.DumpAndStop();
    }

    public void Dispose()
    {
        _loggerQueueManager.DumpAndStop();
    }
}
