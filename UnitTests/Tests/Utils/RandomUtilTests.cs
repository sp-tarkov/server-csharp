using Core.Utils;
using Core.Utils.Cloners;
using UnitTests.Mock;

namespace UnitTests.Tests.Utils;

[TestClass]
public sealed class RandomUtilTests
{
    private readonly RandomUtil _randomUtil = new(new MockLogger<RandomUtil>(), new JsonCloner(new JsonUtil()));

    [TestMethod]
    public void GetIntTest()
    {
        // Run 10000 test cases
        for (var i = 0; i < 10000; i++)
        {
            var result = _randomUtil.GetInt(0, 10);

            if (result < 0 || result > 10)
            {
                Assert.Fail($"GetInt(0, 10) out of range. Expected range [0, 10] but was {result}.");
            }
        }
    }

    [TestMethod]
    public void GetIntExTest()
    {
        // Run 10000 test cases
        for (var i = 0; i < 10000; i++)
        {
            var result = _randomUtil.GetInt(1, 10, true);

            if (result < 1 || result > 9)
            {
                Assert.Fail($"GetInt(10) out of range. Expected range [1, 9] but was {result}.");
            }
        }
    }

    [TestMethod]
    public void GetDoubleTest()
    {
        // Run 10000 test cases
        for (var i = 0; i < 10000; i++)
        {
            var result = _randomUtil.GetDouble(0D, 10D);

            if (result is < 0d or >= 10d)
            {
                Assert.Fail($"GetDouble(0d, 10d) out of range. Expected range [0.0d, 9.999d] but was {result}.");
            }
        }
    }

    [TestMethod]
    public void GetPercentOfValueTest()
    {
        const float expected = 45.5f;
        var result = _randomUtil.GetPercentOfValue(45.5f, 100f);

        Assert.AreEqual(
            expected,
            result,
            0.0001f,
            $"GetPercentOfValue(45.5f, 100f) out of range. Expected: {expected}. Actual: {result}."
        );
    }

    [TestMethod]
    public void ReduceValueByPercentTest()
    {
        const float expected = 54.5f;
        var result = _randomUtil.ReduceValueByPercent(100f, 45.5f);

        Assert.AreEqual(
            expected,
            result,
            0.0001f,
            $"ReduceValueByPercent(100f, 45.5f) out of range. Expected: {expected}. Actual: {result}."
        );
    }

    [TestMethod]
    public void GetChance100Test()
    {
        for (var i = 0; i < 100; i++)
        {
            const bool expectedTrue = true;
            var resultTrue = _randomUtil.GetChance100(100f);

            Assert.AreEqual(
                expectedTrue,
                resultTrue,
                $"GetChance100(100f) out of range. Expected: {expectedTrue}. Actual: {resultTrue}."
            );
        }

        for (var i = 0; i < 100; i++)
        {
            const bool expectedFalse = false;
            var resultFalse = _randomUtil.GetChance100(0f);

            Assert.AreEqual(
                expectedFalse,
                resultFalse,
                $"GetChance100(0f) out of range. Expected: {expectedFalse}. Actual: {resultFalse}."
            );
        }
    }

    // TODO: Missing methods between these two

    [TestMethod]
    public void RandIntTest()
    {
        for (var i = 0; i < 100; i++)
        {
            var result = _randomUtil.RandInt(0, 10);

            if (result < 0 || result > 9)
            {
                Assert.Fail($"RandInt(0, 10) out of range. Expected range [0, 9] but was {result}.");
            }
        }

        for (var i = 0; i < 100; i++)
        {
            var result = _randomUtil.RandInt(10);

            if (result < 0 || result > 9)
            {
                Assert.Fail($"RandInt(10, null) out of range. Expected range [0, 9] but was {result}.");
            }
        }
    }

    [TestMethod]
    public void RandNumTest()
    {
        for (var i = 0; i < 10000; i++)
        {
            var result = _randomUtil.RandNum(0, 10, 15);

            if (result < 0 || result >= 10)
            {
                Assert.Fail($"RandNum(0, 10) out of range. Expected range [0, 9.999d] but was {result}.");
            }

            if (_randomUtil.GetNumberPrecision(result) > RandomUtil.MaxSignificantDigits)
            {
                Assert.Fail($"RandNum(0, 10) precision of {result} exceeds the allowable precision ({RandomUtil.MaxSignificantDigits}) for the given values.");
            }
        }

        for (var i = 0; i < 10000; i++)
        {
            var result = _randomUtil.RandNum(10);

            if (result < 0 || result >= 10)
            {
                Assert.Fail($"RandNum(10) out of range. Expected range [0, 9.999d] but was {result}.");
            }

            if (_randomUtil.GetNumberPrecision(result) > RandomUtil.MaxSignificantDigits)
            {
                Assert.Fail($"RandNum(10) precision of {result} exceeds the allowable precision ({RandomUtil.MaxSignificantDigits}) for the given values.");
            }
        }
    }

    [TestMethod]
    public void ShuffleTest()
    {
        var testList = new List<int>
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10
        };

        var orig = new List<int>(testList);

        var result = _randomUtil.Shuffle(testList);

        Assert.IsFalse(
            result.SequenceEqual(orig),
            $"Shuffle test failed. Expected: {string.Join(", ", orig)}, but got {string.Join(", ", result)}"
        );
    }

    [TestMethod]
    [DataRow(0.1, 1)]
    [DataRow(0.0001, 4)]
    [DataRow(0, 0)]
    [DataRow(10000000, 0)]
    [DataRow(0.000_000_000_000_000_000_000_000_1D, 25)]
    public void GetNumberPrecision_WithDoubles_ReturnsDecimalPoints(double value, int decimalPoints)
    {
        Assert.AreEqual(decimalPoints, _randomUtil.GetNumberPrecision(value));
    }

    [TestMethod]
    [DataRow(new[] { "test" }, "test", "Expected first array value")]
    public void GetArrayValueTest(string[] input, string expectedOutput, string failMessage)
    {
        var result = _randomUtil.GetArrayValue(input);
        Assert.AreEqual(input.First(), result, failMessage);
    }
}
