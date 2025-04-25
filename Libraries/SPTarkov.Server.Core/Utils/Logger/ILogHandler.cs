using SPTarkov.Server.Core.Models.Enums.Logger;
using SPTarkov.Server.Core.Models.Logging;

namespace SPTarkov.Server.Core.Utils.Logger;

public interface ILogHandler
{
    LoggerType LoggerType
    {
        get;
    }

    void Log(SptLogMessage message, BaseSptLoggerReference reference);
}
