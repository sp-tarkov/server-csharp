using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Helpers;

[Injectable]
public class WeatherHelper
{
    private readonly ILogger _logger;
    private readonly TimeUtil _timeUtil;
    private readonly ConfigServer _configServer;

    private readonly WeatherConfig _weatherConfig;

    public WeatherHelper
    (
        ILogger logger,
        TimeUtil timeUtil,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _configServer = configServer;

        _weatherConfig = _configServer.GetConfig<WeatherConfig>(ConfigTypes.WEATHER);
    }

    /// <summary>
    /// Get the current in-raid time - does not include an accurate date, only time
    /// </summary>
    /// <param name="currentDate">(new Date())</param>
    /// <returns>Date object of current in-raid time</returns>
    public DateTime GetInRaidTime(double? timestamp = null)
    {
        // tarkov time = (real time * 7 % 24 hr) + 3 hour
        var russiaOffsetMilliseconds = _timeUtil.GetHoursAsSeconds(3) * 1000;
        var twentyFourHoursMilliseconds = _timeUtil.GetHoursAsSeconds(24) * 1000;
        var currentTimestampMilliSeconds = (timestamp is not null) ? timestamp : _timeUtil.GetTimeStamp();

        return new DateTime().AddMilliseconds(
            (russiaOffsetMilliseconds + russiaOffsetMilliseconds * _weatherConfig.Acceleration) %
                                              twentyFourHoursMilliseconds);
    }

    /// <summary>
    /// Is the current raid at nighttime
    /// </summary>
    /// <param name="timeVariant">PASS OR CURR (from raid settings)</param>
    /// <returns>True when nighttime</returns>
    public bool IsNightTime(DateTimeEnum timeVariant)
    {
        var time = GetInRaidTime();
        
        // getInRaidTime() provides left side value, if player chose right side, set ahead 12 hrs
        if (timeVariant == DateTimeEnum.PAST)
            time.AddHours(12);
        
        // Night if after 9pm or before 5am
        return time.Hour > 21 || time.Hour < 5;
    }

    public bool IsHourAtNightTime(int currentHour)
    {
        return currentHour > 21 || currentHour <= 5;
    }
}
