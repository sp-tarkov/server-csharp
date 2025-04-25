using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using SPTarkov.Server.Core.Models.Enums.Logger;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Utils.Logger;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Extensions;

public static class SptLoggerFilterExtensions
{
    private static ConcurrentDictionary<SptLoggerFilter, Regex> _cachedRegexes = new();

    public static bool Match(this SptLoggerFilter filter, SptLogMessage message)
    {
        switch (filter.MatchingType)
        {
            case MatchingType.Literal:
                if (filter.Name != message.Logger)
                {
                    return false;
                }
                break;
            case MatchingType.Regex:
                if (!_cachedRegexes.TryGetValue(filter, out var regex))
                {
                    regex = new Regex(filter.Name);
                    while(!_cachedRegexes.TryAdd(filter, regex));
                }

                if (!regex.IsMatch(message.Logger))
                {
                    return false;
                }
                break;

        }

        return true;
    }

    public static bool CanLog(this LogLevel logLevel, LogLevel messageLevel)
    {
        return logLevel >= messageLevel;
    }
}
