using BenchmarkDotNet.Attributes;
using SPTarkov.Server.Core.Utils;

namespace Benchmarks
{
    [SimpleJob(warmupCount: 10, iterationCount: 25)]
    [MemoryDiagnoser]
    public class MathUtilInterpBenchmarks
    {
        private MathUtil _mathUtil;

        private double input = 15d;
        private List<double> x = [1, 10, 20, 30, 40, 50, 60];
        private List<double> y = [11000, 20000, 32000, 45000, 58000, 70000, 82000];

        [GlobalSetup]
        public void Setup()
        {
            _mathUtil = new MathUtil();
        }

        [Benchmark]
        public void Interp()
        {
            _mathUtil.Interp1(input, x, y);
        }
    }
}
