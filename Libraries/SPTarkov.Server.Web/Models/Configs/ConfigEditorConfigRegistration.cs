namespace SPTarkov.Server.Web.Models.Configs;

public sealed record ConfigEditorConfigRegistration
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required object RuntimeConfig { get; init; }

    public Type? RuntimeType { get; init; }

    public string? FilePath { get; init; }

    public string? FileName { get; init; }

    public IReadOnlySet<string> IgnoredSectionPaths { get; init; } = new HashSet<string>();

    public Func<CancellationToken, ValueTask<object?>>? LoadFromDiskAsync { get; init; }

    public Func<object, CancellationToken, ValueTask>? SaveToDiskAsync { get; init; }

    public Func<object, CancellationToken, ValueTask>? ApplyToRuntimeAsync { get; init; }

    public static ConfigEditorConfigRegistration Create<TConfig>(
        string id,
        string displayName,
        TConfig runtimeConfig,
        string? filePath = null
    )
        where TConfig : notnull
    {
        return new ConfigEditorConfigRegistration
        {
            Id = id,
            DisplayName = displayName,
            RuntimeConfig = runtimeConfig,
            RuntimeType = typeof(TConfig),
            FilePath = filePath,
        };
    }
}
