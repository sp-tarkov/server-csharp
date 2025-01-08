using Core.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class TimeUtil
{
    public const int OneHourAsSeconds = 3600;

    /// <summary>
    /// Formats the time part of a date as a UTC string.
    /// </summary>
    /// <param name="dateTime">The date to format in UTC.</param>
    /// <returns>The formatted time as 'HH-MM-SS'.</returns>
    public string FormatTime(DateTime dateTime)
    {
        var hour = Pad(dateTime.ToUniversalTime().Hour);
        var minute = Pad(dateTime.ToUniversalTime().Minute);
        var second = Pad(dateTime.ToUniversalTime().Second);

        return $"{hour}-{minute}-{second}";
    }

    /// <summary>
    /// Formats the date part of a date as a UTC string.
    /// </summary>
    /// <param name="dateTime">The date to format in UTC.</param>
    /// <returns>The formatted date as 'YYYY-MM-DD'.</returns>
    public string FormatDate(DateTime dateTime)
    {
        var day = Pad(dateTime.ToUniversalTime().Day);
        var month = Pad(dateTime.ToUniversalTime().Month);
        var year = Pad(dateTime.ToUniversalTime().Year);

        return $"{year}-{month}-{day}";
    }

    /// <summary>
    /// Gets the current date as a formatted UTC string.
    /// </summary>
    /// <returns>The current date as 'YYYY-MM-DD'.</returns>
    public string GetDate()
    {
        return FormatDate(DateTime.Now);
    }

    /// <summary>
    /// Gets the current time as a formatted UTC string.
    /// </summary>
    /// <returns>The current time as 'HH-MM-SS'.</returns>
    public string GetTime()
    {
        return FormatTime(DateTime.Now);
    }

    /// <summary>
    /// Gets the current timestamp in seconds in UTC.
    /// </summary>
    /// <returns>The current timestamp in seconds since the Unix epoch in UTC.</returns>
    public long GetTimeStamp()
    {
        return DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Gets the start of day timestamp for the given date
    /// </summary>
    /// <param name="dateTime">datetime to get the time stamp for, if null it uses current date.</param>
    /// <returns>Unix epoch for the start of day of the calculated date</returns>
    public long GetStartOfDayTimeStamp(DateTime? dateTime)
    {
        var now = dateTime ?? DateTime.Now;

        return new DateTimeOffset(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0))
            .ToUnixTimeSeconds();
    }

    /// <summary>
    /// Get timestamp of today + passed in day count
    /// </summary>
    /// <param name="daysFromNow">Days from now</param>
    /// <returns></returns>
    public long GetTimeStampFromNowDays(int daysFromNow)
    {
        return DateTimeOffset.Now.AddDays(daysFromNow).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Get timestamp of today + passed in hour count
    /// </summary>
    /// <param name="hoursFromNow"></param>
    /// <returns></returns>
    public long GetTimeStampFromNowHours(int hoursFromNow)
    {
        return DateTimeOffset.Now.AddHours(hoursFromNow).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Gets the current time in UTC in a format suitable for mail in EFT.
    /// </summary>
    /// <returns>The current time as 'HH:MM' in UTC.</returns>
    public string GetTimeMailFormat()
    {
        return DateTime.UtcNow.ToString("HH:mm");
    }

    /// <summary>
    /// Gets the current date in UTC in a format suitable for emails in EFT.
    /// </summary>
    /// <returns>The current date as 'DD.MM.YYYY' in UTC.</returns>
    public string GetDateMailFormat()
    {
        return DateTime.UtcNow.ToString("dd.MM.yyyy");
    }

    /// <summary>
    /// Converts a number of hours into seconds.
    /// </summary>
    /// <param name="hours">The number of hours to convert.</param>
    /// <returns>The equivalent number of seconds.</returns>
    public int GetHoursAsSeconds(int hours)
    {
        return OneHourAsSeconds * hours;
    }

    /// <summary>
    /// Gets the time stamp of the start of the next hour in UTC
    /// </summary>
    /// <returns>Time stamp of the next hour in unix time seconds</returns>
    public long GetTimeStampOfNextHour()
    {
        var now = DateTime.UtcNow;

        var nextHour = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            0,
            0,
            DateTimeKind.Utc
        ).AddHours(1);

        return new DateTimeOffset(nextHour).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Returns the current days timestamp at 00:00
    /// e.g. current time: 13th March 14:22 will return 13th March 00:00
    /// </summary>
    /// <returns>Timestamp</returns>
    public long GetTodayMidNightTimeStamp()
    {
        var now = DateTime.UtcNow;

        var midNight = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            DateTimeKind.Utc
        );

        return new DateTimeOffset(midNight).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Pads a number with a leading zero if it is less than 10.
    /// </summary>
    /// <param name="number">The number to pad.</param>
    /// <returns>The padded number as a string.</returns>
    private static string Pad(int number)
    {
        return number.ToString().PadLeft(2, '0');
    }
}