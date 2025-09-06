namespace SPTarkov.Server.Core.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    ///     Formats the time part of a date as a UTC string.
    /// </summary>
    /// <param name="dateTimeOffset">The date to format in UTC.</param>
    /// <returns>The formatted time as 'HH-MM-SS'.</returns>
    public static string FormatToBsgTime(this DateTimeOffset dateTimeOffset)
    {
        var universalTime = dateTimeOffset.ToUniversalTime();
        var hour = Pad(universalTime.Hour);
        var minute = Pad(universalTime.Minute);
        var second = Pad(universalTime.Second);

        return $"{hour}-{minute}-{second}";
    }

    /// <summary>
    ///     Formats the time part of a date as a UTC string.
    /// </summary>
    /// <param name="dateTime">The date to format in UTC.</param>
    /// <returns>The formatted time as 'HH-MM-SS'.</returns>
    public static string FormatToBsgTime(this DateTime dateTime)
    {
        var universalTime = dateTime.ToUniversalTime();
        var hour = Pad(universalTime.Hour);
        var minute = Pad(universalTime.Minute);
        var second = Pad(universalTime.Second);

        return $"{hour}-{minute}-{second}";
    }

    /// <summary>
    ///     Formats the date part of a date as a UTC string.
    /// </summary>
    /// <param name="dateTimeOffset">The date to format in UTC.</param>
    /// <returns>The formatted date as 'YYYY-MM-DD'.</returns>
    public static string FormatToBsgDate(this DateTimeOffset dateTimeOffset)
    {
        var universalTime = dateTimeOffset.ToUniversalTime();
        var day = Pad(universalTime.Day);
        var month = Pad(universalTime.Month);
        var year = Pad(universalTime.Year);

        return $"{year}-{month}-{day}";
    }

    /// <summary>
    ///     Formats the date part of a date as a UTC string.
    /// </summary>
    /// <param name="dateTime">The date to format in UTC.</param>
    /// <returns>The formatted date as 'YYYY-MM-DD'.</returns>
    public static string FormatToBsgDate(this DateTime dateTime)
    {
        var universalTime = dateTime.ToUniversalTime();
        var day = Pad(universalTime.Day);
        var month = Pad(universalTime.Month);
        var year = Pad(universalTime.Year);

        return $"{year}-{month}-{day}";
    }

    /// <summary>
    ///     Pads a number with a leading zero if it is less than 10.
    /// </summary>
    /// <param name="number">The number to pad.</param>
    /// <returns>The padded number as a string.</returns>
    private static string Pad(int number)
    {
        return number.ToString().PadLeft(2, '0');
    }

    /// <summary>
    ///     Get current time formatted to fit BSGs requirement
    /// </summary>
    /// <param name="date"> Date to format into bsg style </param>
    /// <returns> Time formatted in BSG format </returns>
    public static string GetBsgFormattedWeatherTime(this DateTime date)
    {
        return date.FormatToBsgTime().Replace("-", ":").Replace("-", ":");
    }

    /// <summary>
    ///     Does the provided date fit between the two defined dates?
    ///     Excludes year
    ///     Inclusive of end date up to 23 hours 59 minutes
    /// </summary>
    /// <param name="dateToCheck">Date to check is between 2 dates</param>
    /// <param name="startMonth">Lower bound for month</param>
    /// <param name="startDay">Lower bound for day</param>
    /// <param name="endMonth">Upper bound for month</param>
    /// <param name="endDay">Upper bound for day</param>
    /// <returns>True when inside date range</returns>
    public static bool DateIsBetweenTwoDates(this DateTime dateToCheck, int startMonth, int startDay, int endMonth, int endDay)
    {
        var eventStartDate = new DateTime(dateToCheck.Year, startMonth, startDay);
        var eventEndDate = new DateTime(dateToCheck.Year, endMonth, endDay, 23, 59, 0);

        return dateToCheck >= eventStartDate && dateToCheck <= eventEndDate;
    }

    /// <summary>
    /// Get the closest monday to passed in datetime
    /// </summary>
    /// <param name="dateTime">Date to get closest monday of</param>
    /// <param name="startDay">Starting day of week - Default = Monday</param>
    /// <returns>Monday as DateTime</returns>
    public static DateTime GetClosestDate(this DateTime dateTime, DayOfWeek startDay = DayOfWeek.Monday)
    {
        // Calculate difference from current day to Monday
        var diff = (7 + (dateTime.DayOfWeek - startDay)) % 7;

        // Subtract difference to get date of most recent Monday
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Get the most recent requested day from date
    /// </summary>
    /// <param name="dateTime">Date to start from</param>
    /// <param name="desiredDay">Desired day to find</param>
    /// <param name="inclusiveOfToday">Should today be included in check, default = true</param>
    /// <returns>Datetime of desired day</returns>
    public static DateTime GetMostRecentPreviousDay(this DateTime dateTime, DayOfWeek desiredDay, bool inclusiveOfToday = true)
    {
        // Get difference in day count from today to what day we want
        var dayDifferenceCount = (dateTime.DayOfWeek - desiredDay + 7) % 7;

        // Today is wanted day + we are not counting today, we know desired day is exactly 7 days ago
        if (!inclusiveOfToday && dayDifferenceCount == 0)
        {
            dayDifferenceCount = 7;
        }

        // Remove count of day difference to get desired day
        return dateTime.AddDays(-dayDifferenceCount);
    }
}
