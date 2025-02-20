using Core.Models.Spt.Templates;
using Core.Utils;
using Core.Utils.Cloners;
using UnitTests.Mock;

namespace UnitTests.Tests.Utils;

[TestClass]
public class ClonerTest
{
    private Templates? _templates;

    private ICloner _jsonCloner;
    private ICloner _reflectionsCloner;
    private ICloner _fastCloner;

    private JsonUtil _jsonUtil;

    [TestInitialize]
    public void Setup()
    {
        _jsonUtil = new JsonUtil();
        var importer = new ImporterUtil(new MockLogger<ImporterUtil>(), new FileUtil(new MockLogger<FileUtil>()), _jsonUtil);
        var loadTask = importer.LoadRecursiveAsync<Templates>("./TestAssets/");
        loadTask.Wait();
        _templates = loadTask.Result;
        _jsonCloner = new JsonCloner(_jsonUtil);
        _reflectionsCloner = new ReflectionsCloner(new MockLogger<ReflectionsCloner>());
        _fastCloner = new Core.Utils.Cloners.FastCloner();
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
