using System.Collections.Concurrent;
using System.Diagnostics;
using NUnit.Framework;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;

namespace UnitTests.Tests;

[TestFixture]
public class MongoIDTests
{
    [OneTimeSetUp]
    public void Initialize() { }

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

    [TestCase("677ddb67406e9918a0264bbz", false, "677ddb67406e9918a0264bbz contains invalid char `z`, but result was true")]
    [TestCase("677ddb67406e9918a0264bbcc", false, "677ddb67406e9918a0264bbcc is 25 characters, but result was true")]
    [TestCase("677ddb67406e9918a0264bbc", true, "IsValidMongoId() `677ddb67406e9918a0264bbc` is a valid mongoId, but result was false")]
    public void IsValidMongoIdTest(string mongoId, bool passes, string failMessage)
    {
        var result = new MongoId(mongoId);
        Assert.AreEqual(passes, passes, result, failMessage);
    }

    [Test]
    public void MultiThreadedMongoIDGenerationTest()
    {
        var concurrentBag = new ConcurrentBag<string>();
        var random = new Random();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Parallel.For(
            0,
            1000,
            i =>
            {
                Thread.Sleep(random.Next(0, 10));
                var mongoId = new MongoId();
                concurrentBag.Add(mongoId);
            }
        );

        stopwatch.Stop();
        Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        var uniqueCount = concurrentBag.Distinct().Count();
        var totalCount = concurrentBag.Count;
        Assert.AreEqual(
            totalCount,
            uniqueCount,
            $"Expected all generated MongoId's to be unique, but found: {totalCount - uniqueCount} duplicates."
        );
    }
}
