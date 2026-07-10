using Spectre.Console;

namespace SPTarkov.Common.Extensions;

public static class SptLogLevelExtensions
{
    extension(LogLevel microsoftLogLevel)
    {
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
