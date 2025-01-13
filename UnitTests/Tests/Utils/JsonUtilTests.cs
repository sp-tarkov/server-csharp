using Core.Models.Enums;
using Core.Utils;

namespace UnitTests.Tests.Utils;

[TestClass]
public class JsonUtilTests
{
    private readonly JsonUtil _jsonUtil = new();
    [TestMethod]
    public void SerializeAndDeserialize_WithDictionaryOfETFEnum_ExpectCorrectParsing()
    {
        var value = new Dictionary<QuestStatusEnum, int> { { QuestStatusEnum.AvailableForStart, 1 } };
        var result = _jsonUtil.Deserialize<Dictionary<QuestStatusEnum, int>>(_jsonUtil.Serialize(value));
        Assert.AreEqual(value.Count, result.Count);
        Assert.AreEqual(value.First().Key, result.First().Key);
        Assert.AreEqual(value.First().Value, result.First().Value);
    }
}
