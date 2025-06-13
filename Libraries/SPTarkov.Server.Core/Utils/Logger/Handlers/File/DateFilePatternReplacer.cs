using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Utils.Logger.Handlers.File;

[Injectable(InjectionType.Singleton)]
public class DateFilePatternReplacer : IFilePatternReplacer
{
    public string Pattern
    {
        get { return "%DATE%"; }
    }

    public string ReplacePattern(FileSptLoggerReference config, string fileWithPattern)
    {
        return fileWithPattern.Replace(Pattern, DateTime.UtcNow.ToString("yyyyMMdd"));
    }
}
