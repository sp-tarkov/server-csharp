using Core.Models.Eft.Weather;
using Core.Models.Enums;

namespace Core.Services;

public class RaidWeatherService
{
    /// <summary>
    /// Generate 24 hours of weather data starting from midnight today
    /// </summary>
    public void GenerateWeather(Season currentSeason)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a time period to increment by, e.g 15 or 30 minutes as milliseconds
    /// </summary>
    /// <returns>milliseconds</returns>
    protected long GetWeightedWeatherTimePeriodMs()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find the first matching weather object that applies to the current time
    /// </summary>
    public Weather GetCurrentWeather()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find the first matching weather object that applies to the current time + all following weather data generated
    /// </summary>
    public List<Weather> GetUpcomingWeather()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ensure future weather data exists
    /// </summary>
    protected void ValidateWeatherDataExists(Season currentSeason)
    {
        throw new NotImplementedException();
    }
}
