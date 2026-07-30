using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace UnitTests.Tests;

[TestFixture]
public class MongoIDTests
{
    private const string ValidHex = "507f1f77bcf86cd799439011";
    private const string ZeroHexZeroHex = "000000000000000000000000";

    private const string TestHex = "507f1f77bcf86cd799439011";
    private MongoId _testId;
    private JsonSerializerOptions? _options;

    [OneTimeSetUp]
    public void Initialize()
    {
        _testId = new MongoId(TestHex);
        _options = new JsonSerializerOptions()
        {
            // This is required for JSONC support
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NewLine = "\n",
        };
    }

    [Test]
    public void GenerateTest()
    {
        // Generate 100 MongoId's
        for (var i = 0; i < 100; i++)
        {
            // Invalid mongoId character
            var result = new MongoId();

            // Invalid mongoId length
            var test = result.IsValidMongoId();

            Assert.IsTrue(test, $"IsValidMongoId(): `{result}` is not a valid MongoId.");
        }
    }

    [Test]
    public void Constructor_EmptyAndNull_ReturnsDefaultInstance()
    {
        // Arrange & Act
        var fromNull = MongoId.Empty();
        var fromEmpty = new MongoId(string.Empty);
        var fromZeroes = new MongoId(ZeroHexZeroHex);
        var defaultInstance = default(MongoId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(fromNull.IsEmpty, Is.True);
            Assert.That(fromEmpty.IsEmpty, Is.True);
            Assert.That(fromZeroes.IsEmpty, Is.True);
            Assert.That(fromNull, Is.EqualTo(defaultInstance));
            Assert.That(fromEmpty, Is.EqualTo(defaultInstance));
            Assert.That(fromZeroes, Is.EqualTo(defaultInstance));
        });
    }

    [Test]
    public void Constructor_ValidHex_ParsesCorrectly()
    {
        // Arrange & Act
        var id = new MongoId(ValidHex);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(id.IsEmpty, Is.False);
            Assert.That(id.ToString(), Is.EqualTo(ValidHex));
        });
    }

    [TestCase("507f1f77bcf86cd79943901")]  // 23 chars
    [TestCase("507f1f77bcf86cd7994390112")] // 25 chars
    public void Constructor_InvalidLength_ThrowsArgumentException(string invalidLengthHex)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = new MongoId(invalidLengthHex));
    }

    [TestCase("507f1f77bcf86cd79943901g")] // 'g' is out of range
    [TestCase("507f1f77bcf86cd79943901X")] // 'X' is out of range
    [TestCase("507f1f77bcf86cd79943901-")] // Special character
    public void Constructor_InvalidHexCharacters_ThrowsFormatException(string invalidCharHex)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => _ = new MongoId(invalidCharHex));
    }

    [Test]
    public void ParameterlessConstructor_GeneratesUniqueSequentialIds()
    {
        // Arrange & Act
        var id1 = new MongoId();
        var id2 = new MongoId();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(id1.IsEmpty, Is.False);
            Assert.That(id2.IsEmpty, Is.False);
            Assert.That(id1, Is.Not.EqualTo(id2));
        });
    }

    [Test]
    public void Empty_ReturnsDefaultStruct()
    {
        // Arrange & Act
        var empty = MongoId.Empty();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(empty.IsEmpty, Is.True);
            Assert.That(empty, Is.EqualTo(default(MongoId)));
            Assert.That(empty.ToString(), Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void Equals_StateEquality_ReturnsTrueForMatchingData()
    {
        // Arrange
        var id1 = new MongoId(ValidHex);
        var id2 = new MongoId(ValidHex);
        var diff = new MongoId();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(id1.Equals(id2), Is.True);
            Assert.That(id1 == id2, Is.True);
            Assert.That(id1 != id2, Is.False);

            Assert.That(id1.Equals(diff), Is.False);
            Assert.That(id1 == diff, Is.False);
            Assert.That(id1 != diff, Is.True);
        });
    }

    [Test]
    public void Equals_StringComparison_ValidatesCorrectly()
    {
        // Arrange
        var id = new MongoId(ValidHex);
        var mixedCaseHex = "507F1F77BCF86CD799439011";

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(id.Equals(ValidHex), Is.True);
            Assert.That(id.Equals(mixedCaseHex), Is.True); // Hex parsing handles uppercase
            Assert.That(id.Equals("invalidstringlen"), Is.False);
            Assert.That(id.Equals((string?) null), Is.False);
        });
    }

    [Test]
    public void GetHashCode_IdenticalObjects_YieldIdenticalHash()
    {
        // Arrange
        var id1 = new MongoId(ValidHex);
        var id2 = new MongoId(ValidHex);

        // Act & Assert
        Assert.That(id1.GetHashCode(), Is.EqualTo(id2.GetHashCode()));
    }

    [Test]
    public void ImplicitOperators_ConvertCorrectlyBetweenStringAndMongoId()
    {
        // Arrange & Act
        MongoId id = ValidHex;         // Implicit conversion from string
        string hexStr = id;            // Implicit conversion to string

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(id.IsEmpty, Is.False);
            Assert.That(hexStr, Is.EqualTo(ValidHex));
        });
    }

    [Test]
    public void ToString_OutputScenarios_BehavesCorrectly()
    {
        // Arrange
        var upperCaseInput = "507F1F77BCF86CD799439011";
        var expectedLower = "507f1f77bcf86cd799439011";

        var idFromUpper = new MongoId(upperCaseInput);
        var emptyId = MongoId.Empty();

        // Act
        var outputFromParsed = idFromUpper.ToString();
        var outputFromEmpty = emptyId.ToString();

        // Assert
        Assert.Multiple(() =>
        {
            // Verifies zero-allocation path transforms uppercase inputs to lowercase outputs
            Assert.That(outputFromParsed, Is.EqualTo(expectedLower));

            // Verifies empty instance strictly yields string.Empty
            Assert.That(outputFromEmpty, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void Write_ValidMongoId_WritesCorrectJsonString()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new StringToMongoIdConverter();

        // Act
        writer.WriteStartObject();
        writer.WritePropertyName("id");
        converter.Write(writer, _testId, _options!);
        writer.WriteEndObject();
        writer.Flush();

        // Assert
        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());
        Assert.That(jsonOutput, Is.EqualTo($"{{\"id\":\"{TestHex}\"}}"));
    }

    [Test]
    public void WriteAsPropertyName_ValidMongoId_WritesCorrectPropertyName()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new StringToMongoIdConverter();

        // Act
        writer.WriteStartObject();
        converter.WriteAsPropertyName(writer, _testId, _options!);
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();
        writer.Flush();

        // Assert
        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());
        Assert.That(jsonOutput, Is.EqualTo($"{{\"{TestHex}\":true}}"));
    }

    [Test]
    public void Write_EmptyMongoId_WritesZeroHexString()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new StringToMongoIdConverter();
        var emptyId = MongoId.Empty();

        writer.WriteStartObject();
        writer.WritePropertyName("id");
        converter.Write(writer, emptyId, _options!);
        writer.WriteEndObject();
        writer.Flush();

        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());
        Assert.That(jsonOutput, Is.EqualTo("{\"id\":\"\"}"));
        Console.WriteLine($"Output: {jsonOutput}");
    }

    [TestCase("677ddb67406e9918a0264bbz", false, "677ddb67406e9918a0264bbz contains invalid char `z`, but result was true")]
    [TestCase("677ddb67406e9918a0264bbcc", false, "677ddb67406e9918a0264bbcc is 25 characters, but result was true")]
    [TestCase("677ddb67406e9918a0264bbc", true, "IsValidMongoId() `677ddb67406e9918a0264bbc` is a valid mongoId, but result was false")]
    public void IsValidMongoIdTest(string mongoId, bool passes, string failMessage)
    {
        var isValid = MongoId.IsValidMongoId(mongoId);
        Assert.AreEqual(passes, isValid, failMessage);
    }

    [Test]
    public void MultiThreadedMongoIDGenerationTest()
    {
        const int targetCount = 1_000_000;

        // Store raw structs to eliminate heap allocations during generation
        var concurrentBag = new ConcurrentBag<MongoId>();

        var stopwatch = Stopwatch.StartNew();

        Parallel.For(
            0,
            targetCount,
            _ =>
            {
                var mongoId = new MongoId();
                concurrentBag.Add(mongoId);
            }
        );

        stopwatch.Stop();
        Console.WriteLine($"Generated {targetCount:N0} IDs in: {stopwatch.ElapsedMilliseconds} ms");

        // Verify uniqueness with minimum allocations
        var totalCount = concurrentBag.Count;

        // Explicitly size the HashSet capacity to prevent resizing overhead
        var uniqueSet = new HashSet<MongoId>(totalCount);
        foreach (var id in concurrentBag)
        {
            uniqueSet.Add(id);
        }

        var uniqueCount = uniqueSet.Count;

        Assert.That(uniqueCount, Is.EqualTo(totalCount),
            $"Expected all generated MongoId's to be unique, but found: {totalCount - uniqueCount} duplicates.");
    }
}
