using SPTarkov.Server.Core.Models.Spt.Templates;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;
using UnitTests.Mock;

namespace UnitTests.Tests;

[TestClass]
public class Test
{
    private Templates? _templates;

    private static JsonUtil _jsonUtil = new([ new SptJsonConverterRegistrator()]);

    [TestInitialize]
    public async Task Setup()
    {
        var importer = new ImporterUtil(new MockLogger<ImporterUtil>(), new FileUtil(), _jsonUtil);
        _templates = await importer.LoadRecursiveAsync<Templates>("./TestAssets/");

    }

    [TestMethod]
    public void TestMethod1()
    {
        var result = _jsonUtil.Serialize(_templates);
        Console.WriteLine(result);
    }
}
