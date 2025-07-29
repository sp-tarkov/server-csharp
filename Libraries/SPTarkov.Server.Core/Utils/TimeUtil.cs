using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class TimeUtil
{
    public const int OneHourAsSeconds = 3600;

    /// <summary>
    ///     Gets the current date as a formatted UTC string.
    /// </summary>
    /// <returns>The current date as 'YYYY-MM-DD'.</returns>
    public string GetDate()
    {
        return DateTimeOffset.UtcNow.FormatToBsgDate();
    }

    public DateTime GetDateTimeNow()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    ///     Gets the current time as a formatted UTC string.
    /// </summary>
    /// <returns>The current time as 'HH-MM-SS'.</returns>
    public string GetTime()
    {
        return DateTimeOffset.UtcNow.FormatToBsgTime();
    }

    /// <summary>
    ///     Gets the current timestamp in seconds in UTC.
    /// </summary>
    /// <returns>The current timestamp in seconds since the Unix epoch in UTC.</returns>
    public long GetTimeStamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    ///     Gets the start of day timestamp for the given date
    /// </summary>
    /// <param name="timestamp">datetime to get the time stamp for, if null it uses current date.</param>
    /// <returns>Unix epoch for the start of day of the calculated date</returns>
    public long GetStartOfDayTimeStamp(long? timestamp)
    {
        var now = timestamp.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Value).DateTime : GetDateTimeNow();

        var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        return ((DateTimeOffset)startOfDay).ToUnixTimeMilliseconds();
    }

    /// <summary>
    ///     Get timestamp of today + passed in day count
    /// </summary>
    /// <param name="daysFromNow">Days from now</param>
    /// <returns></returns>
    public long GetTimeStampFromNowDays(int daysFromNow)
    {
        return DateTimeOffset.UtcNow.AddDays(daysFromNow).ToUnixTimeSeconds();
    }

    /// <summary>
    ///     Get timestamp of today + passed in hour count
    /// </summary>
    /// <param name="hoursFromNow"></param>
    /// <returns></returns>
    public long GetTimeStampFromNowHours(int hoursFromNow)
    {
        return DateTimeOffset.UtcNow.AddHours(hoursFromNow).ToUnixTimeSeconds();
    }

    /// <summary>
    ///     Gets the current time in UTC in a format suitable for mail in EFT.
    /// </summary>
    /// <returns>The current time as 'HH:MM' in UTC.</returns>
    /// GetTimeMailFormat
    public string GetBsgTimeMailFormat()
    {
        return DateTimeOffset.UtcNow.ToString("HH:mm");
    }

    /// <summary>
    ///     Gets the current date in UTC in a format suitable for emails in EFT.
    /// </summary>
    /// <returns>The current date as 'DD.MM.YYYY' in UTC.</returns>
    public string GetBsgDateMailFormat()
    {
        return DateTimeOffset.UtcNow.ToString("dd.MM.yyyy");
    }

    /// <summary>
    ///     Converts a number of hours into seconds.
    /// </summary>
    /// <param name="hours">The number of hours to convert.</param>
    /// <returns>The equivalent number of seconds.</returns>
    public int GetHoursAsSeconds(int hours)
    {
        return OneHourAsSeconds * hours;
    }

    /// <summary>
    ///     Gets the time stamp of the start of the next hour in UTC
    /// </summary>
    /// <returns>Time stamp of the next hour in unix time seconds</returns>
    public long GetTimeStampOfNextHour()
    {
        var now = DateTime.UtcNow;
        var timeUntilNextHour = TimeSpan
            .FromMinutes(60 - now.Minute)
            .Subtract(TimeSpan.FromSeconds(now.Second))
            .Subtract(TimeSpan.FromMilliseconds(now.Millisecond));

        var time = ((DateTimeOffset)now.Add(timeUntilNextHour)).ToUnixTimeSeconds();

        return time;
    }

    /// <summary>
    ///     Returns the current days timestamp at 00:00
    ///     e.g. current time: 13th March 14:22 will return 13th March 00:00
    /// </summary>
    /// <returns>Timestamp</returns>
    public long GetTodayMidnightTimeStamp()
    {
        var now = DateTime.UtcNow;
        var hours = now.Hour;
        var minutes = now.Minute;

        // If minutes greater than 0, subtract 1 hour
        if (hours > 0 && minutes > 0)
        {
            hours--;
        }

        // Create a new DateTime with the last full hour, 0 minutes, and 0 seconds
        var lastFullHour = new DateTime(now.Year, now.Month, now.Day, hours, 0, 0);

        return ((DateTimeOffset)lastFullHour).ToUnixTimeSeconds();
    }

    /// <summary>
    ///     Takes a timestamp and converts to its date with Epoch
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public DateTime GetDateTimeFromTimeStamp(long timeStamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timeStamp).DateTime;
    }

    public int GetSecondsAsMilliseconds(int seconds)
    {
        return seconds * 60 * 1000;
    }
}
