using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace TestMod2;

public record TestMod2Metadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "com.sp-tarkov.test-mod2";
    public override string Name { get; init; } = "test-mod2";
    public override string Author { get; init; } = "SPTarkov";
    public override List<string>? Contributors { get; init; }
    public override Version Version { get; init; } = new("1.0.0");
    public override Range SptVersion { get; init; } = new("~4.1.0");
    public override bool HasPrepatcher { get; init; } = true;
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override string License { get; init; } = "MIT";

    public string? WWWRootUrl { get; init; }
    public string? HomePage { get; init; }
    public string? HomePageDescription { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class TestMod2(ISptLogger<TestMod2> logger) : IOnLoad
{
    private const string InjectedName = "TestSkillEntry";

    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        logger.Info("Test mod 2 loading!");

        var names = Enum.GetNames<SkillTypes>();
        logger.Info($"SkillTypes ({names.Length} entries): {string.Join(", ", names)}");

        if (Enum.TryParse<SkillTypes>(InjectedName, out var injected))
        {
            logger.Info($"Prepatch applied: {InjectedName} = {(int)injected}");
        }
        else
        {
            logger.Warning($"Prepatch NOT applied: {InjectedName} missing from SkillTypes");
        }

        await Task.CompletedTask;
    }
}
