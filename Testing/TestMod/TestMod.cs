using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace TestMod;

public record TestModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "com.sp-tarkov.test-mod";
    public override string Name { get; init; } = "test-mod";
    public override string Author { get; init; } = "SPTarkov";
    public override List<string>? Contributors { get; init; }
    public override Version Version { get; init; } = new("1.0.0");
    public override Range SptVersion { get; init; } = new("~4.1.0");
    public override bool HasPrepatcher { get; init; } = false;
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override string License { get; init; } = "MIT";

    public string? WWWRootUrl { get; init; }
    public string? HomePage { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public class TestModPreload(ISptLogger<TestMod> logger) : IOnLoad
{
    public async Task OnLoad(CancellationToken cancellationToken)
    {
        logger.Info("Test mod preloading!");

        await Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class TestMod(ISptLogger<TestMod> logger) : IOnLoad
{
    public async Task OnLoad(CancellationToken cancellationToken)
    {
        logger.Info("Test mod loading!");

        await Task.CompletedTask;
    }
}
