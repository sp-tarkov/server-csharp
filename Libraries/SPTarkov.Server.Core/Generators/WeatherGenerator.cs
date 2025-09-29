using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class WeatherGenerator(
    ISptLogger<WeatherGenerator> logger,
    TimeUtil timeUtil,
    SeasonalEventService seasonalEventService,
    WeatherHelper weatherHelper,
    ConfigServer configServer,
    WeightedRandomHelper weightedRandomHelper,
    RandomUtil randomUtil,
    ICloner cloner
)
{
    protected readonly WeatherConfig WeatherConfig = configServer.GetConfig<WeatherConfig>();

    /// <summary>
    ///     Get current + raid datetime and format into correct BSG format.
    /// </summary>
    /// <param name="data"> Weather data </param>
    /// <returns> WeatherData </returns>
    public void CalculateGameTime(WeatherData data)
    {
        var computedDate = timeUtil.GetDateTimeNow();
        var formattedDate = computedDate.FormatToBsgDate();

        data.Date = formattedDate;
        data.Time = GetBsgFormattedInRaidTime();
        data.Acceleration = WeatherConfig.Acceleration;

        data.Season = seasonalEventService.GetActiveWeatherSeason();
    }

    /// <summary>
    ///     Get server uptime seconds multiplied by a multiplier and add to current time as seconds.
    ///     Formatted to BSGs requirements
    /// </summary>
    /// <returns>Formatted time as String </returns>
    protected string GetBsgFormattedInRaidTime()
    {
        return weatherHelper.GetInRaidTime().GetBsgFormattedWeatherTime();
    }

    /// <summary>
    /// Generate a weather object to send to client
    /// </summary>
    /// <param name="currentSeason">Whaat season is weather being generated for</param>
    /// <param name="presetWeights">Weather preset weights to pick from</param>
    /// <param name="timestamp">Optional - Current time</param>
    /// <param name="previousPreset">Optional -What weather preset was last generated</param>
    /// <returns>Weather</returns>
    public Weather GenerateWeather(
        Season currentSeason,
        ref Dictionary<WeatherPreset, double> presetWeights,
        long? timestamp = null,
        WeatherPreset? previousPreset = null
    )
    {
        if (previousPreset is not null && presetWeights.Any())
        {
            // We know last picked preset, Adjust weights
            // Make it less likely to be picked now
            presetWeights[previousPreset.Value] -= 1;
            logger.Info($"{previousPreset.Value} weight reduced by: 1 to: {presetWeights[previousPreset.Value]}");
        }
        else
        {
            // Get fresh cloned weights from config
            presetWeights = cloner.Clone(WeatherConfig.Weather.WeatherPresetWeight);
        }

        // Assign value to previousPreset to be picked up next loop
        previousPreset = weightedRandomHelper.GetWeightedValue(presetWeights);
        logger.Warning($"Chose: {previousPreset}");

        // Check if chosen preset has been exhausted and reset if necessary
        if (presetWeights[previousPreset.Value] <= 0)
        {
            logger.Info($"{previousPreset.Value} is 0, resetting weights");
            // Force fresh presets to be picked
            presetWeights.Clear();
        }

        return GenerateWeatherByPreset(previousPreset.Value, currentSeason, timestamp);
    }

    protected Weather GenerateWeatherByPreset(WeatherPreset chosenPreset, Season currentSeason, long? timestamp)
    {
        // TODO: handle currentSeason, apply additive values/overwrite existing?

        Weather result;
        var presetWeights = GetWeatherWeightsByPreset(chosenPreset);
        switch (chosenPreset)
        {
            case WeatherPreset.SUNNY:
                result = GenerateSunnyWeather(presetWeights);
                break;
            case WeatherPreset.RAINY:
                result = GenerateRainyWeather(presetWeights);
                break;
            case WeatherPreset.CLOUDY:
                result = GenerateCloudyWeather(presetWeights);
                break;
            default:
                presetWeights = GetWeatherWeightsByPreset(WeatherPreset.SUNNY);
                result = GenerateSunnyWeather(presetWeights);
                break;
        }

        // Set time values in result using now or passed in timestamp
        SetCurrentDateTime(result, timestamp);

        // Must occur after SetCurrentDateTime()
        result.Temperature = GetRaidTemperature(presetWeights, result.SptInRaidTimestamp ?? 0);

        // Needed by RaidWeatherService
        result.SptChosenPreset = chosenPreset;

        return result;
    }

    protected Weather GenerateSunnyWeather(PresetWeights weatherWeights)
    {
        var result = new Weather
        {
            Pressure = GetRandomDouble(weatherWeights.Pressure.Min, weatherWeights.Pressure.Max),
            Temperature = 0, // Handled in caller
            Fog = GetWeightedFog(weatherWeights),
            RainIntensity = 0,
            Rain = 0,
            WindGustiness = GetRandomDouble(weatherWeights.WindGustiness.Min, weatherWeights.WindGustiness.Max, 2),
            WindDirection = GetWeightedWindDirection(weatherWeights),
            WindSpeed = GetWeightedWindSpeed(weatherWeights),
            Cloud = GetWeightedClouds(weatherWeights),
            Time = string.Empty,
            Date = string.Empty,
            Timestamp = 0,
            SptInRaidTimestamp = 0, // Handled in caller
        };

        return result;
    }

    protected Weather GenerateRainyWeather(PresetWeights weatherWeights)
    {
        var clouds = GetWeightedClouds(weatherWeights);

        var result = new Weather
        {
            Pressure = GetRandomDouble(weatherWeights.Pressure.Min, weatherWeights.Pressure.Max),
            Temperature = 0, // // Handled in caller
            Fog = GetWeightedFog(weatherWeights),
            RainIntensity = GetRandomDouble(weatherWeights.RainIntensity.Min, weatherWeights.RainIntensity.Max),
            Rain = GetWeightedRain(weatherWeights),
            WindGustiness = GetRandomDouble(weatherWeights.WindGustiness.Min, weatherWeights.WindGustiness.Max, 2),
            WindDirection = GetWeightedWindDirection(weatherWeights),
            WindSpeed = GetWeightedWindSpeed(weatherWeights),
            Cloud = clouds,
            Time = string.Empty,
            Date = string.Empty,
            Timestamp = 0,
            SptInRaidTimestamp = 0, // Handled in caller
        };

        return result;
    }

    protected Weather GenerateCloudyWeather(PresetWeights weatherWeights)
    {
        var clouds = GetWeightedClouds(weatherWeights);

        var result = new Weather
        {
            Pressure = GetRandomDouble(weatherWeights.Pressure.Min, weatherWeights.Pressure.Max),
            Temperature = 0, // Handled in caller
            Fog = GetWeightedFog(weatherWeights),
            RainIntensity = 0,
            Rain = 0,
            WindGustiness = GetRandomDouble(weatherWeights.WindGustiness.Min, weatherWeights.WindGustiness.Max, 2),
            WindDirection = GetWeightedWindDirection(weatherWeights),
            WindSpeed = GetWeightedWindSpeed(weatherWeights),
            Cloud = clouds,
            Time = string.Empty,
            Date = string.Empty,
            Timestamp = 0,
            SptInRaidTimestamp = 0, // Handled in caller
        };

        return result;
    }

    protected PresetWeights GetWeatherWeightsByPreset(WeatherPreset weatherPreset)
    {
        if (!WeatherConfig.Weather.PresetWeights.TryGetValue(weatherPreset.ToString(), out var value))
        {
            return WeatherConfig.Weather.PresetWeights["default"];
        }

        return value;
    }

    /// <summary>
    ///     Choose a temperature for the raid based on time of day
    /// </summary>
    /// <param name="weather"> What season Tarkov is currently in </param>
    /// <param name="inRaidTimestamp"> What time is the raid running at </param>
    /// <returns> Timestamp </returns>
    protected double GetRaidTemperature(PresetWeights weather, long inRaidTimestamp)
    {
        // Convert timestamp to date so we can get current hour and check if its day or night
        var currentRaidTime = new DateTime(inRaidTimestamp);
        var minMax = weatherHelper.IsHourAtNightTime(currentRaidTime.Hour) ? weather.Temp.Night : weather.Temp.Day;

        return Math.Round(randomUtil.GetDouble(minMax.Min, minMax.Max), 2);
    }

    /// <summary>
    ///     Set Weather date/time/timestamp values to now
    /// </summary>
    /// <param name="weather"> Object to update </param>
    /// <param name="timestamp"> Optional, timestamp used </param>
    protected void SetCurrentDateTime(Weather weather, long? timestamp = null)
    {
        var inRaidTime = timestamp is null ? weatherHelper.GetInRaidTime() : weatherHelper.GetInRaidTime(timestamp.Value);
        var normalTime = inRaidTime.GetBsgFormattedWeatherTime();
        var formattedDate = (timestamp.HasValue ? timeUtil.GetDateTimeFromTimeStamp(timestamp.Value) : DateTime.UtcNow).FormatToBsgDate();
        var datetimeBsgFormat = $"{formattedDate} {normalTime}";

        weather.Timestamp = timestamp ?? timeUtil.GetTimeStamp(); // matches weather.date
        weather.Date = formattedDate; // matches weather.timestamp
        weather.Time = datetimeBsgFormat; // matches weather.timestamp
        weather.SptInRaidTimestamp = weather.Timestamp;
    }

    protected WindDirection GetWeightedWindDirection(PresetWeights weather)
    {
        return weightedRandomHelper.WeightedRandom(weather.WindDirection.Values, weather.WindDirection.Weights).Item;
    }

    protected double GetWeightedClouds(PresetWeights weather)
    {
        return double.Parse(weightedRandomHelper.GetWeightedValue(weather.Clouds));
    }

    protected double GetWeightedWindSpeed(PresetWeights weather)
    {
        return weightedRandomHelper.WeightedRandom(weather.WindSpeed.Values, weather.WindSpeed.Weights).Item;
    }

    protected double GetWeightedFog(PresetWeights weather)
    {
        return weightedRandomHelper.WeightedRandom(weather.Fog.Values, weather.Fog.Weights).Item;
    }

    protected double GetWeightedRain(PresetWeights weather)
    {
        return weightedRandomHelper.WeightedRandom(weather.Rain.Values, weather.Rain.Weights).Item;
    }

    protected double GetRandomDouble(double min, double max, int precision = 3)
    {
        return Math.Round(randomUtil.GetDouble(min, max), precision);
    }
}
