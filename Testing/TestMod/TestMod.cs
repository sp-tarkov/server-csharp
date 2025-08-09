using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using Version = SemanticVersioning.Version;

namespace TestMod;

public record TestModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.sp-tarkov.test-mod";
    public override string Name { get; init; } = "test-mod";
    public override string Author { get; init; } = "SPTarkov";
    public override List<string>? Contributors { get; set; }
    public override Version Version { get; } = new("1.0.0");
    public override Version SptVersion { get; } = new("4.0.0");
    public override List<string>? LoadBefore { get; set; }
    public override List<string>? LoadAfter { get; set; }
    public override List<string>? Incompatibilities { get; set; }
    public override Dictionary<string, Version>? ModDependencies { get; set; }
    public override string? Url { get; set; }
    public override bool? IsBundleMod { get; set; }
    public override string? License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class TestMod(ISptLogger<TestMod> logger) : IOnLoad
{
    public Task OnLoad()
    {
        return Task.CompletedTask;
    }
}
