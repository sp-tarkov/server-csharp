using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.Services;

namespace TestMod2;

[Injectable(InjectionType.Singleton)]
public class TestPatch : AbstractPatch
{
    private static DatabaseService _databaseService = default!;
    private static ISptLogger<TestPatch> _logger = default!;

    public TestPatch(DatabaseService databaseService, ISptLogger<TestPatch> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    protected override MethodBase? GetTargetMethod()
    {
        return typeof(HideoutCallbacks).GetMethod(nameof(HideoutCallbacks.OnUpdate));
    }

    [PatchPrefix]
    public static void Prefix()
    {
        _logger.Warning("Hideout callback on update!");
    }
}
