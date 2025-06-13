namespace SPTarkov.Server.Core.Utils.Logger.Handlers.File;

public interface IFilePatternReplacer
{
    string Pattern { get; }
    string ReplacePattern(FileSptLoggerReference config, string filePattern);
}
