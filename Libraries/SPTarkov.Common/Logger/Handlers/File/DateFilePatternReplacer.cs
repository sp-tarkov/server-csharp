using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger.Handlers.File;

internal sealed class DateFilePatternReplacer : IFilePatternReplacer
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
