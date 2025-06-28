using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Utils;

namespace UnitTests.Tests.Utils;

[TestClass]
public class MathUtilTests
{
    private MathUtil _mathUtil;

    [TestInitialize]
    public void Initialize()
    {
        _mathUtil = DI.GetService<MathUtil>();
    }

    [TestMethod]
    public void ListSumTest()
    {
        var test = new List<float> { 1.1f, 2.1f, 3.3f };
        const double expected = 6.5f;

        var actual = test.Sum();

        Assert.AreEqual(expected, actual, $"ListSum() Expected: {expected}, Actual: {actual}");
    }

    [TestMethod]
    public void ListCumSumTest()
    {
        var test = new List<double> { 1f, 2f, 3f, 4f };
        var expected = new List<double> { 1f, 3f, 6f, 10f };

        var actual = test.CumulativeSum().ToList();

        for (var i = 0; i < actual.Count; i++)
        {
            if (Math.Abs(expected[i] - actual[i]) > 0.00001f)
            {
                Assert.Fail(
                    $"ListCumSum() Expected: {string.Join(", ", expected)}, Actual: {string.Join(", ", actual)}"
                );
            }
        }
    }

    [TestMethod]
    public void ListProductTest()
    {
        var test = new List<double> { 1f, 2f, 3f, 4f };
        var expected = new List<double> { 2f, 4f, 6f, 8f };

        var actual = test.Product(2).ToList();

        for (var i = 0; i < actual.Count; i++)
        {
            if (Math.Abs(expected[i] - actual[i]) > 0.00001f)
            {
                Assert.Fail(
                    $"ListProduct() Expected: {string.Join(", ", expected)}, Actual: {string.Join(", ", actual)}"
                );
            }
        }
    }

    [TestMethod]
    public void ListAddTest()
    {
        var test = new List<double> { 1f, 2f, 3f, 4f };
        var expected = new List<double> { 3f, 4f, 5f, 6f };

        var actual = _mathUtil.ListAdd(test, 2);

        for (var i = 0; i < actual.Count; i++)
        {
            if (Math.Abs(expected[i] - actual[i]) > 0.00001f)
            {
                Assert.Fail(
                    $"ListProduct() Expected: {string.Join(", ", expected)}, Actual: {string.Join(", ", actual)}"
                );
            }
        }
    }

    [TestMethod]
    public void MapToRangeTest()
    {
        const double expected = 2;

        var actual = _mathUtil.MapToRange(0.5, 0, 1, 1, 3);

        Assert.AreEqual(expected, actual, $"MapToRange() Expected: {expected}, Actual: {actual}");
    }

    [TestMethod]
    [DataRow(
        15d,
        new double[] { 1, 10, 20, 30, 40, 50, 60 },
        new double[] { 11000, 20000, 32000, 45000, 58000, 70000, 82000 },
        26000d
    )]
    [DataRow(5d, new double[] { 1, 10 }, new double[] { 0, 1000 }, 444.44444444444446d)]
    [DataRow(
        12d,
        new double[] { 1, 10, 500, 510 },
        new double[] { 0, 10, 20, 30 },
        10.040816326530612d
    )]
    [DataRow(1d, new double[] { 1, 10, 500, 510 }, new double[] { 2, 10, 20, 30 }, 2d)]
    [DataRow(11d, new double[] { 1, 10 }, new double[] { 2, 10 }, 10d)]
    public void InterpTest(double input, double[] x, double[] y, double expected)
    {
        var actual = _mathUtil.Interp1(input, x.ToList(), y.ToList());

        Assert.AreEqual(expected, actual, $"Interp1() Expected: {expected}, Actual: {actual}");
    }
}
