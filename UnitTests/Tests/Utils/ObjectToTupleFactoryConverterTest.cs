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
    public void TestEnumToObject()
    {
        var json = "{ \"r\": \"Horizontal\" }";
        var testObject = _jsonUtil.Deserialize<TestObject>(json);
        Assert.AreEqual(testObject.R, new Tuple<ItemRotation?, int?, string?>(ItemRotation.Horizontal, null, null));
    }

    [TestMethod]
    public void TestInt()
    {
        var json = "{ \"r\": 0 }";
        var testObject = _jsonUtil.Deserialize<TestObject>(json);
        Assert.AreEqual(testObject.R, new Tuple<ItemRotation?, int?, string?>(null, 0, null));
    }

    [TestMethod]
    public void TestString()
    {
        const string str = "Just some String";
        var json = "{ \"r\": \"" + str + "\" }";
        Console.WriteLine(json);
        var testObject = _jsonUtil.Deserialize<TestObject>(json);
        Assert.AreEqual(testObject.R, new Tuple<ItemRotation?, int?, string?>(null, null, str));
    }
}
