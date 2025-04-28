using BenchmarkDotNet.Attributes;
using Benchmarks.Mock;
using SPTarkov.Server.Core.Models.Spt.Templates;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace Benchmarks;

[SimpleJob(warmupCount: 10, iterationCount: 10)]
[MemoryDiagnoser]
public class ClonerBenchmarks
{
    private ICloner _fastCloner;

    private ICloner _jsonCloner;
    private ICloner _reflectionsCloner;
    private Templates? _templates;

    [GlobalSetup]
    public void Setup()
    {
        var jsonUtil = new JsonUtil();
        var importer = new ImporterUtil(new MockLogger<ImporterUtil>(), new FileUtil(),
            jsonUtil);
        var loadTask = importer.LoadRecursiveAsync<Templates>("./Assets/database/templates/");
        loadTask.Wait();
        _templates = loadTask.Result;
        _jsonCloner = new JsonCloner(jsonUtil);
        _reflectionsCloner = new ReflectionsCloner(new MockLogger<ReflectionsCloner>());
        _fastCloner = new SPTarkov.Server.Core.Utils.Cloners.FastCloner();
    }

    [Benchmark]
    public void JsonCloner()
    {
        _jsonCloner.Clone(_templates);
    }

    [Benchmark]
    public void ReflectionsCloner()
    {
        _reflectionsCloner.Clone(_templates);
    }

    [Benchmark(Baseline = true)]
    public void FastCloner()
    {
        _fastCloner.Clone(_templates);
    }
}
