using Spectre.Console;

namespace SPTarkov.Common.Extensions;

public static class SptLogLevelExtensions
{
    extension(Models.Logging.LogLevel sptLogLevel)
    {
        public LogLevel ConvertToMicrosoftLogLevel()
        {
            return sptLogLevel switch
            {
                Models.Logging.LogLevel.Trace => LogLevel.Trace,
                Models.Logging.LogLevel.Debug => LogLevel.Debug,
                Models.Logging.LogLevel.Info => LogLevel.Information,
                Models.Logging.LogLevel.Warn => LogLevel.Warning,
                Models.Logging.LogLevel.Error => LogLevel.Error,
                Models.Logging.LogLevel.Fatal => LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(sptLogLevel), sptLogLevel, null),
            };
        }
    }

    extension(LogLevel microsoftLogLevel)
    {
        public Models.Logging.LogLevel ConvertToSPTLogLevel()
        {
            return microsoftLogLevel switch
            {
                LogLevel.Trace => Models.Logging.LogLevel.Trace,
                LogLevel.Debug => Models.Logging.LogLevel.Debug,
                LogLevel.Information => Models.Logging.LogLevel.Info,
                LogLevel.Warning => Models.Logging.LogLevel.Warn,
                LogLevel.Error => Models.Logging.LogLevel.Error,
                LogLevel.Critical => Models.Logging.LogLevel.Fatal,
                LogLevel.None => Models.Logging.LogLevel.Info,
                _ => throw new ArgumentOutOfRangeException(nameof(microsoftLogLevel), microsoftLogLevel, null),
            };
        }

        public Color? GetTextColor()
        {
            return microsoftLogLevel switch
            {
                LogLevel.Trace => null,
                LogLevel.Debug => Color.Gray,
                LogLevel.Information => null,
                LogLevel.Warning => Color.Yellow,
                LogLevel.Error => Color.Red,
                LogLevel.Critical => Color.Black,
                LogLevel.None => null,
                _ => throw new ArgumentOutOfRangeException(nameof(microsoftLogLevel), microsoftLogLevel, null),
            };
        }

        public Color? GetBackgroundColor()
        {
            return microsoftLogLevel switch
            {
                LogLevel.Trace => null,
                LogLevel.Debug => null,
                LogLevel.Information => null,
                LogLevel.Warning => null,
                LogLevel.Error => null,
                LogLevel.Critical => Color.Red,
                LogLevel.None => null,
                _ => throw new ArgumentOutOfRangeException(nameof(microsoftLogLevel), microsoftLogLevel, null),
            };
        }
    }
}
