using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;


namespace Core.Helpers;

[Injectable]
public class WeatherHelper
{
    protected ISptLogger<WeatherHelper> _logger;
    protected TimeUtil _timeUtil;
    protected ConfigServer _configServer;

    protected WeatherConfig _weatherConfig;

    public WeatherHelper
    (
        ISptLogger<WeatherHelper> logger,
        TimeUtil timeUtil,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _configServer = configServer;

        _weatherConfig = _configServer.GetConfig<WeatherConfig>();
    }

    /// <summary>
    /// Get the current in-raid time - does not include an accurate date, only time
    /// </summary>
    /// <param name="currentDate">(new Date())</param>
    /// <returns>Date object of current in-raid time</returns>
    public DateTime GetInRaidTime(long? timestamp = null)
    {
        // tarkov time = (real time * 7 % 24 hr) + 3 hour
        var russiaOffsetMilliseconds = _timeUtil.GetHoursAsSeconds(3) * 1000;
        var twentyFourHoursMilliseconds = _timeUtil.GetHoursAsSeconds(24) * 1000;
        var currentTimestampMilliSeconds = timestamp.HasValue
            ? timestamp ?? 0
            : _timeUtil.GetTimeStampFromEpoch();

        return _timeUtil.GetDateTimeFromTimeStamp((long)
            (russiaOffsetMilliseconds + currentTimestampMilliSeconds * _weatherConfig.Acceleration) %
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
