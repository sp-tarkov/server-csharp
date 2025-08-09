using NUnit.Framework;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils;

namespace UnitTests.Tests.Utils;

[TestFixture]
public class JsonUtilTests
{
    private JsonUtil _jsonUtil;

    [OneTimeSetUp]
    public void Initialize()
    {
        _jsonUtil = DI.GetInstance().GetService<JsonUtil>();
    }

    [Test]
    public void SerializeAndDeserialize_WithDictionaryOfETFEnum_ExpectCorrectParsing()
    {
        var value = new Dictionary<QuestStatusEnum, int> { { QuestStatusEnum.AvailableForStart, 1 } };
        var result = _jsonUtil.Deserialize<Dictionary<QuestStatusEnum, int>>(_jsonUtil.Serialize(value));
        Assert.AreEqual(value.Count, result?.Count);
        Assert.AreEqual(value.First().Key, result?.First().Key);
        Assert.AreEqual(value.First().Value, result?.First().Value);
    }
}
