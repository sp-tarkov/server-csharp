using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace TestMod;

public sealed class TestModMetadata : IModMetadata, IModBlazorMetadata
{
    public string ModGuid { get; init; } = "com.sp-tarkov.test-mod";
    public string Name { get; init; } = "test-mod";
    public string Author { get; init; } = "SPTarkov";
    public List<string>? Contributors { get; init; }
    public Version Version { get; init; } = new("1.0.0");
    public Range SptVersion { get; init; } = new("~4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";

    public string? WWWRootUrl { get; init; }
    public string? HomePage { get; init; } = "/test/page";
    public string? HomePageDescription { get; init; } = "Test mod config page!";
}

[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public class TestModPreload(ISptLogger<TestMod> logger, TestDIClass testClass) : IOnDIConstruct, IOnLoad
{
    public static Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        serviceCollection.AddSingleton(new TestDIClass());

        return Task.CompletedTask;
    }

    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        logger.Info("Test mod preloading!");
        logger.Info(testClass.TestString);

        await Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class TestMod(ISptLogger<TestMod> logger) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        logger.Info("Test mod postloading!");

        await Task.CompletedTask;
    }
}

public record TestDIClass()
{
    public string TestString { get; init; } = "Test mod with it's own singleton injection!";
}
