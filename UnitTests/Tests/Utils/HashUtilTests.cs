using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using UnitTests.Mock;

namespace UnitTests.Tests.Utils;

[TestClass]
public class HashUtilTests
{
    protected HashUtil _hashUtil = new(
        new RandomUtil(new MockLogger<RandomUtil>(), new JsonCloner(new JsonUtil()))
    );

    [TestMethod]
    public void GenerateTest()
    {
        // Generate 100 MongoId's
        for (var i = 0; i < 100; i++)
        {
            // Invalid mongoId character
            var result = _hashUtil.Generate();

            // Invalid mongoId length
            var test = _hashUtil.IsValidMongoId(result);

            Assert.AreEqual(true, test, $"IsValidMongoId() `{result}` is not a valid MongoId.");
        }
    }

    [TestMethod]
    [DataRow(
        "677ddb67406e9918a0264bbz",
        false,
        "677ddb67406e9918a0264bbz contains invalid char `z`, but result was true"
    )]
    [DataRow(
        "677ddb67406e9918a0264bbcc",
        false,
        "677ddb67406e9918a0264bbcc is 25 characters, but result was true"
    )]
    [DataRow(
        "677ddb67406e9918a0264bbc",
        true,
        "IsValidMongoId() `677ddb67406e9918a0264bbc` is a valid mongoId, but result was false"
    )]
    public void IsValidMongoIdTest(string mongoId, bool passes, string failMessage)
    {
        var result = _hashUtil.IsValidMongoId(mongoId);
        Assert.AreEqual(passes, result, failMessage);
    }

    [TestMethod]
    [DataRow(
        "123456789",
        "25F9E794323B453885F5181F1B624D0B",
        "Not valid output, expected '25F9E794323B453885F5181F1B624D0B'"
    )]
    public void GenerateValidMd5Test(string input, string expectedOutput, string failMessage)
    {
        var result = _hashUtil.GenerateMd5ForData(input);
        Assert.AreEqual(expectedOutput, result, failMessage);
    }
}
