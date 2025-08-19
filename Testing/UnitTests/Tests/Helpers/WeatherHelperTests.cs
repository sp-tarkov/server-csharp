using NUnit.Framework;
using SPTarkov.Server.Core.Helpers;

namespace UnitTests.Tests.Helpers;

[TestFixture]
public class WeatherHelperTests
{
    private WeatherHelper _weatherHelper;

    [OneTimeSetUp]
    public void Initialize()
    {
        _weatherHelper = DI.GetInstance().GetService<WeatherHelper>();
    }

    [TestCase(1755621231, 22, 56, 57)]
    [TestCase(1754120368, 8, 36, 16)]
    [TestCase(1714120368, 14, 49, 36)]
    [TestCase(1724120368, 19, 16, 16)]
    public void GetInRaidTime_WithDifferentTimestamps_ExpectCorrectEFTTime(
        long timestamp,
        int expectedHour,
        int expectedMinute,
        int expectedSecond)
    {
        var timeOutput = _weatherHelper.GetInRaidTime(timestamp);

        Assert.AreEqual(expectedHour, timeOutput.Hour, $"Unexpected hour! {timeOutput.Hour}");
        Assert.AreEqual(expectedMinute, timeOutput.Minute, $"Unexpected minute! {timeOutput.Minute}");
        Assert.AreEqual(expectedSecond, timeOutput.Second, $"Unexpected second! {timeOutput.Second}");
    }
}
