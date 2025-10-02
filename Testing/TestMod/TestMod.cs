using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Web;
using Version = SemanticVersioning.Version;

namespace TestMod;

public record TestModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "com.sp-tarkov.test-mod";
    public override string Name { get; init; } = "test-mod";
    public override string Author { get; init; } = "SPTarkov";
    public override List<string>? Contributors { get; init; }
    public override Version Version { get; init; } = new("1.0.0");
    public override Version SptVersion { get; init; } = new("4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Version>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class TestMod(ISptLogger<TestMod> logger) : IOnLoad
{
    public Task OnLoad()
    {
        logger.Info("Test mod loaded!");

        return Task.CompletedTask;
    }
}
