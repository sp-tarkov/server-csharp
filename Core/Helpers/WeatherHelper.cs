namespace Core.Helpers;

public class WeatherHelper
{
    public WeatherHelper()
    {
        
    }
    
    /// <summary>
    /// Get the current in-raid time - does not include an accurate date, only time
    /// </summary>
    /// <param name="currentDate">(new Date())</param>
    /// <returns>Date object of current in-raid time</returns>
    public DateTime GetInRaidTime(double? timestamp = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the current raid at nighttime
    /// </summary>
    /// <param name="timeVariant">PASS OR CURR (from raid settings)</param>
    /// <returns>True when nighttime</returns>
    public bool IsNightTime(DateTime timeVariant)
    {
        throw new NotImplementedException();
    }

    public bool IsHourAtNightTime(int currentHour)
    {
        throw new NotImplementedException();
    }
}
