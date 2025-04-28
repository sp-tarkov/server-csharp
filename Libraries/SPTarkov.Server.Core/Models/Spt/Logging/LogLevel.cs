namespace SPTarkov.Server.Core.Models.Spt.Logging;

public enum LogLevel
{
    // The order are very important for the logger to calculate properly the logging level, do not change!
    Fatal,
    Error,
    Warn,
    Info,
    Debug,
    Trace
}
