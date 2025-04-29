using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace UnitTests.Tests.Utils;

[TestClass]
public class ObjectToTupleFactoryConverterTest()
{
    enum ItemRotation
    {
        // Token: 0x0400259F RID: 9631
        Horizontal,
        // Token: 0x040025A0 RID: 9632
        Vertical
    }
    record TestObject
    {
        [JsonPropertyName("r")]
        [JsonConverter(typeof (ObjectToTupleFactoryConverter))]
        public Tuple<ItemRotation?, int?, string?> R
        {
            get;
            set;
        }
    }

    private JsonUtil _jsonUtil = new JsonUtil();

    [TestMethod]
    public void TestReadEnumToObject()
    {
        var json = @"{ ""r"": ""Horizontal"" }";
        var testObject = _jsonUtil.Deserialize<TestObject>(json);
        Assert.AreEqual(testObject.R, new Tuple<ItemRotation?, int?, string?>(ItemRotation.Horizontal, null, null));
    }

    [TestMethod]
    public void TestReadInt()
    {
        var json = @"{ ""r"": 0 }";
        var testObject = _jsonUtil.Deserialize<TestObject>(json);
        Assert.AreEqual(testObject.R, new Tuple<ItemRotation?, int?, string?>(null, 0, null));
    }

    [TestMethod]
    public void TestReadString()
    {
        var json = @"{ ""r"": ""hello world"" }";
        Console.WriteLine(json);
        var testObject = _jsonUtil.Deserialize<TestObject>(json);
        Assert.AreEqual(testObject.R, new Tuple<ItemRotation?, int?, string?>(null, null, "hello world"));
    }

    [TestMethod]
    public void TestWriteEnum()
    {
        var obj = new TestObject { R = new Tuple<ItemRotation?, int?, string?>(ItemRotation.Horizontal,  1, "hello world"), };
        var json = _jsonUtil.Serialize(obj);
        Assert.AreEqual(@"{""r"":0}", json);
    }

    [TestMethod]
    public void TestWriteInt()
    {
        var obj = new TestObject { R = new Tuple<ItemRotation?, int?, string?>(null,  1, "hello world"), };
        var json = _jsonUtil.Serialize(obj);
        Assert.AreEqual(@"{""r"":1}", json);
    }

    [TestMethod]
    public void TestWriteString()
    {
        var obj = new TestObject { R = new Tuple<ItemRotation?, int?, string?>(null,  null, "hello world"), };
        var json = _jsonUtil.Serialize(obj);
        Assert.AreEqual(@"{""r"":""hello world""}", json);
    }
}
