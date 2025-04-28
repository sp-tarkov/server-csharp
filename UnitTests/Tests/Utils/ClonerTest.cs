using SPTarkov.Server.Core.Models.Spt.Templates;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using UnitTests.Mock;

namespace UnitTests.Tests.Utils;

[TestClass]
public class ClonerTest
{
    private ICloner _fastCloner;

    private ICloner _jsonCloner;

    private JsonUtil _jsonUtil;
    private ICloner _reflectionsCloner;
    private Templates? _templates;

    [TestInitialize]
    public void Setup()
    {
        _jsonUtil = new JsonUtil(new MockLogger<JsonUtil>());
        var importer = new ImporterUtil(new MockLogger<ImporterUtil>(), new FileUtil(), _jsonUtil);
        var loadTask = importer.LoadRecursiveAsync<Templates>("./TestAssets/");
        loadTask.Wait();
        _templates = loadTask.Result;
        _jsonCloner = new JsonCloner(_jsonUtil);
        _reflectionsCloner = new ReflectionsCloner(new MockLogger<ReflectionsCloner>());
        _fastCloner = new SPTarkov.Server.Core.Utils.Cloners.FastCloner();
    }

    [TestMethod]
    public void FastCloner()
    {
        var jsonObject = _jsonCloner.Clone(_templates);
        var reflectionObject = _reflectionsCloner.Clone(_templates);
        var fastObject = _fastCloner.Clone(_templates);
        // This test is just used for cloner comparison, not a real test
    }
}
