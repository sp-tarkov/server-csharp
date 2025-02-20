using BenchmarkDotNet.Attributes;
using Benchmarks.Mock;
using Core.Models.Spt.Templates;
using Core.Utils;
using Core.Utils.Cloners;

namespace Benchmarks;

[SimpleJob(warmupCount: 10, iterationCount: 10)]
[MemoryDiagnoser]
public class ClonerBenchmarks
{
    private Templates? _templates;

    private ICloner _jsonCloner;
    private ICloner _reflectionsCloner;
    private ICloner _fastCloner;

    [GlobalSetup]
    public void Setup()
    {
        var jsonUtil = new JsonUtil();
        var importer = new ImporterUtil(new MockLogger<ImporterUtil>(), new FileUtil(new MockLogger<FileUtil>()),
            jsonUtil);
        var loadTask = importer.LoadRecursiveAsync<Templates>("./Assets/database/templates/");
        loadTask.Wait();
        _templates = loadTask.Result;
        _jsonCloner = new JsonCloner(jsonUtil);
        _reflectionsCloner = new ReflectionsCloner(new MockLogger<ReflectionsCloner>());
        _fastCloner = new Core.Utils.Cloners.FastCloner();
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
