﻿using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class WeatherHelper(
    ISptLogger<WeatherHelper> _logger,
    TimeUtil _timeUtil,
    ConfigServer _configServer
)
{
    protected WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();

    /// <summary>
    ///     Get the current in-raid time - does not include an accurate date, only time
    /// </summary>
    /// <param name="timestamp">(new Date())</param>
    /// <returns>Date object of current in-raid time</returns>
    public DateTime GetInRaidTime(long? timestamp = null)
    {
        // tarkov time = (real time * 7 % 24 hr) + 3 hour
        var russiaOffsetMilliseconds = _timeUtil.GetHoursAsSeconds(3) * 1000;
        var twentyFourHoursMilliseconds = _timeUtil.GetHoursAsSeconds(24) * 1000;
        var currentTimestampMilliSeconds = timestamp ?? _timeUtil.GetTimeStamp();

        return _timeUtil.GetDateTimeFromTimeStamp(
            (long)(
                russiaOffsetMilliseconds
                + currentTimestampMilliSeconds * _weatherConfig.Acceleration
            ) % twentyFourHoursMilliseconds
        );
    }

    /// <summary>
    ///     Is the current raid at nighttime
    /// </summary>
    /// <param name="timeVariant">PASS OR CURR (from raid settings)</param>
    /// <param name="mapLocation">map name. E.g. factory4_day</param>
    /// <returns>True when nighttime</returns>
    public bool IsNightTime(DateTimeEnum timeVariant, string mapLocation)
    {
        switch (mapLocation)
        {
            // Factory differs from other maps, has static times
            case "factory4_night":
                return true;
            case "factory4_day":
                return false;
        }

        var time = GetInRaidTime();

        // getInRaidTime() provides left side value, if player chose right side, set ahead 12 hrs
        if (timeVariant == DateTimeEnum.PAST)
        {
            time = time.AddHours(12);
        }

        // Night if after 9pm or before 5am
        return time.Hour > 21 || time.Hour < 5;
    }

    public bool IsHourAtNightTime(int currentHour)
    {
        return currentHour > 21 || currentHour <= 5;
    }
}
