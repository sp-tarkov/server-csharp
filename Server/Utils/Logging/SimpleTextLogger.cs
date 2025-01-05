using Types.Annotations;
using ILogger = Types.Models.Utils.ILogger;

namespace Server.Utils.Logging;

[Injectable(InjectionType.Singleton)]
public class SimpleTextLogger : ILogger
{
    
}