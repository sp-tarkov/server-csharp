using Core.Models.Eft.Weather;
using Core.Models.Enums;
using Core.Models.Spt.Config;

namespace Core.Generators;

public class WeatherGenerator
{
    public WeatherGenerator()
    {
    }

    /**
     * Get current + raid datetime and format into correct BSG format and return
     * @param data Weather data
     * @returns WeatherData
     */
    public WeatherData CalculateGameTime(WeatherData data)
    {
        throw new NotImplementedException();
    }

    /**
     * Get server uptime seconds multiplied by a multiplier and add to current time as seconds
     * Format to BSGs requirements
     * @param currentDate current date
     * @returns formatted time
     */
    protected string GetBsgFormattedInRaidTime()
    {
        throw new NotImplementedException();
    }

    /**
     * Get current time formatted to fit BSGs requirement
     * @param date date to format into bsg style
     * @returns Time formatted in BSG format
     */
    protected string GetBsgFormattedTime(DateTime date)
    {
        throw new NotImplementedException();
    }

    /**
     * Return randomised Weather data with help of config/weather.json
     * @param currentSeason the currently active season
     * @param timestamp OPTIONAL what timestamp to generate the weather data at, defaults to now when not supplied
     * @returns Randomised weather data
     */
    public Weather GenerateWeather(Season currentSeason, int? timestamp = null)
    {
        throw new NotImplementedException();
    }

    protected SeasonalValues GetWeatherValuesBySeason(Season currentSeason)
    {
        throw new NotImplementedException();
    }

    /**
     * Choose a temprature for the raid based on time of day
     * @param currentSeason What season tarkov is currently in
     * @param inRaidTimestamp What time is the raid running at
     * @returns Timestamp
     */
    protected double GetRaidTemperature(SeasonalValues weather, int inRaidTimestamp)
    {
        throw new NotImplementedException();
    }

    /**
     * Set Weather date/time/timestamp values to now
     * @param weather Object to update
     * @param timestamp OPTIONAL, define timestamp used
     */
    protected void SetCurrentDateTime(Weather weather, int? timestamp = null)
    {
        throw new NotImplementedException();
    }

    protected WindDirection GetWeightedWindDirection(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedClouds(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedWindSpeed(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedFog(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedRain(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetRandomFloat(double min, double max, int precision = 3)
    {
        throw new NotImplementedException();
    }
}