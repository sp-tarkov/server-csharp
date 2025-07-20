using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils.Logger;

[Injectable(TypePriority = int.MinValue)]
public class SptLogger<T> : SptLoggerBase<T>, IDisposable
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

        LoadConfig(
            fileUtil,
            jsonUtil,
            ProgramStatics.DEBUG() ? ConfigurationPathDev : ConfigurationPath
        );

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

    public override void LogWithColorInternal(
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

    protected override void SuccessInternal(string data, Exception? ex = null)
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

    protected override void ErrorInternal(string data, Exception? ex = null)
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

    protected override void WarningInternal(string data, Exception? ex = null)
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

    protected override void InfoInternal(string data, Exception? ex = null)
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

    protected override void DebugInternal(string data, Exception? ex = null)
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

    protected override void CriticalInternal(string data, Exception? ex = null)
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

    protected override void LogInternal(
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

    protected override bool IsLogEnabled(LogLevel level)
    {
        return _config.Loggers.Any(l => l.LogLevel.CanLog(level));
    }

    public bool IsEnabled(LogLevel level)
    {
        return IsLogEnabled(level);
    }

    public override void DumpAndStop()
    {
        _loggerQueueManager.DumpAndStop();
    }

    public void Dispose()
    {
        _loggerQueueManager.DumpAndStop();
    }
}
