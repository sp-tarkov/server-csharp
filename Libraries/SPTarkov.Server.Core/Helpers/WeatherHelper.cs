using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class WeatherHelper(ISptLogger<WeatherHelper> logger, TimeUtil timeUtil, ConfigServer configServer)
{
    protected readonly WeatherConfig WeatherConfig = configServer.GetConfig<WeatherConfig>();

    /// <summary>
    ///     Assumes current time
    ///     Get the current in-raid time - does not include an accurate date, only time
    /// </summary>
    /// <returns>Date object of current in-raid time</returns>
    public DateTime GetInRaidTime()
    {
        return GetInRaidTime(timeUtil.GetTimeStamp());
    }

    /// <summary>
    ///     Get the current in-raid time - does not include an accurate date, only time
    /// </summary>
    /// <param name="timestamp">Fixed timestamp</param>
    /// <returns>Date object of current in-raid time</returns>
    public DateTime GetInRaidTime(long timestamp)
    {
        // tarkov time = (real time * 7 % 24 hr) + 3 hour
        var russiaOffsetSeconds = timeUtil.GetHoursAsSeconds(3);
        var twentyFourHoursSeconds = timeUtil.GetHoursAsSeconds(24);
        var currentTimestampSeconds = timestamp;

        var tarkovTime = timeUtil.GetUtcDateTimeFromTimeStamp(
            (long)(russiaOffsetSeconds + currentTimestampSeconds * WeatherConfig.Acceleration) % twentyFourHoursSeconds
        );

        return tarkovTime;
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
        return time.Hour is > 21 or < 5;
    }

    public bool IsHourAtNightTime(int currentHour)
    {
        return currentHour is > 21 or <= 5;
    }
}
