using SPTarkov.Server.Core.Models.Spt.Templates;
using SPTarkov.Server.Core.Utils;
using UnitTests.Mock;

namespace UnitTests.Tests;

[TestClass]
public class Test
{
    private Templates? _templates;

    [TestInitialize]
    public void Setup()
    {
        var importer = new ImporterUtil(new MockLogger<ImporterUtil>(), new FileUtil(new MockLogger<FileUtil>()), new JsonUtil());
        var loadTask = importer.LoadRecursiveAsync<Templates>("./TestAssets/");
        loadTask.Wait();
        _templates = loadTask.Result;
    }

    [TestMethod]
    public void TestMethod1()
    {
        var result = new JsonUtil().Serialize(_templates);
        Console.WriteLine(result);
    }
}
