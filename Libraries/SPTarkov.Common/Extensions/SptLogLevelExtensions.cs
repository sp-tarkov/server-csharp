namespace SPTarkov.Common.Extensions;

public static class SptLogLevelExtensions
{
    public static Microsoft.Extensions.Logging.LogLevel ConvertToMicrosoftLogLevel(this Models.Logging.LogLevel sptLogLevel)
    {
        switch (sptLogLevel)
        {
            case Models.Logging.LogLevel.Trace:
                return Microsoft.Extensions.Logging.LogLevel.Trace;

            case Models.Logging.LogLevel.Debug:
                return Microsoft.Extensions.Logging.LogLevel.Debug;

            case Models.Logging.LogLevel.Info:
                return Microsoft.Extensions.Logging.LogLevel.Information;

            case Models.Logging.LogLevel.Warn:
                return Microsoft.Extensions.Logging.LogLevel.Warning;

            case Models.Logging.LogLevel.Error:
                return Microsoft.Extensions.Logging.LogLevel.Error;

            case Models.Logging.LogLevel.Fatal:
                return Microsoft.Extensions.Logging.LogLevel.Critical;

            default:
                throw new ArgumentOutOfRangeException(nameof(sptLogLevel), sptLogLevel, null);
        }
    }

    public static Models.Logging.LogLevel ConvertToSPTLogLevel(this Microsoft.Extensions.Logging.LogLevel microsoftLogLevel)
    {
        switch (microsoftLogLevel)
        {
            case Microsoft.Extensions.Logging.LogLevel.Trace:
                return Models.Logging.LogLevel.Trace;

            case Microsoft.Extensions.Logging.LogLevel.Debug:
                return Models.Logging.LogLevel.Debug;

            case Microsoft.Extensions.Logging.LogLevel.Information:
                return Models.Logging.LogLevel.Info;

            case Microsoft.Extensions.Logging.LogLevel.Warning:
                return Models.Logging.LogLevel.Warn;

            case Microsoft.Extensions.Logging.LogLevel.Error:
                return Models.Logging.LogLevel.Error;

            case Microsoft.Extensions.Logging.LogLevel.Critical:
                return Models.Logging.LogLevel.Fatal;

            default:
                throw new ArgumentOutOfRangeException(nameof(microsoftLogLevel), microsoftLogLevel, null);
        }
    }
}
